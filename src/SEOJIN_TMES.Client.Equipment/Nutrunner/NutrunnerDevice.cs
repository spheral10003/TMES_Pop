using SEOJIN_TMES.Client.Equipment.Core;
using SEOJIN_TMES.Client.Modbus;
using SEOJIN_TMES.Client.Modbus.Abstractions;
using SEOJIN_TMES.Client.Modbus.Protocol;
using SEOJIN_TMES.Client.Modbus.Transport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;

namespace SEOJIN_TMES.Client.Equipment.Nutrunner
{
    /// <summary>
    /// <see cref="INutrunner"/> 기본 구현체.
    ///
    /// 통신은 전적으로 SEOJIN_TMES.Client.Modbus 에 위임한다.
    ///  - Request 채널     : <see cref="IModbusClient"/> (TransactionId = 장비 ID, ProtocolId = 0)
    ///  - Event Transport  : <see cref="IModbusTransport"/> 원시 채널 + Nutrunner 전용 파서 (ProtocolId = 1)
    ///
    /// 장비 고유 로직(이벤트 포맷, 핸드셰이크, Ack)은 이 클래스 안에만 존재한다.
    /// </summary>
    public sealed class NutrunnerDevice : INutrunner
    {
        private readonly IModbusClient _client;
        private readonly IModbusTransport _eventTransport;
        private readonly IModbusTransport? _ownedRequestTransport;

        /// <summary>요청 송신 직렬화용. (시퀀스 Thread / 소켓 콜백 / UI Thread 가 같이 쓴다)</summary>
        private readonly object _requestSync = new();

        private const ushort EventEnableValue = 2;

        /// <summary>
        /// 토크 레지스터는 소수점 없는 16bit 정수라 실수 토크에 이 배율을 곱해 보낸다(1.25 → 125).
        /// TODO: 가정값 — 한타스/Mountz 레지스터 맵에서 모델(세대)별 토크 소수 자릿수를 확인할 것.
        /// </summary>
        private const decimal TorqueScale = 100m;

        /// <summary>DriverLock 레지스터 값 — 0 = 해제(체결 가능).</summary>
        private const ushort DriverUnlockValue = 0;

        /// <summary>
        /// DriverLock 레지스터 값 — 3 = 잠금(체결 불가).
        /// TODO: 3의 의미(비트별 잠금 대상)는 장비 레지스터 맵으로 확인할 것.
        /// </summary>
        private const ushort DriverLockValue = 3;

        /// <summary>
        /// PresetChange 레지스터 값 — 1 = 방금 쓴 토크 설정을 장비 프리셋에 적용.
        /// 토크 레지스터만 쓰면 장비가 새 목표 토크로 바꾸지 않아서 함께 보낸다.
        /// </summary>
        private const ushort PresetChangeValue = 1;

        /// <summary>ConnectTimeoutMs가 0 이하(접속은 OS 기본값)일 때 쓰는 요청 하나의 응답 대기 시간.</summary>
        private const int DefaultResponseTimeoutMs = 1000;

        /// <summary>이벤트 채널 연결 후 첫 송신까지 기다리는 시간. 장비가 이벤트 세션을 준비할 여유.</summary>
        private const double EventEnableDelayMs = 300;

        /// <summary>응답이 이 시간 안에 오지 않으면 다시 보낸다.</summary>
        private const double EventEnableTimeoutMs = 1000;

        /// <summary>이 횟수만큼 실패하면 이벤트 채널을 끊고 재연결부터 다시 한다.</summary>
        private const int EventEnableMaxRetry = 5;

        private readonly object _handshakeSync = new();
        private readonly Stopwatch _eventEnableTimer = new();
        private volatile bool _eventEnabled;
        private int _eventEnableRetry;

        /// <summary>
        /// 각 채널이 접속을 시도하는 중인지. 접속 실패는 별도 이벤트 없이 ErrorOccurred로만 올라오므로,
        /// 시도 중에 난 오류를 "접속 실패"로 한 번만 보고하기 위해 둔다.
        /// </summary>
        private volatile bool _requestConnecting;
        private volatile bool _eventConnecting;

        private bool _disposed;

        /// <summary>장비가 EventDataEnable 쓰기를 받았음을 응답으로 확인했는지 여부.</summary>
        public bool IsEventEnabled => _eventEnabled;

