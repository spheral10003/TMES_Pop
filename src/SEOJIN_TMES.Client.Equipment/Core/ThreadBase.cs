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

        private readonly object _sync = new();
        private Thread? _thread;
        private volatile bool _stopRequested;
        private volatile bool _suspended;
        private bool _disposed;

        public bool IsRunning => _thread is { IsAlive: true } && !_suspended;

        /// <summary>Stop()이 호출되었는지 여부. 파생 클래스의 루프 종료 판단용.</summary>
        protected bool IsStopRequested => _stopRequested;

        public virtual void Start()
        {
            // 스스로 Stop() 한 Thread가 아직 살아 있을 수 있으므로 먼저 확실히 정리한다.
            Stop();

            lock (_sync)
            {
                if (_thread is { IsAlive: true })
                {
                    return;
                }

                _stopRequested = false;
                _suspended = false;

                _thread = new Thread(Loop)
                {
                    IsBackground = true,
                    Name = GetType().Name
                };
                _thread.Start();
            }
        }

        public virtual void Stop()
        {
            Thread? thread;

            lock (_sync)
            {
                _stopRequested = true;
                _suspended = false;
                thread = _thread;

                // 자기 자신이 호출한 경우에는 참조를 지우지 않는다.
                // (루프가 실제로 빠져나갈 때 finally 에서 지운다.)
                if (thread != Thread.CurrentThread)
                {
                    _thread = null;
                }
            }

            if (thread is { IsAlive: true } && thread != Thread.CurrentThread)
            {
                thread.Join(500);
            }
        }

        public virtual void Suspend() => _suspended = true;

        public virtual void Resume() => _suspended = false;

        private void Loop()
        {
            try
            {
                OnStarted();

                while (!_stopRequested)
                {
                    if (_suspended)
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
                    if (_thread == Thread.CurrentThread)
                    {
                        _thread = null;
                    }
                }

                OnStopped();
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
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Stop();
        }
    }
}
