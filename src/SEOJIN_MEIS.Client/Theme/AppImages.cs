using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using DevExpress.Utils.Svg;

namespace SEOJIN_MEIS.Client.Theme
{
    /// <summary>
    /// 브랜드 이미지 자산(Images 폴더) 로더.
    ///
    /// 두 파일은 <b>어셈블리에 포함(EmbeddedResource)</b>해서 배포한다 —
    /// exe만 복사해도 로고가 따라가고, 현장에서 파일이 지워져 그림이 안 나오는 일이 없다.
    /// 파일을 교체할 때는 Images 폴더의 원본만 바꾸면 된다(빌드 시 다시 포함된다).
    ///
    /// 로고 SVG는 원본이 &lt;style&gt;의 CSS 클래스로 색을 지정하는데,
    /// 워드마크("TEXON") 글자에는 색 지정이 없어 기본값인 <b>검정</b>으로 그려진다.
    /// 사이드바는 짙은 남보라(<see cref="AppTheme.SidebarBrand"/>)라 그대로 쓰면 글자가 보이지 않으므로,
    /// 읽어 들일 때 클래스 색을 fill 속성으로 펴고 워드마크 색만 원하는 색으로 바꾼다(녹아웃 로고).
    /// </summary>
    public static class AppImages
    {
        private const string LoginIllustrationResource = "SEOJIN_MEIS.Client.Images.login_icon.png";
        private const string BrandLogoResource = "SEOJIN_MEIS.Client.Images.texon_logo.svg";

        /// <summary>창·작업 표시줄 아이콘. exe 아이콘(csproj의 ApplicationIcon)과 같은 파일이다.</summary>
        private const string AppIconResource = "SEOJIN_MEIS.Client.Images.texon.ico";

        /// <summary>상단 바 아이콘(원본은 640×640 단색 실루엣 + 투명 배경).</summary>
        public const string IconBell = "SEOJIN_MEIS.Client.Images.bell-regular.png";
        public const string IconUser = "SEOJIN_MEIS.Client.Images.circle-user-regular.png";
        public const string IconPowerOff = "SEOJIN_MEIS.Client.Images.power-off-solid.png";

        /// <summary>홈 — 상단 바 브레드크럼 맨 앞과 홈 탭에서 "홈"이라는 글자 대신 쓴다.</summary>
        public const string IconHome = "SEOJIN_MEIS.Client.Images.house-solid.png";

        /// <summary>국기 아이콘(24×24). 뒤에 두 글자 언어 코드가 붙는다 — flag_ko / flag_en / flag_vi.</summary>
        private const string FlagResourcePrefix = "SEOJIN_MEIS.Client.Images.flag_";

        /// <summary>크기·색까지 적용해 만들어 둔 아이콘. 같은 조합을 다시 만들지 않도록 캐시한다.</summary>
        private static readonly Dictionary<string, Image> RasterCache = new Dictionary<string, Image>();

        /// <summary>원본 SVG의 &lt;style&gt;에 정의된 브랜드 색. 파일이 바뀌면 이 값도 함께 맞출 것.</summary>
        private const string SwooshColor = "#225ea8";
        private const string MarkColor = "#94ce23";

        /// <summary>
        /// 심벌(초록 지구본)만 잘라 내는 viewBox. 접힌 사이드바(64px)처럼 좁은 자리에 쓴다.
        /// 원본 viewBox는 "0 0 776 183.89"이며 심벌은 좌상단 일부만 차지한다.
        /// </summary>
        private const string MarkViewBox = "28 -6 150 108";

        private static Image _loginIllustration;
        private static Icon _appIcon;
        private static SvgImage _brandSignature;
        private static Color _brandSignatureColor;
        private static SvgImage _brandMark;