        public event EventHandler<EquipmentConnectionEventArgs>? Connected;
        public event EventHandler<EquipmentConnectionEventArgs>? Disconnected;
        public event EventHandler<Exception>? ErrorOccurred;
        public event EventHandler<NutrunnerRegisterEventArgs>? RegisterDataReceived;
        public event EventHandler<NutrunnerFasteningEventArgs>? FasteningReceived;
        public event EventHandler<NutrunnerConnectAttemptEventArgs>? ConnectAttempted;
        public event EventHandler<NutrunnerSettingWriteEventArgs>? SettingWritten;

        public NutrunnerInfo Info { get; }

        public int Id => Info.Id;

        public string Name => Info.DisplayName;

        public bool IsConnected => _client.IsConnected;

        public bool IsEventChannelConnected => _eventTransport.IsConnected;

        public FasteningData LastFastening { get; private set; }

        /// <summary>
        /// 기본 생성자. 장비 ID로 소켓/TransactionId 를 구분해서 여러 대를 동시에 운용할 수 있다.
        /// </summary>
        public NutrunnerDevice(NutrunnerInfo info)
            : this(info,
                   new TcpModbusTransport(info.Id * 2,
                       info.ConnectTimeoutMs, info.KeepAliveTimeMs, info.KeepAliveIntervalMs),
                   new TcpModbusTransport((info.Id * 2) + 1,
                       info.ConnectTimeoutMs, info.KeepAliveTimeMs, info.KeepAliveIntervalMs))
        {
        }

        private NutrunnerDevice(NutrunnerInfo info, IModbusTransport requestTransport, IModbusTransport eventTransport)
            : this(info,
                   new ModbusTcpClient(requestTransport, new ModbusTcpCodec(), info.TransactionId)
                   {
                       // Alive 응답 대기(NutrunnerSequence)와 같은 기준 — 요청은 하나씩 응답을 받은 뒤 다음 것을 보낸다.
                       ResponseTimeoutMs = info.ConnectTimeoutMs > 0 ? info.ConnectTimeoutMs : DefaultResponseTimeoutMs
                   },
                   eventTransport)
        {
            // 직접 만든 Request 전송은 이 클래스가 Dispose 책임을 진다.
            _ownedRequestTransport = requestTransport;
        }

        /// <summary>
        /// 통신 구현을 직접 주입하는 생성자 (테스트 더블 / 다른 전송 방식용).
        /// </summary>
        public NutrunnerDevice(NutrunnerInfo info, IModbusClient client, IModbusTransport eventTransport)
        {
            Info = info ?? throw new ArgumentNullException(nameof(info));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _eventTransport = eventTransport ?? throw new ArgumentNullException(nameof(eventTransport));

            _client.Connected += OnClientConnected;
            _client.Disconnected += OnClientDisconnected;
            _client.ErrorOccurred += OnClientError;
            _client.ResponseReceived += OnResponseReceived;

            _eventTransport.Connected += OnEventTransportConnected;
            _eventTransport.DataReceived += OnEventDataReceived;
            _eventTransport.ErrorOccurred += OnEventTransportError;
            _eventTransport.Disconnected += OnEventTransportDisconnected;
        }



        public void Connect()
        {
            _requestConnecting = true;
            _client.Connect(Info.Ip, Info.Port);
        }

        public void ConnectEventChannel()
        {
            if (!_client.IsConnected || _eventTransport.IsConnected)
            {
                return;
            }

            ConnectEventTransport();
        }

        private void ConnectEventTransport()
        {
            _eventConnecting = true;
            _eventTransport.Connect(Info.Ip, Info.ResolvedEventPort);
        }

        public void Disconnect()
        {
            _eventTransport.Disconnect();
            _client.Disconnect();
        }

        public void ReadRegisters(ushort address, ushort count,
            ModbusFunctionCode function = ModbusFunctionCode.ReadInputRegisters)
        {
            lock (_requestSync)
            {
                _client.ReadRegisters(address, count, function);
            }
        }

        public void WriteRegister(ushort address, ushort value,
            ModbusFunctionCode function = ModbusFunctionCode.WriteSingleRegister)
        {
            lock (_requestSync)
            {
                _client.WriteRegister(address, value, function);
            }
        }

