using System;
using SEOJIN_TMES.Client.Equipment.Core;
using SEOJIN_TMES.Client.Modbus.Protocol;

namespace SEOJIN_TMES.Client.Equipment.Nutrunner
{
    /// <summary>
    /// Nutrunner(체결기) 한 대를 표현하는 인터페이스.
    ///
    /// 두 개의 채널을 캡슐화한다.
    ///  - Request 채널      : 레지스터 Read/Write (ProtocolId = 0)
    ///  - Event Transport   : 체결 결과 Push 수신 (ProtocolId = 1)
    ///
    /// 두 채널 모두 <see cref="NutrunnerInfo.TransactionId"/>(= 장비 ID)로 장비를 구분한다.
    /// </summary>
    public interface INutrunner : IEquipment
    {
        /// <summary>장비 정보(ID / 시리얼 / 모델 등).</summary>
        NutrunnerInfo Info { get; }

        /// <summary>마지막으로 수신한 체결 결과 데이터.</summary>
        FasteningData LastFastening { get; }

        /// <summary>Event Transport 채널의 연결 여부. (Request 채널과 별개로 끊길 수 있다)</summary>
        bool IsEventChannelConnected { get; }

        /// <summary>Event Transport 채널만 다시 연결한다.</summary>
        void ConnectEventChannel();

        /// <summary>Request 채널의 Read/Write 응답 수신.</summary>
        event EventHandler<NutrunnerRegisterEventArgs>? RegisterDataReceived;

        /// <summary>Event Transport 채널의 체결 결과 수신.</summary>
        event EventHandler<NutrunnerFasteningEventArgs>? FasteningReceived;

        void ReadRegisters(ushort address, ushort count,
            ModbusFunctionCode function = ModbusFunctionCode.ReadInputRegisters);

        void WriteRegister(ushort address, ushort value,
            ModbusFunctionCode function = ModbusFunctionCode.WriteSingleRegister);

        /// <summary>장비 전용 레지스터에 값을 쓴다.</summary>
        void WriteRegister(NutrunnerRegister register, ushort value);
    }
}
