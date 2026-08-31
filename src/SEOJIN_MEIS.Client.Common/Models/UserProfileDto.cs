namespace SEOJIN_MEIS.Client.Common.Models
{
    /// <summary>
    /// 로그인 사용자 프로필(사용자 정보 팝업에 표시). 서버 조회 결과를 그대로 담는 계약이며,
    /// 클라이언트는 값 판단 없이 화면에 표시만 한다.
    /// TODO: 실제 서버 API의 필드명이 확정되면 이 DTO의 속성명을 맞추거나 JsonProperty로 매핑할 것.
    /// </summary>
    public class UserProfileDto
    {
        /// <summary>사번.</summary>
        public string EmployeeNo { get; set; }

        /// <summary>이름.</summary>
        public string UserName { get; set; }

        /// <summary>연락처.</summary>
        public string PhoneNumber { get; set; }

        /// <summary>부서명.</summary>
        public string DepartmentName { get; set; }

        /// <summary>부서 코드.</summary>
        public string DeptCode { get; set; }
    }
}