        public void WriteRegister(NutrunnerRegister register, ushort value)
        {
            WriteRegister((ushort)register, value);
        }

        public bool SetTorque(decimal torque)
        {
            // 쓰기는 Request 채널로만 나간다 — 이벤트 채널 상태와 상관없이 Request 연결만 본다.
            if (!_client.IsConnected)
            {
                return false;
            }

            decimal raw = decimal.Round(torque * TorqueScale, 0, MidpointRounding.AwayFromZero);
            if (raw<ushort.MinValue || raw> ushort.MaxValue)
            {
                ErrorOccurred?.Invoke(this, new ArgumentOutOfRangeException(nameof(torque), torque,
                    $"[{Info.Code}] 토크 값이 레지스터 범위(0 ~ {ushort.MaxValue / TorqueScale})를 벗어났습니다."));
                return false;
            }

            WriteRegister(NutrunnerRegister.Torque, (ushort) raw);
            RaiseSettingWritten(NutrunnerRegister.Torque, false, true, new[] { (ushort)raw });

            // 목표 토크를 쓴 다음에 프리셋 변경을 알린다 — 순서가 바뀌면 이전 토크로 프리셋이 적용된다.
            // 요청은 ModbusTcpClient가 하나씩 응답을 받은 뒤 보내므로 토크 쓰기가 끝난 뒤에 나간다.
            WriteRegister(NutrunnerRegister.PresetChange, PresetChangeValue);
            RaiseSettingWritten(NutrunnerRegister.PresetChange, false, true, new[] { PresetChangeValue });
            return true;
        }

        public bool SetDriverLock(bool locked)
        {
            if (!_client.IsConnected)
            {
                return false;
            }

            ushort value = locked ? DriverLockValue : DriverUnlockValue;
            WriteRegister(NutrunnerRegister.DriverLock, value);
            RaiseSettingWritten(NutrunnerRegister.DriverLock, false, true, new[] { value });
            return true;
        }

        private void ResetEventHandshake()
        {
            lock (_handshakeSync)
            {
                _eventEnabled = false;
                _eventEnableRetry = 0;
                _eventEnableTimer.Restart();
            }
        }

        /// <summary>
        /// 시퀀스 Thread 에서 매 tick 호출한다.
        /// 응답으로 확인될 때까지 EventDataEnable 을 다시 보낸다.
        /// </summary>
        public void ProcessEventHandshake()
        {
            if (_eventEnabled || !_client.IsConnected || !_eventTransport.IsConnected)
            {
                return;
            }

            lock (_handshakeSync)
            {
                double wait = _eventEnableRetry == 0 ? EventEnableDelayMs : EventEnableTimeoutMs;
                if (_eventEnableTimer.Elapsed.TotalMilliseconds < wait)
                {
                    return;
                }

                if (_eventEnableRetry >= EventEnableMaxRetry)
                {
                    ErrorOccurred?.Invoke(this, new TimeoutException(
                        $"EventDataEnable 응답 없음 ({EventEnableMaxRetry}회). 이벤트 채널을 다시 연결합니다."));

                    // 끊어 두면 KeepAlive 의 ConnectEventChannel 이 다시 붙이고, 핸드셰이크도 처음부터 다시 한다.
                    _eventTransport.Disconnect();
                    return;
                }

                _eventEnableRetry++;
                _eventEnableTimer.Restart();
            }

            WriteRegister(NutrunnerRegister.EventDataEnable, EventEnableValue);
        }

        private void OnClientConnected(object? sender, EventArgs e)
        {
            _requestConnecting = false;
            RaiseConnectAttempted("Request", Info.Port, true, null);

            // Request 채널이 붙으면 Event Transport 채널도 연결한다.
            ConnectEventTransport();
            Connected?.Invoke(this, new EquipmentConnectionEventArgs(Info.Code, true));
        }

        private void OnClientDisconnected(object? sender, EventArgs e)
        {
            _eventTransport.Disconnect();
            Disconnected?.Invoke(this, new EquipmentConnectionEventArgs(Info.Code, false));
        }

