using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using DevExpress.Utils.Svg;

namespace SEOJIN_TMES.Client.Theme
{
    /// <summary>
    /// 화면에서 쓰는 SVG 아이콘 모음.
    /// DevExpress의 SVG 이미지 지원(<see cref="SvgImage"/>)을 쓰기 때문에 화면 배율(DPI)이 바뀌어도 깨지지 않고,
    /// 이미지 파일을 프로젝트에 넣지 않아도 되며(문자열로 보관) 색만 바꿔 여러 곳에 재사용할 수 있다.
    ///
    /// 각 상수는 색을 채워 넣을 자리(`{0}`)를 가진 템플릿이고, <see cref="Get"/>가 색을 적용해
    /// SvgImage를 만들어 캐시한다(같은 아이콘+같은 색은 한 번만 생성).
    /// </summary>
    public static class AppIcons
    {
        private const string Wrapper = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\" fill=\"none\" stroke-linecap=\"round\" stroke-linejoin=\"round\">{0}</svg>";

        /// <summary>햄버거(메뉴 접기/펴기).</summary>
        public const string Menu = "<path d=\"M3.5 6.5h17M3.5 12h17M3.5 17.5h17\" stroke=\"{0}\" stroke-width=\"2\"/>";

        /// <summary>알림(종).</summary>
        public const string Bell = "<path d=\"M12 3.2a5.2 5.2 0 0 0-5.2 5.2v3.4L4.9 15h14.2l-1.9-3.2V8.4A5.2 5.2 0 0 0 12 3.2z\" stroke=\"{0}\" stroke-width=\"1.7\"/><path d=\"M9.9 18a2.1 2.1 0 0 0 4.2 0\" stroke=\"{0}\" stroke-width=\"1.7\"/>";

        /// <summary>다크 모드(달).</summary>
        public const string Moon = "<path d=\"M20 14.2A8.2 8.2 0 1 1 10.4 3.4a6.6 6.6 0 0 0 9.6 10.8z\" stroke=\"{0}\" stroke-width=\"1.7\"/>";

        /// <summary>라이트 모드(해).</summary>
        public const string Sun = "<circle cx=\"12\" cy=\"12\" r=\"4\" stroke=\"{0}\" stroke-width=\"1.7\"/><path d=\"M12 2.6v2.6M12 18.8v2.6M2.6 12h2.6M18.8 12h2.6M5.4 5.4l1.8 1.8M16.8 16.8l1.8 1.8M18.6 5.4l-1.8 1.8M7.2 16.8l-1.8 1.8\" stroke=\"{0}\" stroke-width=\"1.7\"/>";

        /// <summary>사용자.</summary>
        public const string User = "<circle cx=\"12\" cy=\"8.4\" r=\"3.6\" stroke=\"{0}\" stroke-width=\"1.7\"/><path d=\"M4.8 20c1.6-3.6 4.1-5.2 7.2-5.2s5.6 1.6 7.2 5.2\" stroke=\"{0}\" stroke-width=\"1.7\"/>";

        /// <summary>홈(브레드크럼 시작).</summary>
        public const string Home = "<path d=\"M4 11l8-6.2 8 6.2v8.2a1 1 0 0 1-1 1h-4.6v-5.6h-4.8V20H5a1 1 0 0 1-1-1z\" stroke=\"{0}\" stroke-width=\"1.7\"/>";

        /// <summary>대시보드(타일 4개).</summary>
        public const string Dashboard = "<rect x=\"3.5\" y=\"3.5\" width=\"7\" height=\"7\" rx=\"1.8\" stroke=\"{0}\" stroke-width=\"1.7\"/><rect x=\"13.5\" y=\"3.5\" width=\"7\" height=\"7\" rx=\"1.8\" stroke=\"{0}\" stroke-width=\"1.7\"/><rect x=\"3.5\" y=\"13.5\" width=\"7\" height=\"7\" rx=\"1.8\" stroke=\"{0}\" stroke-width=\"1.7\"/><rect x=\"13.5\" y=\"13.5\" width=\"7\" height=\"7\" rx=\"1.8\" stroke=\"{0}\" stroke-width=\"1.7\"/>";

        /// <summary>작업(클립보드).</summary>
        public const string Clipboard = "<rect x=\"5\" y=\"4.5\" width=\"14\" height=\"16\" rx=\"2.2\" stroke=\"{0}\" stroke-width=\"1.7\"/><path d=\"M9 4.5V3h6v1.5M8.6 10h6.8M8.6 14h6.8M8.6 17.6h4.2\" stroke=\"{0}\" stroke-width=\"1.7\"/>";

        /// <summary>현황(막대 차트).</summary>
        public const string Chart = "<path d=\"M4 20h16\" stroke=\"{0}\" stroke-width=\"1.7\"/><rect x=\"6\" y=\"11\" width=\"3.4\" height=\"6\" rx=\"1\" fill=\"{0}\"/><rect x=\"11.3\" y=\"7\" width=\"3.4\" height=\"10\" rx=\"1\" fill=\"{0}\"/><rect x=\"16.6\" y=\"13\" width=\"3.4\" height=\"4\" rx=\"1\" fill=\"{0}\"/>";

