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
        event EventHandler<NutrunnerRegisterEventArgs>? RegisterDataReceived;
        event EventHandler<NutrunnerFasteningEventArgs>? FasteningReceived;
        event EventHandler<AlarmEventArgs>? AlarmOccurred;

        /// <summary>장비를 등록한다. 같은 ID가 이미 있으면 예외.</summary>
        INutrunner Add(NutrunnerInfo info);

        /// <summary>장비를 정지하고 등록 해제한다.</summary>
        bool Remove(int id);

        bool TryGet(int id, out INutrunner nutrunner);
        bool TryGet(string ip, out INutrunner nutrunner);
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
