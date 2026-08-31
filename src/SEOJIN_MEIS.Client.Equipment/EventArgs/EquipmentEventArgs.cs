using System;
using SEOJIN_MEIS.Client.Equipment.Protocol;

namespace SEOJIN_MEIS.Client.Equipment
{
    /// <summary>
    /// 설비에서 Modbus 프레임을 수신했을 때.
    /// <see cref="Channel"/>이 <see cref="EquipmentChannel.Event"/>면 Customized Connection으로 올라온 실시간 이벤트,
    /// <see cref="EquipmentChannel.Standard"/>면 표준 Connection에서 요청과 짝이 맞지 않은(비요청) 프레임이다.
    /// </summary>
    public class EquipmentDataReceivedEventArgs : EventArgs
    {
        public string EquipmentId { get; }
        public ModbusFrame Frame { get; }
        public EquipmentChannel Channel { get; }

        public EquipmentDataReceivedEventArgs(string equipmentId, ModbusFrame frame, EquipmentChannel channel)
        {
            EquipmentId = equipmentId;
            Frame = frame;
            Channel = channel;
        }
    }

    public class EquipmentConnectionChangedEventArgs : EventArgs
    {
        public string EquipmentId { get; }
        public EquipmentConnectionState State { get; }
        public string Message { get; }
        public EquipmentChannel Channel { get; }

        public EquipmentConnectionChangedEventArgs(string equipmentId, EquipmentConnectionState state, string message, EquipmentChannel channel)
        {
            EquipmentId = equipmentId;
            State = state;
            Message = message;
            Channel = channel;
        }
    }

    public class EquipmentErrorEventArgs : EventArgs
    {
        public string EquipmentId { get; }
        public Exception Exception { get; }
        public EquipmentChannel Channel { get; }

        public EquipmentErrorEventArgs(string equipmentId, Exception exception, EquipmentChannel channel)
        {
            EquipmentId = equipmentId;
            Exception = exception;
            Channel = channel;
        }
    }
}
