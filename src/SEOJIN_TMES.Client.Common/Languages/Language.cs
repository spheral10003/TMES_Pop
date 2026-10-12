using System.Globalization;
using System.Reflection;
using System.Resources;

namespace SEOJIN_TMES.Client.Common.Languages
{

    /// <summary>언어 콤보 항목 — 표시 문구와 문화권 코드 한 쌍.</summary>
    public class LanguageOption
    {
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
        public static string WorkEntry_WorkOrderNo => Get(nameof(WorkEntry_WorkOrderNo));
        public static string WorkEntry_SerialNo => Get(nameof(WorkEntry_SerialNo));
        public static string WorkEntry_ListTitle => Get(nameof(WorkEntry_ListTitle));
        public static string WorkEntry_ConditionRequired => Get(nameof(WorkEntry_ConditionRequired));
        public static string WorkEntry_SelectWorkOrder => Get(nameof(WorkEntry_SelectWorkOrder));
        public static string WorkEntry_NoResult => Get(nameof(WorkEntry_NoResult));
        public static string WorkEntry_NoRouting => Get(nameof(WorkEntry_NoRouting));
        public static string WorkEntryConfirm_Title => Get(nameof(WorkEntryConfirm_Title));
        public static string WorkEntryConfirm_ListTitle => Get(nameof(WorkEntryConfirm_ListTitle));
        public static string WorkEntryConfirm_Move => Get(nameof(WorkEntryConfirm_Move));
        public static string WorkEntryConfirm_SelectProcess => Get(nameof(WorkEntryConfirm_SelectProcess));
        public static string Col_No => Get(nameof(Col_No));
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
        public static string Pop_SkipProcess => Get(nameof(Pop_SkipProcess));
        public static string Pop_StopProcess => Get(nameof(Pop_StopProcess));
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
        public static string Pop_SaveResult => Get(nameof(Pop_SaveResult));
        public static string Pop_SavedResult => Get(nameof(Pop_SavedResult));
        public static string Pop_SerialNotRequired => Get(nameof(Pop_SerialNotRequired));
        public static string Pop_NoProcessList => Get(nameof(Pop_NoProcessList));
        public static string Pop_ProcessMoved => Get(nameof(Pop_ProcessMoved));
        public static string Pop_FirstProcess => Get(nameof(Pop_FirstProcess));
        public static string Pop_LastProcess => Get(nameof(Pop_LastProcess));
        public static string Pop_ProcessNotInList => Get(nameof(Pop_ProcessNotInList));
        public static string Pop_ConfirmStart => Get(nameof(Pop_ConfirmStart));
        public static string Pop_ConfirmStop => Get(nameof(Pop_ConfirmStop));
        public static string Pop_ConfirmSkip => Get(nameof(Pop_ConfirmSkip));
        public static string Pop_ConfirmPrevious => Get(nameof(Pop_ConfirmPrevious));
        public static string Pop_ConfirmNext => Get(nameof(Pop_ConfirmNext));
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
        public static string Pop_EquipmentUsable => Get(nameof(Pop_EquipmentUsable));
        public static string Pop_EquipmentUnusable => Get(nameof(Pop_EquipmentUnusable));
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
        public static string AdminAlarm_DetailTitle => Get(nameof(AdminAlarm_DetailTitle));
        public static string AdminAlarm_Target => Get(nameof(AdminAlarm_Target));
        public static string AdminAlarm_Send => Get(nameof(AdminAlarm_Send));
        public static string AdminAlarm_SelectType => Get(nameof(AdminAlarm_SelectType));
        public static string AdminAlarm_SelectTarget => Get(nameof(AdminAlarm_SelectTarget));
        public static string AdminAlarm_NoTypes => Get(nameof(AdminAlarm_NoTypes));
    }
}
