using System;

namespace SEOJIN_MEIS.Client.ApiClient
{
    /// <summary>
    /// 로그인 후 발급된 토큰을 프로세스 메모리에만 보관한다.
    /// 클라이언트는 토큰 유효성 판단(만료 시각 등)을 갖지 않고, 서버가 401을 반환하면 재로그인/리프레시만 수행한다.
    /// </summary>
    public interface ITokenProvider
    {
        string AccessToken { get; }
        string RefreshToken { get; }
        bool IsAuthenticated { get; }

        void SetTokens(string accessToken, string refreshToken);
        void Clear();

        event EventHandler SessionExpired;
        void RaiseSessionExpired();
    }

    public class TokenProvider : ITokenProvider
    {
        private readonly object _lock = new object();

        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }
        public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

        public event EventHandler SessionExpired;

        public void SetTokens(string accessToken, string refreshToken)
        {
            lock (_lock)
            {
                AccessToken = accessToken;
                RefreshToken = refreshToken;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                AccessToken = null;
                RefreshToken = null;
            }
        }

        public void RaiseSessionExpired() => SessionExpired?.Invoke(this, EventArgs.Empty);
    }
}
