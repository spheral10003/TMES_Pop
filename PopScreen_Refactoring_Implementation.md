# PopScreen 리팩토링 구현 예제

이 파일은 PopScreen.cs의 Partial Class 통합 리팩토링을 위한 구체적인 코드 예제를 제시합니다.

---

## 1️⃣ Phase 1: 탭 초기화 통합 구현

### 1.1 탭 설정 데이터 구조 정의

```csharp
// PopScreen.cs (기존 내용 위에 추가)

/// <summary>
/// 각 탭의 초기화 설정을 데이터로 관리합니다.
/// 새 탭 추가 시 배열에만 항목을 추가하면 됩니다.
/// </summary>
private record TabInitConfig(
	PopTab TabType,
	GridControl GridControl,
	MeisGridView GridView,
	Color ThemeColor,
	string TabName,
	Func<Task> LoadDataAsync,
	Action ConfigureColumns
);

// 탭 설정 배열 (읽기 전용)
private readonly TabInitConfig[] _tabConfigs;

// 생성자 또는 Load 이벤트에서 초기화
private void InitializeTabConfigs()
{
	_tabConfigs = new[]
	{
		new TabInitConfig(
			PopTab.Material,
			gridMaterial,
			gridViewMaterial,
			AppTheme.Emerald,
			"MaterialTab",
			FetchAndBindMaterialAsync,
			ConfigureMaterialGrid
		),
		new TabInitConfig(
			PopTab.BomSpec,
			gridBomSpec,
			gridViewBomSpec,
			AppTheme.Indigo,
			"BomTab",
			FetchAndBindBomAsync,
			ConfigureBomSpecGrid
		),
		new TabInitConfig(
			PopTab.Inspection,
			gridInspection,
			gridViewInspection,
			AppTheme.Violet,
			"InspectionTab",
			FetchAndBindInspectionAsync,
			ConfigureInspectionGrid
		),
		new TabInitConfig(
			PopTab.Equipment,
			gridEquipment,
			gridViewEquipment,
			AppTheme.Cyan,
			"EquipmentTab",
			FetchAndBindEquipmentAsync,
			ConfigureEquipmentGrid
		)
	};
}
```

### 1.2 통합 초기화 메서드

```csharp
/// <summary>
/// 모든 탭을 동일한 방식으로 초기화합니다.
/// </summary>
private async Task InitializeAllTabsAsync()
{
	foreach (var config in _tabConfigs)
	{
		try
		{
			// Theme 적용
			AppTheme.Grid(config.GridControl, config.GridView, config.ThemeColor);

			// 칼럼 구성
			config.ConfigureColumns();

			// 데이터 로드
			await config.LoadDataAsync();
		}
		catch (Exception ex)
		{
			Logger.Error($"탭 초기화 실패: {config.TabName}", ex);
			AppMessageBox.Warn(this, $"탭 '{config.TabName}' 초기화 중 오류 발생", 
				Language.Pop_Title);
		}
	}
}
```

### 1.3 기존 Material/Bom/Inspection/Equipment Init() 메서드 통합

**Before:**
```csharp
// PopScreen.Material.cs
public void MaterialInit()
{
	AppTheme.Grid(gridMaterial, gridViewMaterial, AppTheme.Emerald);
	ConfigureMaterialGrid();
	LoadMaterialAsync();
}

// PopScreen.Bom.cs
public void BomInit()
{
	AppTheme.Grid(gridBomSpec, gridViewBomSpec, AppTheme.Indigo);
	ConfigureBomSpecGrid();
	LoadBomAsync();
}

// ... Inspection.cs, Equipment.cs 동일 패턴
```

**After:**
```csharp
// PopScreen.cs
// 기존 Init() 메서드들은 모두 제거/병합
// 대신 InitializeAllTabsAsync()만 호출:

private async void PopScreen_Load(object sender, EventArgs e)
{
	try
	{
		InitializeTabConfigs();  // 설정 배열 초기화
		ApplyTheme();            // UI 테마 적용
		StartClock();            // 시계 시작

		// 기존: MaterialInit(); InspectionInit(); BomInit(); EquipmentInit();
		// 신규:
		await InitializeAllTabsAsync();  // 모든 탭 초기화 (통합)

		// 초기 탭 설정
		SetActiveTab(PopTab.BomSpec);
	}
	catch (Exception ex)
	{
		Logger.Error("PopScreen 초기화 실패", ex);
		AppMessageBox.Fatal(this, ex.Message, Language.Pop_Title);
	}
}
```

