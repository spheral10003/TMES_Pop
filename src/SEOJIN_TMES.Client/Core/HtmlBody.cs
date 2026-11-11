using System.Text.RegularExpressions;

namespace SEOJIN_TMES.Client.Core
{
    /// <summary>
    /// 서버가 준 본문 HTML(알림 메일 · 공지)을 WebBrowser(IE 엔진)에 넣을 수 있게 다듬는다.
    /// 알림 화면(NotificationForm)과 홈 공지 보기(NoticeViewer)가 같은 규칙을 쓰도록 한 곳에 둔다.
    /// </summary>
    public static class HtmlBody
    {
        private const string CompatibleMeta = "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">";
        private const RegexOptions Options = RegexOptions.IgnoreCase | RegexOptions.Singleline;

        /// <summary>
        /// - <b>X-UA-Compatible(IE=edge)를 head 맨 앞에 넣는다.</b> WebBrowser 컨트롤은 레지스트리 설정이 없으면
        ///   IE7 호환 모드로 그려서 인라인 CSS(여백 · 배경 · 테두리)와 표 레이아웃이 어긋난다.
        ///   이 meta는 head 안에서 title/meta 말고 다른 요소보다 앞에 있어야 효과가 있다.
        /// - <b>script 블록은 지운다.</b> 본문은 보여 주기만 하는 내용이라 스크립트가 필요 없고,
        ///   스크립트로 외부 주소를 부르는 것을 막기 위해서다(링크 · 새 창은 각 화면의 Navigating / NewWindow에서 막는다).
        ///
        /// 본문의 색 · 레이아웃 자체는 건드리지 않는다 — 서버가 만든 모양을 그대로 보여 준다.
        /// </summary>
        public static string ForWebBrowser(string? body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return string.Empty;

            var html = Regex.Replace(body, @"<script\b.*?</script\s*>", string.Empty, Options);

            var head = Regex.Match(html, @"<head\b[^>]*>", Options);
            if (head.Success)
                return html.Insert(head.Index + head.Length, CompatibleMeta);

            // head가 없는 조각 HTML이면 html 태그 바로 뒤에, 그것도 없으면 맨 앞에 head를 만들어 붙인다.
            // (공지 본문은 <div>로 시작하는 조각이라 맨 아래 경로를 탄다.)
            var root = Regex.Match(html, @"<html\b[^>]*>", Options);
            if (root.Success)
                return html.Insert(root.Index + root.Length, "<head>" + CompatibleMeta + "</head>");

            return "<!DOCTYPE html><html><head>" + CompatibleMeta + "</head><body>" + html + "</body></html>";
        }
    }
}
