using System;
using System.Collections.Generic;
using SEOJIN_TMES.Client.Equipment.Core;
using SEOJIN_TMES.Client.Equipment.Nutrunner;

namespace SEOJIN_TMES.Client.Equipment.Management
{
    /// <summary>
    /// 여러 대의 Nutrunner를 동시에 관리하는 인터페이스.
    /// 장비 구분은 ID(int index)로 하며, ID는 그대로 Modbus TransactionId 로 사용된다.
    /// 상위(View)는 장비 하나하나가 아니라 이 인터페이스 하나만 사용하면 된다.
    /// </summary>
    public interface INutrunnerManager : IDisposable
    {
        int Count { get; }

        /// <summary>등록된 장비 ID 목록.</summary>
        IReadOnlyList<int> Ids { get; }

        /// <summary>ID로 장비를 조회한다. 없으면 null.</summary>
        INutrunner? this[int id] { get; }

        event EventHandler<EquipmentConnectionEventArgs>? ConnectionChanged;

        /// <summary>장비의 <see cref="INutrunner.IsReady"/>가 바뀌었을 때. 화면의 장비 상태 표시는 이 이벤트로 갱신한다.</summary>
        event EventHandler<EquipmentConnectionEventArgs>? ReadyChanged;
        event EventHandler<NutrunnerRegisterEventArgs>? RegisterDataReceived;
        event EventHandler<NutrunnerFasteningEventArgs>? FasteningReceived;
        event EventHandler<AlarmEventArgs>? AlarmOccurred;
        event EventHandler<NutrunnerConnectAttemptEventArgs>? ConnectAttempted;
        event EventHandler<NutrunnerSettingWriteEventArgs>? SettingWritten;

        /// <summary>장비를 등록한다. 같은 ID가 이미 있으면 예외.</summary>
        INutrunner Add(NutrunnerInfo info);

        /// <summary>장비를 정지하고 등록 해제한다.</summary>
        bool Remove(int id);

        bool TryGet(int id, out INutrunner nutrunner);
        bool TryGet(string code, out INutrunner nutrunner);

        /// <summary>
        /// 장비코드(<see cref="NutrunnerInfo.Code"/>)로 장비를 찾아 토크 설정값을 쓴다.
        /// 장비가 없거나 네트워크(Request 채널)에 연결돼 있지 않으면 false.
        /// </summary>
        bool SetTorque(string code, decimal torque);

        /// <summary>
        /// 장비코드로 장비를 찾아 DriverLock을 잠금(3) 또는 해제(0)한다.
        /// 장비가 없거나 네트워크(Request 채널)에 연결돼 있지 않으면 false.
        /// </summary>
        bool SetDriverLock(string code, bool locked);

        int FindIdByIp(string ip);
        INutrunner? FindByIp(string ip);
        /// <summary>해당 장비의 통신 시퀀스를 시작한다.</summary>
        void Start(int id);

        void StartAll();

        /// <summary>해당 장비의 통신 시퀀스를 정지하고 연결을 끊는다.</summary>
        void Stop(int id);

        void StopAll();
    }
}
