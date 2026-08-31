using System;
using SEOJIN_MEIS.Client.Common.Configuration;

namespace SEOJIN_MEIS.Client.Equipment.Protocol
{
    /// <summary>POP 화면에서 설비로 내리는 제어 명령의 논리적 종류. 실제 Modbus 주소 매핑은 <see cref="ModbusRegisterMap"/>가 담당.</summary>
    public enum EquipmentCommandType
    {
        Start,
        Stop,
        Reset,
        StatusRequest,
        Custom
    }

    /// <summary>
    /// 화면/업무 코드가 다루는 논리 명령. 이 객체를 <see cref="ToModbusRequest"/>로 변환하면
    /// 설정(ModbusRegisterMap)에 따라 실제 Modbus 요청(Write/Read)이 만들어진다.
    /// 표준 Modbus-TCP connection으로만 전송할 수 있다(이벤트 Connection은 Read/Write 미지원).
    /// </summary>
    public class EquipmentCommand
    {
        public EquipmentCommandType Type { get; set; }

        /// <summary>Type이 Custom일 때 그대로 전송할 Modbus 요청.</summary>
        public ModbusRequest CustomRequest { get; set; }

        public static EquipmentCommand Start() => Of(EquipmentCommandType.Start);
        public static EquipmentCommand Stop() => Of(EquipmentCommandType.Stop);
        public static EquipmentCommand Reset() => Of(EquipmentCommandType.Reset);
        public static EquipmentCommand StatusRequest() => Of(EquipmentCommandType.StatusRequest);

        public static EquipmentCommand Custom(ModbusRequest request)
        {
            var command = new EquipmentCommand();
            command.Type = EquipmentCommandType.Custom;
            command.CustomRequest = request;
            return command;
        }

        /// <summary>논리 명령을 설정된 레지스터 맵에 따라 실제 Modbus 요청으로 변환한다.</summary>
        public ModbusRequest ToModbusRequest(ModbusRegisterMap map)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));

            switch (Type)
            {
                case EquipmentCommandType.Start:
                    return ModbusRequest.WriteSingleRegister(map.ControlRegisterAddress, map.StartValue);
                case EquipmentCommandType.Stop:
                    return ModbusRequest.WriteSingleRegister(map.ControlRegisterAddress, map.StopValue);
                case EquipmentCommandType.Reset:
                    return ModbusRequest.WriteSingleRegister(map.ControlRegisterAddress, map.ResetValue);
                case EquipmentCommandType.StatusRequest:
                    return map.StatusUsesInputRegisters
                        ? ModbusRequest.ReadInputRegisters(map.StatusRegisterAddress, map.StatusRegisterCount)
                        : ModbusRequest.ReadHoldingRegisters(map.StatusRegisterAddress, map.StatusRegisterCount);
                case EquipmentCommandType.Custom:
                    if (CustomRequest == null)
                        throw new InvalidOperationException("Custom 명령에는 CustomRequest가 필요합니다.");
                    return CustomRequest;
                default:
                    throw new NotSupportedException($"지원하지 않는 명령입니다: {Type}");
            }
        }

        private static EquipmentCommand Of(EquipmentCommandType type)
        {
            var command = new EquipmentCommand();
            command.Type = type;
            return command;
        }
    }
}
