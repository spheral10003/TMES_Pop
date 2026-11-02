using Newtonsoft.Json;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Equipment.Management;
using SEOJIN_TMES.Client.Equipment.Nutrunner;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.Core
{
    public class Devices
    {
        /// <summary>App.config appSettings 에서 설비 목록(JSON 배열)을 담는 키.</summary>
        private const string EquipmentsKey = "Equipments";

        private INutrunnerManager _manager;

        /// <summary>지금 장비 연결을 맡긴 화면(<see cref="Attach"/>). 이 화면만 <see cref="Detach"/>로 해제할 수 있다.</summary>
        private object _owner;

        public INutrunnerManager Manager { get => _manager;}

        /// <summary>마지막으로 남긴 네트워크 상태. 주소 변경 알림은 한 번 바뀔 때 여러 번 오므로 달라졌을 때만 남긴다.</summary>
        private string? _lastNetwork;
        private readonly object _networkLogSync = new object();

        public Devices()
        {
            Init();

            // Wi-Fi 재연결 · SSID 변경으로 IP가 바뀌는 순간을 로그에 남긴다 — 장비 응답이 끊기고 돌아오는 시점과 맞춰 보기 위해서다.
            // Devices는 앱이 끝날 때까지 하나만 살아 있어 해제하지 않는다(Init은 다시 불릴 수 있어 생성자에서 건다).
            NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;
        }

        private void OnNetworkAddressChanged(object sender, EventArgs e)
        {
            LogNetwork("네트워크 변경");
        }

        /// <summary>
        /// PC의 IP · SSID를 로그에 남긴다. netsh를 띄우므로 화면 스레드를 막지 않게 백그라운드에서 돈다.
        /// </summary>
        private void LogNetwork(string reason)
        {
            Task.Run(() =>
            {
                var snapshot = NetworkSnapshot.Describe();
                lock (_networkLogSync)
                {
                    if (reason == "네트워크 변경" && snapshot == _lastNetwork)
                        return;
                    _lastNetwork = snapshot;
                }

                AppLog.Info($"[네트워크] {reason} - {snapshot}");
            });
        }

        /// <summary>
        /// App.config 의 설비 목록으로 매니저를 구성한다.
        ///
        /// 이 메서드는 Global 싱글톤 생성 중에 불리므로 여기서 예외가 나가면 Lazy가 예외를 저장해 두고,
        /// 이후 모든 Global.Instance 접근(종료 정리 포함)이 같은 예외로 실패해 원인 로그도 남지 않는다.
        /// 그래서 설정이 잘못돼도 예외를 던지지 않고, 로그를 남긴 뒤 쓸 수 있는 장비만 등록한다.
        /// </summary>
        public void Init()
        {
            _manager = new NutrunnerManager();

            // 접속 결과와 설정 변경 이력은 화면과 상관없이 항상 서버로 보낸다 — 매니저를 만드는 이 자리 한 곳에서 건다.
            _manager.ConnectAttempted += OnConnectAttempted;
            _manager.SettingWritten += OnSettingWritten;

            //foreach (var info in LoadEquipments())
            //{
            //    TryAdd(info);
            //}

            //AppLog.Step($"설비 {_manager.Count}대 등록 완료");
        }

        /// <summary>장비 접속 성공/실패 — IP·포트와 함께 서버로 보낸다(통신 스레드에서 불린다).</summary>
        private void OnConnectAttempted(object sender, NutrunnerConnectAttemptEventArgs e)
        {
            ServerLog.EquipmentConnected(e.Code, e.Channel, e.Host, e.Port, e.Success, e.Reason, e.LocalEndPoint);
        }

        /// <summary>설정 레지스터 쓰기의 송신 · 응답 이력 — 서버로 보낸다(통신 스레드에서 불린다).</summary>
        private void OnSettingWritten(object sender, NutrunnerSettingWriteEventArgs e)
        {
            var request = $"WriteSingleRegister({e.Register}={(ushort)e.Register})";
            var values = string.Join(",", e.Values);
            ServerLog.ModbusWritten(e.Code, e.Host, e.Port, request, e.IsResponse, values, e.Success);
        }

        private static List<NutrunnerInfo> LoadEquipments()
        {
            //var equipmentsJson = ConfigurationManager.AppSettings[EquipmentsKey];

            return new List<NutrunnerInfo>();

            //// 키가 없으면 null 이 오고, JsonConvert.DeserializeObject(null)은 ArgumentNullException을 던진다.
            //if (string.IsNullOrWhiteSpace(equipmentsJson))
            //{
            //    AppLog.Warn($"App.config에 '{EquipmentsKey}' 설정이 없거나 비어 있음 - 설비 없이 시작한다.");
            //    return new List<NutrunnerInfo>();
            //}

            //try
            //{
            //    return JsonConvert.DeserializeObject<List<NutrunnerInfo>>(equipmentsJson)
            //           ?? new List<NutrunnerInfo>();
            //}
            //catch (JsonException ex)
            //{
            //    // JSON 문법 오류(JsonReaderException), 형식 불일치(JsonSerializationException) 모두 여기로 온다.
            //    AppLog.Error($"App.config '{EquipmentsKey}' 설정의 JSON 형식이 잘못됨 - 설비 없이 시작한다.", ex);
            //    return new List<NutrunnerInfo>();
            //}
        }

        /// <summary>
        /// 화면(POP)이 설비 목록을 넘겨 연결을 맡긴다. 이 화면이 장비의 새 주인이 된다.
        ///
        /// POP 화면은 동시에 둘이 될 수 있다(새 작업 진입 중에 이전 POP가 아직 닫히지 않은 경우).
        /// 예전에는 새 POP의 등록이 "같은 ID 이미 등록"으로 건너뛰어지고, 뒤이어 닫힌 이전 POP가 매니저를
        /// 통째로 정지 · 폐기해 새 POP에 장비가 하나도 남지 않았다. 그래서 주인을 두고, 주인만 해제할 수 있게 한다.
        ///
        /// 이미 같은 접속 정보(코드 · IP · 포트)로 등록된 장비는 그대로 둔다 — 장비가 접속을 1개만 받아
        /// 끊었다 다시 붙이면 그 사이 체결 이벤트를 놓치고, 재접속이 막힐 수도 있다.
        /// </summary>
        public void Attach(object owner, IReadOnlyList<NutrunnerInfo> infos)
        {
            _owner = owner;

            var wanted = new Dictionary<int, NutrunnerInfo>();
            foreach (var info in infos)
            {
                if (info != null)
                    wanted[info.Id] = info;
            }

            foreach (var id in _manager.Ids)
            {
                var current = _manager[id]?.Info;
                if (current != null && wanted.TryGetValue(id, out var info) && IsSameEndpoint(current, info))
                    continue;

                _manager.Stop(id);
                _manager.Remove(id);
            }

            foreach (var info in wanted.Values)
            {
                if (_manager[info.Id] == null)
                    TryAdd(info);
            }

            _manager.StartAll();
            AppLog.Step($"설비 {_manager.Count}대 연결 시작");
            LogNetwork("설비 연결 시작");
        }

        /// <summary>
        /// 화면이 닫힐 때 장비를 정지 · 해제한다. 그 사이 다른 화면이 <see cref="Attach"/>로 넘겨받았으면 아무것도 하지 않는다.
        /// 매니저 자체는 폐기하지 않는다 — 앱 종료 때 <see cref="Global"/>이 정리한다.
        /// </summary>
        public void Detach(object owner)
        {
            if (!ReferenceEquals(_owner, owner))
                return;

            _owner = null;
            _manager.StopAll();
            foreach (var id in _manager.Ids)
                _manager.Remove(id);
        }

        private static bool IsSameEndpoint(NutrunnerInfo a, NutrunnerInfo b) =>
            a.Code == b.Code && a.Ip == b.Ip && a.Port == b.Port && a.ResolvedEventPort == b.ResolvedEventPort;

        /// <summary>
        /// 장비 한 대를 등록한다. 한 대의 설정이 잘못됐다고 나머지 장비까지 못 쓰게 되지 않도록
        /// 문제가 있는 항목만 건너뛴다.
        /// </summary>
        public void TryAdd(NutrunnerInfo info)
        {
            // JSON 배열 안의 null 항목("[null, {...}]")은 Add()에서 ArgumentNullException이 된다.
            if (info == null)
            {
                AppLog.Warn($"'{EquipmentsKey}' 설정에 빈 항목이 있어 건너뛴다.");
                return;
            }

            if (string.IsNullOrWhiteSpace(info.Ip))
            {
                AppLog.Warn($"설비 {info} - IP가 비어 있어 등록하지 않는다.");
                return;
            }

            try
            {
                _manager.Add(info);
            }
            catch (ArgumentException ex)
            {
                // 같은 ID가 두 번 적힌 경우. 먼저 적힌 장비를 살린다.
                AppLog.Warn($"설비 {info} - 같은 ID({info.Id})가 이미 등록돼 있어 건너뛴다. ({ex.GetType().Name})");
            }
        }

        public void Save()
        {
            // Manager 인덱서는 순번이 아니라 장비 ID로 조회한다(없으면 null).
            // 0..Count-1 로 돌면 ID가 1부터인 설정에서 null.Info 로 NullReferenceException이 나므로 Ids로 돈다.
            List<NutrunnerInfo> info = new List<NutrunnerInfo>();
            foreach (var id in _manager.Ids)
            {
                var nutrunner = _manager[id];
                if (nutrunner != null)
                {
                    info.Add(nutrunner.Info);
                }
            }

            var infoJson = JsonConvert.SerializeObject(info, Formatting.None);

            Configuration config =
            ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // 키가 없는 설정 파일이면 Settings[key]가 null이라 .Value 대입에서 NullReferenceException이 난다.
            var setting = config.AppSettings.Settings[EquipmentsKey];
            if (setting == null)
            {
                config.AppSettings.Settings.Add(EquipmentsKey, infoJson);
            }
            else
            {
                setting.Value = infoJson;
            }

            config.Save(ConfigurationSaveMode.Modified);

            // 캐시된 값 갱신 (이거 없으면 읽을 때 옛날 값 나옴)
            ConfigurationManager.RefreshSection("appSettings");

        }
    }
}