### 1.4 효과

- **코드 감소:** 100줄 → 30줄 (70% 감소)
- **일관성:** 모든 탭이 동일한 초기화 프로세스 따름
- **확장성:** 새 탭 추가 시 배열에 1개 항목만 추가
- **테스트 용이성:** 탭 초기화를 테스트할 수 있는 단일 메서드

---

## 2️⃣ Phase 2: DataLoading 통합 구현

### 2.1 제너릭 데이터 로드 메서드

```csharp
/// <summary>
/// 탭 그리드에 데이터를 로드하고 바인딩하는 통합 메서드입니다.
/// 모든 탭의 데이터 로드 로직을 이 메서드로 통합합니다.
/// </summary>
private async Task<List<T>> LoadAndBindGridAsync<T>(
	GridControl grid,
	Func<Task<List<T>>> fetchFunc,
	string tabName) where T : class
{
	try
	{
		var data = await fetchFunc();
		if (data == null)
			data = new List<T>();

		grid.DataSource = data;
		Logger.Info($"탭 '{tabName}' 데이터 로드 완료: {data.Count}건");

		return data;
	}
	catch (ApiException ex)
	{
		Logger.Error($"탭 '{tabName}' 데이터 로드 실패: {ex.Message}", ex);
		AppMessageBox.Warn(this, ex.Message, Language.Pop_Title);
		return new List<T>();
	}
	catch (Exception ex)
	{
		Logger.Error($"탭 '{tabName}' 예상 외 오류", ex);
		AppMessageBox.Fatal(this, ex.Message, Language.Pop_Title);
		return new List<T>();
	}
}
```

### 2.2 탭별 Fetch & Bind 메서드 (Async Wrapper)

```csharp
// 기존 코드 구조 유지하면서 async 래핑

/// <summary>
/// Material 탭 데이터 로드 (통합 메서드 호출)
/// </summary>
private async Task FetchAndBindMaterialAsync()
{
	_materials = await LoadAndBindGridAsync(
		gridMaterial,
		FetchMaterialsAsync,
		"Material"
	);
}

/// <summary>
/// BOM 탭 데이터 로드 + 추가 로직
/// </summary>
private async Task FetchAndBindBomAsync()
{
	_bomSpecs = await LoadAndBindGridAsync(
		gridBomSpec,
		FetchBomSpecsAsync,
		"BOM"
	);

	// BOM 탭 추가 로직 (이미 색상 지정된 로우 강조 등)
	ApplyBomSpecHighlighting(_bomSpecs);
}

/// <summary>
/// Inspection 탭 데이터 로드
/// </summary>
private async Task FetchAndBindInspectionAsync()
{
	_inspections = await LoadAndBindGridAsync(
		gridInspection,
		FetchInspectionsAsync,
		"Inspection"
	);
}

/// <summary>
/// Equipment 탭 데이터 로드 + 상태 갱신
/// </summary>
private async Task FetchAndBindEquipmentAsync()
{
	_equipments = await LoadAndBindGridAsync(
		gridEquipment,
		FetchEquipmentsAsync,
		"Equipment"
	);

	// Equipment 탭 추가 로직
	RefreshEquipmentStates();
}

/// <summary>
/// BOM Spec 가시화 강조 (기존 LoadBomAsync의 추가 로직)
/// </summary>
private void ApplyBomSpecHighlighting(List<PopBomSpecDto> specs)
{
	for (int i = 0; i < gridViewBomSpec.RowCount; i++)
	{
		var target = gridViewBomSpec.GetRow(i) as PopBomSpecDto;
		if (target == null) continue;

		if (decimal.TryParse(target.TorqueResult?.ToString(), out decimal value))
		{
			decimal.TryParse(target.TorqueLower?.ToString(), out decimal min);
			decimal.TryParse(target.TorqueUpper?.ToString(), out decimal max);

			if (value < min || value > max)
			{
				gridViewBomSpec.Columns["TorqueResult"].AppearanceCell.ForeColor = AppTheme.Rose;
			}
			else
			{
				gridViewBomSpec.FocusedRowHandle = i;
			}
		}
	}
}
```

### 2.3 기존 Load 메서드 제거 예정 파일 목록

