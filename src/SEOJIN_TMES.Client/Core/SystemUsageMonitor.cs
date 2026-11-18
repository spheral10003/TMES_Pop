using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenHardwareMonitor.Hardware;
using SEOJIN_TMES.Client.Common.Logging;

namespace SEOJIN_TMES.Client.Core
{
    /// <summary>
    /// 이 PC의 CPU · 메모리 · 드라이브 사용률을 주기적으로 읽어 <see cref="Updated"/>로 알린다(홈 화면 시스템 사용량 타일).
    ///
    /// CPU · 메모리는 OpenHardwareMonitorLib에서 읽는다.
    /// 드라이브는 <see cref="DriveInfo"/>로 읽는다 — OpenHardwareMonitorLib의 Storage는 물리 디스크 단위라 C · D 같은
    /// 드라이브 문자로 나뉘지 않고, 디스크 장치를 직접 열어야 해서 관리자 권한이 없으면(ClickOnce는 일반 권한) 아예 잡히지 않는다
    /// (2026-11-18 일반 권한 실측: Cpu · Memory만 나오고 Storage는 0건).
    ///
    /// 읽기는 백그라운드에서 한다. <see cref="Computer.Open"/>이 실측 3초 넘게 걸려 UI 스레드에서 하면 홈 화면이 멈춘다.
    /// 결과는 <see cref="Start"/>를 부른 스레드(UI)의 SynchronizationContext로 넘겨 받는 쪽이 Invoke 없이 컨트롤을 만지게 한다.
    /// </summary>
    public sealed class SystemUsageMonitor : IDisposable
    {
        /// <summary>
        /// 타일에 둘 수 있는 드라이브 수. 타일 높이를 다른 정보 타일(84)과 맞추면 CPU · 메모리와 함께 네 줄이 한계다 —
        /// 넘치는 드라이브는 문자 순서 뒤쪽부터 뺀다(C · D만 남는다).
        /// </summary>
        public const int MaxDrives = 2;

        private CancellationTokenSource _cts;
        private SynchronizationContext _uiContext;

        /// <summary>새 값을 읽을 때마다(UI 스레드에서) 발생한다.</summary>
        public event EventHandler<SystemUsageSnapshot> Updated;

        /// <summary>
        /// 주기 읽기를 시작한다. <paramref name="intervalMs"/>가 0 이하면 시작하지 않는다(App.config SystemMonitorPollingMs).
        /// UI 스레드에서 불러야 결과도 UI 스레드로 온다.
        /// </summary>
        public void Start(int intervalMs)
        {
            if (intervalMs <= 0 || _cts != null)
                return;

            _uiContext = SynchronizationContext.Current;
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            Task.Run(() => RunLoop(intervalMs, token));
        }

        public void Dispose()
        {
            // 루프는 취소를 보고 스스로 Computer를 닫는다 — 여기서 닫으면 읽는 도중인 Update와 엇갈린다.
            _cts?.Cancel();
            _cts = null;
            Updated = null;
        }

