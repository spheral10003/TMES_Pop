using SEOJIN_TMES.Client.Common.Files;
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
        Task<FileUploadResponse> UploadAsync(FileBinary file, string category, string refKey, CancellationToken cancellationToken = default);
    }

    public class FileApiClient : ApiClientBase, IFileApiClient
    {
        private const string UploadPath = ApiPrefix + "files/upload";

        public FileApiClient(HttpClient httpClient) : base(httpClient) { }
        
        public async Task<FileUploadResponse> UploadAsync(FileBinary file, string category, string refKey, CancellationToken cancellationToken = default)
        {
            using (var form = new MultipartFormDataContent())
            {
                // 파일 본문 — StreamContent가 아니라 ByteArrayContent여야 401 재시도 때 내용이 다시 실린다.
                var filePart = new ByteArrayContent(file.Content);
                filePart.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                form.Add(filePart, "file", file.FileName);

                // 함께 보내는 일반 필드. 값이 없는 필드는 보내지 않는다(BuildQuery와 같은 규칙).
                if (!string.IsNullOrWhiteSpace(category))
                    form.Add(new StringContent(category.Trim()), "category");
                if (!string.IsNullOrWhiteSpace(refKey))
                    form.Add(new StringContent(refKey.Trim()), "refKey");

                return await PostMultipartAsync<FileUploadResponse>(UploadPath, form, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
