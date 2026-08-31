using System;
using System.IO;
using SEOJIN_MEIS.Client.Common.Logging;
using Newtonsoft.Json;

namespace SEOJIN_MEIS.Client.Common.Configuration
{
    /// <summary>
    /// 사용자가 화면에서 고른 값 중 다음 실행에도 남겨야 하는 것(현재는 로그인 아이디 저장 여부).
    /// PC/계정 단위 설정이므로 App.config가 아니라 %APPDATA%\SEOJIN_MEIS.Client\user-preferences.json에 둔다.
    ///
    /// 비밀번호는 저장하지 않는다 — 아이디만 저장한다.
    /// 파일을 못 읽거나 못 쓰더라도 로그인 자체는 영향을 받지 않아야 하므로 실패는 로그만 남기고 넘어간다.
    /// </summary>
    public class UserPreferences
    {
        /// <summary>로그인 화면의 "아이디 저장" 체크 상태.</summary>
        public bool RememberUserId { get; set; }

        /// <summary>저장해 둔 로그인 아이디(<see cref="RememberUserId"/>가 true일 때만 값이 있다).</summary>
        public string UserId { get; set; }

        private static string FilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SEOJIN_MEIS.Client",
            "user-preferences.json");

        /// <summary>저장된 설정을 읽는다. 파일이 없거나 깨졌으면 기본값(저장 안 함)을 돌려준다.</summary>
        public static UserPreferences Load()
        {
            try
            {
                var path = FilePath;
                if (!File.Exists(path))
                    return new UserPreferences();

                return JsonConvert.DeserializeObject<UserPreferences>(File.ReadAllText(path))
                       ?? new UserPreferences();
            }
            catch (Exception ex)
            {
                AppLog.Warn($"사용자 설정 로드 실패(기본값 사용): {ex.Message}");
                return new UserPreferences();
            }
        }

        /// <summary>현재 값을 저장한다. 실패해도 예외를 올리지 않는다(로그인 흐름을 막지 않기 위함).</summary>
        public void Save()
        {
            try
            {
                var path = FilePath;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            catch (Exception ex)
            {
                AppLog.Warn($"사용자 설정 저장 실패: {ex.Message}");
            }
        }
    }
}
