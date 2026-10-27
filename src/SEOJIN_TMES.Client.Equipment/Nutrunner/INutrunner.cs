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

        bool IsEventEnabled { get; }

        /// <summary>Event Transport 채널만 다시 연결한다.</summary>
        void ConnectEventChannel();

        /// <summary>Request 채널의 Read/Write 응답 수신.</summary>
        event EventHandler<NutrunnerRegisterEventArgs>? RegisterDataReceived;

        /// <summary>Event Transport 채널의 체결 결과 수신.</summary>
        event EventHandler<NutrunnerFasteningEventArgs>? FasteningReceived;

        /// <summary>접속 시도 결과(성공/실패, IP·포트 포함). Request · Event 채널 모두.</summary>
        event EventHandler<NutrunnerConnectAttemptEventArgs>? ConnectAttempted;

        /// <summary>설정 레지스터(토크 · 드라이버 잠금) 쓰기의 송신과 응답.</summary>
        event EventHandler<NutrunnerSettingWriteEventArgs>? SettingWritten;

        void ReadRegisters(ushort address, ushort count,
            ModbusFunctionCode function = ModbusFunctionCode.ReadInputRegisters);

        void WriteRegister(ushort address, ushort value,
            ModbusFunctionCode function = ModbusFunctionCode.WriteSingleRegister);

        /// <summary>장비 전용 레지스터에 값을 쓴다.</summary>
        void WriteRegister(NutrunnerRegister register, ushort value);

        /// <summary>
        /// 토크 설정값을 <see cref="NutrunnerRegister.Torque"/>에 쓴다(실수 → 레지스터 정수 변환 포함).
        /// Request 채널이 끊겨 있거나 레지스터 범위를 넘으면 보내지 않고 false.
        /// </summary>
        /// <remarks>송신만 한다 — 장비가 받아들였는지는 RegisterDataReceived(StartAddress = Torque)로 확인한다.</remarks>
        bool SetTorque(decimal torque);

        /// <summary>
        /// <see cref="NutrunnerRegister.DriverLock"/>에 잠금/해제 값을 쓴다.
        /// Request 채널이 끊겨 있으면 보내지 않고 false.
        /// </summary>
        /// <param name="locked">true = 잠금(3), false = 해제(0).</param>
        bool SetDriverLock(bool locked);

        void ProcessEventHandshake();
    }
}
