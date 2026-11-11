using Newtonsoft.Json.Linq;
using SEOJIN_TMES.Client.Common.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.ApiClient
{
    /// <summary>
    /// 시스템 설정 API — <c>GET api/v1.0/sm/configurations</c>. 응답은 <c>{"data": ...}</c> 봉투로 온다.
    /// 로그인 직후 "초기 비밀번호로 로그인했는지" 안내하려고 설정의 초기 비밀번호 값만 꺼내 쓴다.
    ///
    /// <b>2026-11-11 기준 응답 필드명을 확인하지 못했다</b>(Swagger가 인증을 요구하고 웹 소스도 없다).
    /// 그래서 설정이 객체 하나로 오든(<c>{initPwd: ...}</c>) 코드/값 목록으로 오든(<c>[{cfgCd, cfgVal}]</c>)
    /// 후보 이름으로 찾아 읽는다. 실제 이름이 확인되면 후보 목록을 그 이름 하나로 줄일 것.
    /// </summary>
    public interface IConfigurationApiClient
    {
        /// <summary>설정의 초기 비밀번호 값. 찾지 못하면 null.</summary>
        Task<string> GetInitialPassword(CancellationToken cancellationToken = default);
    }

    public class ConfigurationApiClient : ApiClientBase, IConfigurationApiClient
    {
        /// <summary>설정이 객체 하나로 올 때 초기 비밀번호가 들어 있을 만한 속성 이름(앞쪽이 우선).</summary>
        private static readonly string[] PasswordKeys =
        {
            "initPwd", "initPw", "initPassword", "initialPassword", "initPasswd",
            "defaultPwd", "defaultPw", "defaultPassword", "pwd", "password"
        };

        /// <summary>설정이 코드/값 목록으로 올 때 코드 칸 · 값 칸 이름 후보.</summary>
        private static readonly string[] CodeKeys = { "cfgCd", "configCd", "cfgKey", "configKey", "key", "code", "name", "cfgNm" };
        private static readonly string[] ValueKeys = { "cfgVal", "configVal", "cfgValue", "configValue", "value", "val" };

        public ConfigurationApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<string> GetInitialPassword(CancellationToken cancellationToken = default)
        {
            var data = await GetDataAsync<JToken>(ApiPrefix + "sm/configurations", cancellationToken).ConfigureAwait(false);

            var password = FindPassword(data);
            if (password == null)
            {
                // 값은 비밀번호라 남기지 않고 모양만 남긴다 — 현장 로그로 실제 필드명을 확인하기 위함.
                AppLog.Warn($"설정(sm/configurations)에서 초기 비밀번호 값을 찾지 못함 - 응답 형태={DescribeShape(data)}");
            }

            return password;
        }

        private static string FindPassword(JToken data)
        {
            if (data is JObject obj)
                return FromObject(obj);

            if (data is JArray array)
            {
                var items = array.OfType<JObject>().ToList();

                // 코드/값 목록 — 코드에 PW · PASS가 들어간 줄의 값을 쓴다.
                foreach (var item in items)
                {
                    var code = FirstValue(item, CodeKeys);
                    if (code != null && LooksLikePassword(code))
                    {
                        var value = FirstValue(item, ValueKeys);
                        if (value != null)
                            return value;
                    }
                }

                // 객체 여러 개(사업장별 설정 등) — 첫 번째로 값이 있는 것을 쓴다.
                return items.Select(FromObject).FirstOrDefault(value => value != null);
            }

            return null;
        }

        private static string FromObject(JObject obj) => FirstValue(obj, PasswordKeys);

        private static string FirstValue(JObject obj, string[] keys)
        {
            foreach (var key in keys)
            {
                if (obj.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out var token)
                    && token.Type != JTokenType.Null && token.Type != JTokenType.Object && token.Type != JTokenType.Array)
                {
                    var text = token.ToString();
                    if (!string.IsNullOrEmpty(text))
                        return text;
                }
            }

            return null;
        }

        private static bool LooksLikePassword(string code) =>
            code.IndexOf("PW", StringComparison.OrdinalIgnoreCase) >= 0
            || code.IndexOf("PASS", StringComparison.OrdinalIgnoreCase) >= 0;

        /// <summary>로그용 응답 모양 — 값 없이 형식과 속성 이름만.</summary>
        private static string DescribeShape(JToken data)
        {
            if (data == null)
                return "null";

            var first = data is JArray array ? array.FirstOrDefault() as JObject : data as JObject;
            var names = first == null ? string.Empty : string.Join(",", first.Properties().Select(p => p.Name));
            return $"{data.Type}[{names}]";
        }
    }
}
