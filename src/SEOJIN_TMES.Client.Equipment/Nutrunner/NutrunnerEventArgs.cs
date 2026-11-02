using System;
using System.Collections.Generic;
using SEOJIN_TMES.Client.Equipment.Core;

namespace SEOJIN_TMES.Client.Equipment.Nutrunner
{
    /// <summary>Request 채널(Read/Write)의 응답 수신 이벤트.</summary>
    public sealed class NutrunnerRegisterEventArgs : EquipmentEventArgs
    {
        public NutrunnerRegisterEventArgs(string code, bool success, ushort startAddress, IReadOnlyList<ushort> data)
            : base(code)
        {
            Success = success;
            StartAddress = startAddress;
            Data = data ?? Array.Empty<ushort>();
        }

        public bool Success { get; }

        public ushort StartAddress { get; }

        public IReadOnlyList<ushort> Data { get; }
    }

    /// <summary>Event Transport 채널의 체결 결과 수신 이벤트.</summary>
    public sealed class NutrunnerFasteningEventArgs : EquipmentEventArgs
    {
        public NutrunnerFasteningEventArgs(string code, FasteningData data) : base(code) => Data = data;

        public FasteningData Data { get; }
    }

    /// <summary>
    /// 접속 시도 1회의 결과(성공/실패). 접속 대상(IP·포트)을 함께 싣는다 —
    /// <see cref="EquipmentConnectionEventArgs"/>는 장비 코드와 연결 여부만 있어 어디로 붙으려 했는지 알 수 없다.
    /// </summary>
    public sealed class NutrunnerConnectAttemptEventArgs : EquipmentEventArgs
    {
        public NutrunnerConnectAttemptEventArgs(string code, string channel, string host, int port, bool success, string? reason,
            string? localEndPoint = null)
            : base(code)
        {
            Channel = channel;
            Host = host;
            Port = port;
            Success = success;
            Reason = reason;
            LocalEndPoint = localEndPoint;
        }

        /// <summary><c>Request</c>(레지스터 Read/Write) / <c>Event</c>(체결 결과 Push).</summary>
        public string Channel { get; }

        public string Host { get; }

        public int Port { get; }

        public bool Success { get; }

        /// <summary>실패 사유. 성공이면 null.</summary>
        public string? Reason { get; }

        /// <summary>이 PC 쪽 소켓 주소(<c>IP:포트</c>). 접속에 성공했을 때만 있다.</summary>
        public string? LocalEndPoint { get; }
    }

    /// <summary>
    /// 설정 레지스터(토크 · 드라이버 잠금) 쓰기의 송신 또는 응답 수신 1건.
    /// 핸드셰이크(EventDataEnable) · 수신 확인(EventAck) · Alive(Status 읽기)는 설정 변경이 아니라서 올리지 않는다.
    /// </summary>
    public sealed class NutrunnerSettingWriteEventArgs : EquipmentEventArgs
    {
        public NutrunnerSettingWriteEventArgs(string code, string host, int port, NutrunnerRegister register,
            bool isResponse, bool success, IReadOnlyList<ushort> values)
            : base(code)
        {
            Host = host;
            Port = port;
            Register = register;
            IsResponse = isResponse;
            Success = success;
            Values = values ?? Array.Empty<ushort>();
        }

        public string Host { get; }

        public int Port { get; }

        public NutrunnerRegister Register { get; }

        /// <summary>false = 장비로 보낸 값, true = 장비가 돌려준 응답.</summary>
        public bool IsResponse { get; }

        /// <summary>송신은 항상 true, 응답은 장비가 예외 응답을 주지 않았는지.</summary>
        public bool Success { get; }

        /// <summary>송신이면 쓴 값, 응답이면 장비가 돌려준 값.</summary>
        public IReadOnlyList<ushort> Values { get; }
    }
}
