using System;
using System.Configuration;
using SEOJIN_MEIS.Client.Common.Configuration;
using Newtonsoft.Json;

namespace SEOJIN_MEIS.Client.Composition
{
    /// <summary>App.config(appSettings)에서 AppSettings POCO를 로드한다.</summary>
    public static class AppSettingsLoader
    {
        public static AppSettings Load()
        {
            var settings = new AppSettings
            {
                ApiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"],
                DefaultCulture = ConfigurationManager.AppSettings["DefaultCulture"] ?? "ko-KR"
            };

            if (int.TryParse(ConfigurationManager.AppSettings["ApiTimeoutSeconds"], out var timeout))
                settings.ApiTimeoutSeconds = timeout;

            if (int.TryParse(ConfigurationManager.AppSettings["SessionTimeoutMinutes"], out var sessionTimeout))
                settings.SessionTimeoutMinutes = sessionTimeout;

            if (bool.TryParse(ConfigurationManager.AppSettings["LogApiResponseBody"], out var logResponseBody))
                settings.LogApiResponseBody = logResponseBody;

            if (int.TryParse(ConfigurationManager.AppSettings["LogApiResponseBodyMaxLength"], out var bodyMaxLength))
                settings.LogApiResponseBodyMaxLength = bodyMaxLength;

            if (bool.TryParse(ConfigurationManager.AppSettings["UseMockData"], out var useMockData))
                settings.UseMockData = useMockData;

            if (int.TryParse(ConfigurationManager.AppSettings["NotificationPollingSeconds"], out var pollingSeconds))
                settings.NotificationPollingSeconds = pollingSeconds;

            var equipmentsJson = ConfigurationManager.AppSettings["Equipments"];
            if (!string.IsNullOrWhiteSpace(equipmentsJson))
            {
                settings.Equipments = JsonConvert.DeserializeObject<System.Collections.Generic.List<EquipmentEndpoint>>(equipmentsJson)
                                       ?? new System.Collections.Generic.List<EquipmentEndpoint>();
            }

            var documentShareJson = ConfigurationManager.AppSettings["DocumentShare"];
            if (!string.IsNullOrWhiteSpace(documentShareJson))
            {
                settings.DocumentShare = JsonConvert.DeserializeObject<DocumentShareSettings>(documentShareJson)
                                         ?? new DocumentShareSettings();
            }

            return settings;
        }
    }
}
