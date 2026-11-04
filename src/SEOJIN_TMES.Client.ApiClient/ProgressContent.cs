using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.ApiClient
{
    /// <summary>요청 본문을 보낸 정도. 화면은 이 값을 그대로 프로그레스 바에 옮긴다.</summary>
    public sealed class TransferProgress
    {
        public long BytesSent { get; }
        public long TotalBytes { get; }

        /// <summary>0~100. 전체 크기를 모르면 0.</summary>
        public int Percent => TotalBytes <= 0 ? 0 : (int)Math.Min(100, BytesSent * 100 / TotalBytes);

        public TransferProgress(long bytesSent, long totalBytes)
        {
            BytesSent = bytesSent;
            TotalBytes = totalBytes;
        }
    }

    /// <summary>
    /// 다른 HttpContent를 감싸 <b>네트워크로 써 나간 바이트 수</b>를 <see cref="IProgress{T}"/>로 알린다.
    ///
    /// 본문을 일정 크기(<see cref="ChunkSize"/>)로 잘라 쓰는 이유:
    /// MultipartFormDataContent는 ByteArrayContent 파트를 <b>한 번의 Write</b>로 통째로 넘기므로,
    /// 그대로 세면 0% → 100%로 한 번에 건너뛰어 진행 표시가 의미가 없다.
    ///
    /// 보고는 퍼센트가 바뀔 때만 한다 — Progress&lt;T&gt;는 보고마다 UI 스레드로 넘기므로
    /// 조각마다 보고하면 200MB 파일에서 수천 번 화면을 다시 그린다.
    /// </summary>
    public sealed class ProgressContent : HttpContent
    {
        private const int ChunkSize = 64 * 1024;

        private readonly HttpContent _inner;
        private readonly IProgress<TransferProgress> _progress;

        public ProgressContent(HttpContent inner, IProgress<TransferProgress> progress)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _progress = progress;

            // 바깥 요청에는 이 래퍼의 헤더가 실린다 — multipart boundary가 든 Content-Type을 그대로 옮겨야 서버가 본문을 나눌 수 있다.
            foreach (var header in inner.Headers)
                Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            long total = _inner.Headers.ContentLength ?? -1;
            using (var counting = new ProgressStream(stream, total, _progress))
            {
                await _inner.CopyToAsync(counting).ConfigureAwait(false);
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            // 길이를 알려야 HttpClientHandler가 Content-Length로 보낸다(모르면 chunked로 바뀐다).
            var contentLength = _inner.Headers.ContentLength;
            length = contentLength ?? -1;
            return contentLength.HasValue;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _inner.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>쓰기만 받아 넘기면서 바이트 수를 센다. 바깥 스트림의 수명은 HttpClient가 관리하므로 닫지 않는다.</summary>
        private sealed class ProgressStream : Stream
        {
            private readonly Stream _target;
            private readonly long _total;
            private readonly IProgress<TransferProgress> _progress;
            private long _sent;
            private int _lastPercent = -1;

            public ProgressStream(Stream target, long total, IProgress<TransferProgress> progress)
            {
                _target = target;
                _total = total;
                _progress = progress;
                Report();
            }

            public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                while (count > 0)
                {
                    int size = Math.Min(ChunkSize, count);
                    await _target.WriteAsync(buffer, offset, size, cancellationToken).ConfigureAwait(false);
                    offset += size;
                    count -= size;
                    _sent += size;
                    Report();
                }
            }

            // .NET Framework의 ByteArrayContent · MultipartContent는 BeginWrite로 쓰고, 그 기본 구현이 이 Write를 부른다.
            public override void Write(byte[] buffer, int offset, int count)
            {
                while (count > 0)
                {
                    int size = Math.Min(ChunkSize, count);
                    _target.Write(buffer, offset, size);
                    offset += size;
                    count -= size;
                    _sent += size;
                    Report();
                }
            }

            private void Report()
            {
                if (_progress == null)
                    return;

                var current = new TransferProgress(_sent, _total);
                if (current.Percent == _lastPercent)
                    return;

                _lastPercent = current.Percent;
                _progress.Report(current);
            }

            public override Task FlushAsync(CancellationToken cancellationToken) => _target.FlushAsync(cancellationToken);
            public override void Flush() => _target.Flush();

            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => _sent;
            public override long Position { get => _sent; set => throw new NotSupportedException(); }
            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
        }
    }
}
