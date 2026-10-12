using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SEOJIN_TMES.Client.Common.Models
{
    /// <summary>
    /// POP 본 화면이 다루는 "작업 대상 한 건"을 가리키는 키.
    /// 작업지시 + 생산품목 시리얼 + 라우팅(그룹/공정) + 페이지 공정까지 있어야 한 공정이 유일하게 정해지므로,
    /// 탭 4개의 조회 API가 모두 이 키를 조건으로 받는다.
    /// </summary>
    public class PopProcessKey
    {
        public string CorpCode { get; set; }
        public string WorkstationRef { get; set; }
        public string WorkOrderNo { get; set; }
        public string SerialNo { get; set; }
        public string RoutingGroup { get; set; }
        public string RoutingGroupRef { get; set; }
        public string RoutingOperation { get; set; }
        public string PageOperation { get; set; }
        public string SubRoutingRef { get; set; }
    }

    /// <summary>
    /// POP 본 화면 우측 &lt;진행 중 공정 정보&gt; 한 건.
    /// 시간/상태 값은 전부 서버가 계산해서 내려주며, 클라이언트는 표시만 한다.
    /// TODO: 실제 서버 응답 필드명이 확정되면 속성명을 맞추거나 JsonProperty로 매핑할 것.
    /// </summary>
    public class PopProcessInfoDto
    {
        /// <summary>작업지시번호 (예: WO_260608-01).</summary>
        public string WorkOrderNo { get; set; }

        /// <summary>생산품목코드 (예: ITEM_01).</summary>
        public string ItemCode { get; set; }

        /// <summary>생산품목 시리얼번호 (예: SER-01).</summary>
        public string SerialNo { get; set; }

        /// <summary>라우팅 그룹 (예: MAIN_01).</summary>
        public string RoutingGroup { get; set; }

        /// <summary>라우팅 공정 (예: OP_10).</summary>
        public string RoutingOperation { get; set; }

        /// <summary>페이지 공정 (예: 0010).</summary>
        public string PageOperation { get; set; }

        /// <summary>페이지 공정 시작시각. 아직 시작 전이면 null.</summary>
        public DateTime? PageStartTime { get; set; }

        /// <summary>페이지 공정 종료시각. 진행 중이면 null.</summary>
        public DateTime? PageEndTime { get; set; }

        /// <summary>페이지 공정 가동시간.</summary>
        public TimeSpan? RunTime { get; set; }

        /// <summary>페이지 공정 비가동시간.</summary>
        public TimeSpan? IdleTime { get; set; }

        /// <summary>페이지 공정 작업시간.</summary>
        public TimeSpan? WorkTime { get; set; }

        /// <summary>작업 중인 워크스테이션 코드 (예: Workstation01).</summary>
        public string WorkstationCode { get; set; }

        /// <summary>워크스테이션 명 (예: 워크스테이션01).</summary>
        public string WorkstationName { get; set; }

        /// <summary>공정 진행 상태 문구. 서버가 준 문자열을 그대로 하단 띠에 표시한다(예: 공정 진행중).</summary>
        public string ProgressState { get; set; }

        /// <summary>
        /// 화면 좌측에 띄울 작업 표준/도면 PDF의 경로 — <b>서버가 내려주는 값</b>이다.
        /// SMB 공유 폴더 루트(App.config의 <c>DocumentShare.RootPath</c>) 기준 상대 경로를 기본으로 하며
        /// (예: <c>2026\ITEM_01\OP_10.pdf</c>), 전체 UNC 경로(<c>\\서버\공유\...</c>)를 내려줘도 그대로 사용한다.
        /// 비어 있으면 도면 영역에 "표시할 도면이 없습니다" 안내만 나온다.
        /// </summary>
        public string DrawingFilePath { get; set; }

        [JsonProperty("equipmentList")] 
        public List<PopEquipmentDto> EquipmentList { get; set; } = new List<PopEquipmentDto>();
        [JsonProperty("inputMaterialList")] 
        public List<PopInputMaterialDto> InputMaterialList { get; set; } = new List<PopInputMaterialDto>();
        [JsonProperty("bomInspSpecList")] 
        public List<PopBomSpecDto> BomInspSpecList { get; set; } = new List<PopBomSpecDto>();
        [JsonProperty("inspChkItemList")] 
        public List<PopInspectionSpecDto> InspChkItemList { get; set; } = new List<PopInspectionSpecDto>();



        /// <summary>화면 우측 아래 워크스테이션 띠에 쓸 표기: "코드(명)".</summary>
        public string WorkstationDisplay =>
            string.IsNullOrEmpty(WorkstationName) ? WorkstationCode : $"{WorkstationCode}({WorkstationName})";
    }

    /// <summary>
    /// [투입자재] 탭의 한 줄. 이 페이지 공정에서 투입해야 하는 자재 한 품목이다.
    /// 시리얼번호 입력이 필요한 자재는 <see cref="SerialCheckRequired"/>가 true이며,
    /// 그 자재만 &lt;투입자재 정보 입력&gt; 팝업으로 시리얼번호를 받는다.
    /// </summary>
    public class PopInputMaterialDto
    {
        /// <summary>투입자재 코드 (예: M-00001-01010).</summary>
        public string MaterialCode { get; set; }

        /// <summary>품목코드 — 시리얼번호 입력 팝업에 함께 보여준다 (예: PN-010101010-11).</summary>
        public string ItemCode { get; set; }

        /// <summary>투입 수량. 시리얼번호 검사 대상이면 이 수량만큼 시리얼번호를 받는다.</summary>
        public int InputQty { get; set; }

        /// <summary>투입 단위 (예: EA).</summary>
        public string InputUnit { get; set; }

        /// <summary>투입 위치.</summary>
        public string InputPosition { get; set; }

        /// <summary>시리얼번호 검사 대상 여부.</summary>
        public bool SerialCheckRequired { get; set; }

        /// <summary>입력이 끝난 시리얼번호 목록. 검사 대상이 아니면 비어 있다.</summary>
        public List<string> SerialNumbers { get; set; } = new List<string>();

        /// <summary>[투입자재] 그리드의 "시리얼번호 검사여부" 표시 문구.</summary>
        public string SerialCheckText => SerialCheckRequired ? "검사필요" : "검사불필요";

        /// <summary>
        /// [투입자재] 그리드의 "시리얼번호 입력상태" 표시 문구.
        /// 검사 대상이 아니면 "-", 수량만큼 다 채웠으면 "완료", 아니면 "미완료".
        /// </summary>
        public string SerialInputStateText
        {
            get
            {
                if (!SerialCheckRequired) return "-";
                return IsSerialInputCompleted ? "완료" : "미완료";
            }
        }

        /// <summary>투입 수량만큼 시리얼번호가 모두 채워졌는지.</summary>
        public bool IsSerialInputCompleted
        {
            get
            {
                if (!SerialCheckRequired) return true;
                if (SerialNumbers == null) return false;

                var filled = 0;
                foreach (var serial in SerialNumbers)
                {
                    if (!string.IsNullOrWhiteSpace(serial)) filled++;
                }
                return filled >= InputQty;
            }
        }
    }

    /// <summary>
    /// [BOM 스팩] 탭의 한 줄 — 기준정보에 등록된 토크 작업 스펙 하나.
    /// 하한/상한 범위는 기준정보 값이고, <see cref="TorqueResult"/>만 실적(설비 수신값 또는 수동 입력값)이다.
    /// 합/불 판정은 서버가 하므로 클라이언트는 값만 담아 보낸다.
    /// </summary>
    public class PopBomSpecDto
    {
        /// <summary>투입자재 코드.</summary>
        public string MaterialCode { get; set; }

        /// <summary>투입위치.</summary>
        public string InputPosition { get; set; }

        /// <summary>BOM 스팩명 (예: 토크작업01).</summary>
        public string BomSpecName { get; set; }

        /// <summary>스팩 유형 (예: 토크 작업).</summary>
        public string SpecType { get; set; }

        /// <summary>토크 하한값.</summary>
        public decimal? TorqueLower { get; set; }

        /// <summary>토크 상한값.</summary>
        public decimal? TorqueUpper { get; set; }

        /// <summary>토크 값.</summary>
        public decimal? TorqueTarget { get; set; }

        /// <summary>토크 단위 (예: lb).</summary>
        public string TorqueUnit { get; set; }

        /// <summary>토크 결과값 — 연결 장비에서 받거나(자동) 수동 렌치 모드에서 직접 입력한다. 미측정이면 null.</summary>
        public decimal? TorqueResult { get; set; }

        /// <summary>각도 하한값.</summary>
        public decimal? AngleLower { get; set; }

        /// <summary>각도 상한값.</summary>
        public decimal? AngleUpper { get; set; }

        /// <summary>각도 값.</summary>
        public decimal? AngleTarget { get; set; }

        /// <summary>각도 단위 (예: lb).</summary>
        public string AngleUnit { get; set; }

        /// <summary>각도 결과값 — 연결 장비에서 받거나(자동) 수동 렌치 모드에서 직접 입력한다. 미측정이면 null.</summary>
        public decimal? AngleResult { get; set; }

        /// <summary>결과값을 수동으로 입력했는지(수동 렌치). 서버 저장 시 입력 경로를 구분하기 위한 값.</summary>
        public bool IsManualInput { get; set; }
    }

    /// <summary>
    /// [검사 스팩] 탭의 한 줄 — 기준정보에 등록된 검사 항목 하나.
    /// 결과상태/결과값은 작업자가 화면에서 직접 입력한다.
    /// </summary>
    public class PopInspectionSpecDto
    {
        /// <summary>검사 스팩명.</summary>
        public string InspectionSpecName { get; set; }

        /// <summary>검사 스팩 비고(작업자 안내 문구).</summary>
        public string InspectionRemark { get; set; }

        /// <summary>검사방법 — 정량/정성.</summary>
        public string InspectionMethod { get; set; }

        /// <summary>결과상태 — 합격/불합격. 미입력이면 비어 있다.</summary>
        public string ResultState { get; set; }

        /// <summary>결과값. 정량 검사에서만 숫자를 받는다(정성은 비워 둔다).</summary>
        public string ResultValue { get; set; }

        /// <summary>검사방법이 정량인지(결과값 입력을 요구할지) 판단용.</summary>
        public bool IsQuantitative =>
            string.Equals(InspectionMethod, "정량", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// [연결 장비] 탭의 한 줄 — 현재 워크스테이션에 물려 있는 장비 하나.
    /// 장비 기준정보(코드/명/유형/가동범위)는 서버에서 오고,
    /// 통신상태/장비상태는 클라이언트의 Modbus 연결 상태로 실시간 갱신된다.
    /// </summary>
    public class PopEquipmentDto
    {
        public int Id { get; set; } = 0;
        /// <summary>장비코드 — App.config의 EquipmentId와 같은 값이어야 실시간 연결상태가 붙는다.</summary>
        public string EquipmentCode { get; set; }

        /// <summary>장비명 (예: TORQUE_01).</summary>
        public string EquipmentName { get; set; }

        public string EquipmentIp { get; set; }

        public string EquipmentPort { get; set; }

        /// <summary>장비 비고.</summary>
        public string EquipmentRemark { get; set; }

        /// <summary>장비유형 (예: 토크장비).</summary>
        public string EquipmentType { get; set; }

        /// <summary>통신상태 — 연결 성공/연결 실패 등. 클라이언트 Modbus 연결 상태로 채운다.</summary>
        public string CommunicationState { get; set; }

        /// <summary>장비상태 — 가동가능/사용불가.</summary>
        public string EquipmentState { get; set; }

        /// <summary>가동범위 하한값.</summary>
        public decimal? RangeLower { get; set; }

        /// <summary>가동범위 상한값.</summary>
        public decimal? RangeUpper { get; set; }
    }

    /// <summary>
    /// &lt;관리자 알람 유형 선택&gt; 팝업의 알람 유형 하나(기준정보).
    /// 유형마다 함께 보내야 하는 대상이 달라서(<see cref="TargetKind"/>) 팝업의 상세 입력 영역이 바뀐다.
    /// </summary>
    public class AdminAlarmTypeDto
    {
        /// <summary>알람 유형 코드(서버 전송용).</summary>
        public string Code { get; set; }

        /// <summary>알람 유형 명(콤보에 표시).</summary>
        public string Name { get; set; }

        /// <summary>
        /// 이 유형이 요구하는 대상 종류.
        /// <see cref="AdminAlarmTargetKind.Material"/>이면 투입자재 목록에서,
        /// <see cref="AdminAlarmTargetKind.Equipment"/>이면 연결 장비 목록에서 대상을 고르게 한다.
        /// </summary>
        public AdminAlarmTargetKind TargetKind { get; set; }

        /// <summary>대상 입력란에 붙일 라벨(예: 결품 자재 코드). 비어 있으면 화면 기본 문구를 쓴다.</summary>
        public string TargetCaption { get; set; }

        /// <summary>콤보에는 유형 명만 보여 준다.</summary>
        public override string ToString() => Name;
    }

    /// <summary>관리자 알람 유형이 함께 요구하는 대상의 종류.</summary>
    public enum AdminAlarmTargetKind
    {
        /// <summary>대상 없음 — 비고만 적어 보낸다.</summary>
        None,

        /// <summary>투입자재 한 건(예: 자재 결품).</summary>
        Material,

        /// <summary>연결 장비 한 대(예: 설비 이상).</summary>
        Equipment
    }

    /// <summary>
    /// 투입자재 시리얼번호 저장 요청 — &lt;투입자재 정보 입력&gt; 팝업의 [저장].
    /// 저장 판정(중복/유효성)은 서버가 하고, 클라이언트는 입력값을 모아 보내기만 한다.
    /// </summary>
    public class PopMaterialSerialSaveRequest
    {
        public string WorkOrderNo { get; set; }
        public string SerialNo { get; set; }
        public string RoutingGroup { get; set; }
        public string RoutingOperation { get; set; }
        public string PageOperation { get; set; }
        public string MaterialCode { get; set; }
        public List<string> SerialNumbers { get; set; } = new List<string>();
    }
}
