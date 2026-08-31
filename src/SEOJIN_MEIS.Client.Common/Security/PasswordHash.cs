using System;
using System.Security.Cryptography;
using System.Text;

namespace SEOJIN_MEIS.Client.Common.Security
{
    /// <summary>
    /// 로그인 비밀번호 해시. 평문 비밀번호는 절대 서버로 보내지 않는다.
    ///
    /// 서버(B/E)는 nonce 방식이라 로그인 전에 my-salt로 salt를 먼저 받아오고,
    /// 로그인 바디에는 <b>SHA256(평문 비밀번호 + salt)</b> 와 받은 salt를 함께 올린다
    /// (서버는 같은 salt로 같은 계산을 해서 대조한다 — 그래서 매번 해시 값이 달라진다).
    ///
    /// 이어붙이는 순서/횟수가 서버 구현과 다르면 인증이 통과되지 않는다.
    /// 서버 스펙이 다르면 <see cref="ForLogin"/> 한 줄만 고치면 된다
    /// (예: salt를 앞에 붙이는 방식 → Sha256Hex(salt + plainPassword)).
    /// </summary>
    public static class PasswordHash
    {
        /// <summary>
        /// 로그인 바디의 password 값(소문자 16진수 64자) = SHA256(평문 비밀번호 + salt).
        /// salt가 비어 있으면(서버가 안 줬으면) 비밀번호만 해시한다.
        /// </summary>
        public static string ForLogin(string plainPassword, string salt) =>
            Sha256Hex((plainPassword ?? string.Empty) + (salt ?? string.Empty));

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
