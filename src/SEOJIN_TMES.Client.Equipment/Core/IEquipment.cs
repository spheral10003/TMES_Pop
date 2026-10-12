using System;

namespace SEOJIN_TMES.Client.Equipment.Core
{
    /// <summary>
    /// Edge Device 공통 인터페이스. 장비 종류와 무관한 최소 계약만 정의한다.
    /// 상위(View/Manager)는 구체 클래스가 아니라 이 인터페이스에만 의존한다.
    /// </summary>
    public interface IEquipment : IDisposable
    {
        /// <summary>장비 구분 ID (Modbus TransactionId와 동일).</summary>
        int Id { get; }

        /// <summary>표시용 이름.</summary>
        string Name { get; }

        bool IsConnected { get; }

        event EventHandler<EquipmentConnectionEventArgs>? Connected;

        /// <summary>통신이 끊겼을 때 발생. 재연결 시퀀스의 트리거로 사용된다.</summary>
        event EventHandler<EquipmentConnectionEventArgs>? Disconnected;

        event EventHandler<Exception>? ErrorOccurred;

        void Connect();

        void Disconnect();
    }
}
