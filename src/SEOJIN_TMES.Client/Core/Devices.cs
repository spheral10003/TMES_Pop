using Newtonsoft.Json;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Equipment.Management;
using SEOJIN_TMES.Client.Equipment.Nutrunner;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace SEOJIN_TMES.Client.Core
{
    public class Devices
    {
        /// <summary>App.config appSettings 에서 설비 목록(JSON 배열)을 담는 키.</summary>
        private const string EquipmentsKey = "Equipments";

        private INutrunnerManager _manager;

        public INutrunnerManager Manager { get => _manager;}

        public Devices()
        {
            Init();
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

            foreach (var info in LoadEquipments())
            {
                TryAdd(info);
            }

            AppLog.Step($"설비 {_manager.Count}대 등록 완료");
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
        /// 장비 한 대를 등록한다. 한 대의 설정이 잘못됐다고 나머지 장비까지 못 쓰게 되지 않도록
        /// 문제가 있는 항목만 건너뛴다.
        /// </summary>
        private void TryAdd(NutrunnerInfo info)
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
