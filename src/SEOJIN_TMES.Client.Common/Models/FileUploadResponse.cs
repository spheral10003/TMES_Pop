using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.Common.Models
{
    /// <summary>
    /// 첨부 업로드 응답의 data — 작업지시 키와 이번에 올라간 파일 목록.
    /// data가 배열이 아니라 이 객체로 오므로 List로 읽으면 서버엔 저장됐는데 클라이언트만 실패로 본다(2026-10-29 로그).
    /// </summary>
    public class FileUploadResult
    {
        [JsonProperty("corpCd")]
        public string CorpCode { get; set; }

        [JsonProperty("woNo")]
        public string WorkOrderNo { get; set; }

        [JsonProperty("sn")]
        public string SerialNo { get; set; }

        [JsonProperty("woRef")]
        public string WorkOrderRef { get; set; }

        [JsonProperty("uploadedAttachmentList")]
        public List<FileUploadResponse> Files { get; set; } = new List<FileUploadResponse>();
    }

    /// <summary>
    /// 업로드된 파일 한 건(업로드 응답의 uploadedAttachmentList 항목).
    /// sysObjId · atchRef · fileNm · fileSize는 서버 응답으로 확인했다.
    /// TODO: 나머지 필드명은 <b>가정값</b>이다 — 서버 스펙이 확정되면 [JsonProperty] 이름을 맞출 것.
    /// </summary>
    public class FileUploadResponse
    {
        /// <summary>서버가 저장한 파일의 식별자. 이후 조회·삭제·다른 업무 데이터와 연결할 때 이 값을 쓴다.</summary>
        [JsonProperty("sysObjId")]
        public string FileId { get; set; }

        [JsonProperty("fileNm")]
        public string FileName { get; set; }

        [JsonProperty("fileExtension")]
        public string FileExtension { get; set; }

        [JsonProperty("fileSize")]
        public long FileSize { get; set; }

        /// <summary>첨부 식별자. 다운로드 · 삭제할 때 이 값을 쓴다. TODO: 필드명은 가정값.</summary>
        [JsonProperty("atchRef")]
        public string AttachRef { get; set; }

        /// <summary>작성자. TODO: 필드명은 가정값.</summary>
        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        /// <summary>최종 수정자.</summary>
        [JsonProperty("updatedBy")]
        public string UpdatedBy { get; set; }

        /// <summary>작성일시. TODO: 필드명은 가정값.</summary>
        [JsonProperty("createdAt")]
        public DateTime? CreatedAt { get; set; }

        private static readonly HashSet<string> ImageExtensions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "png", "jpg", "jpeg", "bmp", "gif" };

        /// <summary>
        /// 확장자까지 붙인 파일 이름(목록 · 그리드 표시용).
        /// 서버가 이름과 확장자를 따로 줄 때가 있어, 이름에 확장자가 없으면 붙여 준다.
        /// </summary>
        [JsonIgnore]
        public string DisplayName
        {
            get
            {
                var name = FileName ?? "";
                var ext = (FileExtension ?? "").Trim().TrimStart('.');
                if (ext.Length == 0 || name.EndsWith("." + ext, StringComparison.OrdinalIgnoreCase))
                    return name;
                return name + "." + ext;
            }
        }

        /// <summary>사진인지(첨부유형 이미지/문서 구분). 서버가 준 확장자를 보고, 없으면 파일 이름에서 꺼낸다.</summary>
        [JsonIgnore]
        public bool IsImage
        {
            get
            {
                var ext = string.IsNullOrWhiteSpace(FileExtension)
                    ? System.IO.Path.GetExtension(FileName ?? "")
                    : FileExtension;
                return ImageExtensions.Contains(ext.TrimStart('.'));
            }
        }

        /// <summary>ListBox가 항목 글자로 이 값을 쓴다.</summary>
        public override string ToString() => DisplayName;
    }
}
