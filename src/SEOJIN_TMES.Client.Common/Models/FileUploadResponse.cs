using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.Common.Models
{
    /// <summary>
    /// 파일 업로드 응답.
    /// TODO: 필드명은 <b>가정값</b>이다 — 서버 스펙이 확정되면 [JsonProperty] 이름을 맞출 것.
    /// </summary>
    public class FileUploadResponse
    {
        /// <summary>서버가 저장한 파일의 식별자. 이후 조회·삭제·다른 업무 데이터와 연결할 때 이 값을 쓴다.</summary>
        [JsonProperty("fileId")]
        public string FileId { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("fileExtension")]
        public string FileExtension { get; set; }

        [JsonProperty("fileSize")]
        public long FileSize { get; set; }

        
        
    }
}
