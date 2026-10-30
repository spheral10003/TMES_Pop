using System;
using System.Configuration;
using SEOJIN_TMES.Client.Common.Configuration;
using Newtonsoft.Json;

namespace SEOJIN_TMES.Client.Composition
{
    /// <summary>App.config(appSettings)에서 AppSettings POCO를 로드한다.</summary>
    public static class AppSettingsLoader
    {
        public static AppSettings Load()
        {
            var settings = new AppSettings
            {
                ApiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"],
                DefaultCulture = ConfigurationManager.AppSettings["DefaultCulture"] ?? "ko-KR",
                ServerName = ConfigurationManager.AppSettings["ServerName"] ?? "GUM"
            };

            if (int.TryParse(ConfigurationManager.AppSettings["UploadMaxMegaBytes"], out var uploadMaxMb))
                settings.UploadMaxMegaBytes = uploadMaxMb;

            if (int.TryParse(ConfigurationManager.AppSettings["ApiTimeoutSeconds"], out var timeout))
                settings.ApiTimeoutSeconds = timeout;

            if (int.TryParse(ConfigurationManager.AppSettings["SessionTimeoutMinutes"], out var sessionTimeout))
                settings.SessionTimeoutMinutes = sessionTimeout;

            if (bool.TryParse(ConfigurationManager.AppSettings["LogApiResponseBody"], out var logResponseBody))
                settings.LogApiResponseBody = logResponseBody;

            if (int.TryParse(ConfigurationManager.AppSettings["LogApiResponseBodyMaxLength"], out var bodyMaxLength))
                settings.LogApiResponseBodyMaxLength = bodyMaxLength;

            if (int.TryParse(ConfigurationManager.AppSettings["NotificationPollingSeconds"], out var pollingSeconds))
                settings.NotificationPollingSeconds = pollingSeconds;

            if (bool.TryParse(ConfigurationManager.AppSettings["IsTestServer"], out var isTestServer))
                settings.IsTestServer = isTestServer;

            if (int.TryParse(ConfigurationManager.AppSettings["ModbusConnectTimeoutMs"], out var connectTimeoutMs))
                settings.ModbusConnectTimeoutMs = connectTimeoutMs;

            if (int.TryParse(ConfigurationManager.AppSettings["ModbusKeepAliveTimeMs"], out var keepAliveTimeMs))
                settings.ModbusKeepAliveTimeMs = keepAliveTimeMs;

            if (int.TryParse(ConfigurationManager.AppSettings["ModbusKeepAliveIntervalMs"], out var keepAliveIntervalMs))
                settings.ModbusKeepAliveIntervalMs = keepAliveIntervalMs;

            if (int.TryParse(ConfigurationManager.AppSettings["ModbusTestTimeoutMs"], out var testTimeoutMs))
                settings.ModbusTestTimeoutMs = testTimeoutMs;

            if (int.TryParse(ConfigurationManager.AppSettings["ModbusTestIntervalMs"], out var testIntervalMs))
                settings.ModbusTestIntervalMs = testIntervalMs;

            //var equipmentsJson = ConfigurationManager.AppSettings["Equipments"];
            //if (!string.IsNullOrWhiteSpace(equipmentsJson))
            //{
            //    settings.Equipments = JsonConvert.DeserializeObject<System.Collections.Generic.List<EquipmentEndpoint>>(equipmentsJson)
            //                           ?? new System.Collections.Generic.List<EquipmentEndpoint>();
            //}

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