        /// <summary>통합(레이어).</summary>
        public const string Layers = "<path d=\"M12 3.6l8 4.2-8 4.2-8-4.2z\" stroke=\"{0}\" stroke-width=\"1.7\"/><path d=\"M4 12.4l8 4.2 8-4.2M4 16.6l8 4.2 8-4.2\" stroke=\"{0}\" stroke-width=\"1.7\"/>";

        /// <summary>설비 시작.</summary>
        public const string Play = "<path d=\"M8.5 5.4l10 6.6-10 6.6z\" fill=\"{0}\"/>";

        /// <summary>설비 정지.</summary>
        public const string Stop = "<rect x=\"7\" y=\"7\" width=\"10\" height=\"10\" rx=\"2.4\" fill=\"{0}\"/>";

        /// <summary>리셋(반시계 회전).</summary>
        public const string Reset = "<path d=\"M4.6 12a7.4 7.4 0 1 1 2.4 5.5\" stroke=\"{0}\" stroke-width=\"1.9\"/><path d=\"M4.6 6.6V12h5.4\" stroke=\"{0}\" stroke-width=\"1.9\"/>";

        /// <summary>새로 고침(시계 회전).</summary>
        public const string Refresh = "<path d=\"M19.4 12a7.4 7.4 0 1 0-2.4 5.5\" stroke=\"{0}\" stroke-width=\"1.9\"/><path d=\"M19.4 6.6V12H14\" stroke=\"{0}\" stroke-width=\"1.9\"/>";

        /// <summary>조회(돋보기).</summary>
        public const string Search = "<circle cx=\"11\" cy=\"11\" r=\"6\" stroke=\"{0}\" stroke-width=\"1.9\"/><path d=\"M15.6 15.6L20 20\" stroke=\"{0}\" stroke-width=\"1.9\"/>";

        /// <summary>로그아웃.</summary>
        public const string Logout = "<path d=\"M14.6 4H19a1 1 0 0 1 1 1v14a1 1 0 0 1-1 1h-4.4\" stroke=\"{0}\" stroke-width=\"1.7\"/><path d=\"M9.6 8.2L5.4 12l4.2 3.8M5.4 12h9\" stroke=\"{0}\" stroke-width=\"1.7\"/>";

        /// <summary>탭 닫기.</summary>
        public const string Close = "<path d=\"M7 7l10 10M17 7L7 17\" stroke=\"{0}\" stroke-width=\"2\"/>";

        /// <summary>창 최소화(가로줄 한 개).</summary>
        public const string WindowMinimize = "<path d=\"M6 12.5h12\" stroke=\"{0}\" stroke-width=\"1.8\"/>";

        /// <summary>창 최대화(빈 사각형).</summary>
        public const string WindowMaximize = "<rect x=\"6.5\" y=\"6.5\" width=\"11\" height=\"11\" rx=\"1.4\" stroke=\"{0}\" stroke-width=\"1.6\"/>";

        /// <summary>창 복원(겹친 사각형 두 개) — 최대화 상태에서는 최대화 버튼이 이 모양으로 바뀐다.</summary>
        public const string WindowRestore = "<path d=\"M8.6 8.6V7.3a1.2 1.2 0 0 1 1.2-1.2h6.9a1.2 1.2 0 0 1 1.2 1.2v6.9a1.2 1.2 0 0 1-1.2 1.2h-1.3\" stroke=\"{0}\" stroke-width=\"1.6\"/><rect x=\"6\" y=\"8.6\" width=\"9.3\" height=\"9.3\" rx=\"1.2\" stroke=\"{0}\" stroke-width=\"1.6\"/>";

        /// <summary>메뉴 하위 항목 표시용 점.</summary>
        public const string Dot = "<circle cx=\"12\" cy=\"12\" r=\"3.2\" fill=\"{0}\"/>";

        /// <summary>
        /// 대기 스피너의 점 하나 — <b>상자를 꽉 채우는 원</b>이라 렌더 크기가 곧 점 크기가 된다.
        /// <see cref="Dot"/>은 둘레에 여백이 넓어(지름이 상자의 27%) 그대로 쓰면
        /// 점 크기에 맞춘 이미지가 너무 커져 스피너의 점 간격이 어긋난다.
        /// </summary>
        public const string SpinnerDot = "<circle cx=\"12\" cy=\"12\" r=\"11\" fill=\"{0}\"/>";

        /// <summary>제품 로고 자리(둥근 사각형 + 링).</summary>
        public const string Brand = "<rect x=\"2.5\" y=\"2.5\" width=\"19\" height=\"19\" rx=\"5.5\" fill=\"{0}\"/><circle cx=\"12\" cy=\"12\" r=\"5.4\" stroke=\"#FFFFFF\" stroke-width=\"1.8\" opacity=\"0.92\"/><circle cx=\"12\" cy=\"12\" r=\"1.9\" fill=\"#FFFFFF\"/>";

