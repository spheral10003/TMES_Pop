using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.Common.Models
{
    // <summary>
    /// 작업지시에 붙은 첨부파일 한 건.
    /// TODO: atchRef 외의 필드명은 <b>가정값</b>이다 — 서버 스펙이 확정되면 [JsonProperty] 이름을 맞출 것.
    /// </summary>
    public class WorkOrderAttachmentDto
    {
        /// <summary>첨부 식별자. 삭제할 때 이 값을 배열로 보낸다.</summary>
        [JsonProperty("atchRef")]
        public string AttachRef { get; set; }

        [JsonProperty("fileNm")]
        public string FileName { get; set; }

        [JsonProperty("fileSize")]
        public long FileSize { get; set; }

        [JsonProperty("fileExtn")]
        public string FileExtension { get; set; }

        /// <summary>작성자. TODO: 필드명은 가정값.</summary>
        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        /// <summary>작성일시. TODO: 필드명은 가정값.</summary>
        [JsonProperty("createdAt")]
        public DateTime? CreatedAt { get; set; }

        private static readonly HashSet<string> ImageExtensions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "png", "jpg", "jpeg", "bmp", "gif" };

        /// <summary>사진인지(미리보기를 띄울지 판단할 때 쓴다). 서버가 준 확장자를 보고, 없으면 파일 이름에서 꺼낸다.</summary>
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
        public override string ToString() => FileName;
    }

    public class WorkOrderAttachment
    {
        [JsonProperty("attachmentList")]
        public List<WorkOrderAttachmentDto> Attachments = new List<WorkOrderAttachmentDto>();
    }
}
