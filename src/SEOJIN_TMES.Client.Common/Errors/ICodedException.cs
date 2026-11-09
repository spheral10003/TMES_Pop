using System.Globalization;

namespace SEOJIN_TMES.Client.Common.Errors
{
    /// <summary>
    /// 오류 코드와 <b>화면에 그대로 띄울 수 있는 안내 문구</b>를 함께 들고 있는 예외(ApiException · DocumentShareException).
    /// <see cref="System.Exception.Message"/>가 현재 화면 언어의 안내 + "오류 코드" 줄이므로
    /// 화면은 <c>ex.Message</c>를 그대로 띄우면 된다.
    /// (Common은 ApiClient를 모르므로 ApiException도 이 계약으로만 본다.)
    /// </summary>
    public interface ICodedException
    {
        /// <summary>
        /// 오류 코드 — <see cref="ErrorCodes"/>의 클라이언트 코드, 또는 서버가 준 업무 오류 코드(예: <c>WORK_005</c>).
        /// </summary>
        string Code { get; }

        /// <summary>
        /// 지정한 언어의 안내 문구(코드 줄 제외). 서버 로그는 화면 언어와 상관없이 한국어로 보내므로 이 메서드로 꺼낸다.
        /// </summary>
        string MessageFor(CultureInfo culture);
    }
}
