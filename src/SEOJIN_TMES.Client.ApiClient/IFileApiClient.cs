using SEOJIN_TMES.Client.Common.Files;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.ApiClient
{
    /// <summary>
    /// 파일/사진 업로드 API.
    /// TODO: 경로(files/upload)와 폼 필드명(file, category, refKey)은 <b>가정값</b>이다.
    ///       서버 스펙이 확정되면 아래 상수와 필드명을 맞출 것.
    /// </summary>
    public interface IFileApiClient
    {
        /// <summary>
        /// 파일 한 개를 multipart/form-data로 올린다.
        /// </summary>
        /// <param name="file">바이너리로 읽어 둔 파일(<see cref="FileBinary.Load"/>).</param>
        /// <param name="category">파일 분류(예: 불량사진, 작업표준). 서버가 저장 위치를 나눌 때 쓴다.</param>
        /// <param name="refKey">연결할 업무 키(예: 시리얼 번호, 작업지시 번호). 없으면 null.</param>
        /// <param name="progress">보낸 바이트 수를 받을 곳(화면의 프로그레스 바용). null이면 알리지 않는다.</param>
        Task<FileUploadResponse> UploadAsync(FileBinary file, PopProcessKey key, IProgress<TransferProgress> progress = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// 여러 파일을 multipart/form-data <b>한 번의 요청</b>으로 올린다. 주소는 UploadAsync와 같다.
        /// 제한 시간은 일반 API(ApiTimeoutSeconds)가 아니라 본문 크기로 정한다(<see cref="ApiClientOptions.UploadTimeoutFor"/>).
        /// </summary>
        Task<List<FileUploadResponse>> UploadManyAsync(IReadOnlyList<FileBinary> files, PopProcessKey key, IProgress<TransferProgress> progress = null, CancellationToken cancellationToken = default);

        /// <summary>작업지시에 붙은 첨부파일 목록.</summary>
        Task<WorkOrderAttachment> GetAttachmentsAsync(PopProcessKey key, CancellationToken cancellationToken = default);

        /// <summary>첨부파일 한 건을 받는다. 파일 이름과 원본 바이트(content)가 함께 온다.</summary>
        Task<AttachmentFile> DownloadAttachmentAsync(PopProcessKey key, string atchRef, CancellationToken cancellationToken = default);
        /// <summary>첨부파일을 삭제한다. 주소는 UploadAsync와 같고, 바디는 atchRef 문자열 배열(<c>["A","B"]</c>)이다.</summary>
        Task DeleteAttachmentsAsync(PopProcessKey key, IEnumerable<string> atchRefs, CancellationToken cancellationToken = default);
        Task DeleteAttachmentsAsync(PopProcessKey key, string atchRefs, CancellationToken cancellationToken = default);
    }

    public class FileApiClient : ApiClientBase, IFileApiClient
    {
        private const string FilesField = "attachmentFiles";

        /// <summary>업로드 전용 — Timeout이 무제한이고, 제한 시간은 호출마다 본문 크기로 건다.</summary>
        private readonly HttpClient _uploadClient;
        private readonly ApiClientOptions _options;

        public FileApiClient(HttpClient httpClient, HttpClient uploadClient, ApiClientOptions options) : base(httpClient)
        {
            _uploadClient = uploadClient ?? throw new ArgumentNullException(nameof(uploadClient));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<FileUploadResponse> UploadAsync(FileBinary file, PopProcessKey key, IProgress<TransferProgress> progress = null, CancellationToken cancellationToken = default)
        {
            using (var form = new MultipartFormDataContent())
            {
                // 파일 본문 — StreamContent가 아니라 ByteArrayContent여야 401 재시도 때 내용이 다시 실린다.
                
                string query = BuildQuery(("corpCd", key.CorpCode), ("sn", key.SerialNo));
                string url = $"{ApiPrefix}md/work-orders/{key.WorkOrderNo}/attachments{query}";
                var filePart = new ByteArrayContent(file.Content);
                filePart.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                form.Add(filePart, FilesField, file.FileName);

                // 응답 data는 파일 목록을 품은 객체다(UploadManyAsync 참고) — 한 건만 올렸으니 첫 항목이 그 파일이다.
                var result = await PostUploadAsync(url, form, progress, cancellationToken).ConfigureAwait(false);
                return result?.Files?.FirstOrDefault();
            }
        }

        public async Task<List<FileUploadResponse>> UploadManyAsync(IReadOnlyList<FileBinary> files, PopProcessKey key, IProgress<TransferProgress> progress = null, CancellationToken cancellationToken = default)
        {
            if (files == null || files.Count == 0)
                throw new ArgumentException("업로드할 파일이 없습니다.", nameof(files));

            string query = BuildQuery(("corpCd", key.CorpCode), ("sn", key.SerialNo));
            string url = $"{ApiPrefix}md/work-orders/{key.WorkOrderNo}/attachments{query}";


            using (var form = new MultipartFormDataContent())
            {
                foreach (var file in files)
                {
                    var filePart = new ByteArrayContent(file.Content);
                    filePart.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                    // FileBinary.FileName은 FileInfo.Name이라 확장자까지 들어 있다(예: "불량사진.jpg").
                    form.Add(filePart, FilesField, file.FileName);
                }

                // 업무 키는 URL(경로 + corpCd/sn)로 이미 전달됐으므로 본문에는 파일만 담는다.
                // 응답 data는 배열이 아니라 { corpCd, woNo, sn, woRef, uploadedAttachmentList:[...] } 객체다.
                // 배열로 읽으면 서버엔 저장됐는데 해석 실패로 업로드 실패 창이 떴다.
                var result = await PostUploadAsync(url, form, progress, cancellationToken).ConfigureAwait(false);
                return result?.Files ?? new List<FileUploadResponse>();
            }
        }

        /// <summary>
        /// 업로드 공통 — 본문 크기로 제한 시간을 정해 업로드 전용 HttpClient로 보낸다.
        /// 일반 HttpClient(ApiTimeoutSeconds, 기본 30초)로 보내면 200MB 문서는 전송 도중 30초에 끊겼다.
        /// </summary>
        private Task<FileUploadResult> PostUploadAsync(string url, MultipartFormDataContent form, IProgress<TransferProgress> progress, CancellationToken cancellationToken)
        {
            long length = form.Headers.ContentLength ?? 0;
            var timeout = _options.UploadTimeoutFor(length);
            AppLog.Info($"업로드 제한 시간 {timeout.TotalSeconds:N0}초 (본문 {length:N0} byte)");

            return PostMultipartAsync<FileUploadResult>(_uploadClient, url, form, timeout, progress, cancellationToken);
        }

        public async Task<WorkOrderAttachment> GetAttachmentsAsync(PopProcessKey key, CancellationToken cancellationToken = default)
        {
            string query = BuildQuery(("corpCd", key.CorpCode), ("sn", key.SerialNo));
            var url = $"{ApiPrefix}md/work-orders/with-attachments/{key.WorkOrderNo}{query}";
            var response = await GetAsync<ApiDataResponse<WorkOrderAttachment>>(url, cancellationToken);
            return response?.Data ?? new WorkOrderAttachment();
        }

        public Task<AttachmentFile> DownloadAttachmentAsync(PopProcessKey key, string atchRef, CancellationToken cancellationToken = default)
        {
            string url = $"{ApiPrefix}common/file/{atchRef}";

            // 본문이 바이너리가 아니라 {filename, content} JSON이라 GetBytesAsync가 아닌 GetDataAsync로 읽는다.
            // GetDataAsync는 {"data":...} 봉투가 있으면 벗기고 없으면 그대로 읽으므로 봉투 유무와 상관없이 맞다.
            return GetDataAsync<AttachmentFile>(url, cancellationToken);
        }

        public Task DeleteAttachmentsAsync(PopProcessKey key, IEnumerable<string> atchRefs, CancellationToken cancellationToken = default)
        {
            string query = BuildQuery(("corpCd", key.CorpCode), ("sn", key.SerialNo));
            string url = $"{ApiPrefix}md/work-orders/{key.WorkOrderNo}/attachments{query}";
            // 바디를 객체({})로 감싸지 않고 배열만 보낸다 → ["ATCH001","ATCH002"]
            var body = (atchRefs ?? Enumerable.Empty<string>())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Trim())
                .ToArray();

            if (body.Length == 0)
                throw new ArgumentException("삭제할 첨부가 없습니다.", nameof(atchRefs));

            return DeleteAsync(url, body, cancellationToken);
        }

        public Task DeleteAttachmentsAsync(PopProcessKey key, string atchRefs, CancellationToken cancellationToken = default)
        {
            string url = $"{ApiPrefix}md/work-orders/{key.WorkOrderNo}/attachments";
            // 바디를 객체({})로 감싸지 않고 배열만 보낸다 → ["ATCH001","ATCH002"]
            string body = "[" + atchRefs + "]";
                

            if (body.Length == 0)
                throw new ArgumentException("삭제할 첨부가 없습니다.", nameof(atchRefs));

            return DeleteAsync(url, body, cancellationToken);
        }
    }
}