        private void OnEventTransportConnected(object? sender, EventArgs e)
        {
            _eventConnecting = false;
            RaiseConnectAttempted("Event", Info.ResolvedEventPort, true, null);

            // 이벤트 채널 핸드셰이크: EventDataEnable 레지스터 ON.
            ResetEventHandshake();
            //WriteRegister(NutrunnerRegister.EventDataEnable, 2);
            //Console.WriteLine("\n\n\n\n");
            //Console.WriteLine($"EventDataEnable {NutrunnerRegister.EventDataEnable} : {2}");
        }

        private void OnEventTransportDisconnected(object sender, EventArgs e)
        {
            ResetEventHandshake();
        }

        private void OnResponseReceived(object? sender, ModbusResponse response)
        {
            // TransactionId 로 장비를 구분한다. (0 은 장비가 echo 하지 않는 경우 대비)
            //if (response.TransactionId != 0 && response.TransactionId != Info.TransactionId)
            //{
            //    return;
            //}

            if (!response.IsError
                && response.FunctionCode == ModbusFunctionCode.WriteSingleRegister
                && response.StartAddress == (ushort)NutrunnerRegister.EventDataEnable)
            {
                _eventEnabled = true;
            }

            // 설정 레지스터 쓰기의 응답만 이력으로 올린다(핸드셰이크 · Ack · Alive 응답은 제외).
            if (response.FunctionCode == ModbusFunctionCode.WriteSingleRegister
                && (response.StartAddress == (ushort)NutrunnerRegister.Torque
                    || response.StartAddress == (ushort)NutrunnerRegister.PresetChange
                    || response.StartAddress == (ushort)NutrunnerRegister.DriverLock))
            {
                RaiseSettingWritten((NutrunnerRegister)response.StartAddress, true, !response.IsError, response.Data);
            }

            RegisterDataReceived?.Invoke(this,
                new NutrunnerRegisterEventArgs(Info.Code, !response.IsError, response.StartAddress, response.Data));
        }

        private void OnEventDataReceived(object? sender, byte[] data)
        {
            if (!NutrunnerEventParser.TryParse(data, out FasteningData fastening))
            {
                return;
            }

            LastFastening = fastening;
            
            //FasteningReceived?.Invoke(this, new NutrunnerFasteningEventArgs(Id, fastening));
            FasteningReceived?.Invoke(this, new NutrunnerFasteningEventArgs(Info.Code, fastening));

            // 수신 확인(Ack).
            WriteRegister(NutrunnerRegister.EventAck, 1);
        }

        private void OnClientError(object? sender, Exception exception)
        {
            if (_requestConnecting && !_client.IsConnected)
            {
                _requestConnecting = false;
                RaiseConnectAttempted("Request", Info.Port, false, exception.Message);
            }

            ErrorOccurred?.Invoke(this, exception);
        }

        private void OnEventTransportError(object? sender, Exception exception)
        {
            if (_eventConnecting && !_eventTransport.IsConnected)
            {
                _eventConnecting = false;
                RaiseConnectAttempted("Event", Info.ResolvedEventPort, false, exception.Message);
            }

            ErrorOccurred?.Invoke(this, exception);
        }

        private void RaiseConnectAttempted(string channel, int port, bool success, string? reason)
            => ConnectAttempted?.Invoke(this, new NutrunnerConnectAttemptEventArgs(Info.Code, channel, Info.Ip, port, success, reason));

        private void RaiseSettingWritten(NutrunnerRegister register, bool isResponse, bool success, IReadOnlyList<ushort> values)
            => SettingWritten?.Invoke(this, new NutrunnerSettingWriteEventArgs(Info.Code, Info.Ip, Info.Port, register, isResponse, success, values));



        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _client.Connected -= OnClientConnected;
            _client.Disconnected -= OnClientDisconnected;
            _client.ErrorOccurred -= OnClientError;
            _client.ResponseReceived -= OnResponseReceived;

            _eventTransport.Connected -= OnEventTransportConnected;
            _eventTransport.DataReceived -= OnEventDataReceived;
            _eventTransport.ErrorOccurred -= OnEventTransportError;
            _eventTransport.Disconnected -= OnEventTransportDisconnected;

            _client.Dispose();
            _eventTransport.Dispose();
            _ownedRequestTransport?.Dispose();
        }
    }
}