```csharp
// 다음 메서드들을 제거하거나 통합:

// PopScreen.Material.cs
// - LoadMaterialAsync() 제거 (FetchAndBindMaterialAsync로 통합)

// PopScreen.Bom.cs
// - LoadBomAsync() 제거 (FetchAndBindBomAsync로 통합)

// PopScreen.Inspection.cs
// - LoadInspectionAsync() 제거 (FetchAndBindInspectionAsync로 통합)

// PopScreen.Equipment.cs
// - LoadEquipAsync() 제거 (FetchAndBindEquipmentAsync로 통합)

// Fetch 메서드는 유지 (각 탭별로 API 호출 로직이 다름)
```

### 2.4 효과

- **코드 감소:** 150줄 → 50줄 (67% 감소)
- **에러 처리 일관성:** 모든 탭이 동일한 에러 처리
- **로깅 일관성:** 모든 탭의 로드 결과를 동일하게 로깅
- **유지보수:** 에러 처리 변경 시 한 곳만 수정

---

## 3️⃣ Phase 3: Event Handler 통합

### 3.1 Designer.cs에 추가할 필수 Event 등록

```csharp
// Designer.cs의 InitializeComponent() 메서드 - 이벤트 등록 항목 추가

// 이미 등록된 것:
this.tilePrevProcess.ItemClick += tilePrevProcess_ItemClick;
this.tileNextProcess.ItemClick += tileNextProcess_ItemClick;
this.tileSkipProcess.ItemClick += tileSkipProcess_ItemClick;
this.tileStopProcess.ItemClick += tileStopProcess_ItemClick;
this.tileStartProcess.ItemClick += tileStartProcess_ItemClick;
this.tileProcessList.ItemClick += tileProcessList_ItemClick;
this.tileAdminAlarm.ItemClick += tileAdminAlarm_ItemClick;

this.btnTabMaterial.Click += btnTabMaterial_Click;
this.btnTabBomSpec.Click += btnTabBomSpec_Click;
this.btnTabInspection.Click += btnTabInspection_Click;
this.btnTabEquipment.Click += btnTabEquipment_Click;

this.btnSaveResults.Click += btnSaveResults_Click;

// 추가해야 할 것:
this.Load += PopScreen_Load;
this.Disposed += PopScreen_Disposed;

this.viewerSurface.Paint += ViewerSurface_Paint;
this.viewerSurface.Resize += ViewerSurface_Resize;

this.gridViewMaterial.RowCellClick += GridViewMaterial_RowCellClick;
this.gridViewBomSpec.ShowingEditor += GridViewBomSpec_ShowingEditor;
this.gridViewInspection.ShowingEditor += GridViewInspection_ShowingEditor;

// ... 기타 Grid Events
```

### 3.2 PopScreen.cs에서 제거할 Event 중복 등록

```csharp
// 제거 전 (PopScreen.cs의 Init 또는 Load)
private void RegisterEventHandlers()  // 이 메서드 제거
{
	// ❌ 제거 대상 (Designer에서 이미 등록)
	// Load += PopScreen_Load;
	// Disposed += PopScreen_Disposed;
	// viewerSurface.Paint += ViewerSurface_Paint;

	// ✓ 유지 (동적 이벤트는 코드만 가능)
	// 특별한 경우만 런타임 등록
}
```

### 3.3 효과

- **이벤트 관리 단순화:** Designer에서 모든 시각적 Event 관리
- **코드 응집도:** UI 구조와 Event가 함께 관리
- **버그 감소:** 이벤트 누락/중복 등록 방지

---

## 4️⃣ Phase 4: Theme 관리 통합

### 4.1 개선된 ApplyTheme() 메서드