        /// <summary>
        /// 창·작업 표시줄에 뜨는 프로그램 아이콘.
        ///
        /// exe 아이콘(csproj의 <c>ApplicationIcon</c>)과 <b>같은 파일</b>이지만 경로가 다르다 —
        /// 그쪽은 빌드할 때 실행 파일에 박히고, 이쪽은 실행 중에 각 Form의 <c>Icon</c>에 넣는 값이다.
        /// 둘 중 하나만 지정하면 탐색기와 작업 표시줄의 그림이 서로 달라진다.
        ///
        /// 아이콘은 앱이 끝날 때까지 재사용하므로 한 번 만들어 캐시한다.
        /// </summary>
        public static Icon AppIcon()
        {
            if (_appIcon == null)
            {
                using (var stream = OpenResource(AppIconResource))
                    _appIcon = new Icon(stream);
            }

            return _appIcon;
        }

        /// <summary>로그인 화면 좌측에 놓는 일러스트(스마트팩토리 아이소메트릭, 배경 투명).</summary>
        public static Image LoginIllustration()
        {
            if (_loginIllustration == null)
            {
                // Image.FromStream은 이미지가 살아 있는 동안 스트림도 살아 있어야 한다.
                // 여기서 만든 이미지는 앱이 끝날 때까지 캐시로 들고 있으므로 MemoryStream을 닫지 않는다.
                var buffer = new MemoryStream();
                using (var stream = OpenResource(LoginIllustrationResource))
                    stream.CopyTo(buffer);

                buffer.Position = 0;
                _loginIllustration = Image.FromStream(buffer);
            }

            return _loginIllustration;
        }

        /// <summary>
        /// 로고 전체(심벌 + 스우시 + TEXON 워드마크). 가로:세로가 약 4.2:1이라 넓은 자리에 쓴다.
        /// </summary>
        /// <param name="wordmarkColor">워드마크 글자 색. 어두운 배경에는 흰색을 넘긴다.</param>
        public static SvgImage BrandSignature(Color wordmarkColor)
        {
            if (_brandSignature != null && _brandSignatureColor == wordmarkColor)
                return _brandSignature;

            var markup = InlineStyles(ReadLogoMarkup(), ToHex(wordmarkColor));

            _brandSignature = FromMarkup(markup);
            _brandSignatureColor = wordmarkColor;
            return _brandSignature;
        }

        /// <summary>로고의 심벌(초록 지구본)만. 접힌 사이드바처럼 폭이 좁은 자리에 쓴다.</summary>
        public static SvgImage BrandMark()
        {
            if (_brandMark == null)
                _brandMark = FromMarkup(MarkOnly(ReadLogoMarkup()));

            return _brandMark;
        }

        /// <summary>
        /// 상단 바용 아이콘 — 원본(640×640)을 쓸 크기로 줄이고 <paramref name="tint"/> 색으로 칠한다.
        ///
        /// 원본이 짙은 남색 단색이라 그대로 쓰면 다크 모드에서 배경에 묻힌다.
        /// 모양(알파 채널)만 남기고 색은 테마에서 받아 쓰면, 기존 SVG 아이콘과 똑같이 테마를 따라간다.
        /// </summary>
        public static Image Icon(string resourceName, int size, Color tint) =>
            Raster(resourceName, new Size(size, size), tint);

        /// <summary>언어 국기 아이콘. 원래 색을 그대로 살려야 하므로 칠하지 않고 크기만 맞춘다.</summary>
        public static Image Flag(string twoLetterCode, int size) =>
            Raster(FlagResourcePrefix + twoLetterCode + ".png", new Size(size, size), Color.Empty);

        /// <summary>
        /// 비트맵 리소스를 지정 크기로 줄이고(필요하면) 단색으로 칠해 돌려준다.
        /// 만든 결과는 앱이 끝날 때까지 캐시한다 — 상단 바 아이콘은 테마를 바꿀 때마다 다시 요청된다.
        /// </summary>
        private static Image Raster(string resourceName, Size size, Color tint)
        {
            var key = resourceName + "|" + size.Width + "x" + size.Height + "|" + tint.ToArgb();

            Image cached;
            if (RasterCache.TryGetValue(key, out cached))
                return cached;

            var scaled = new Bitmap(size.Width, size.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var source = LoadBitmap(resourceName))
            using (var graphics = Graphics.FromImage(scaled))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(source, new Rectangle(Point.Empty, size));
            }

