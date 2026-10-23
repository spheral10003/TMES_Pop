using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace SEOJIN_TMES.Client.Common.Imaging
{
    public class ByteToImage
    {
        public Image BytesToImage(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var img = Image.FromStream(ms))
            {
                return new Bitmap(img);
            }
        }

        // Base64 텍스트 판별 및 디코딩 ("data:image/png;base64," 접두어도 처리)
        public bool TryDecodeBase64(byte[] raw, out byte[] result)
        {
            result = null;

            // 이미 이미지 시그니처가 있으면 Base64가 아님
            if (FindImageOffset(raw) == 0) return false;

            try
            {
                string text = Encoding.ASCII.GetString(raw).Trim();
                int comma = text.IndexOf(',');
                if (text.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && comma > 0)
                    text = text.Substring(comma + 1);

                text = text.Replace("\r", "").Replace("\n", "").Replace(" ", "");
                if (text.Length == 0 || text.Length % 4 != 0) return false;

                result = Convert.FromBase64String(text);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        // 이미지 시그니처(PNG/JPEG/GIF/BMP/TIFF) 시작 위치 탐색. 없으면 -1
        public int FindImageOffset(byte[] d)
        {
            if (d == null || d.Length < 8) return -1;

            // 처음부터 시그니처 일치 여부
            if (IsSignatureAt(d, 0)) return 0;

            // OLE 헤더(Access/SQL Server image 컬럼 등) 대비: 앞부분 일정 범위에서 탐색
            int limit = Math.Min(d.Length - 4, 512);
            for (int i = 1; i < limit; i++)
            {
                // BMP("BM")는 오탐이 많아 탐색 대상에서 제외하고, 나머지 시그니처만 사용
                if (IsSignatureAt(d, i, false)) return i;
            }
            return -1;
        }

        public bool IsSignatureAt(byte[] d, int i, bool includeBmp = true)
        {
            if (i + 4 > d.Length) return false;

            // PNG
            if (d[i] == 0x89 && d[i + 1] == 0x50 && d[i + 2] == 0x4E && d[i + 3] == 0x47) return true;
            // JPEG
            if (d[i] == 0xFF && d[i + 1] == 0xD8 && d[i + 2] == 0xFF) return true;
            // GIF
            if (d[i] == 0x47 && d[i + 1] == 0x49 && d[i + 2] == 0x46 && d[i + 3] == 0x38) return true;
            // TIFF (II*\0 / MM\0*)
            if (d[i] == 0x49 && d[i + 1] == 0x49 && d[i + 2] == 0x2A && d[i + 3] == 0x00) return true;
            if (d[i] == 0x4D && d[i + 1] == 0x4D && d[i + 2] == 0x00 && d[i + 3] == 0x2A) return true;
            // BMP
            if (includeBmp && d[i] == 0x42 && d[i + 1] == 0x4D) return true;

            return false;
        }
    }
}
