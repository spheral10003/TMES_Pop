namespace SEOJIN_TMES.Client.Equipment.Core
{
    /// <summary>
    /// 주기 작업을 수행하는 단일 Thread의 제어 인터페이스.
    /// 실제 작업 내용(Execute)은 구현체 내부에 캡슐화되며, 외부로는 제어만 노출한다.
    /// </summary>
    public interface IThreadBase
    {
        /// <summary>Thread가 살아 있고 일시정지 상태가 아니면 true.</summary>
        bool IsRunning { get; }

        void Start();

        void Stop();

        void Suspend();

        void Resume();
    }
}