        /// <summary>안내(i).</summary>
        public const string Info = "<circle cx=\"12\" cy=\"12\" r=\"9.2\" stroke=\"{0}\" stroke-width=\"1.8\"/><path d=\"M12 10.6v6.2\" stroke=\"{0}\" stroke-width=\"2\"/><circle cx=\"12\" cy=\"7.4\" r=\"1.2\" fill=\"{0}\"/>";

        /// <summary>질문(?) — 확인/취소를 묻는 창.</summary>
        public const string Question = "<circle cx=\"12\" cy=\"12\" r=\"9.2\" stroke=\"{0}\" stroke-width=\"1.8\"/><path d=\"M9.4 9.2a2.7 2.7 0 1 1 3.6 2.6c-.7.3-1 .9-1 1.7v.5\" stroke=\"{0}\" stroke-width=\"1.8\"/><circle cx=\"12\" cy=\"17\" r=\"1.2\" fill=\"{0}\"/>";

        /// <summary>완료(체크).</summary>
        public const string Check = "<circle cx=\"12\" cy=\"12\" r=\"9.2\" stroke=\"{0}\" stroke-width=\"1.8\"/><path d=\"M7.8 12.4l2.9 2.9 5.5-6.2\" stroke=\"{0}\" stroke-width=\"2\"/>";

        /// <summary>주의(삼각형 + !).</summary>
        public const string Warning = "<path d=\"M12 3.6l9 15.4H3z\" stroke=\"{0}\" stroke-width=\"1.8\"/><path d=\"M12 9.4v4.6\" stroke=\"{0}\" stroke-width=\"2\"/><circle cx=\"12\" cy=\"16.6\" r=\"1.1\" fill=\"{0}\"/>";

        /// <summary>오류(원 + X).</summary>
        public const string ErrorMark = "<circle cx=\"12\" cy=\"12\" r=\"9.2\" stroke=\"{0}\" stroke-width=\"1.8\"/><path d=\"M8.8 8.8l6.4 6.4M15.2 8.8l-6.4 6.4\" stroke=\"{0}\" stroke-width=\"2\"/>";

        /// <summary>설비/장비(공장).</summary>
        public const string Factory = "<path d=\"M4 20V10l5 3V10l5 3V7l6 4v9z\" stroke=\"{0}\" stroke-width=\"1.7\"/><path d=\"M4 20h16\" stroke=\"{0}\" stroke-width=\"1.7\"/>";

        private static readonly Dictionary<string, SvgImage> Cache = new Dictionary<string, SvgImage>();

        /// <summary>
        /// 좌측 메뉴의 화면(리프) 항목 앞에 붙는 "트리 가지 + 색 점" 아이콘.
        /// 세로 안내선이 아이콘 높이(행 높이와 동일)를 가득 채우므로, 항목이 연달아 있으면 선이 이어져
        /// 상위 분류 아래에 매달린 계층 구조가 눈에 보인다. 마지막 항목은 선을 중간까지만 그려 "└" 모양이 된다.
        /// </summary>
        public static SvgImage TreeLeaf(Color dot, Color guide, bool lastChild)
        {
            var dotHex = ToHex(dot);
            var guideHex = ToHex(guide);
            var key = $"tree|{dotHex}|{guideHex}|{lastChild}";

            lock (Cache)
            {
                if (Cache.TryGetValue(key, out var cached))
                    return cached;

                var vertical = lastChild ? "M7 0 V17" : "M7 0 V34";
                var svg =
                    "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 34\" fill=\"none\">" +
                    $"<path d=\"{vertical}\" stroke=\"{guideHex}\" stroke-width=\"1.2\"/>" +
                    $"<path d=\"M7 17 H12.6\" stroke=\"{guideHex}\" stroke-width=\"1.2\"/>" +
                    $"<circle cx=\"16.6\" cy=\"17\" r=\"3.4\" fill=\"{dotHex}\"/>" +
                    "</svg>";

                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg)))
                {
                    var image = SvgImage.FromStream(stream);
                    Cache[key] = image;
                    return image;
                }
            }
        }

        /// <summary>아이콘 템플릿에 색을 적용한 SvgImage를 돌려준다(같은 조합은 캐시 재사용).</summary>
        public static SvgImage Get(string template, Color color)
        {
            var hex = ToHex(color);
            var key = hex + "|" + template;

            lock (Cache)
            {
                if (Cache.TryGetValue(key, out var cached))
                    return cached;

                var svg = string.Format(Wrapper, string.Format(template, hex));
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg)))
                {
                    var image = SvgImage.FromStream(stream);
                    Cache[key] = image;
                    return image;
                }
            }
        }

        private static string ToHex(Color color) => "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
    }
}
