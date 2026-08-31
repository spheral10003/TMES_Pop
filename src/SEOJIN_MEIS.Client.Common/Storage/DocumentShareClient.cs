using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SEOJIN_MEIS.Client.Common.Configuration;
using SEOJIN_MEIS.Client.Common.Logging;

namespace SEOJIN_MEIS.Client.Common.Storage
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
                throw new DocumentShareException("표시할 도면 파일 경로가 비어 있습니다.");

            var normalized = path.Trim().Replace('/', '\\');

            if (normalized.StartsWith(@"\\", StringComparison.Ordinal) || Path.IsPathRooted(normalized))
                return normalized;

            if (!_settings.IsConfigured)
                throw new DocumentShareException("도면 공유 폴더(App.config의 DocumentShare.RootPath)가 설정되지 않았습니다.");

            return Path.Combine(_settings.RootPath.TrimEnd('\\'), normalized.TrimStart('\\'));
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
                        throw new DocumentShareException($"도면 파일을 찾을 수 없습니다.\n{fullPath}");

                    if (info.Length > MaxFileBytes)
                        throw new DocumentShareException($"도면 파일이 너무 큽니다({info.Length / (1024 * 1024)}MB). 최대 {MaxFileBytes / (1024 * 1024)}MB까지 표시합니다.");

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
                    throw new DocumentShareException($"도면 파일에 접근할 권한이 없습니다.\n{fullPath}", ex);
                }
                catch (IOException ex)
                {
                    throw new DocumentShareException($"도면 파일을 읽지 못했습니다.\n{fullPath}\n{ex.Message}", ex);
                }
            }, cancellationToken);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
