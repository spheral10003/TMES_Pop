using Newtonsoft.Json;
using System.Collections.Generic;

namespace SEOJIN_TMES.Client.Common.Models
{
    /// <summary>
    /// 로그인 성공 후 클라이언트가 화면 표시용으로만 들고 있는 사용자 정보 (권한 판단은 서버가 수행).
    ///
    /// 값의 출처가 두 가지다:
    ///  - <see cref="UserId"/>/<see cref="Workstation"/>/<see cref="Server"/> : 로그인 화면에서 입력·선택한 값(클라이언트가 아는 정보)
    ///  - <see cref="EmployeeNo"/>/<see cref="UserName"/>/<see cref="PhoneNumber"/>/<see cref="DepartmentName"/> :
    ///    서버 프로필 조회 API 응답(<see cref="UserProfileDto"/>)을 채워 넣은 값. 조회 전이거나 실패하면 비어 있다.
    /// </summary>
    public class UserSession
    {
        /// <summary>로그인 아이디.</summary>
        public string UserId { get; set; }

        /// <summary>사번.</summary>
        [JsonProperty("orgNo")]
        public string EmployeeNo { get; set; }

        /// <summary>이름.</summary>
        [JsonProperty("userNm")]
        public string UserName { get; set; }

        /// <summary>연락처.</summary>
        public string PhoneNumber { get; set; }

        /// <summary>부서명.</summary>
        public string DepartmentName { get; set; }

        /// <summary>부서 코드.</summary>
        public string DeptCode { get; set; }

        /// <summary>로그인 시 선택한 워크스테이션.</summary>
        [JsonProperty("workstationList")]
        public string Workstation { get; set; }

        /// <summary>접속 서버.</summary>
        [JsonProperty("corpCd")]
        public string Server { get; set; }

        /// <summary>워크스테이션 정보.</summary>
        public WorkstationInfo WorkstationInfo { get; set; }

        /// <summary>서버에서 조회한 프로필을 세션에 반영한다.</summary>
        public void ApplyProfile(UserProfileDto profile)
        {
            if (profile == null)
                return;

            EmployeeNo = profile.EmployeeNo;
            UserName = profile.UserName;
            PhoneNumber = profile.PhoneNumber;
            DepartmentName = profile.DepartmentName;
            DeptCode = profile.DeptCode;
        }
    }

    public class WorkstationDto
    {
        /// <summary>접속 서버.</summary>
        [JsonProperty("corpCd")]
        public string Server { get; set; }

        /// <summary>사번.</summary>
        [JsonProperty("orgNo")]
        public string EmployeeNo { get; set; }

        /// <summary>이름.</summary>
        [JsonProperty("userNm")]
        public string UserName { get; set; }

        /// <summary>로그인 시 선택한 워크스테이션.</summary>
        [JsonProperty("workstationList")]
        public List<WorkstationInfo> Workstation { get; set; } = new List<WorkstationInfo>();
    }


    public class WorkstationInfo
    {
        [JsonProperty("wkstnRef")]
        public string WorkstationRef { get; set; }
        /// <summary>워크스테이션 코드.</summary>
        [JsonProperty("wkstnCd")]
        public string WorkstationCode { get; set; }
        /// <summary>워크스테이션 이름.</summary>
        [JsonProperty("wkstnNm")]
        public string WorkstationName { get; set; }
        /// <summary>워크스테이션 사용 여부.</summary>
        [JsonProperty("useYn")]
        public string Usable { get; set; }
        /// <summary>부서코드.</summary>
        [JsonProperty("deptCd")]
        public string DepartmentCode { get; set; }
        /// <summary>부서명.</summary>
        [JsonProperty("deptNm")]
        public string DepartmentName { get; set; }
        /// <summary>라인 코드.</summary>
        [JsonProperty("lineCd")]
        public string LineCode { get; set; }
        /// <summary>라인 이름.</summary>
        [JsonProperty("lineNm")]
        public string LineName { get; set; }


    }
}
