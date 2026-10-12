using System;
using System.Runtime.InteropServices;
using SEOJIN_TMES.Client.Common.Logging;

namespace SEOJIN_TMES.Client.Common.Storage
{
    /// <summary>
    /// SMB 공유 폴더 한 곳에 대한 연결(윈도우 네트워크 리소스 연결)을 관리한다.
    ///
    /// 윈도우에서 SMB는 별도 라이브러리 없이 <b>UNC 경로에 대한 일반 파일 입출력</b>으로 쓰면 되고,
    /// 필요한 것은 "그 공유에 접근할 자격증명이 현재 세션에 붙어 있는가" 뿐이다.
    /// 그래서 이 클래스는 파일을 직접 읽지 않고, mpr.dll의 WNetAddConnection2로
    /// <b>세션에 자격증명만 붙여 주는 역할</b>만 한다(드라이브 문자를 할당하지 않는 무명 연결).
    ///
    /// 계정이 비어 있으면 아무것도 하지 않는다 — 이미 현재 윈도우 계정으로 접근 권한이 있다는 뜻이다.
    ///
    /// 한 공유에 대해 연결은 프로세스 전체에서 하나면 충분하므로 <see cref="Ensure"/>는 여러 번 불러도 안전하다.
    /// </summary>
    public class SmbConnection : IDisposable
    {
        private const int NoError = 0;
        private const int ErrorAlreadyAssigned = 85;               // 이미 같은 이름으로 연결되어 있음
        private const int ErrorSessionCredentialConflict = 1219;   // 같은 서버에 다른 자격증명으로 이미 연결됨
        private const int ResourceTypeDisk = 1;

        private readonly object _gate = new object();
        private readonly string _shareRoot;
        private readonly string _userName;
        private readonly string _password;

        private bool _connected;
        private bool _disposed;

        /// <summary>
        /// <paramref name="path"/>가 UNC 경로면 그 안의 <c>\\서버\공유</c> 부분만 잘라 연결 대상으로 삼는다
        /// (WNetAddConnection2는 하위 폴더가 아니라 공유 단위로 연결한다).
        /// 로컬 경로면 연결이 필요 없으므로 이 연결은 아무 일도 하지 않는다.
        /// </summary>
        public SmbConnection(string path, string userName, string password)
        {
            _shareRoot = ExtractShareRoot(path);
            _userName = string.IsNullOrWhiteSpace(userName) ? null : userName;
            _password = password;
        }

        /// <summary>자격증명을 붙여야 하는 연결인지(계정이 있고 대상이 UNC 경로일 때만).</summary>
        public bool IsRequired => _userName != null && _shareRoot != null;

        /// <summary>
        /// 공유에 자격증명을 붙인다. 이미 붙어 있으면 아무것도 하지 않는다.
        /// 같은 서버에 다른 계정으로 이미 연결돼 있으면(1219) 기존 연결을 끊고 한 번 더 시도한다 —
        /// 윈도우는 한 서버에 서로 다른 자격증명 두 개를 동시에 유지하지 못하기 때문이다.
        /// </summary>
        public void Ensure()
        {
            if (!IsRequired)
                return;

            lock (_gate)
            {
                if (_connected)
                    return;

                var result = AddConnection();

                if (result == ErrorSessionCredentialConflict)
                {
                    AppLog.Info($"[SMB] {_shareRoot} 기존 연결과 자격증명이 충돌하여 연결을 해제하고 재시도합니다.");
                    WNetCancelConnection2(_shareRoot, 0, true);
                    result = AddConnection();
                }

                if (result != NoError && result != ErrorAlreadyAssigned)
                    throw new DocumentShareException(DescribeConnectError(result, _shareRoot), result);

                _connected = true;
                AppLog.Info($"[SMB] {_shareRoot} 연결 완료 (계정 {_userName})");
            }
        }

        private int AddConnection()
        {
            var resource = new NetResource();
            resource.Scope = 0;
            resource.ResourceType = ResourceTypeDisk;
            resource.DisplayType = 0;
            resource.Usage = 0;
            resource.LocalName = null;      // 드라이브 문자를 쓰지 않는 무명 연결
            resource.RemoteName = _shareRoot;
            resource.Comment = null;
            resource.Provider = null;

            return WNetAddConnection2(ref resource, _password, _userName, 0);
        }

        /// <summary>
        /// <c>\\서버\공유</c>까지만 잘라낸다. UNC가 아니면 null(연결 불필요).
        /// 예: <c>\\fs01\drawings\2026\a.pdf</c> → <c>\\fs01\drawings</c>
        /// </summary>
        internal static string ExtractShareRoot(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            var trimmed = path.Trim();
            if (!trimmed.StartsWith(@"\\", StringComparison.Ordinal))
                return null;

            var parts = trimmed.Substring(2).Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                return null;

            return @"\\" + parts[0] + @"\" + parts[1];
        }

        /// <summary>자주 나오는 오류 코드는 원인을 바로 알 수 있게 우리말로 풀어 준다.</summary>
        private static string DescribeConnectError(int code, string shareRoot)
        {
            switch (code)
            {
                case 5:    return $"공유 폴더 접근이 거부되었습니다({shareRoot}). 계정 권한을 확인하세요.";
                case 53:   return $"공유 폴더를 찾을 수 없습니다({shareRoot}). 서버 이름/주소를 확인하세요.";
                case 67:   return $"공유 이름이 올바르지 않습니다({shareRoot}).";
                case 86:   return $"공유 폴더 비밀번호가 올바르지 않습니다({shareRoot}).";
                case 1326: return $"공유 폴더 계정 또는 비밀번호가 올바르지 않습니다({shareRoot}).";
                case 1219: return $"같은 서버에 다른 계정으로 이미 연결되어 있습니다({shareRoot}).";
                default:   return $"공유 폴더에 연결하지 못했습니다({shareRoot}). 오류 코드 {code}.";
            }
        }

        public void Dispose()
        {
            lock (_gate)
            {
                if (_disposed)
                    return;

                _disposed = true;
                if (!_connected)
                    return;

                WNetCancelConnection2(_shareRoot, 0, true);
                _connected = false;
                AppLog.Info($"[SMB] {_shareRoot} 연결 해제");
            }
        }

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        private static extern int WNetAddConnection2(ref NetResource netResource, string password, string userName, int flags);

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        private static extern int WNetCancelConnection2(string name, int flags, bool force);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NetResource
        {
            public int Scope;
            public int ResourceType;
            public int DisplayType;
            public int Usage;
            public string LocalName;
            public string RemoteName;
            public string Comment;
            public string Provider;
        }
    }

    /// <summary>
    /// 공유 폴더 접근 실패. 화면은 이 예외의 <see cref="Exception.Message"/>를 그대로 보여 주면 되도록
    /// 사람이 읽을 수 있는 문구를 담는다(ApiException과 같은 방식).
    /// </summary>
    public class DocumentShareException : Exception
    {
        /// <summary>윈도우 오류 코드(있으면). 코드가 없는 경우 0.</summary>
        public int ErrorCode { get; }

        public DocumentShareException(string message, int errorCode = 0)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public DocumentShareException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