```csharp
/// <summary>
/// 모든 UI 컨트롤의 Theme을 통합으로 관리합니다.
/// 기존의 산발적인 Theme 호출을 여기로 집중합니다.
/// </summary>
private void ApplyTheme()
{
	// ✓ Tile Strip
	AppTheme.TileStrip(tileProcess, AppTheme.Indigo);

	// ✓ Process Tiles (7개)
	var tileConfigs = new[]
	{
		(tilePrevProcess, "이전", AppTheme.Indigo),
		(tileNextProcess, "다음", AppTheme.Indigo),
		(tileSkipProcess, "건너뛰기", AppTheme.Rose),
		(tileStopProcess, "중지", AppTheme.Rose),
		(tileStartProcess, "시작", AppTheme.Emerald),
		(tileProcessList, "목록", AppTheme.Blue),
		(tileAdminAlarm, "알람", AppTheme.Violet)
	};

	foreach (var (tile, label, color) in tileConfigs)
	{
		AppTheme.Tile(tile, color, AppTheme.TileSize.Large);
		// 텍스트 설정도 필요하면 여기서
	}

	// ✓ Info Card Fields (11개 레이블 + 에디터)
	ThemeInfoCardLabels();
	ThemeInfoCardEditors();

	// ✓ Grid Themes (기존 각 탭 Init에서 제거)
	// 이미 InitializeAllTabsAsync()에서 처리
	// AppTheme.Grid(gridMaterial, gridViewMaterial, AppTheme.Emerald);
	// ... 기타

	// ✓ Process State Banner
	ThemeStyleProcessStateBanner();

	// ✓ 기타 컨트롤
	ThemeOtherControls();

	// 모든 Theme 적용 완료
	Logger.Info("Theme 적용 완료");
}

private void ThemeInfoCardLabels()
{
	var labels = new[]
	{
		lblWorkOrderNo, lblItemCode, lblProductSerialNo,
		lblRoutingGroup, lblRoutingOperation, lblPageOperation,
		lblPageStartTime, lblPageEndTime, lblPageRunTime,
		lblPageIdleTime, lblPageWorkTime
	};

	foreach (var label in labels)
	{
		AppTheme.Caption(label);
	}
}

private void ThemeInfoCardEditors()
{
	var editors = new[]
	{
		txtWorkOrderNo, txtItemCode, txtProductSerialNo,
		txtRoutingGroup, txtRoutingOperation, txtPageOperation,
		txtPageStartTime, txtPageEndTime, txtPageRunTime,
		txtPageIdleTime, txtPageWorkTime
	};

	foreach (var editor in editors)
	{
		AppTheme.FieldEditor(editor); // 기존 메서드 호출
	}
}

private void ThemeStyleProcessStateBanner()
{
	AppTheme.CardPanel(bannerPanel);
	lblProgressState.ForeColor = AppTheme.TextDark;
	lblWorkstation.ForeColor = AppTheme.TextDark;
}

private void ThemeOtherControls()
{
	AppTheme.CardPanel(fieldPanel);
	AppTheme.CardPanel(infoCard);
	AppTheme.CardPanel(toolbarCard);
	AppTheme.CardPanel(bottomCard);
}
```

### 4.2 각 탭의 Init 메서드에서 Theme 제거

**Before:**
```csharp
// PopScreen.Material.cs
public void MaterialInit()
{
	AppTheme.Grid(gridMaterial, gridViewMaterial, AppTheme.Emerald);  // ❌ 제거
	ConfigureMaterialGrid();
	LoadMaterialAsync();
}
```

**After:**
```csharp
// PopScreen.Material.cs - 이 메서드는 제거
// 모든 초기화는 InitializeAllTabsAsync() 통합 메서드가 처리
```

### 4.3 Grid Configuration에서 Theme 제거

```csharp
// Before
private void ConfigureMaterialGrid()
{
	gridMaterial.DataSource = new List<PopInputMaterialDto>();
	// 칼럼 설정만 여기서
	GridColumns.Apply(gridViewMaterial, Language.Col_No, ...);
	gridViewMaterial.OptionsView.ColumnAutoWidth = true;

	// ❌ Theme 제거 (이미 InitializeAllTabsAsync에서 적용)
	// AppTheme.Grid(gridMaterial, gridViewMaterial, AppTheme.Emerald);
}
```

### 4.4 효과

- **런타임 깜빡임 제거:** Theme이 한 번에 적용
- **관리 단순화:** Theme 변경 시 한 곳만 수정
- **로드 순서 명확:** Theme 적용 → 데이터 로드 순서 보장

---

## 5️⃣ Phase 5: Visibility 관리 개선

### 5.1 개선된 SetActiveTab 메서드

