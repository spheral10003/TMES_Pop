using System;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace SEOJIN_TMES.Client.Common.Languages
{

    /// <summary>언어 콤보 항목 — 표시 문구와 문화권 코드 한 쌍.</summary>
    public class LanguageOption
    {
        /// <summary>
        /// 언어 콤보(로그인 · 메인 화면 공용) 목록.
        /// 표시 문구는 화면 언어와 상관없이 각 나라 말로 고정한다 — 읽을 수 없는 언어로 바뀌었어도 자기 나라 말은 찾을 수 있어야 한다.
        ///
        /// 언어를 추가할 때는 <b>세 곳을 함께</b> 갖춰야 한다 —
        /// 여기 한 줄 + Language.{두 글자 코드}.resx + Images\flag\{두 글자 코드}.png.
        /// </summary>
        public static readonly LanguageOption[] All =
        {
            new LanguageOption("한국어", "ko-KR"),
            new LanguageOption("English", "en-US"),
            new LanguageOption("Tiếng Việt", "vi-VN")
        };

        /// <summary>문화권 코드에 해당하는 항목 위치. 못 찾으면 0(한국어).</summary>
        public static int IndexOf(string culture)
        {
            if (string.IsNullOrEmpty(culture) || culture.Length < 2) return 0;

            var twoLetter = culture.Substring(0, 2);
            for (var i = 0; i < All.Length; i++)
            {
                if (All[i].FlagCode.Equals(twoLetter, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return 0;
        }


        public string Display { get; }
        public string Culture { get; }

        /// <summary>국기 파일 이름에 쓰는 두 글자 코드("ko-KR" → "ko").</summary>
        public string FlagCode { get; }

        public LanguageOption(string display, string culture)
        {
            Display = display;
            Culture = culture;
            FlagCode = culture.Substring(0, 2);
        }
    }



    /// <summary>
    /// Strings.resx(기본=한국어) / Strings.en.resx(영어) / Strings.vi.resx(베트남어) 위성 리소스에 대한 강타입 접근자.
    /// Visual Studio의 resx 디자이너 코드생성기 없이 수동으로 작성되었으며,
    /// 새 문자열 키를 추가할 때는 Strings.resx / Strings.en.resx / Strings.vi.resx 세 파일과 이 클래스의 속성을 함께 갱신해야 한다.
    /// </summary>
    public static class Language
    {
        
        private static readonly ResourceManager ResourceManager =
            new ResourceManager("SEOJIN_TMES.Client.Common.Languages.Language", typeof(Language).GetTypeInfo().Assembly);

        private static string Get(string name) => ResourceManager.GetString(name, CultureInfo.CurrentUICulture);

        /// <summary>
        /// 스레드 문화권과 상관없이 지정한 문화권의 문구를 꺼낸다.
        /// 로그인 창처럼 "이 화면만 다른 언어로" 보여야 할 때 쓴다 — 전체 언어는 바꾸지 않는다.
        /// </summary>
        public static string GetString(string name, CultureInfo culture) => ResourceManager.GetString(name, culture);

        public static string Login_Title => Get(nameof(Login_Title));
        public static string Login_UserInfoGroup => Get(nameof(Login_UserInfoGroup));
        public static string Login_Server => Get(nameof(Login_Server));
        public static string Login_UserId => Get(nameof(Login_UserId));
        public static string Login_Password => Get(nameof(Login_Password));
        public static string Login_Workstation => Get(nameof(Login_Workstation));
        public static string Login_Connect => Get(nameof(Login_Connect));
        public static string Login_Close => Get(nameof(Login_Close));
        public static string Login_ValidationMessage => Get(nameof(Login_ValidationMessage));
        public static string Login_FailedMessage => Get(nameof(Login_FailedMessage));
        public static string Login_ConnectionErrorMessage => Get(nameof(Login_ConnectionErrorMessage));
        public static string Login_RememberId => Get(nameof(Login_RememberId));
        public static string Login_StationTitle => Get(nameof(Login_StationTitle));
        public static string Login_StationHint => Get(nameof(Login_StationHint));
        public static string Login_NoStation => Get(nameof(Login_NoStation));
        public static string Login_Language => Get(nameof(Login_Language));
        public static string Login_Business => Get(nameof(Login_Business));
        public static string Login_CorpGumi => Get(nameof(Login_CorpGumi));
        public static string Login_CorpHwaseong => Get(nameof(Login_CorpHwaseong));
        public static string Login_CorpYongin => Get(nameof(Login_CorpYongin));
        public static string Login_Exit => Get(nameof(Login_Exit));
        public static string Login_KeyHint => Get(nameof(Login_KeyHint));
        public static string Login_Online => Get(nameof(Login_Online));
        public static string Login_Offline => Get(nameof(Login_Offline));

        public static string MainForm_Title => Get(nameof(MainForm_Title));
        public static string MainForm_VersionFormat => Get(nameof(MainForm_VersionFormat));
        public static string MainForm_Welcome => Get(nameof(MainForm_Welcome));
        public static string MainForm_SessionRemainingHint => Get(nameof(MainForm_SessionRemainingHint));
        public static string MainForm_SessionExpired => Get(nameof(MainForm_SessionExpired));
        public static string MainForm_LanguageHint => Get(nameof(MainForm_LanguageHint));
        public static string MainForm_Minimize => Get(nameof(MainForm_Minimize));
        public static string MainForm_Maximize => Get(nameof(MainForm_Maximize));
        public static string MainForm_Restore => Get(nameof(MainForm_Restore));
        public static string MainForm_LogoutConfirm => Get(nameof(MainForm_LogoutConfirm));
        public static string MainForm_WelcomeHint => Get(nameof(MainForm_WelcomeHint));
        public static string MainForm_TileServer => Get(nameof(MainForm_TileServer));
        public static string MainForm_TileWorkstation => Get(nameof(MainForm_TileWorkstation));
        public static string MainForm_TileEquipment => Get(nameof(MainForm_TileEquipment));
        public static string MainForm_TileUser => Get(nameof(MainForm_TileUser));
        public static string MainForm_EquipmentCount => Get(nameof(MainForm_EquipmentCount));
        public static string MainForm_NoAlerts => Get(nameof(MainForm_NoAlerts));

        public static string Menu_MEIS => Get(nameof(Menu_MEIS));
        public static string Menu_WorkManagement => Get(nameof(Menu_WorkManagement));
        public static string Menu_POP => Get(nameof(Menu_POP));
        public static string Menu_WorkStatusInquiry => Get(nameof(Menu_WorkStatusInquiry));
        public static string Menu_ProductionStatusInquiry => Get(nameof(Menu_ProductionStatusInquiry));
        public static string Menu_WorkIntegratedOverview => Get(nameof(Menu_WorkIntegratedOverview));
        public static string Menu_WorkIntegratedStatusInquiry => Get(nameof(Menu_WorkIntegratedStatusInquiry));

        public static string Common_Connect => Get(nameof(Common_Connect));
        public static string Common_Disconnect => Get(nameof(Common_Disconnect));
        public static string Common_Refresh => Get(nameof(Common_Refresh));
        public static string Common_Logout => Get(nameof(Common_Logout));
        public static string Common_Close => Get(nameof(Common_Close));
        public static string Common_Confirm => Get(nameof(Common_Confirm));
        public static string Common_Cancel => Get(nameof(Common_Cancel));
        public static string Common_PleaseWait => Get(nameof(Common_PleaseWait));
        public static string Common_Loading => Get(nameof(Common_Loading));
        public static string Common_Notice => Get(nameof(Common_Notice));
        public static string Common_UnexpectedError => Get(nameof(Common_UnexpectedError));
        public static string Common_Search => Get(nameof(Common_Search));

        public static string Notification_Title => Get(nameof(Notification_Title));
        public static string Notification_UserName => Get(nameof(Notification_UserName));
        public static string Notification_Subject => Get(nameof(Notification_Subject));
        public static string Notification_ReadState => Get(nameof(Notification_ReadState));
        public static string Notification_ReadStateAll => Get(nameof(Notification_ReadStateAll));
        public static string Notification_ReadStateRead => Get(nameof(Notification_ReadStateRead));
        public static string Notification_ReadStateUnread => Get(nameof(Notification_ReadStateUnread));
        public static string Notification_ListTitle => Get(nameof(Notification_ListTitle));
        public static string Notification_MarkRead => Get(nameof(Notification_MarkRead));
        public static string Notification_SenderId => Get(nameof(Notification_SenderId));
        public static string Notification_Content => Get(nameof(Notification_Content));
        public static string Notification_SentAt => Get(nameof(Notification_SentAt));
        public static string Notification_SelectRow => Get(nameof(Notification_SelectRow));
        public static string Notification_AlreadyRead => Get(nameof(Notification_AlreadyRead));
        public static string Notification_CorpCd => Get(nameof(Notification_CorpCd));
        public static string Notification_IdntyTime => Get(nameof(Notification_IdntyTime));
        public static string Notification_CreatedAt => Get(nameof(Notification_CreatedAt));

        public static string Message_TitleInfo => Get(nameof(Message_TitleInfo));
        public static string Message_TitleQuestion => Get(nameof(Message_TitleQuestion));
        public static string Message_TitleSuccess => Get(nameof(Message_TitleSuccess));
        public static string Message_TitleWarning => Get(nameof(Message_TitleWarning));
        public static string Message_TitleError => Get(nameof(Message_TitleError));

        public static string Pop_Title => Get(nameof(Pop_Title));
        public static string Pop_Collapse => Get(nameof(Pop_Collapse));
        public static string Pop_Expand => Get(nameof(Pop_Expand));
        public static string Pop_ToggleInfo => Get(nameof(Pop_ToggleInfo));
        public static string Pop_ToggleTabs => Get(nameof(Pop_ToggleTabs));
        public static string Pop_EquipmentStatus => Get(nameof(Pop_EquipmentStatus));
        public static string Pop_Start => Get(nameof(Pop_Start));
        public static string Pop_Stop => Get(nameof(Pop_Stop));
        public static string Pop_Reset => Get(nameof(Pop_Reset));
        public static string Pop_EndProcess => Get(nameof(Pop_EndProcess));
        public static string Pop_RestartProcess => Get(nameof(Pop_RestartProcess));
        public static string Pop_AutoStarted => Get(nameof(Pop_AutoStarted));
        public static string Pop_AutoRestarted => Get(nameof(Pop_AutoRestarted));
        public static string Pop_AutoStartFailed => Get(nameof(Pop_AutoStartFailed));
        public static string Pop_AutoEnded => Get(nameof(Pop_AutoEnded));
        public static string Pop_ProcessEnded => Get(nameof(Pop_ProcessEnded));
        public static string Pop_ProcessMovedUnfinished => Get(nameof(Pop_ProcessMovedUnfinished));
        public static string Pop_ProcessIncompleteMoveConfirm => Get(nameof(Pop_ProcessIncompleteMoveConfirm));
        public static string Pop_InputNotAllowed => Get(nameof(Pop_InputNotAllowed));

        public static string Pop_WorkOrderList => Get(nameof(Pop_WorkOrderList));
        public static string Pop_EquipmentLog => Get(nameof(Pop_EquipmentLog));

        public static string ProductionStatus_Title => Get(nameof(ProductionStatus_Title));
        public static string ProductionStatus_StandardDate => Get(nameof(ProductionStatus_StandardDate));
        public static string ProductionStatus_FilterTitle => Get(nameof(ProductionStatus_FilterTitle));
        public static string WorkIntegratedStatus_Title => Get(nameof(WorkIntegratedStatus_Title));

        public static string UserProfile_Title => Get(nameof(UserProfile_Title));
        public static string UserProfile_EmployeeNo => Get(nameof(UserProfile_EmployeeNo));
        public static string UserProfile_UserName => Get(nameof(UserProfile_UserName));
        public static string UserProfile_Phone => Get(nameof(UserProfile_Phone));
        public static string UserProfile_Department => Get(nameof(UserProfile_Department));
        public static string UserProfile_Workstation => Get(nameof(UserProfile_Workstation));
        public static string UserProfile_Loading => Get(nameof(UserProfile_Loading));
        public static string UserProfile_LoadFailed => Get(nameof(UserProfile_LoadFailed));
        public static string WorkEntry_Title => Get(nameof(WorkEntry_Title));
        public static string WorkEntry_Search => Get(nameof(WorkEntry_Search));
        public static string WorkEntry_Work => Get(nameof(WorkEntry_Work));
        /// <summary>작업 진입 화면 — 내 워크스테이션 작업지시만 조회하는 체크박스.</summary>
        public static string WorkEntry_WorkStation => Get(nameof(WorkEntry_WorkStation));
        public static string WorkEntry_WorkOrderNo => Get(nameof(WorkEntry_WorkOrderNo));
        public static string WorkEntry_SerialNo => Get(nameof(WorkEntry_SerialNo));
        public static string WorkEntry_ListTitle => Get(nameof(WorkEntry_ListTitle));
        public static string WorkEntry_ConditionRequired => Get(nameof(WorkEntry_ConditionRequired));
        public static string WorkEntry_SelectWorkOrder => Get(nameof(WorkEntry_SelectWorkOrder));
        public static string WorkEntry_NoResult => Get(nameof(WorkEntry_NoResult));
        public static string WorkEntry_NoRouting => Get(nameof(WorkEntry_NoRouting));
        public static string WorkEntry_NoWorkstation => Get(nameof(WorkEntry_NoWorkstation));
        public static string WorkEntryConfirm_Title => Get(nameof(WorkEntryConfirm_Title));
        public static string WorkEntryConfirm_ListTitle => Get(nameof(WorkEntryConfirm_ListTitle));
        public static string WorkEntryConfirm_Move => Get(nameof(WorkEntryConfirm_Move));
        public static string WorkEntryConfirm_SelectProcess => Get(nameof(WorkEntryConfirm_SelectProcess));
        public static string WorkEntryConfirm_TabRouting => Get(nameof(WorkEntryConfirm_TabRouting));
        public static string WorkEntryConfirm_TabAllPages => Get(nameof(WorkEntryConfirm_TabAllPages));
        public static string WorkEntryConfirm_MaterialSearch => Get(nameof(WorkEntryConfirm_MaterialSearch));
        public static string WorkEntryConfirm_NoMaterialMatch => Get(nameof(WorkEntryConfirm_NoMaterialMatch));
        public static string Col_No => Get(nameof(Col_No));
        public static string Col_Description => Get(nameof(Col_Description));
        public static string Col_WorkOrderNo => Get(nameof(Col_WorkOrderNo));
        public static string Col_ItemCode => Get(nameof(Col_ItemCode));
        public static string Col_SerialNo => Get(nameof(Col_SerialNo));
        public static string Col_OrderQty => Get(nameof(Col_OrderQty));
        public static string Col_WorkQty => Get(nameof(Col_WorkQty));
        public static string Col_SalesOrderNo => Get(nameof(Col_SalesOrderNo));
        public static string Col_CustomerPoNo => Get(nameof(Col_CustomerPoNo));
        public static string Col_Fcid => Get(nameof(Col_Fcid));
        public static string Col_PlanStartDate => Get(nameof(Col_PlanStartDate));
        public static string Col_PlanEndDate => Get(nameof(Col_PlanEndDate));
        public static string Col_ActualStartDate => Get(nameof(Col_ActualStartDate));
        public static string Col_ActualEndDate => Get(nameof(Col_ActualEndDate));
        public static string Col_StatusName => Get(nameof(Col_StatusName));
        public static string Col_Remark => Get(nameof(Col_Remark));
        public static string Col_RoutingGroup => Get(nameof(Col_RoutingGroup));
        public static string Col_RoutingOperation => Get(nameof(Col_RoutingOperation));
        public static string Col_PageOperation => Get(nameof(Col_PageOperation));
        public static string Col_ProgressState => Get(nameof(Col_ProgressState));
        public static string Col_CorpCode => Get(nameof(Col_CorpCode));
        public static string Col_FormCode => Get(nameof(Col_FormCode));
        public static string Pop_EntryTarget => Get(nameof(Pop_EntryTarget));

        // POP 본 화면 (진행 중 공정 정보 + 하단 4개 탭)
        public static string Pop_PrevProcess => Get(nameof(Pop_PrevProcess));
        public static string Pop_NextProcess => Get(nameof(Pop_NextProcess));
        public static string Pop_StopProcess => Get(nameof(Pop_StopProcess));

        // 공정 진행 상태 — 서버가 내려주는 workStatus 코드(Ready/Progress/Stopped/Finished)의 표시 문구
        public static string ProgressState_Ready => Get(nameof(ProgressState_Ready));
        public static string ProgressState_Progress => Get(nameof(ProgressState_Progress));
        public static string ProgressState_Stopped => Get(nameof(ProgressState_Stopped));
        public static string ProgressState_Finished => Get(nameof(ProgressState_Finished));

        /// <summary>
        /// 서버가 준 진행 상태 코드를 화면 언어의 문구로 바꾼다. 코드 값 자체는 바꾸지 않으므로 상태 비교는 원래 코드로 한다.
        /// 문구가 없는 코드(서버에 새로 생긴 상태 등)는 받은 값을 그대로 돌려준다 — 빈칸으로 숨기면 원인을 찾기 어렵다.
        /// </summary>
        public static string ProgressStateText(string code)
        {
            // 화면의 상태 비교(PopScreen.IsProgressState)가 대소문자를 가리지 않으므로 여기서도 같게 맞춘다.
            switch (code?.Trim().ToUpperInvariant())
            {
                case "READY": return ProgressState_Ready;
                case "PROGRESS": return ProgressState_Progress;
                case "STOPPED": return ProgressState_Stopped;
                case "FINISHED": return ProgressState_Finished;
                // 작업지시 상태는 완료를 Completed로 내려준다(공정은 Finished) — 화면에는 같은 "완료".
                case "COMPLETED": return ProgressState_Finished;
                default: return code;
            }
        }

        public static string Pop_StartProcess => Get(nameof(Pop_StartProcess));
        public static string Pop_ProcessList => Get(nameof(Pop_ProcessList));
        public static string Pop_AdminAlarm => Get(nameof(Pop_AdminAlarm));
        public static string Pop_Worker => Get(nameof(Pop_Worker));
        public static string Pop_CurrentTime => Get(nameof(Pop_CurrentTime));
        public static string Pop_ProcessInfoTitle => Get(nameof(Pop_ProcessInfoTitle));
        public static string Pop_WorkOrderNo => Get(nameof(Pop_WorkOrderNo));
        public static string Pop_ItemCode => Get(nameof(Pop_ItemCode));
        public static string Pop_ProductSerialNo => Get(nameof(Pop_ProductSerialNo));
        public static string Pop_RoutingGroup => Get(nameof(Pop_RoutingGroup));
        public static string Pop_RoutingOperation => Get(nameof(Pop_RoutingOperation));
        public static string Pop_PageOperation => Get(nameof(Pop_PageOperation));
        public static string Pop_PageStartTime => Get(nameof(Pop_PageStartTime));
        public static string Pop_PageEndTime => Get(nameof(Pop_PageEndTime));
        public static string Pop_PageRunTime => Get(nameof(Pop_PageRunTime));
        public static string Pop_PageIdleTime => Get(nameof(Pop_PageIdleTime));
        public static string Pop_PageWorkTime => Get(nameof(Pop_PageWorkTime));
        public static string Pop_Drawing => Get(nameof(Pop_Drawing));
        public static string Pop_TabMaterial => Get(nameof(Pop_TabMaterial));
        public static string Pop_TabBomSpec => Get(nameof(Pop_TabBomSpec));
        public static string Pop_TabInspection => Get(nameof(Pop_TabInspection));
        public static string Pop_TabEquipment => Get(nameof(Pop_TabEquipment));
        public static string Pop_ManualWrench => Get(nameof(Pop_ManualWrench));
        public static string Pop_ResultInputColumn => Get(nameof(Pop_ResultInputColumn));
        public static string Pop_InputButton => Get(nameof(Pop_InputButton));
        public static string Pop_SerialNotRequired => Get(nameof(Pop_SerialNotRequired));
        public static string Pop_NoProcessList => Get(nameof(Pop_NoProcessList));
        public static string Pop_ProcessMoved => Get(nameof(Pop_ProcessMoved));
        public static string Pop_LastProcess => Get(nameof(Pop_LastProcess));
        public static string Pop_AdminAlarmSent => Get(nameof(Pop_AdminAlarmSent));
        public static string Pop_ManualWrenchOnly => Get(nameof(Pop_ManualWrenchOnly));
        public static string Pop_NoTargetProcess => Get(nameof(Pop_NoTargetProcess));
        public static string Pop_TorqueNoPendingRow => Get(nameof(Pop_TorqueNoPendingRow));
        public static string Pop_ResultPass => Get(nameof(Pop_ResultPass));
        public static string Pop_ResultFail => Get(nameof(Pop_ResultFail));
        public static string Pop_CommConnected => Get(nameof(Pop_CommConnected));
        public static string Pop_CommFailed => Get(nameof(Pop_CommFailed));
        public static string Pop_CommConnecting => Get(nameof(Pop_CommConnecting));
        public static string Pop_CommUnused => Get(nameof(Pop_CommUnused));
        public static string Pop_ReconnectColumn => Get(nameof(Pop_ReconnectColumn));
        public static string Pop_ReconnectButton => Get(nameof(Pop_ReconnectButton));
        public static string Pop_ReconnectFailed => Get(nameof(Pop_ReconnectFailed));
        public static string Pop_EquipmentConnectFailed => Get(nameof(Pop_EquipmentConnectFailed));
        public static string Pop_EquipmentUsable => Get(nameof(Pop_EquipmentUsable));
        public static string Pop_EquipmentUnusable => Get(nameof(Pop_EquipmentUnusable));
        public static string Upload_ConnectionError => Get(nameof(Upload_ConnectionError));
        public static string Upload_Failed => Get(nameof(Upload_Failed));
        public static string Upload_FileOpen => Get(nameof(Upload_FileOpen));
        public static string Upload_ProgressFormat => Get(nameof(Upload_ProgressFormat));
        public static string Upload_ReadFailed => Get(nameof(Upload_ReadFailed));
        public static string Upload_SelectFile => Get(nameof(Upload_SelectFile));
        public static string Upload_DeleteFailed => Get(nameof(Upload_DeleteFailed));
        public static string Upload_Delete => Get(nameof(Upload_Delete));
        public static string Upload_DeleteConfirmFormat => Get(nameof(Upload_DeleteConfirmFormat));
        public static string Upload_Title => Get(nameof(Upload_Title));
        public static string Upload_Upload => Get(nameof(Upload_Upload));
        public static string Col_MaterialCode => Get(nameof(Col_MaterialCode));
        public static string Col_InputQty => Get(nameof(Col_InputQty));
        public static string Col_InputUnit => Get(nameof(Col_InputUnit));
        public static string Col_InputPosition => Get(nameof(Col_InputPosition));
        public static string Col_SerialCheck => Get(nameof(Col_SerialCheck));
        public static string Col_SerialInputState => Get(nameof(Col_SerialInputState));
        public static string Col_BomSpecName => Get(nameof(Col_BomSpecName));
        public static string Col_SpecType => Get(nameof(Col_SpecType));
        public static string Col_TorqueLower => Get(nameof(Col_TorqueLower));
        public static string Col_TorqueUpper => Get(nameof(Col_TorqueUpper));
        public static string Col_TorqueUnit => Get(nameof(Col_TorqueUnit));
        public static string Col_TorqueResult => Get(nameof(Col_TorqueResult));
        public static string Col_InspectionSpecName => Get(nameof(Col_InspectionSpecName));
        public static string Col_InspectionRemark => Get(nameof(Col_InspectionRemark));
        public static string Col_InspectionMethod => Get(nameof(Col_InspectionMethod));
        public static string Col_ResultState => Get(nameof(Col_ResultState));
        public static string Col_ResultValue => Get(nameof(Col_ResultValue));
        public static string Col_InspectionLower => Get(nameof(Col_InspectionLower));
        public static string Col_InspectionUpper => Get(nameof(Col_InspectionUpper));
        public static string Col_InspectionTarget => Get(nameof(Col_InspectionTarget));
        public static string Pop_InspectionQuantitative => Get(nameof(Pop_InspectionQuantitative));
        public static string Pop_InspectionQualitative => Get(nameof(Pop_InspectionQualitative));
        public static string Col_EquipmentCode => Get(nameof(Col_EquipmentCode));
        public static string Col_EquipmentName => Get(nameof(Col_EquipmentName));
        public static string Col_EquipmentRemark => Get(nameof(Col_EquipmentRemark));
        public static string Col_EquipmentType => Get(nameof(Col_EquipmentType));
        public static string Col_CommunicationState => Get(nameof(Col_CommunicationState));
        public static string Col_EquipmentState => Get(nameof(Col_EquipmentState));
        public static string Col_RangeLower => Get(nameof(Col_RangeLower));
        public static string Col_RangeUpper => Get(nameof(Col_RangeUpper));
        public static string Col_SerialNumber => Get(nameof(Col_SerialNumber));
        public static string PopSerial_Title => Get(nameof(PopSerial_Title));
        public static string PopSerial_MaterialCode => Get(nameof(PopSerial_MaterialCode));
        public static string PopSerial_Qty => Get(nameof(PopSerial_Qty));
        public static string PopSerial_ItemCode => Get(nameof(PopSerial_ItemCode));
        public static string PopSerial_ListTitle => Get(nameof(PopSerial_ListTitle));
        public static string PopSerial_ItemCodeListTitle => Get(nameof(PopSerial_ItemCodeListTitle));
        public static string PopSerial_ItemCodeIncomplete => Get(nameof(PopSerial_ItemCodeIncomplete));
        public static string PopSerial_ItemCodeMismatch => Get(nameof(PopSerial_ItemCodeMismatch));
        public static string PopSerial_Save => Get(nameof(PopSerial_Save));
        public static string PopSerial_Incomplete => Get(nameof(PopSerial_Incomplete));
        public static string PopSerial_Duplicated => Get(nameof(PopSerial_Duplicated));
        public static string PopSerial_Saved => Get(nameof(PopSerial_Saved));
        public static string PopTorque_Title => Get(nameof(PopTorque_Title));
        public static string PopTorque_Invalid => Get(nameof(PopTorque_Invalid));
        public static string Pop_TorqueRangeMissing => Get(nameof(Pop_TorqueRangeMissing));
        public static string Pop_DrawingLoading => Get(nameof(Pop_DrawingLoading));
        public static string Pop_DrawingNone => Get(nameof(Pop_DrawingNone));
        public static string Pop_DrawingShareNotConfigured => Get(nameof(Pop_DrawingShareNotConfigured));
        public static string Pop_DrawingLoadFailed => Get(nameof(Pop_DrawingLoadFailed));
        public static string Pop_ImageFit => Get(nameof(Pop_ImageFit));
        public static string Pop_ImageActualSize => Get(nameof(Pop_ImageActualSize));

        // 작업 조회 현황 / 생산품목 선택 / 관리자 알람
        public static string Common_All => Get(nameof(Common_All));
        public static string Menu_WorkStatusSearch => Get(nameof(Menu_WorkStatusSearch));
        public static string WorkStatus_Title => Get(nameof(WorkStatus_Title));
        public static string WorkStatus_OrderDate => Get(nameof(WorkStatus_OrderDate));
        public static string WorkStatus_Item => Get(nameof(WorkStatus_Item));
        public static string WorkStatus_OrderListTitle => Get(nameof(WorkStatus_OrderListTitle));
        public static string WorkStatus_ProgressTitle => Get(nameof(WorkStatus_ProgressTitle));
        public static string WorkStatus_DateRequired => Get(nameof(WorkStatus_DateRequired));
        public static string WorkStatus_DateReversed => Get(nameof(WorkStatus_DateReversed));
        public static string WorkStatus_StatusWaiting => Get(nameof(WorkStatus_StatusWaiting));
        public static string WorkStatus_StatusRunning => Get(nameof(WorkStatus_StatusRunning));
        public static string WorkStatus_StatusDone => Get(nameof(WorkStatus_StatusDone));
        public static string WorkStatus_StatusHold => Get(nameof(WorkStatus_StatusHold));
        public static string Col_PageOperationName => Get(nameof(Col_PageOperationName));
        public static string Col_SpecName => Get(nameof(Col_SpecName));
        public static string Col_SpecTypeName => Get(nameof(Col_SpecTypeName));
        public static string Col_SpecResultState => Get(nameof(Col_SpecResultState));
        public static string Col_ItemName => Get(nameof(Col_ItemName));
        public static string Col_Specification => Get(nameof(Col_Specification));
        public static string Col_Unit => Get(nameof(Col_Unit));

        // 작업통합 현황 조회 (작업 통합 개요 트리)
        public static string WorkIntegrated_OverviewTitle => Get(nameof(WorkIntegrated_OverviewTitle));
        public static string WorkIntegrated_ExcelExport => Get(nameof(WorkIntegrated_ExcelExport));
        public static string WorkIntegrated_SerialRequired => Get(nameof(WorkIntegrated_SerialRequired));
        public static string WorkIntegrated_ExportEmpty => Get(nameof(WorkIntegrated_ExportEmpty));
        public static string WorkIntegrated_ExportDone => Get(nameof(WorkIntegrated_ExportDone));
        public static string WorkIntegrated_ExportFailed => Get(nameof(WorkIntegrated_ExportFailed));
        public static string Col_NodeCode => Get(nameof(Col_NodeCode));
        public static string Col_NodeDescription => Get(nameof(Col_NodeDescription));
        public static string Col_TorqueTarget => Get(nameof(Col_TorqueTarget));
        public static string Col_TorqueMax => Get(nameof(Col_TorqueMax));
        public static string Col_AngleTarget => Get(nameof(Col_AngleTarget));
        public static string Col_AngleLower => Get(nameof(Col_AngleLower));
        public static string Col_AngleUpper => Get(nameof(Col_AngleUpper));
        public static string Col_AngleUnit => Get(nameof(Col_AngleUnit));
        public static string Col_AngleResult => Get(nameof(Col_AngleResult));
        public static string ItemPicker_Title => Get(nameof(ItemPicker_Title));
        public static string ItemPicker_Keyword => Get(nameof(ItemPicker_Keyword));
        public static string ItemPicker_ListTitle => Get(nameof(ItemPicker_ListTitle));
        public static string ItemPicker_Select => Get(nameof(ItemPicker_Select));
        public static string ItemPicker_SelectItem => Get(nameof(ItemPicker_SelectItem));
        public static string AdminAlarm_Title => Get(nameof(AdminAlarm_Title));
        public static string AdminAlarm_Type => Get(nameof(AdminAlarm_Type));
        public static string AdminAlarm_ListTitle => Get(nameof(AdminAlarm_ListTitle));
        public static string AdminAlarm_Send => Get(nameof(AdminAlarm_Send));
        public static string AdminAlarm_SelectAlarm => Get(nameof(AdminAlarm_SelectAlarm));
        public static string AdminAlarm_NoTypes => Get(nameof(AdminAlarm_NoTypes));
        public static string AlarmType_EQP => Get(nameof(AlarmType_EQP));
        public static string AlarmType_QLT => Get(nameof(AlarmType_QLT));
        public static string AlarmType_MAT => Get(nameof(AlarmType_MAT));
        public static string AlarmType_PRD => Get(nameof(AlarmType_PRD));
        public static string AlarmType_EHS => Get(nameof(AlarmType_EHS));
        public static string AlarmType_SYS => Get(nameof(AlarmType_SYS));
        public static string AlarmGrade_P1 => Get(nameof(AlarmGrade_P1));
        public static string AlarmGrade_P2 => Get(nameof(AlarmGrade_P2));
        public static string AlarmGrade_P3 => Get(nameof(AlarmGrade_P3));
        public static string AlarmGrade_P4 => Get(nameof(AlarmGrade_P4));

        public static string Menu_AttachmentSearch => Get(nameof(Menu_AttachmentSearch));
        public static string Attach_Title => Get(nameof(Attach_Title));
        public static string Attach_RegDate => Get(nameof(Attach_RegDate));
        public static string Attach_ListTitle => Get(nameof(Attach_ListTitle));
        public static string Attach_FileListTitle => Get(nameof(Attach_FileListTitle));
        public static string Attach_Download => Get(nameof(Attach_Download));
        public static string Attach_Delete => Get(nameof(Attach_Delete));
        public static string Attach_Upload => Get(nameof(Attach_Upload));
        public static string Attach_FileTypeImage => Get(nameof(Attach_FileTypeImage));
        public static string Attach_FileTypeDocument => Get(nameof(Attach_FileTypeDocument));
        public static string Attach_SelectOrder => Get(nameof(Attach_SelectOrder));
        public static string Attach_SelectFile => Get(nameof(Attach_SelectFile));
        public static string Attach_DownloadDone => Get(nameof(Attach_DownloadDone));
        public static string Attach_DownloadFailed => Get(nameof(Attach_DownloadFailed));
        /// <summary>OpenFileDialog.Filter 형식("표시|패턴|...") 그대로 번역한다 — 구분자 '|' 개수를 바꾸지 말 것.</summary>
        public static string Attach_UploadFilter => Get(nameof(Attach_UploadFilter));
        public static string Attach_ConditionRequired => Get(nameof(Attach_ConditionRequired));
        public static string Col_AttachmentCount => Get(nameof(Col_AttachmentCount));
        public static string Col_CreatedDate => Get(nameof(Col_CreatedDate));
        public static string Col_FileName => Get(nameof(Col_FileName));
        public static string Col_FileType => Get(nameof(Col_FileType));
        public static string Col_FileSize => Get(nameof(Col_FileSize));
        public static string Col_CreatedBy => Get(nameof(Col_CreatedBy));
        public static string Col_CreatedAt => Get(nameof(Col_CreatedAt));
        public static string Col_UpdatedBy => Get(nameof(Col_UpdatedBy));
        public static string Common_NoData => Get(nameof(Common_NoData));
        public static string StopIssue_Title => Get(nameof(StopIssue_Title));
        public static string StopIssue_Code => Get(nameof(StopIssue_Code));
        public static string StopIssue_Remark => Get(nameof(StopIssue_Remark));
        public static string StopIssue_SelectCode => Get(nameof(StopIssue_SelectCode));
        public static string ModbusTest_Title => Get(nameof(ModbusTest_Title));
        public static string ModbusTest_RepeatCount => Get(nameof(ModbusTest_RepeatCount));
        public static string ModbusTest_TimeoutMs => Get(nameof(ModbusTest_TimeoutMs));
        public static string ModbusTest_IntervalInfo => Get(nameof(ModbusTest_IntervalInfo));
        public static string ModbusTest_LoadEquipments => Get(nameof(ModbusTest_LoadEquipments));
        public static string ModbusTest_AddRow => Get(nameof(ModbusTest_AddRow));
        public static string ModbusTest_RemoveRow => Get(nameof(ModbusTest_RemoveRow));
        public static string ModbusTest_ListTitle => Get(nameof(ModbusTest_ListTitle));
        public static string ModbusTest_LogTitle => Get(nameof(ModbusTest_LogTitle));
        public static string ModbusTest_Start => Get(nameof(ModbusTest_Start));
        public static string ModbusTest_Stop => Get(nameof(ModbusTest_Stop));
        public static string ModbusTest_ColHost => Get(nameof(ModbusTest_ColHost));
        public static string ModbusTest_ColPort => Get(nameof(ModbusTest_ColPort));
        public static string ModbusTest_ColEventPort => Get(nameof(ModbusTest_ColEventPort));
        public static string ModbusTest_ColProgress => Get(nameof(ModbusTest_ColProgress));
        public static string ModbusTest_ColSuccess => Get(nameof(ModbusTest_ColSuccess));
        public static string ModbusTest_ColFail => Get(nameof(ModbusTest_ColFail));
        public static string ModbusTest_ColState => Get(nameof(ModbusTest_ColState));
        public static string ModbusTest_ColLastResult => Get(nameof(ModbusTest_ColLastResult));
        public static string ModbusTest_StateRunning => Get(nameof(ModbusTest_StateRunning));
        public static string ModbusTest_StateDone => Get(nameof(ModbusTest_StateDone));
        public static string ModbusTest_StateStopped => Get(nameof(ModbusTest_StateStopped));
        public static string ModbusTest_ResultOk => Get(nameof(ModbusTest_ResultOk));
        public static string ModbusTest_ResultFail => Get(nameof(ModbusTest_ResultFail));
        public static string ModbusTest_Timeout => Get(nameof(ModbusTest_Timeout));
        public static string ModbusTest_NoResponse => Get(nameof(ModbusTest_NoResponse));
        public static string ModbusTest_NoTargets => Get(nameof(ModbusTest_NoTargets));
        public static string ModbusTest_InvalidTarget => Get(nameof(ModbusTest_InvalidTarget));
        public static string ModbusTest_NoEquipments => Get(nameof(ModbusTest_NoEquipments));
        public static string ModbusTest_ReportStopped => Get(nameof(ModbusTest_ReportStopped));
        public static string ModbusTest_ReportLine => Get(nameof(ModbusTest_ReportLine));
        public static string Col_MaterialGroup => Get(nameof(Col_MaterialGroup));
        public static string Pop_Rework => Get(nameof(Pop_Rework));
        public static string Pop_ReworkStarted => Get(nameof(Pop_ReworkStarted));
        public static string Col_MaterialName => Get(nameof(Col_MaterialName));
        public static string Col_PurchasePlanMethod => Get(nameof(Col_PurchasePlanMethod));
    }
}