        private async Task RunLoop(int intervalMs, CancellationToken token)
        {
            // 하드웨어 초기화(3초 이상)를 기다리는 동안에도 드라이브 줄이 먼저 그려지게 한 번 보낸다.
            Publish(new SystemUsageSnapshot(0, 0, ReadDrives()), token);

            var computer = OpenComputer();
            try
            {
                while (!token.IsCancellationRequested)
                {
                    Publish(Read(computer), token);
                    await Task.Delay(intervalMs, token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // 홈 화면이 닫혔다 — 정상 종료.
            }
            catch (Exception ex)
            {
                // 백그라운드 예외가 새면 앱이 죽는다. 타일 값은 멈춘 채로 두고 로그만 남긴다.
                AppLog.Error("[시스템 사용량] 읽기 중단", ex);
            }
            finally
            {
                CloseComputer(computer);
            }
        }

        /// <summary>CPU · 메모리만 켠다. 실패하면 null — 드라이브 줄은 계속 갱신하고 CPU · 메모리는 0%로 둔다.</summary>
        private static Computer OpenComputer()
        {
            try
            {
                var computer = new Computer();
                computer.IsCpuEnabled = true;
                computer.IsMemoryEnabled = true;
                computer.Open(false);
                AppLog.Info($"[시스템 사용량] 하드웨어 {computer.Hardware.Count}건: " +
                            string.Join(", ", computer.Hardware.Select(h => $"{h.HardwareType}({h.Identifier})")));
                return computer;
            }
            catch (Exception ex)
            {
                AppLog.Warn($"[시스템 사용량] OpenHardwareMonitor 초기화 실패(CPU · 메모리는 0%로 표시) - {ex.Message}");
                return null;
            }
        }

        private static void CloseComputer(Computer computer)
        {
            try
            {
                computer?.Close();
            }
            catch (Exception ex)
            {
                AppLog.Warn($"[시스템 사용량] OpenHardwareMonitor 닫기 실패 - {ex.Message}");
            }
        }

        private static SystemUsageSnapshot Read(Computer computer)
        {
            float cpu = 0, memory = 0;
            if (computer != null)
            {
                cpu = ReadCpu(computer);
                memory = ReadMemory(computer);
            }

            return new SystemUsageSnapshot(cpu, memory, ReadDrives());
        }

        /// <summary>
        /// CPU 전체 사용률. CPU 부하는 앞뒤 두 번의 Update 차이로 계산되므로 매 주기 Update가 필요하다.
        /// 소켓이 여럿이면 소켓별 Total의 평균.
        /// </summary>
        private static float ReadCpu(Computer computer)
        {
            var totals = new List<float>();
            foreach (var hardware in computer.Hardware.Where(h => h.HardwareType == HardwareType.Cpu))
            {
                hardware.Update();
                var total = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name == "Total");
                if (total?.Value != null)
                    totals.Add(total.Value.Value);
            }

            return totals.Count == 0 ? 0 : totals.Average();
        }

        /// <summary>
        /// 물리 메모리 사용률. 라이브러리는 가상 메모리(/vram)도 Memory로 내주므로 물리 메모리(/pram)를 고른다.
        /// 식별자가 바뀐 버전이면 첫 Memory를 쓴다.
        /// </summary>
        private static float ReadMemory(Computer computer)
        {
            var memories = computer.Hardware.Where(h => h.HardwareType == HardwareType.Memory).ToList();
            var physical = memories.FirstOrDefault(h => h.Identifier.ToString() == "/pram") ?? memories.FirstOrDefault();
            if (physical == null)
                return 0;

            physical.Update();
            var load = physical.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load);
            return load?.Value ?? 0;
        }

        /// <summary>
        /// 고정 드라이브(C · D …)의 사용률. 이동식 · 네트워크 · 준비 안 된 드라이브는 뺀다 —
        /// USB를 꽂았다 뺄 때마다 줄이 생겼다 사라지면 타일이 흔들린다.
        /// </summary>
        private static List<DriveUsage> ReadDrives()
        {
            var drives = new List<DriveUsage>();
            try
            {
                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.DriveType != DriveType.Fixed || !drive.IsReady || drive.TotalSize <= 0)
                        continue;

                    var used = drive.TotalSize - drive.AvailableFreeSpace;
                    drives.Add(new DriveUsage(drive.Name.Substring(0, 1), used * 100f / drive.TotalSize));
                    if (drives.Count == MaxDrives)
                        break;
                }
            }
            catch (Exception ex)
            {
                // 드라이브가 읽는 도중 사라지는 등 — 이번 주기만 건너뛴다.
                AppLog.Warn($"[시스템 사용량] 드라이브 읽기 실패 - {ex.Message}");
            }

            return drives;
        }

        private void Publish(SystemUsageSnapshot snapshot, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            if (_uiContext == null)
            {
                Updated?.Invoke(this, snapshot);
                return;
            }

            _uiContext.Post(RaiseUpdated, snapshot);
        }

        private void RaiseUpdated(object state)
        {
            Updated?.Invoke(this, (SystemUsageSnapshot)state);
        }
    }

    /// <summary>한 번 읽은 사용률(0~100%).</summary>
    public sealed class SystemUsageSnapshot
    {
        public float CpuPercent { get; }
        public float MemoryPercent { get; }
        public IReadOnlyList<DriveUsage> Drives { get; }

        public SystemUsageSnapshot(float cpuPercent, float memoryPercent, IReadOnlyList<DriveUsage> drives)
        {
            CpuPercent = cpuPercent;
            MemoryPercent = memoryPercent;
            Drives = drives;
        }
    }

    /// <summary>드라이브 하나의 사용률. <see cref="Letter"/>는 "C"처럼 문자 하나.</summary>
    public sealed class DriveUsage
    {
        public string Letter { get; }
        public float Percent { get; }

        public DriveUsage(string letter, float percent)
        {
            Letter = letter;
            Percent = percent;
        }
    }
}
