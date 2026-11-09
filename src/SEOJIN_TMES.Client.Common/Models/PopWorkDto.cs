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
        [JsonProperty("sysObjId")] public string WorkOrderRef { get; set; }
        /// <summary>작업지시번호 (예: WO_260608-01).</summary>
        [JsonProperty("woNo")] public string WorkOrderNo { get; set; }

        /// <summary>생산품목코드 (예: ITEM_01).</summary>
        [JsonProperty("itemCd")] public string ItemCode { get; set; }

        /// <summary>생산품목 시리얼번호 (예: SER-01).</summary>
        [JsonProperty("sn")] public string SerialNo { get; set; }

        /// <summary>라우팅 그룹 (예: MAIN_01).</summary>
        [JsonProperty("routingGroupCd")] public string RoutingGroup { get; set; }

        /// <summary>라우팅 공정 (예: OP_10).</summary>
        [JsonProperty("routingCd")] public string RoutingOperation { get; set; }

        /// <summary>페이지 공정 (예: 0010).</summary>
        [JsonProperty("procCd")] public string PageOperation { get; set; }

        /// <summary>페이지 공정 시작시각. 아직 시작 전이면 null.</summary>
        [JsonProperty("trackInDtm")] public DateTime? PageStartTime { get; set; }

        /// <summary>페이지 공정 종료시각. 진행 중이면 null.</summary>
        [JsonProperty("trackOutDtm")] public DateTime? PageEndTime { get; set; }

        /// <summary>페이지 공정 가동시간.</summary>
        [JsonProperty("actlWorkDurSec")] public TimeSpan? RunTime { get; set; }

        /// <summary>페이지 공정 비가동시간.</summary>
        [JsonProperty("dttmDurSec")] public TimeSpan? IdleTime { get; set; }

        /// <summary>페이지 공정 작업시간.</summary>
        [JsonProperty("totalWorkDurSec")] public TimeSpan? WorkTime { get; set; }

        /// <summary>작업 중인 워크스테이션 코드 (예: Workstation01).</summary>
        public string WorkstationCode { get; set; }

        /// <summary>워크스테이션 명 (예: 워크스테이션01).</summary>
        public string WorkstationName { get; set; }

        /// <summary>공정 진행 상태 문구. 서버가 준 문자열을 그대로 하단 띠에 표시한다(예: 공정 진행중).</summary>
        [JsonProperty("workStatus")] public string ProgressState { get; set; }

        /// <summary>
        /// 화면 좌측에 띄울 작업 표준/도면 PDF의 경로 — <b>서버가 내려주는 값</b>이다.
        /// SMB 공유 폴더 루트(App.config의 <c>DocumentShare.RootPath</c>) 기준 상대 경로를 기본으로 하며
        /// (예: <c>2026\ITEM_01\OP_10.pdf</c>), 전체 UNC 경로(<c>\\서버\공유\...</c>)를 내려줘도 그대로 사용한다.
        /// 비어 있으면 도면 영역에 "표시할 도면이 없습니다" 안내만 나온다.
        /// </summary>
        public string DrawingFilePath { get; set; }

        //[JsonProperty("equipmentList")] 
        //public List<PopEquipmentDto> EquipmentList { get; set; } = new List<PopEquipmentDto>();
        //[JsonProperty("inputMaterialList")] 
        //public List<PopInputMaterialDto> InputMaterialList { get; set; } = new List<PopInputMaterialDto>();
        //[JsonProperty("bomInspSpecList")] 
        //public List<PopBomSpecDto> BomInspSpecList { get; set; } = new List<PopBomSpecDto>();
        //[JsonProperty("inspChkItemList")] 
        //public List<PopInspectionSpecDto> InspChkItemList { get; set; } = new List<PopInspectionSpecDto>();



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

        /// <summary>자재 품명(서버 inputItemNm) — 화면 표시용.</summary>
        public string MaterialName { get; set; }

        /// <summary>품목코드 — 시리얼번호 입력 팝업에 함께 보여준다 (예: PN-010101010-11).</summary>
        public string ItemCode { get; set; }

        /// <summary>투입 수량. 시리얼번호 검사 대상이면 이 수량만큼 시리얼번호를 받는다.</summary>
        public int InputQty { get; set; }

        /// <summary>투입 단위 (예: EA).</summary>
        public string InputUnit { get; set; }

        /// <summary>투입 위치(서버 indctLoc).</summary>
        public string InputPosition { get; set; }

        /// <summary>자재 그룹(서버 inputItemGrp) — 서버 값을 그대로 보여 준다.</summary>
        public string MaterialGroup { get; set; }

        /// <summary>시리얼번호 검사 대상 여부.</summary>
        public bool SerialCheckRequired { get; set; }

        /// <summary>입력이 끝난 시리얼번호 목록. 검사 대상이 아니면 비어 있다.</summary>
        public List<string> SerialNumbers { get; set; } = new List<string>();

        /// <summary>품목번호 확인 대상 여부(서버 itemCdVrfcYn). 시리얼 확인과 별개로 서버가 공정의 할 일로 센다.</summary>
        public bool ItemCodeCheckRequired { get; set; }

        /// <summary>
        /// 품목번호 입력이 끝났는지 — 서버가 내려준 값(itemCdInputCompleted)이고, 화면이 판정하지 않는다.
        /// 시리얼 저장이 품목번호(inputtedItemCd)를 함께 보내므로, 그 저장이 성공하면 화면도 완료로 바꿔 둔다.
        /// </summary>
        public bool ItemCodeInputCompleted { get; set; }

        /// <summary>
        /// 참조 항목인지(서버 refYn=Y). 참조 자재는 작업자가 입력하지 않는다 —
        /// 화면은 OK로 칠하고, 입력 팝업을 열지 않으며, 완료 판단에서도 끝난 것으로 본다.
        /// </summary>
        public bool IsReference { get; set; }

        /// <summary>작업자가 입력해야 할 것이 있는 자재인지(시리얼 확인 또는 품목번호 확인 대상, 참조 자재 제외).</summary>
        public bool HasInputItem => !IsReference && (SerialCheckRequired || ItemCodeCheckRequired);

        /// <summary>이 자재의 입력이 모두 끝났는지 — 시리얼(대상이면) · 품목번호(대상이면) 둘 다. 참조 자재는 늘 끝난 것으로 본다.</summary>
        public bool IsInputCompleted => IsReference || (IsSerialInputCompleted && (!ItemCodeCheckRequired || ItemCodeInputCompleted));

        /// <summary>[투입자재] 그리드의 "시리얼번호 검사여부" 표시 문구.</summary>
        public string SerialCheckText => SerialCheckRequired ? "검사필요" : "검사불필요";

        /// <summary>
        /// 품목코드만 받는 자재인지(시리얼 확인 N · 품목코드 확인 Y). 입력 팝업이 시리얼 대신 품목코드 한 줄을 받는다.
        /// 시리얼 확인 대상이면 시리얼 저장이 품목코드도 함께 보내므로 시리얼 입력으로 처리한다.
        /// </summary>
        public bool IsItemCodeInputOnly => !SerialCheckRequired && ItemCodeCheckRequired;

        /// <summary>
        /// [투입자재] 그리드의 "시리얼번호 입력상태" 표시 문구.
        /// 입력 대상이 아니면 "-", 다 입력했으면 "완료", 아니면 "미완료".
        /// 품목코드만 받는 자재는 서버가 내려준 품목코드 완료 여부를 보여 준다.
        /// </summary>
        public string SerialInputStateText
        {
            get
            {
                if (IsReference) return "-";
                if (IsItemCodeInputOnly) return ItemCodeInputCompleted ? "완료" : "미완료";
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

        /// <summary>자재 품명(서버 inputItemNm) — 화면 표시용.</summary>
        public string MaterialName { get; set; }

        /// <summary>구매 계획 방식(서버 prchsPlanMth: null / MRP / ROP / 빈 문자열) — 서버 값을 그대로 보여 준다.</summary>
        public string PurchasePlanMethod { get; set; }

        /// <summary>투입위치(서버 indctLoc). 화면 표시용이다.</summary>
        public string InputPosition { get; set; }

        /// <summary>
        /// BOM 스팩 순번(서버 seq). 결과 저장 URL에서 행을 찾는 키로 쓴다 —
        /// 예전에는 이 값을 <see cref="InputPosition"/>에 담았는데, 투입위치를 따로 받으면서 분리했다.
        /// </summary>
        public int Seq { get; set; }

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

        public string EquipmentCode { get; set; }

        /// <summary>
        /// 참조 항목인지(서버 refYn=Y). 참조 줄은 체결 · 입력 대상이 아니다 —
        /// 화면은 OK로 칠하고, 설비 결과를 채울 행에서 건너뛰며, 결과값 칸 편집도 막는다.
        /// </summary>
        public bool IsReference { get; set; }
    }

    public class PopBomSpecResultDto
    {
        /// <summary>투입자재 코드.</summary>
        public string eqpCd { get; set; }

        /// <summary>토크 결과값 — 연결 장비에서 받거나(자동) 수동 렌치 모드에서 직접 입력한다. 미측정이면 null.</summary>
        public decimal? torqueRsltVl { get; set; }

        /// <summary>각도 하한값.</summary>
        public decimal? angleRsltVl { get; set; }
    }

    /// <summary>
    /// [검사 스팩] 탭의 한 줄 — 기준정보에 등록된 검사 항목 하나.
    /// 결과상태/결과값은 작업자가 화면에서 직접 입력한다.
    /// </summary>
    public class PopInspectionSpecDto
    {
        public string InspectionItemCode { get; set; }
        /// <summary>검사 스팩명.</summary>
        public string InspectionSpecName { get; set; }

        /// <summary>투입위치(서버 indctLoc).</summary>
        public string InputPosition { get; set; }

        /// <summary>검사 스팩 비고(작업자 안내 문구).</summary>
        public string InspectionRemark { get; set; }

        /// <summary>정량 검사의 서버 코드. 이 값이 아니면 모두 정성으로 본다.</summary>
        public const string MethodQuantitative = "QNT";

        /// <summary>
        /// 검사방법 — 서버가 준 코드 그대로(QNT 정량 / 그 외 정성).
        /// 화면의 "정량/정성" 문구는 그리드가 화면 언어로 바꿔 보여 주므로 여기에는 번역 문구를 넣지 않는다.
        /// </summary>
        public string InspectionMethod { get; set; }

        /// <summary>판정 값 — 서버 코드 그대로 쓴다(합격 OK / 불합격 NG). 화면 콤보와 저장 Body가 같은 값을 쓴다.</summary>
        public const string ResultOk = "OK";
        public const string ResultNg = "NG";

        /// <summary>결과상태 — <see cref="ResultOk"/> / <see cref="ResultNg"/>. 미입력이면 비어 있다.</summary>
        public string ResultState { get; set; }

        /// <summary>최소값(서버 lsl). 정량 검사의 기준 범위이며 정성 검사는 비어 있다.</summary>
        public decimal? InspectionLower { get; set; }

        /// <summary>최대값(서버 usl).</summary>
        public decimal? InspectionUpper { get; set; }

        /// <summary>타겟값(서버 target).</summary>
        public decimal? InspectionTarget { get; set; }

        /// <summary>결과값. 정량 검사에서만 숫자를 받는다(정성은 비워 둔다).</summary>
        public string ResultValue { get; set; }

        /// <summary>검사 결과 비고 — 작업자가 화면에서 직접 입력한다.</summary>
        public string InspectionResultRemark { get; set; }

        /// <summary>
        /// 결과값 기준 문자(서버 stdLtl, 예: 저항 측정의 "OL"). 작업자가 넣은 결과값이 이 문자와 같으면
        /// 화면이 판정을 OK로 채운다(공백 제거 · 대소문자 무시 — PopScreen.IsStdLtlMatched). 없으면 비교하지 않는다.
        /// </summary>
        public string InspectionStdLtl { get; set; }

        /// <summary>검사방법이 정량(QNT)인지 — 결과값 입력 · 전송 여부를 가른다.</summary>
        public bool IsQuantitative =>
            string.Equals(InspectionMethod, MethodQuantitative, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// [검사 스팩] 탭 한 행의 결과 저장 요청 본문.
    /// 정량 검사는 판정(<see cref="inspRsltVl"/>) · 결과값(<see cref="ResultValue"/>) · 비고를,
    /// 정성 검사는 판정 · 비고를 보낸다 — 정성은 <see cref="ResultValue"/>를 null로 두면 Body에서 빠진다.
    ///
    /// 검사 항목 코드는 이 클래스에 두지 않는다 — 저장 URL 경로(<c>insp-chk-results/{코드}</c>)로만 보내고
    /// Body에는 담지 않기 위해서다(IPopApiClient.SaveInspectionResultAsync가 인자로 따로 받는다).
    /// TODO: 직렬화 필드명은 가정값 — 서버 스펙 확정 시 [JsonProperty]로 맞출 것.
    /// </summary>
    public class PopInspectionResult
    {
        /// <summary>판정(결과상태) — OK / NG 만 보낸다.</summary>
        public string jgmtRslt { get; set; } = string.Empty;

        /// <summary>결과값. 정량 검사에서만 보내고, 정성 검사는 null이라 Body에서 빠진다.</summary>
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string inspRsltVl { get; set; } = string.Empty;

        /// <summary>작업자가 적은 검사 결과 비고. 정량 · 정성 모두 보낸다(미입력은 빈 문자열).</summary>
        public string inspRemark { get; set; } = string.Empty;
    }

    /// <summary>
    /// [연결 장비] 탭의 한 줄 — 현재 워크스테이션에 물려 있는 장비 하나.
    /// 장비 기준정보(코드/명/유형/가동범위)는 서버에서 오고,
    /// 통신상태/장비상태는 클라이언트의 Modbus 연결 상태로 실시간 갱신된다.
    /// </summary>
    public class PopEquipmentDto
    {
        public int Id { get; set; }
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

        /// <summary>장비상태 — 가동가능/사용불가.</summary>
        public string EquipmentModel { get; set; }

        /// <summary>가동범위 하한값.</summary>
        public decimal? RangeLower { get; set; }

        /// <summary>가동범위 상한값.</summary>
        public decimal? RangeUpper { get; set; }
    }

    /// <summary>
    /// 이상 알람 유형 기준정보 한 건 — <c>GET api/v1.0/md/abnormal-alarm-types</c> 응답 항목.
    /// &lt;알람 조회 / 등록&gt; 팝업에서 타일 하나로 보여 주고, 고른 타일의 유형·코드를 그대로 등록에 쓴다.
    /// 응답 필드명은 조회 조건(쿼리) 이름과 같다고 보고 맞췄다 — 서버 응답이 다르면 JsonProperty를 고칠 것.
    /// </summary>
    public class AbnormalAlarmTypeDto
    {
        [JsonProperty("corpCd")] public string CorpCode { get; set; }

        /// <summary>알람 구분 — EQP(설비) / QLT(품질) / MAT(자재) / PRD(생산) / EHS(안전·환경) / SYS(시스템).</summary>
        [JsonProperty("alrmType")] public string AlarmType { get; set; }

        /// <summary>알람 코드(예: ALM001).</summary>
        [JsonProperty("alrmTypeCd")] public string AlarmTypeCode { get; set; }

        /// <summary>알람 명 — 타일에 크게 보이는 조치 내용.</summary>
        [JsonProperty("alrmTypeNm")] public string AlarmTypeName { get; set; }

        /// <summary>알람 등급 — P1(긴급) / P2(주요) / P3(경미) / P4(정보).</summary>
        [JsonProperty("alrmGrd")] public string AlarmGrade { get; set; }

        [JsonProperty("useYn")] public string UseYn { get; set; }
    }

    /// <summary>
    /// 이상 알람 유형 조회 조건. 회사 코드만 필수이고, 나머지는 비워 두면 그 조건 없이(전체) 조회한다.
    /// </summary>
    public class AbnormalAlarmTypeQuery
    {
        public string CorpCode { get; set; }
        public string AlarmType { get; set; }
        public string AlarmTypeCode { get; set; }
        public string AlarmTypeName { get; set; }
        public string AlarmGrade { get; set; }
        public string UseYn { get; set; }
    }

    /// <summary>
    /// 이상 알람 등록 본문 — <c>POST api/v1.0/pop/{sn}/{subRpRef}/abnormal-alarms</c>.
    /// 공정·작업지시는 URL 경로와 쿼리로 가므로 본문에는 알람 유형과 알람 코드 두 가지만 담는다.
    /// 속성 이름을 서버 필드명 그대로 둔다(공정 중지 요청 <c>dttmCd</c> · <c>remark</c>와 같은 방식) —
    /// 직렬화 설정과 상관없이 본문이 항상 <c>{"alrmType":..., "alrmTypeCd":...}</c>로 나가게 하기 위해서다.
    /// </summary>
    public class AbnormalAlarmRequest
    {
        /// <summary>알람 유형(대분류) — EQP / QLT / MAT / PRD / EHS / SYS.</summary>
        public string alrmType { get; set; }

        /// <summary>알람 코드.</summary>
        public string alrmTypeCd { get; set; }
    }

    /// <summary>
    /// 투입자재 시리얼번호 저장 요청 — &lt;투입자재 정보 입력&gt; 팝업의 [저장].
    /// 저장 판정(중복/유효성)은 서버가 하고, 클라이언트는 입력값을 모아 보내기만 한다.
    /// </summary>
    public class PopMaterialSerialSaveRequest
    {
        //public string CorpCode { get; set; }
        //public string WorkOrderNo { get; set; }
        //public string SerialNo { get; set; }
        //public string RoutingGroup { get; set; }
        //public string RoutingGroupRef { get; set; }
        //public string RoutingOperation { get; set; }
        public string inputtedItemCd { get; set; }
        public List<string> inputtedSerialList { get; set; } = new List<string>();
    }
}
