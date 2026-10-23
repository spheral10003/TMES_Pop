using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.Common.Models
{
    /// <summary>
    /// 첨부파일 다운로드 응답. 서버는 원본 바이트를 그대로 내려주지 않고
    /// <c>{ "filename": "...", "content": "..." }</c> JSON으로 감싸서 준다 — 실제 데이터는 content에 있다.
    /// </summary>
    public class AttachmentFile
    {
        [JsonProperty("filename")]
        public string FileName { get; set; }

        /// <summary>
        /// 파일 원본 바이트. Newtonsoft는 JSON의 Base64 문자열(Java byte[] 기본 직렬화)과
        /// 숫자 배열 두 형식 모두를 byte[]로 읽으므로 따로 디코딩하지 않는다.
        /// </summary>
        [JsonProperty("content")]
        public byte[] Content { get; set; }
    }
}
