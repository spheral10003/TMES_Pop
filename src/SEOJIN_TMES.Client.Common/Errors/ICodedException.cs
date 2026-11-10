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

        /// <summary>
        /// 코드의 설명 한 줄 — 화면의 "오류 코드" 줄과 서버 로그(<c>msg</c> · <c>errMsg</c>)에 코드와 함께 붙인다.
        /// 클라이언트 코드는 <c>Error_{코드}</c> 문구, 서버 업무 코드(클라이언트에 문구가 없는 코드)는 서버가 내려준 사유.
        /// </summary>
        string DescriptionFor(CultureInfo culture);
    }
}