```csharp
/// <summary>
/// 활성 탭을 전환하고 포함된 모든 컨트롤의 Visibility를 관리합니다.
/// </summary>
private void SetActiveTab(PopTab tab)
{
	// 탭별 가시성 맵 (데이터 기반)
	var tabVisibilityMap = new Dictionary<PopTab, (GridControl Grid, Control[] AdditionalControls)>
	{
		{
			PopTab.Material,
			(gridMaterial, new Control[] { })
		},
		{
			PopTab.BomSpec,
			(gridBomSpec, new Control[] { chkManualWrench, btnSaveResults })
		},
		{
			PopTab.Inspection,
			(gridInspection, new Control[] { btnSaveResults })
		},
		{
			PopTab.Equipment,
			(gridEquipment, new Control[] { })
		}
	};

	// 모든 그리드 숨기기
	foreach (var config in _tabConfigs)
	{
		config.GridControl.Visible = false;
	}

	// 위험: 여기서 부분적으로 관리되는 컨트롤 숨기기
	chkManualWrench.Visible = false;
	btnSaveResults.Visible = false;

	// 활성 탭만 표시
	if (tabVisibilityMap.TryGetValue(tab, out var visibility))
	{
		visibility.Grid.Visible = true;
		foreach (var control in visibility.AdditionalControls)
		{
			control.Visible = true;
		}
	}

	// 탭 버튼 활성화 표시 (스타일)
	UpdateTabButtonStyles(tab);

	Logger.Info($"활성 탭 변경: {tab}");
}

private void UpdateTabButtonStyles(PopTab tab)
{
	// 모든 탭 버튼 비활성화 스타일
	btnTabMaterial.Appearance.Options.UseBorderColor = false;
	btnTabBomSpec.Appearance.Options.UseBorderColor = false;
	btnTabInspection.Appearance.Options.UseBorderColor = false;
	btnTabEquipment.Appearance.Options.UseBorderColor = false;

	// 활성 탭 버튼만 강조 스타일
	switch (tab)
	{
		case PopTab.Material:
			btnTabMaterial.Appearance.Options.UseBorderColor = true;
			btnTabMaterial.Appearance.BorderColor = AppTheme.Emerald;
			break;
		case PopTab.BomSpec:
			btnTabBomSpec.Appearance.Options.UseBorderColor = true;
			btnTabBomSpec.Appearance.BorderColor = AppTheme.Indigo;
			break;
		case PopTab.Inspection:
			btnTabInspection.Appearance.Options.UseBorderColor = true;
			btnTabInspection.Appearance.BorderColor = AppTheme.Violet;
			break;
		case PopTab.Equipment:
			btnTabEquipment.Appearance.Options.UseBorderColor = true;
			btnTabEquipment.Appearance.BorderColor = AppTheme.Cyan;
			break;
	}
}
```

### 5.2 효과

- **가시성 관리 집중화:** 탭별 컨트롤 가시성이 명확
- **유지보수 용이:** 새 탭 추가 시 Dictionary에만 추가
- **버그 감소:** Visibility 누락/중복 제거

---

## 6️⃣ 최종 Integration: PopScreen.cs 구조

### 6.1 새로운 메서드 배치 순서

```csharp
namespace SEOJIN_TMES.Client.Forms.Pop
{
	public partial class PopScreen : Form
	{
		// ========== 상수/설정 ==========
		private const double DefaultZoom = 1.0;
		// ... 기타 상수

		// ========== 필드 ==========
		private TabInitConfig[] _tabConfigs;
		private List<PopInputMaterialDto> _materials;
		private List<PopBomSpecDto> _bomSpecs;
		// ... 기타 필드

		// ========== Tab 초기화 관련 ==========
		private void InitializeTabConfigs() { ... }
		private async Task InitializeAllTabsAsync() { ... }

		// ========== Data Loading 통합 ==========
		private async Task<List<T>> LoadAndBindGridAsync<T>(...) where T : class { ... }
		private async Task FetchAndBindMaterialAsync() { ... }
		private async Task FetchAndBindBomAsync() { ... }
		private async Task FetchAndBindInspectionAsync() { ... }
		private async Task FetchAndBindEquipmentAsync() { ... }

		// ========== 기존 Fetch 메서드 (유지) ==========
		private async Task<List<PopInputMaterialDto>> FetchMaterialsAsync() { ... }
		private async Task<List<PopBomSpecDto>> FetchBomSpecsAsync() { ... }
		// ... 기타 Fetch

		// ========== Grid Configuration (유지하되 정리) ==========
		private void ConfigureMaterialGrid() { ... }
		private void ConfigureBomSpecGrid() { ... }
		// ... 기타 Configure

		// ========== Theme 통합 관리 ==========
		private void ApplyTheme() { ... }  // 모든 Theme 집중
		private void ThemeInfoCardLabels() { ... }
		private void ThemeInfoCardEditors() { ... }

		// ========== 이벤트 핸들러 ==========
		private void PopScreen_Load(object sender, EventArgs e) { ... }
		private void PopScreen_Disposed(object sender, EventArgs e) { ... }
		private void ViewerSurface_Paint(object sender, PaintEventArgs e) { ... }
		// ... 기타 핸들러

		// ========== UI 상태 관리 ==========
		private void SetActiveTab(PopTab tab) { ... }
		private void UpdateTabButtonStyles(PopTab tab) { ... }

		// ========== 프로세스 제어 (기존) ==========
		private async Task RequestProcessActionAsync(...) { ... }
		private async Task MoveByProcessListAsync(...) { ... }
		// ... 기타 프로세스 로직
	}
}
```

