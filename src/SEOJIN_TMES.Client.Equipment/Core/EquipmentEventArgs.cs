using System;

namespace SEOJIN_TMES.Client.Equipment.Core
{
    /// <summary>
    /// 모든 장비 이벤트의 공통 기반. 어떤 장비(ID)에서 발생했는지를 항상 담는다.
    /// </summary>
    public abstract class EquipmentEventArgs : EventArgs
    {
        /// <summary>장비 구분 ID. Modbus TransactionId와 동일한 값이다.</summary>
        public int Id { get; }

        protected EquipmentEventArgs(int id)
        {
            Id = id;
        }

    }

    /// <summary>연결 상태 변경 이벤트.</summary>
    public sealed class EquipmentConnectionEventArgs : EquipmentEventArgs
    {
        public bool IsConnected { get; }

        public EquipmentConnectionEventArgs(int id, bool isConnected) : base(id)
        {
            IsConnected = isConnected;
        }
    }

    /// <summary>알람 코드.</summary>
    public enum AlarmCode
    {
        None = 0,

        /// <summary>재연결 지정 횟수를 초과해서 연결에 실패함.</summary>
        ConnectFailed = 1001,

        /// <summary>통신 중 예외 발생.</summary>
        CommunicationError = 1002
    }

    /// <summary>알람 이벤트.</summary>
    public sealed class AlarmEventArgs : EquipmentEventArgs
    {
        public AlarmCode Code { get; }

        public string Message { get; }

        public DateTime OccurredAt { get; }

        public override string ToString() => $"[{OccurredAt:HH:mm:ss}] ID:{Id} {Code} - {Message}";

        public AlarmEventArgs(int id, AlarmCode code, string message) : base(id)
        {
            Code = code;
            Message = message;
            OccurredAt = DateTime.Now;
        }

        
    }
}
