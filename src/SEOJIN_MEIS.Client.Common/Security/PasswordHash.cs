using System;
using System.Security.Cryptography;
using System.Text;

namespace SEOJIN_MEIS.Client.Common.Security
{
    /// <summary>
    /// 로그인 비밀번호 해시. 평문 비밀번호는 절대 서버로 보내지 않는다.
    ///
    /// 서버(B/E)는 nonce 방식이라 로그인 전에 my-salt로 salt를 먼저 받아오고,
    /// 로그인 바디에는 <b>SHA256(SHA256(평문 비밀번호) + salt)</b> 와 받은 salt를 함께 올린다
    /// (서버는 같은 salt로 같은 계산을 해서 대조한다 — 그래서 매번 해시 값이 달라진다).
    ///
    /// <b>두 번 해시하는 이유</b>: 안쪽 SHA256(비밀번호)은 salt와 무관한 고정값이라 서버가 그것만 저장해 두면 되고,
    /// 평문 비밀번호는 클라이언트 밖으로 한 번도 나가지 않는다. 바깥 해시가 salt를 섞어 매번 값을 바꾸므로
    /// 오간 값을 가로채 그대로 재사용하는 것(리플레이)도 막힌다.
    ///
    /// 이어붙이는 순서/횟수가 서버 구현과 다르면 인증이 통과되지 않는다.
    /// 서버 스펙이 다르면 <see cref="ForLogin"/> 한 줄만 고치면 된다
    /// (예: salt를 앞에 붙이는 방식 → Sha256Hex(salt + Sha256Hex(plainPassword))).
    /// </summary>
    public static class PasswordHash
    {
        /// <summary>
        /// 로그인 바디의 password 값(소문자 16진수 64자) = SHA256(SHA256(평문 비밀번호) + salt).
        ///
        /// 안쪽 해시 결과는 <b>소문자 16진수 문자열 64자</b>이고, 거기에 salt를 <b>문자열로 이어붙여</b> 다시 해시한다
        /// (바이트 배열을 잇는 것이 아니다 — 서버와 이 표현이 어긋나면 값이 달라져 인증이 실패한다).
        ///
        /// salt가 비어 있으면(서버가 안 줬으면) 이어붙일 것이 없어 SHA256(SHA256(비밀번호))가 된다.
        /// </summary>
        public static string ForLogin(string plainPassword, string salt) =>
            Sha256Hex(Sha256Hex(plainPassword ?? string.Empty) + (salt ?? string.Empty));

        /// <summary>UTF-8 문자열의 SHA256을 소문자 16진수 문자열로 돌려준다.</summary>
        public static string Sha256Hex(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));

                var builder = new StringBuilder(hash.Length * 2);
                foreach (var b in hash)
                    builder.Append(b.ToString("x2"));

                return builder.ToString();
            }
        }
    }
}
