// ============================================================
// PopScreen.cs 리팩토링 예제 (구체적인 구현)
// ============================================================

#region 📌 1. 탭 전환 로직 통합 예제

// ❌ 현재 코드 (중복도 100%)
/*
private void btnTabMaterial_Click(object sender, EventArgs e) 
    => SetActiveTab(PopTab.Material);
private void btnTabBomSpec_Click(object sender, EventArgs e) 
    => SetActiveTab(PopTab.BomSpec);
private void btnTabInspection_Click(object sender, EventArgs e) 
    => SetActiveTab(PopTab.Inspection);
private void btnTabEquipment_Click(object sender, EventArgs e) 
    => SetActiveTab(PopTab.Equipment);
*/

// ✅ 개선된 코드
public partial class PopScreen : XtraUserControl
{
    private void InitializeTabButtons()
    {
        // Designer 코드에서 설정:
        // btnTabMaterial.Click += TabButton_Click;
        // btnTabMaterial.Tag = PopTab.Material;

        var buttons = new[] 
        { 
            (btnTabMaterial, PopTab.Material),
            (btnTabBomSpec, PopTab.BomSpec),
            (btnTabInspection, PopTab.Inspection),
            (btnTabEquipment, PopTab.Equipment)
        };

        foreach (var (button, tab) in buttons)
        {
            button.Tag = tab;
            button.Click += TabButton_Click;
        }
    }

    private void TabButton_Click(object sender, EventArgs e)
    {
        if (sender is MeisButton button && button.Tag is PopTab tab)
        {
            SetActiveTab(tab);
        }
    }
}

#endregion

#region 📌 2. SetActiveTab() 로직 단순화

// ❌ 현재 코드 (반복과 중복)
/*
private void SetActiveTab(PopTab tab)
{
    _activeTab = tab;

    gridMaterial.Visible = tab == PopTab.Material;
    gridBomSpec.Visible = tab == PopTab.BomSpec;
    gridInspection.Visible = tab == PopTab.Inspection;
    gridEquipment.Visible = tab == PopTab.Equipment;

    chkManualWrench.Visible = tab == PopTab.BomSpec;
    btnSaveResults.Visible = tab == PopTab.BomSpec || tab == PopTab.Inspection;

    StyleTabButtons();
}
*/

// ✅ 개선된 코드
public partial class PopScreen : XtraUserControl
{
    // 각 탭의 그리드 매핑
    private readonly Dictionary<PopTab, Control> _gridMap = new()
    {
        { PopTab.Material, _gridMaterial },
        { PopTab.BomSpec, _gridBomSpec },
        { PopTab.Inspection, _gridInspection },
        { PopTab.Equipment, _gridEquipment }
    };

    // 각 탭이 보여야 할 추가 컨트롤
    private readonly Dictionary<PopTab, Control[]> _visibilityMap = new()
    {
        { PopTab.Material, Array.Empty<Control>() },
        { PopTab.BomSpec, new Control[] { chkManualWrench, btnSaveResults } },
        { PopTab.Inspection, new Control[] { btnSaveResults } },
        { PopTab.Equipment, Array.Empty<Control>() }
    };

    private void SetActiveTab(PopTab tab)
    {
        _activeTab = tab;

        // 1. 그리드 표시/숨김
        foreach (var (tabKey, grid) in _gridMap)
        {
            grid.Visible = tabKey == tab;
        }

        // 2. 탭별 추가 컨트롤 표시/숨김
        foreach (var (tabKey, controls) in _visibilityMap)
        {
            bool isActive = tabKey == tab;
            foreach (var control in controls)
            {
                control.Visible = isActive;
            }
        }

        // 3. 탭 버튼 스타일 적용
        StyleTabButtons();
    }
}

#endregion

#region 📌 3. StyleTabButtons() 통합

// ❌ 현재 코드 (반복)
/*
private void StyleTabButtons()
{
    StyleTabButton(btnTabMaterial, _activeTab == PopTab.Material);
    StyleTabButton(btnTabBomSpec, _activeTab == PopTab.BomSpec);
    StyleTabButton(btnTabInspection, _activeTab == PopTab.Inspection);
    StyleTabButton(btnTabEquipment, _activeTab == PopTab.Equipment);
}
*/

