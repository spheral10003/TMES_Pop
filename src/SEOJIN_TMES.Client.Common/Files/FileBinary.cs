using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.Common.Files
{
    /// <summary>
    /// 서버로 보낼 파일 한 개를 메모리(byte[])에 읽어 둔 것 — 파일 이름 · Content-Type · 내용.
    ///
    /// 파일을 열어 둔 채로(스트림) 보내지 않고 미리 다 읽어 두는 이유:
    ///  - 401 재시도(AuthTokenHandler)가 본문을 다시 읽어 한 번 더 보내는데, 스트림은 두 번째에 비어 있다.
    ///  - 전송 중에 현장 PC에서 파일이 지워지거나 잠겨도 이미 읽은 내용으로 끝까지 보낼 수 있다.
    /// 대신 큰 파일은 메모리를 그만큼 쓰므로 <see cref="Load"/>에서 크기 상한을 둔다.
    /// </summary>
    public class FileBinary
    {
        /// <summary>
        /// 확장자 → Content-Type. 서버가 이미지/문서를 구분해 저장할 때 이 값을 본다.
        /// 목록에 없는 확장자는 application/octet-stream(그냥 바이너리)으로 보낸다.
        /// </summary>
        private static readonly Dictionary<string, string> ContentTypes =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { ".png",  "image/png" },
                { ".jpg",  "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".bmp",  "image/bmp" },
                { ".gif",  "image/gif" },
                { ".pdf",  "application/pdf" },
                { ".txt",  "text/plain" },
                { ".csv",  "text/csv" },
                { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" }
            };

        public string FileName { get; }
        public string ContentType { get; }
        public byte[] Content { get; }

        public long Length => Content.LongLength;

        /// <summary>사진인지(화면에서 미리보기를 띄울지 판단할 때 쓴다).</summary>
        public bool IsImage => ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

        public FileBinary(string fileName, string contentType, byte[] content)
        {
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType;
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        /// <summary>
        /// 파일을 바이너리로 읽는다. 상한을 넘거나 빈 파일이면 읽기 전에 막는다
        /// (서버에서 거절당할 파일을 메모리에 올리고 네트워크로 보내는 낭비를 없앤다).
        /// 실패는 예외로 올리고, 사용자 안내는 화면이 정한다.
        /// </summary>
        /// <param name="path">읽을 파일 경로.</param>
        /// <param name="maxBytes">허용 최대 크기(byte). 0 이하면 제한하지 않는다.</param>
        /// <exception cref="FileNotFoundException">파일이 없을 때.</exception>
        /// <exception cref="InvalidDataException">빈 파일이거나 상한을 넘을 때.</exception>
        public static FileBinary Load(string path, long maxBytes)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("파일 경로가 비어 있습니다.", nameof(path));

            var info = new FileInfo(path);
            if (!info.Exists)
                throw new FileNotFoundException("파일을 찾을 수 없습니다.", path);

            if (info.Length == 0)
                throw new InvalidDataException($"빈 파일입니다: {info.Name}");

            if (maxBytes > 0 && info.Length > maxBytes)
                throw new InvalidDataException($"파일 크기 초과: {info.Name} ({info.Length:N0} / {maxBytes:N0} byte)");

            // ReadAllBytes는 FileShare.Read로 연다 — 다른 프로그램이 쓰기 중으로 잠갔으면 IOException이 난다.
            var bytes = File.ReadAllBytes(path);
            return new FileBinary(info.Name, ResolveContentType(info.Extension), bytes);
        }

        private static string ResolveContentType(string extension) =>
            !string.IsNullOrEmpty(extension) && ContentTypes.TryGetValue(extension, out var type)
                ? type
                : "application/octet-stream";
    }
}
