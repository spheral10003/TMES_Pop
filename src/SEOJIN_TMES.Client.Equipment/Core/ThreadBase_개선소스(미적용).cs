using System;
using System.Threading;

namespace SEOJIN_TMES.Client.Equipment.Core
{
    /// <summary>
    /// 반복 실행 Thread의 공통 기반 클래스.
    ///
    /// 상속 받는 쪽은 <see cref="Execute"/> 한 번의 동작(1 tick)만 구현하면 되고,
    /// Thread 생성/정지/일시정지 같은 제어 로직은 전부 이 클래스가 캡슐화한다.
    /// </summary>
    public abstract class ThreadBase : IThreadBase, IDisposable
    {
        /// <summary>1 tick 사이의 대기 시간(ms).</summary>
        protected const int ThreadSleepTime = 50;

        /// <summary>
        /// Thread 1개의 수명 동안만 유효한 제어 상태.
        ///
        /// 정지 플래그를 클래스 필드 하나로 공유하면, Stop()의 Join이 시간 초과된 뒤
        /// Start()가 플래그를 false로 되돌리는 순간 아직 Execute()에 막혀 있던 옛 Thread까지
        /// 되살아나 루프가 2개가 된다. Thread마다 자기 상태를 따로 들고 있게 해서
        /// 새 Thread를 띄워도 옛 Thread의 정지 요청은 절대 취소되지 않도록 한다.
        /// </summary>
        private sealed class RunState
        {
            public readonly ThreadBase Owner;
            public volatile bool StopRequested;
            public volatile bool Suspended;

            public RunState(ThreadBase owner) => Owner = owner;
        }

        /// <summary>
        /// 지금 코드를 실행 중인 Thread가 속한 RunState.
        /// 파생 클래스가 Execute() 안에서 IsStopRequested를 볼 때 "현재 활성 Thread"가 아니라
        /// "자기 Thread"의 정지 여부를 봐야 하므로 Thread별로 기억해 둔다.
        /// </summary>
        [ThreadStatic]
        private static RunState? _ownRun;

        private readonly object _sync = new();
        private Thread? _thread;
        private RunState? _run;

        /// <summary>
        /// 가장 최근에 띄운 Thread. Stop()으로 활성 참조(_thread)를 지운 뒤에도
        /// 그 Thread가 실제로 끝날 때까지 남겨 두어, 다음 Thread가 종료를 기다릴 수 있게 한다.
        /// </summary>
        private Thread? _lastThread;
        private int _disposed;

        public bool IsRunning
        {
            get
            {
                lock (_sync)
                {
                    return _thread is { IsAlive: true } && _run is { StopRequested: false, Suspended: false };
                }
            }
        }

        /// <summary>
        /// Stop()이 호출되었는지 여부. 파생 클래스의 루프 종료 판단용.
        /// 루프 Thread 안에서 부르면 그 Thread 자신의 정지 요청을 돌려준다.
        /// </summary>
        protected bool IsStopRequested
        {
            get
            {
                var own = _ownRun;
                if (own != null && own.Owner == this)
                {
                    return own.StopRequested;
                }

                lock (_sync)
                {
                    return _run?.StopRequested ?? true;
                }
            }
        }

        public virtual void Start()
        {
            // 이전 Thread에 정지를 요청한다. Join이 시간 초과돼도 그 Thread의 RunState는
            // StopRequested=true 로 남으므로, Execute()에서 빠져나오는 즉시 스스로 끝난다.
            Stop();

            lock (_sync)
            {
                if (_disposed != 0)
                {
                    return;
                }

                var run = new RunState(this);
                var previous = _lastThread;
                var thread = new Thread(() => Loop(run, previous))
                {
                    IsBackground = true,
                    Name = GetType().Name
                };

                _run = run;
                _thread = thread;
                _lastThread = thread;
                thread.Start();
            }
        }

        public virtual void Stop()
        {
            Thread? thread;

            lock (_sync)
            {
                if (_run != null)
                {
                    _run.StopRequested = true;
                    _run.Suspended = false;
                }

                thread = _thread;

                // 참조를 지워도 옛 Thread는 자기 RunState로 종료를 판단하므로 안전하다.
                _thread = null;
                _run = null;
            }

            // 자기 자신(루프 Thread)이 호출한 경우에는 기다리지 않는다 — Join하면 교착된다.
            if (thread is { IsAlive: true } && thread != Thread.CurrentThread)
            {
                thread.Join(500);
            }
        }

        public virtual void Suspend()
        {
            lock (_sync)
            {
                if (_run != null)
                {
                    _run.Suspended = true;
                }
            }
        }

        public virtual void Resume()
        {
            lock (_sync)
            {
                if (_run != null)
                {
                    _run.Suspended = false;
                }
            }
        }

        private void Loop(RunState run, Thread? previous)
        {
            _ownRun = run;

            // 옛 Thread가 아직 Execute()에 막혀 있으면 끝날 때까지 기다린 뒤 시작한다.
            // 그래야 같은 설비에 대해 Execute()가 절대 겹쳐 돌지 않고, 옛 Thread의 OnStopped()도
            // 새 Thread의 OnStarted()보다 먼저 끝난다.
            // 호출자(UI 등)를 막지 않도록 Start()가 아니라 새 Thread 안에서 기다린다.
            // 정지 요청을 받아도 기다림은 끝까지 한다 — 중간에 빠지면 그다음 Thread가 옛 Thread와 겹친다.
            if (previous != null && previous != Thread.CurrentThread)
            {
                previous.Join();
            }

            var started = false;

            try
            {
                // 기다리는 사이 정지 요청을 받았으면 OnStarted/OnStopped 없이 조용히 끝낸다.
                if (run.StopRequested)
                {
                    return;
                }

                started = true;
                OnStarted();

                while (!run.StopRequested)
                {
                    if (run.Suspended)
                    {
                        Thread.Sleep(ThreadSleepTime);
                        continue;
                    }

                    Execute();
                    Thread.Sleep(ThreadSleepTime);
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            finally
            {
                lock (_sync)
                {
                    // 예외로 빠져나온 경우에도 IsRunning이 false가 되도록 활성 상태를 정리한다.
                    // 이미 새 Thread로 교체됐다면 그쪽 상태는 건드리지 않는다.
                    if (_run == run)
                    {
                        _run = null;
                        _thread = null;
                    }

                    if (_lastThread == Thread.CurrentThread)
                    {
                        _lastThread = null;
                    }
                }

                if (started)
                {
                    OnStopped();
                }

                _ownRun = null;
            }
        }

        /// <summary>Thread 루프에서 반복 호출되는 1 tick 동작.</summary>
        protected abstract void Execute();

        /// <summary>Thread 시작 직후 1회 호출.</summary>
        protected virtual void OnStarted()
        {
        }

        /// <summary>Thread 종료 직전 1회 호출.</summary>
        protected virtual void OnStopped()
        {
        }

        /// <summary>Thread 루프에서 예외가 발생했을 때 호출.</summary>
        protected virtual void OnError(Exception exception)
        {
        }

        public virtual void Dispose()
        {
            // 여러 Thread에서 동시에 Dispose를 불러도 정리는 한 번만 한다.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            Stop();
        }
    }
}
