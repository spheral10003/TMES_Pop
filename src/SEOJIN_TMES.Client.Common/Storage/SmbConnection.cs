using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SEOJIN_TMES.Client.Common.Errors;
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
                    throw new DocumentShareException(ConnectErrorCode(result), _shareRoot, result, result);

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

        /// <summary>
        /// 자주 나오는 Windows 오류는 원인을 바로 알 수 있게 오류 코드를 따로 둔다(문구는 Languages의 Error_FILE_xxx).
        /// 그 밖의 번호는 FILE_013 하나로 묶고 문구에 Windows 오류 번호를 넣는다.
        /// </summary>
        private static string ConnectErrorCode(int win32Error)
        {
            switch (win32Error)
            {
                case 5:    return ErrorCodes.ShareAccessDenied;
                case 53:   return ErrorCodes.ShareNotFound;
                case 67:   return ErrorCodes.ShareBadName;
                case 86:
                case 1326: return ErrorCodes.ShareBadCredential;
                case 1219: return ErrorCodes.ShareCredentialConflict;
                default:   return ErrorCodes.ShareConnectFailed;
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
    /// 공유 폴더 접근 실패. 화면은 이 예외의 <see cref="Message"/>를 그대로 보여 주면 된다(ApiException과 같은 방식) —
    /// 오류 코드(FILE_xxx)로 문구를 정하고, Message를 <b>읽을 때</b> 그 시점의 화면 언어로 만든다
    /// (파일 읽기는 백그라운드 스레드에서 실패하므로 만들 때 문구를 굳히면 언어가 어긋날 수 있다).
    /// 끝에는 "오류 코드" 줄이 붙는다. 서버 로그용 한국어 문구는 <see cref="MessageFor"/>로 꺼낸다.
    /// </summary>
    public class DocumentShareException : Exception, ICodedException
    {
        /// <summary>윈도우 오류 코드(있으면). 코드가 없는 경우 0.</summary>
        public int ErrorCode { get; }

        /// <summary>오류 코드(FILE_xxx).</summary>
        public string Code { get; }

        /// <summary>문구 아래에 덧붙이는 상세(파일 · 공유 경로, 입출력 오류 원인). 경로라 번역하지 않는다.</summary>
        private readonly string _detail;

        /// <summary>코드 문구에 넣을 인자(FILE_006 크기, FILE_013 Windows 오류 번호).</summary>
        private readonly object[] _codeArgs;

        public DocumentShareException(string code, string detail = null, int win32Error = 0, params object[] codeArgs)
            : this(code, detail, null, win32Error, codeArgs)
        {
        }

        public DocumentShareException(string code, string detail, Exception innerException, int win32Error = 0, params object[] codeArgs)
            : base(ErrorText.Korean(code, codeArgs), innerException)
        {
            Code = code;
            ErrorCode = win32Error;
            _detail = detail;
            _codeArgs = codeArgs;
        }

        /// <summary>현재 화면 언어의 안내 문구 + 상세 + 오류 코드 줄.</summary>
        public override string Message => ErrorText.WithCode(MessageFor(CultureInfo.CurrentUICulture), Code);

        /// <summary>지정한 언어의 안내 문구 + 상세(코드 줄 제외).</summary>
        public string MessageFor(CultureInfo culture)
        {
            var message = ErrorText.Format(Code, culture, _codeArgs);
            return string.IsNullOrWhiteSpace(_detail) ? message : message + Environment.NewLine + _detail;
        }
    }
}