// ✅ 개선된 코드
public partial class PopScreen : XtraUserControl
{
    private readonly MeisButton[] _tabButtons;

    private void InitializeTabButtons()
    {
        _tabButtons = new[] 
        { 
            btnTabMaterial, 
            btnTabBomSpec, 
            btnTabInspection, 
            btnTabEquipment 
        };

        var tabs = new[] 
        { 
            PopTab.Material, 
            PopTab.BomSpec, 
            PopTab.Inspection, 
            PopTab.Equipment 
        };

        for (int i = 0; i < _tabButtons.Length; i++)
        {
            _tabButtons[i].Tag = tabs[i];
            _tabButtons[i].Click += TabButton_Click;
        }
    }

    private void StyleTabButtons()
    {
        var tabs = (PopTab[])_tabButtons.Select(b => (PopTab)b.Tag).ToArray();

        for (int i = 0; i < _tabButtons.Length; i++)
        {
            StyleTabButton(_tabButtons[i], tabs[i] == _activeTab);
        }
    }
}

#endregion

#region 📌 4. ThemeFieldLabels/Editors 통합

// ❌ 현재 코드 (복제됨)
/*
private void ThemeFieldLabels()
{
    AppTheme.Caption(lblWorkOrderNo);
    AppTheme.Caption(lblItemCode);
    AppTheme.Caption(lblProductSerialNo);
    AppTheme.Caption(lblRoutingGroup);
    AppTheme.Caption(lblRoutingOperation);
    AppTheme.Caption(lblPageOperation);
    AppTheme.Caption(lblPageStartTime);
    AppTheme.Caption(lblPageEndTime);
    AppTheme.Caption(lblPageRunTime);
    AppTheme.Caption(lblPageIdleTime);
    AppTheme.Caption(lblPageWorkTime);
}

private void ThemeFieldEditors()
{
    AppTheme.Editor(txtWorkOrderNo);
    AppTheme.Editor(txtItemCode);
    AppTheme.Editor(txtProductSerialNo);
    AppTheme.Editor(txtRoutingGroup);
    AppTheme.Editor(txtRoutingOperation);
    AppTheme.Editor(txtPageOperation);
    AppTheme.Editor(txtPageStartTime);
    AppTheme.Editor(txtPageEndTime);
    AppTheme.Editor(txtPageRunTime);
    AppTheme.Editor(txtPageIdleTime);
    AppTheme.Editor(txtPageWorkTime);
}
*/

// ✅ 개선된 코드
public partial class PopScreen : XtraUserControl
{
    private record FieldMapping(Label Label, Control Editor);

    private readonly FieldMapping[] _fieldMappings = new[]
    {
        new FieldMapping(lblWorkOrderNo, txtWorkOrderNo),
        new FieldMapping(lblItemCode, txtItemCode),
        new FieldMapping(lblProductSerialNo, txtProductSerialNo),
        new FieldMapping(lblRoutingGroup, txtRoutingGroup),
        new FieldMapping(lblRoutingOperation, txtRoutingOperation),
        new FieldMapping(lblPageOperation, txtPageOperation),
        new FieldMapping(lblPageStartTime, txtPageStartTime),
        new FieldMapping(lblPageEndTime, txtPageEndTime),
        new FieldMapping(lblPageRunTime, txtPageRunTime),
        new FieldMapping(lblPageIdleTime, txtPageIdleTime),
        new FieldMapping(lblPageWorkTime, txtPageWorkTime),
    };

    private void ThemeFieldControls()
    {
        foreach (var mapping in _fieldMappings)
        {
            AppTheme.Caption(mapping.Label);
            AppTheme.Editor(mapping.Editor);
        }
    }

    private override void ApplyTheme()
    {
        // ... 기존 코드 ...
        ThemeFieldControls();
    }
}

#endregion

#region 📌 5. ProcessTile 테마 통합

// ❌ 현재 코드 (반복)
/*
AppTheme.Tile(tilePrevProcess, AppTheme.Indigo, AppIcons.Reset);
AppTheme.Tile(tileNextProcess, AppTheme.Indigo, AppIcons.Refresh);
AppTheme.Tile(tileSkipProcess, AppTheme.Slate, AppIcons.Close);
AppTheme.Tile(tileStopProcess, AppTheme.Rose, AppIcons.Stop);
AppTheme.Tile(tileStartProcess, AppTheme.Emerald, AppIcons.Play);
AppTheme.Tile(tileProcessList, AppTheme.Violet, AppIcons.Layers);
AppTheme.Tile(tileAdminAlarm, AppTheme.Amber, AppIcons.Bell);
*/

