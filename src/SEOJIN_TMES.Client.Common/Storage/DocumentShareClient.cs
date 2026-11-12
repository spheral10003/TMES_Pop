using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SEOJIN_TMES.Client.Common.Configuration;
using SEOJIN_TMES.Client.Common.Errors;
using SEOJIN_TMES.Client.Common.Logging;

namespace SEOJIN_TMES.Client.Common.Storage
{
    /// <summary>
    /// 작업 표준/도면 문서(PDF)가 들어 있는 SMB 공유 폴더에서 파일을 읽어 오는 클라이언트.
    ///
    /// 서버가 내려준 파일 경로를 공유 폴더 루트와 합쳐 실제 UNC 경로를 만들고,
    /// 필요하면 <see cref="SmbConnection"/>으로 자격증명을 붙인 뒤 파일을 통째로 읽어 바이트 배열로 돌려준다.
    ///
    /// 파일을 로컬에 남기지 않고 메모리로만 올리는 이유:
    ///  - 도면은 보통 수 MB라 메모리로 충분하고,
    ///  - 뷰어가 네트워크 파일을 잡고 있으면 공유 쪽에서 파일 교체가 막히며,
    ///  - 작업장 PC에 도면 사본이 흩어지지 않는다.
    ///
    /// 프로세스 전체에서 하나만 만들어 공유한다(Program.cs에서 조립, 종료 시 Dispose).
    /// </summary>
    public class DocumentShareClient : IDisposable
    {
        /// <summary>도면 한 건의 크기 상한(64MB). 잘못된 경로로 거대한 파일을 통째로 올리는 것을 막는다.</summary>
        private const long MaxFileBytes = 64L * 1024 * 1024;

        private readonly DocumentShareSettings _settings;
        private readonly SmbConnection _connection;

        public DocumentShareClient(DocumentShareSettings settings)
        {
            _settings = settings ?? new DocumentShareSettings();
            _connection = new SmbConnection(_settings.RootPath, _settings.ResolveUserName(), _settings.Password);
        }

        /// <summary>공유 폴더 루트가 App.config에 설정되어 있는지.</summary>
        public bool IsConfigured => _settings.IsConfigured;

        /// <summary>
        /// 서버가 준 경로를 실제 파일 경로로 바꾼다.
        ///  - 이미 UNC(<c>\\...</c>)거나 드라이브 경로면 그대로 쓴다(서버가 전체 경로를 내려준 경우).
        ///  - 그 외에는 공유 폴더 루트와 합친다. 서버가 <c>/</c> 구분자를 쓰더라도 <c>\</c>로 바꿔 준다.
        /// </summary>
        public string Resolve(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new DocumentShareException(ErrorCodes.DrawingPathEmpty);

            var normalized = path.Trim().Replace('/', '\\');

            if (normalized.StartsWith(@"\\", StringComparison.Ordinal) || Path.IsPathRooted(normalized))
                return normalized;

            if (!_settings.IsConfigured)
                throw new DocumentShareException(ErrorCodes.DrawingShareNotConfigured);
            return normalized.TrimStart('\\');
            //return Path.Combine(_settings.RootPath.TrimEnd('\\'), normalized.TrimStart('\\'));
        }

        /// <summary>
        /// 공유 폴더의 파일을 통째로 읽는다.
        ///
        /// UNC 경로에 대한 파일 열기는 서버가 응답하지 않을 때 수 초~수십 초 멈출 수 있어
        /// 전 과정을 백그라운드 스레드에서 수행한다(호출한 화면 스레드가 멈추지 않도록).
        /// 실패는 모두 <see cref="DocumentShareException"/>으로 바꿔 올리므로,
        /// 화면은 예외 메시지를 그대로 안내에 쓰면 된다.
        /// </summary>
        public Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
        {
            var fullPath = Resolve(path);

            return Task.Run(() =>
            {
                try
                {
                    _connection.Ensure();

                    var info = new FileInfo(fullPath);
                    if (!info.Exists)
                        throw new DocumentShareException(ErrorCodes.DrawingNotFound, fullPath);

                    if (info.Length > MaxFileBytes)
                        throw new DocumentShareException(ErrorCodes.DrawingTooLarge, fullPath, 0,
                            info.Length / (1024 * 1024), MaxFileBytes / (1024 * 1024));

                    AppLog.Info($"[SMB] 도면 읽기 시작 - {fullPath} ({info.Length:N0} bytes)");

                    // FileShare.ReadWrite: 공유 쪽에서 같은 파일을 열어 두고 있어도 읽을 수 있게 한다.
                    using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var buffer = new MemoryStream())
                    {
                        stream.CopyTo(buffer);
                        cancellationToken.ThrowIfCancellationRequested();
                        return buffer.ToArray();
                    }
                }
                catch (DocumentShareException)
                {
                    throw;
                }
                catch (UnauthorizedAccessException ex)
                {
                    throw new DocumentShareException(ErrorCodes.DrawingAccessDenied, fullPath, ex);
                }
                catch (IOException ex)
                {
                    throw new DocumentShareException(ErrorCodes.FileShareFailed, $"{fullPath}\n{ex.Message}", ex);
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    // 경로에 쓸 수 없는 문자(ArgumentException · NotSupportedException), 보안 정책(SecurityException) 등
                    // 위에서 못 잡은 실패도 감싸서, 화면이 오류 코드 · 경로가 붙은 안내를 일관되게 띄우게 한다.
                    // 취소는 감싸지 않는다 — 호출한 쪽이 취소로 구분해 조용히 넘어가야 한다.
                    throw new DocumentShareException(ErrorCodes.FileShareFailed, $"{fullPath}\n{ex.Message}", ex);
                }
            }, cancellationToken);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