            if (tint != Color.Empty)
                Tint(scaled, tint);

            RasterCache[key] = scaled;
            return scaled;
        }

        /// <summary>
        /// 모양은 그대로 두고 색만 바꾼다 — 알파는 건드리지 않아 가장자리 부드러움이 유지된다.
        /// 아이콘 크기가 20px 안팎이라 픽셀을 하나씩 훑어도 부담이 없다.
        /// </summary>
        private static void Tint(Bitmap bitmap, Color color)
        {
            for (var y = 0; y < bitmap.Height; y++)
            {
                for (var x = 0; x < bitmap.Width; x++)
                {
                    var alpha = bitmap.GetPixel(x, y).A;
                    if (alpha != 0)
                        bitmap.SetPixel(x, y, Color.FromArgb(alpha, color.R, color.G, color.B));
                }
            }
        }

        private static Bitmap LoadBitmap(string resourceName)
        {
            using (var stream = OpenResource(resourceName))
                return new Bitmap(stream);
        }

        /// <summary>
        /// CSS 클래스로 지정된 색을 각 도형의 fill 속성으로 펴고, 색이 없는 워드마크에 색을 넣는다.
        /// (원본을 그대로 써도 렌더링은 되지만, 워드마크 색을 배경에 맞춰 바꿀 수가 없다.)
        /// </summary>
        private static string InlineStyles(string markup, string wordmarkColor)
        {
            var result = RemoveDefs(markup);

            result = result.Replace("class=\"cls-1\"", "fill=\"" + SwooshColor + "\"");
            result = result.Replace("class=\"cls-2\"", "fill=\"" + MarkColor + "\"");

            // 클래스가 붙은 도형은 위에서 이미 fill을 받았으므로, 여기 걸리는 건 워드마크 글자뿐이다.
            result = result.Replace("<path d=", "<path fill=\"" + wordmarkColor + "\" d=");

            return result;
        }

        /// <summary>심벌만 남기고(워드마크·스우시 제거) viewBox를 심벌 영역으로 좁힌다.</summary>
        private static string MarkOnly(string markup)
        {
            var result = RemoveDefs(markup);

            result = Regex.Replace(result, "<path d=\"[^\"]*\"\\s*/>", string.Empty);
            result = Regex.Replace(result, "<path class=\"cls-1\"[^>]*/>", string.Empty);

            result = result.Replace("class=\"cls-2\"", "fill=\"" + MarkColor + "\"");
            result = Regex.Replace(result, "viewBox=\"[^\"]*\"", "viewBox=\"" + MarkViewBox + "\"");

            return result;
        }

        /// <summary>&lt;defs&gt;의 &lt;style&gt; 규칙은 fill 속성으로 폈으므로 통째로 걷어낸다.</summary>
        private static string RemoveDefs(string markup) =>
            Regex.Replace(markup, "<defs>.*?</defs>", string.Empty, RegexOptions.Singleline);

        private static string ReadLogoMarkup()
        {
            using (var stream = OpenResource(BrandLogoResource))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
                return reader.ReadToEnd();
        }

        private static SvgImage FromMarkup(string markup)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(markup)))
                return SvgImage.FromStream(stream);
        }

        private static Stream OpenResource(string name)
        {
            var stream = typeof(AppImages).Assembly.GetManifestResourceStream(name);
            if (stream == null)
                throw new InvalidOperationException($"이미지 리소스를 찾을 수 없다: {name} (csproj의 EmbeddedResource/LogicalName 확인)");

            return stream;
        }

        private static string ToHex(Color color) =>
            "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
    }
}