// ✅ 개선된 코드
public partial class PopScreen : XtraUserControl
{
    private record TileConfig(TileItem Tile, Color AccentColor, Icon TileIcon);

    private void ApplyProcessTileTheme()
    {
        var tileConfigs = new[]
        {
            new TileConfig(tilePrevProcess, AppTheme.Indigo, AppIcons.Reset),
            new TileConfig(tileNextProcess, AppTheme.Indigo, AppIcons.Refresh),
            new TileConfig(tileSkipProcess, AppTheme.Slate, AppIcons.Close),
            new TileConfig(tileStopProcess, AppTheme.Rose, AppIcons.Stop),
            new TileConfig(tileStartProcess, AppTheme.Emerald, AppIcons.Play),
            new TileConfig(tileProcessList, AppTheme.Violet, AppIcons.Layers),
            new TileConfig(tileAdminAlarm, AppTheme.Amber, AppIcons.Bell),
        };

        AppTheme.TileStrip(tileProcess, ProcessTileHeight, ProcessTileWidth);

        foreach (var config in tileConfigs)
        {
            AppTheme.Tile(config.Tile, config.AccentColor, config.TileIcon);
        }
    }
}

#endregion

#region 📌 6. ProcessAction 버튼 이벤트 통합

// ❌ 현재 코드 (동일한 패턴 반복)
/*
private async void tileStartProcess_ItemClick(object sender, TileItemEventArgs e) =>
    await RequestProcessActionAsync(PopProcessAction.Start, Language.Pop_ConfirmStart, MockPopData.StateRunning);

private async void tileStopProcess_ItemClick(object sender, TileItemEventArgs e) =>
    await RequestProcessActionAsync(PopProcessAction.Stop, Language.Pop_ConfirmStop, MockPopData.StateStopped);

private async void tileSkipProcess_ItemClick(object sender, TileItemEventArgs e) =>
    await RequestProcessActionAsync(PopProcessAction.Skip, Language.Pop_ConfirmSkip, MockPopData.StateSkipped);
*/

// ✅ 개선된 코드
public partial class PopScreen : XtraUserControl
{
    private record ProcessActionTileInfo(PopProcessAction Action, string ConfirmMessage, string MockState);

    private void InitializeProcessActionTiles()
    {
        RegisterActionTile(tileStartProcess, PopProcessAction.Start, Language.Pop_ConfirmStart, MockPopData.StateRunning);
        RegisterActionTile(tileStopProcess, PopProcessAction.Stop, Language.Pop_ConfirmStop, MockPopData.StateStopped);
        RegisterActionTile(tileSkipProcess, PopProcessAction.Skip, Language.Pop_ConfirmSkip, MockPopData.StateSkipped);
    }

    private void RegisterActionTile(TileItem tile, PopProcessAction action, string confirmMessage, string mockState)
    {
        tile.Tag = new ProcessActionTileInfo(action, confirmMessage, mockState);
        tile.ItemClick += ProcessActionTile_ItemClick;
    }

    private async void ProcessActionTile_ItemClick(object sender, TileItemEventArgs e)
    {
        if (sender is TileItem tile && tile.Tag is ProcessActionTileInfo info)
        {
            await RequestProcessActionAsync(info.Action, info.ConfirmMessage, info.MockState);
        }
    }
}

#endregion

#region 📌 7. LoadAllAsync() 정리

// ❌ 현재 코드 (주석이 많음)
/*
private async Task LoadAllAsync()
{
    try
    {
        _info = await FetchProcessInfoAsync();
        //_materials = await FetchMaterialsAsync();
        //_bomSpecs = await FetchBomSpecsAsync();
        //_inspections = await FetchInspectionsAsync();
        //_equipments = await FetchEquipmentsAsync();
    }
    catch (ApiException ex)
    {
        AppMessageBox.Warn(this, ex.Message, Language.Pop_Title);
        return;
    }

    BindProcessInfo();

    //gridMaterial.DataSource = _materials;
    //gridBomSpec.DataSource = _bomSpecs;
    //gridInspection.DataSource = _inspections;
    //gridEquipment.DataSource = _equipments;

    //RefreshEquipmentStates();
    await LoadDrawingAsync();
}
*/