### 6.2 호출 흐름

```
PopScreen_Load()
	├─ InitializeTabConfigs()          // 탭 설정 배열 생성
	├─ ApplyTheme()                    // UI Theme 적용
	├─ StartClock()                    // 시계 시작
	├─ await InitializeAllTabsAsync()  // 모든 탭 초기화 (통합)
	│   ├─ foreach TabConfig in _tabConfigs
	│   │   ├─ AppTheme.Grid()         // Theme
	│   │   ├─ config.ConfigureColumns()  // 칼럼 설정
	│   │   └─ await config.LoadDataAsync()  // 데이터 로드
	│   │       ├─ await FetchXxxAsync()    // API 호출
	│   │       └─ await LoadAndBindGridAsync()  // 바인딩 + 에러 처리
	└─ SetActiveTab(PopTab.BomSpec)    // 초기 탭 선택
```

---

## 📊 마이그레이션 체크리스트

### Step 1: 탭 설정 데이터화 ✅
- [ ] TabInitConfig 레코드 정의
- [ ] _tabConfigs 배열 초기화
- [ ] InitializeAllTabsAsync() 구현
- [ ] 기존 MaterialInit(), BomInit(), InspectionInit(), EquipmentInit() 제거 준비

### Step 2: Data Loading 통합 ✅
- [ ] LoadAndBindGridAsync<T>() 구현
- [ ] FetchAndBindMaterialAsync() 등 Async Wrapper 구현
- [ ] 기존 LoadMaterialAsync() 제거 준비
- [ ] ApplyBomSpecHighlighting() 등 추가 로직 분격

### Step 3: Event Handler 통합 ✅
- [ ] Designer.cs 검증 (이미 등록된 핸들러)
- [ ] 누락된 Event 등록 (Load, Disposed 등)
- [ ] 코드의 RegisterEventHandlers() 제거
- [ ] 이벤트 중복 등록 검증

### Step 4: Theme 통합 ✅
- [ ] 새로운 ApplyTheme() 구현
- [ ] ThemeInfoCardLabels(), ThemeInfoCardEditors() 구현
- [ ] 각 탭의 Init에서 AppTheme.Grid() 제거
- [ ] 테마 적용 순서 검증

### Step 5: Visibility 관리 ✅
- [ ] SetActiveTab() 개선 (Dictionary 기반)
- [ ] UpdateTabButtonStyles() 구현
- [ ] 새 탭 추가 시 2곳만 수정하도록 검증

### Step 6: Partial 파일 정리 ✅
- [ ] Material, Bom, Inspection 통합 검토
- [ ] PopScreen.Tabs.cs 생성 (선택적)
- [ ] 불필요한 Using 제거
- [ ] 파일 크기 검증

---

## 🧪 테스트 계획

### Unit Tests (제안)
```csharp
[Test]
public void InitializeTabConfigs_Should_CreateFourTabConfigs()
{
	// Arrange
	var screen = new PopScreen();

	// Act
	screen.InitializeTabConfigs();

	// Assert
	Assert.AreEqual(4, screen._tabConfigs.Length);
}

[Test]
public async Task LoadAndBindGridAsync_Should_HandleApiException()
{
	// Arrange
	var mockGrid = new Mock<GridControl>();
	var mockFetch = new Mock<Func<Task<List<object>>>>();
	mockFetch.Setup(f => f()).ThrowsAsync(new ApiException("Test Error"));

	// Act
	var result = await screen.LoadAndBindGridAsync(mockGrid.Object, mockFetch.Object, "Test");

	// Assert
	Assert.AreEqual(0, result.Count);
}
```

### Integration Tests (수동)
1. 각 탭 로드 및 데이터 표시 검증
2. 탭 전환 시 Visibility 변경 검증
3. Theme 적용 (깜빡임 없음) 검증
4. 이벤트 핸들러 중복 등록 확인
5. 예외 처리 및 에러 메시지 표시 검증

---

**완성 후 예상 결과:**
- 코드 라인 수: 3,544 → 2,800 (21% 감소)
- 중복도: 14% → 3% (79% 감소)
- 파일 수: 6 → 4 (선택적)
- 유지보수성: 크게 향상 ⬆️
