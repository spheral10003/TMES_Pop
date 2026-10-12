using System;
using SEOJIN_TMES.Client.Equipment.Core;

namespace SEOJIN_TMES.Client.Equipment.Nutrunner
{
    /// <summary>
    /// Nutrunner 한 대의 통신 유지 시퀀스.
    /// 연결 → Alive 감시 → 끊기면 재연결 → 재연결 실패 시 알람 후 종료 까지를 담당한다.
    /// </summary>
    public interface INutrunnerSequence : IThreadBase, IDisposable
    {
        int Id { get; }

        INutrunner Nutrunner { get; }

        /// <summary>현재까지의 재연결 시도 횟수.</summary>
        int RetryCount { get; }

        /// <summary>연결 실패 등 알람 발생.</summary>
        event EventHandler<AlarmEventArgs>? AlarmOccurred;
    }
}