// ✅ 개선된 코드
public partial class PopScreen : XtraUserControl
{
    /// <summary>
    /// 공정 정보와 도면을 로드합니다.
    /// 각 탭의 데이터는 해당 탭의 Init 메서드에서 필요하면 로드합니다.
    /// </summary>
    private async Task LoadAllAsync()
    {
        try
        {
            _info = await FetchProcessInfoAsync();
        }
        catch (ApiException ex)
        {
            AppMessageBox.Warn(this, ex.Message, Language.Pop_Title);
            return;
        }

        BindProcessInfo();
        await LoadDrawingAsync();
    }

    /// <summary>
    /// 화면의 모든 탭을 초기화합니다.
    /// </summary>
    private async Task InitializeTabsAsync()
    {
        // TODO: 각 Init 메서드는 LazyLoading을 고려하여 
        // 해당 탭이 활성화될 때만 로드하도록 리팩토링할 수 있습니다.
        MaterialInit();
        InspectionInit();
        BomInit();
        EquipmentInit();
    }
}

#endregion

#region 📌 8. Init() 오버로드 개선

// ❌ 현재 코드 (불명확)
/*
public void Init()
{
    Init(null);
}

public void Init(WorkEntryContext entry)
{
    // ...
}
*/

// ✅ 개선된 코드
public partial class PopScreen : XtraUserControl
{
    /// <summary>
    /// POP 화면을 초기화합니다.
    /// </summary>
    /// <param name="entry">작업 정보. null이면 빈 화면으로 표시됩니다.</param>
    public void Init(WorkEntryContext entry = null)
    {
        _appSet = Global.Instance.Settings;
        _equip = Global.Instance.Devices;
        _entry = entry;
        _key = BuildKey(entry);
        _processes = new List<RoutingProcessDto>();

        // UI 구성
        ApplyTexts();
        ApplyTheme();
        SetActiveTab(PopTab.Material);

        // 이벤트 등록
        RegisterEventHandlers();

        // 클대로그
        StartClock();
    }

    private void RegisterEventHandlers()
    {
        viewerSurface.Paint += ViewerSurface_Paint;
        viewerSurface.Resize += (s, e) => viewerSurface.Invalidate();
        gridViewMaterial.RowCellClick += GridViewMaterial_RowCellClick;

        Load += PopScreen_Load;
        Disposed += PopScreen_Disposed;

        // 탭 버튼 이벤트 (새로 추가)
        InitializeTabButtons();
        InitializeProcessActionTiles();
    }

    /// <summary>
    /// 빈 화면 상태로 초기화합니다.
    /// </summary>
    [Obsolete("Init() 메서드를 매개변수 없이 호출하세요")]
    public void Init() => Init(null);
}

#endregion

#region 📌 9. 리팩토링 마이그레이션 가이드

/*
마이그레이션 순서:
1. 새 PopScreen 버전을 별도로 작성하고 테스트
2. 점진적으로 각 섹션을 이동
3. 각 단계마다 테스트 실행
4. Git에 커밋 (작은 단위로)

커밋 메시지 예:
- chore: 주석 처리된 코드 정리 (ConfigureBomSpecGrid 제거)
- refactor: 탭 버튼 클릭 핸들러 통합
- refactor: SetActiveTab 로직 단순화 (딕셔너리 기반)
- refactor: ThemeFieldLabels/Editors 통합
- refactor: ProcessTile 테마 적용 구조화
- refactor: ProcessAction 이벤트 통합
- refactor: LoadAllAsync 주석 정리 및 문서화
- refactor: Init 오버로드 개선
- test: 리팩토링된 기능 단위 테스트 추가

테스트 체크리스트:
□ 모든 탭이 정상 전환되는가?
□ 각 탭의 데이터가 정상 표시되는가?
□ 공정 제어 버튼이 정상 작동하는가?
□ 공정 목록 이동이 정상 작동하는가?
□ 관리자 알림이 정상 작동하는가?
□ 도면 표시가 정상 작동하는가?
□ 성능 저하가 없는가?
*/

#endregion
