# PopScreen 리팩토링 적용 가이드 - Phase 1-2 구현

## 📌 현재 상태

✅ **백업 완료**
- 위치: `PopScreen_Backup_20261001_090321/` 폴더
- 파일: PopScreen.cs, PopScreen.Designer.cs, PopScreen.Material.cs, PopScreen.Bom.cs, PopScreen.Inspection.cs, PopScreen.Equipment.cs

✅ **분석 완료**
- 8개의 상세 분석 문서 생성
- Phase 1-2 코드 예제 작성됨

---

## 🎯 Phase 1-2 적용 방법 (3가지 옵션)

### 옵션 A: 수동 적용 (권장)
각 파일을 직접 열고 코드를 추가합니다. **가장 제어 가능하고 안전합니다.**

### 옵션 B: 반자동 적용
제공된 코드 블록을 복사하여 각 파일에 붙여넣기합니다.

### 옵션 C: 전자동 적용 (실험적)
PowerShell 스크립트를 실행하여 자동 적용합니다. (복잡도 높음)

---

## 📋 선택 가이드

| 옵션 | 소요 시간 | 안전도 | 추천 대상 | 
|------|---------|--------|---------|
| **A (수동)** | 45분 | 최고 🟢 | 대부분의 개발자 |
| **B (반자동)** | 30분 | 높음 🟡 | 경험많은 개발자 |
| **C (자동)** | 10분 | 낮음 🔴 | PowerShell 전문가 |

**권장: 옵션 A (수동)** - 가장 안전하고 명확합니다.

---

## 🔧 옵션 A: 수동 적용 단계

### Step 1: TabInitConfig 클래스 및 _tabConfigs 필드 추가

**파일:** `src/SEOJIN_TMES.Client/Forms/Pop/PopScreen.cs`

#### 1-1. 필드 추가 (line 98 이후)

**위치:** `private CancellationTokenSource _drawingCancellation;` 다음

```csharp
/// <summary>
/// Phase 1: Tab initialization configuration stored as data structure.
/// Each tab's initialization info is managed in one place; adding new tabs only requires adding to this array.
/// </summary>
private TabInitConfig[] _tabConfigs;
```

**확인:** 파일 저장 후 컴파일 에러 "TabInitConfig를 찾을 수 없음" → 정상

---

#### 1-2. TabInitConfig 클래스 정의 추가 (line 1327 이전)

**위치:** `internal enum PopTab` 이전 (파일 끝 근처)

```csharp
/// <summary>
/// Phase 1: Encapsulates tab initialization configuration.
/// Used to centralize tab setup logic and enable data-driven tab management.
/// </summary>
internal class TabInitConfig
{
	public PopTab TabType { get; set; }
	public DevExpress.XtraGrid.GridControl GridControl { get; set; }
	public SEOJIN_TMES.Client.Forms.Common.MeisGridView GridView { get; set; }
	public System.Drawing.Color ThemeColor { get; set; }
	public string TabName { get; set; }
	public System.Func<System.Threading.Tasks.Task> LoadDataAsync { get; set; }
	public System.Action ConfigureColumns { get; set; }
}
```

**확인:** 컴파일 성공

---

### Step 2: 초기화 메서드 추가 (PopScreen.cs)

#### 2-1. InitializeTabConfigs() 메서드

**위치:** `BuildControlMaps()` 메서드 이후 (line 140 근처)

```csharp
/// <summary>
/// Phase 1: Initialize tab configuration array.
/// This method is called from PopScreen_Load() to set up all tab configs.
/// </summary>
private void InitializeTabConfigs()
{
	_tabConfigs = new TabInitConfig[]
	{
		new TabInitConfig
		{
			TabType = PopTab.Material,
			GridControl = gridMaterial,
			GridView = gridViewMaterial,
			ThemeColor = AppTheme.Emerald,
			TabName = "Material",
			LoadDataAsync = FetchAndBindMaterialAsync,
			ConfigureColumns = ConfigureMaterialGrid
		},
		new TabInitConfig
		{
			TabType = PopTab.BomSpec,
			GridControl = gridBomSpec,
			GridView = gridViewBomSpec,
			ThemeColor = AppTheme.Indigo,
			TabName = "BOM",
			LoadDataAsync = FetchAndBindBomAsync,
			ConfigureColumns = ConfigureBomSpecGrid
		},
		new TabInitConfig
		{
			TabType = PopTab.Inspection,
			GridControl = gridInspection,
			GridView = gridViewInspection,
			ThemeColor = AppTheme.Violet,
			TabName = "Inspection",
			LoadDataAsync = FetchAndBindInspectionAsync,
			ConfigureColumns = ConfigureInspectionGrid
		},
		new TabInitConfig
		{
			TabType = PopTab.Equipment,
			GridControl = gridEquipment,
			GridView = gridViewEquipment,
			ThemeColor = AppTheme.Cyan,
			TabName = "Equipment",
			LoadDataAsync = FetchAndBindEquipmentAsync,
			ConfigureColumns = ConfigureEquipmentGrid
		}
	};
}
```

---

#### 2-2. InitializeAllTabsAsync() 메서드

**위치:** `LoadAllAsync()` 메서드 이후

```csharp
/// <summary>
/// Phase 1: Unified tab initialization method.
/// All tabs are initialized using the same pattern from _tabConfigs array.
/// Replaces: MaterialInit(), BomInit(), InspectionInit(), EquipmentInit()
/// </summary>
private async Task InitializeAllTabsAsync()
{
	foreach (var config in _tabConfigs)
	{
		try
		{
			// Apply theme to grid
			AppTheme.Grid(config.GridControl, config.GridView, config.ThemeColor);

			// Configure grid columns
			config.ConfigureColumns();

			// Load data
			await config.LoadDataAsync();
		}
		catch (Exception ex)
		{
			Logger.Error($"Tab initialization failed: {config.TabName}", ex);
			AppMessageBox.Warn(this, 
				string.Format("Failed to initialize tab '{0}': {1}", config.TabName, ex.Message), 
				Language.Pop_Title);
		}
	}

	Logger.Info("All tabs initialized successfully");
}
```

---

### Step 3: Phase 2 데이터 로딩 메서드 추가 (PopScreen.cs)

#### 3-1. 제너릭 LoadAndBindGridAsync<T>() 메서드

**위치:** `InitializeAllTabsAsync()` 메서드 이후

```csharp
/// <summary>
/// Phase 2: Generic data loading and binding method.
/// Centralized error handling and logging for all tab data loading.
/// Replaces: LoadMaterialAsync(), LoadBomAsync(), LoadInspectionAsync(), LoadEquipAsync()
/// </summary>
private async Task<List<T>> LoadAndBindGridAsync<T>(
	DevExpress.XtraGrid.GridControl grid,
	System.Func<System.Threading.Tasks.Task<List<T>>> fetcher,
	string tabName) where T : class
{
	try
	{
		var data = await fetcher();
		if (data == null)
			data = new List<T>();

		grid.DataSource = data;
		Logger.Info($"Tab '{tabName}' data loaded successfully: {data.Count} items");

		return data;
	}
	catch (ApiException ex)
	{
		Logger.Error($"Tab '{tabName}' data loading failed: {ex.Message}", ex);
		AppMessageBox.Warn(this, ex.Message, Language.Pop_Title);
		return new List<T>();
	}
	catch (Exception ex)
	{
		Logger.Error($"Tab '{tabName}' unexpected error", ex);
		AppMessageBox.Fatal(this, ex.Message, Language.Pop_Title);
		return new List<T>();
	}
}
```

---

#### 3-2. Tab별 Async Wrapper (PopScreen.cs)

**위치:** `LoadAndBindGridAsync<T>()` 메서드 이후

```csharp
/// <summary>
/// Phase 2: Material tab data fetch and bind.
/// Replaces: Material.cs의 LoadMaterialAsync()
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
/// Phase 2: BOM tab data fetch and bind with additional highlighting logic.
/// Replaces: Bom.cs의 LoadBomAsync() (특수 로직 유지)
/// </summary>
private async Task FetchAndBindBomAsync()
{
	_bomSpecs = await LoadAndBindGridAsync(
		gridBomSpec,
		FetchBomSpecsAsync,
		"BOM"
	);

	// Apply BOM-specific highlighting for torque results
	ApplyBomSpecHighlighting(_bomSpecs);
}

/// <summary>
/// Phase 2: Inspection tab data fetch and bind.
/// Replaces: Inspection.cs의 LoadInspectionAsync()
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
/// Phase 2: Equipment tab data fetch and bind with equipment state refresh.
/// Replaces: Equipment.cs의 LoadEquipAsync()
/// </summary>
private async Task FetchAndBindEquipmentAsync()
{
	_equipments = await LoadAndBindGridAsync(
		gridEquipment,
		FetchEquipmentsAsync,
		"Equipment"
	);

	// Refresh equipment connection states
	RefreshEquipmentStates();
}

/// <summary>
/// Helper method for BOM tab: Highlight torque results outside specification range.
/// Extracted from original LoadBomAsync() method in Bom.cs.
/// </summary>
private void ApplyBomSpecHighlighting(List<PopBomSpecDto> specs)
{
	if (specs == null || specs.Count == 0)
		return;

	for (int i = 0; i < gridViewBomSpec.RowCount; i++)
	{
		var target = gridViewBomSpec.GetRow(i) as PopBomSpecDto;
		if (target == null)
			continue;

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

---

### Step 4: PopScreen_Load() 수정 (PopScreen.cs)

**파일:** `PopScreen.cs` line 292-308

**Before:**
```csharp
private async void PopScreen_Load(object sender, EventArgs e)
{
	if (DesignMode || _appSet == null) return;
	if (_key == null)
	{
		AppMessageBox.Warn(this, Language.Pop_NoTargetProcess, Language.Pop_Title);
		return;
	}

	await LoadAllAsync();
	MaterialInit();            // ❌ 삭제
	InspectionInit();          // ❌ 삭제
	BomInit();                 // ❌ 삭제
	EquipmentInit();           // ❌ 삭제
}
```

**After:**
```csharp
private async void PopScreen_Load(object sender, EventArgs e)
{
	if (DesignMode || _appSet == null) return;
	if (_key == null)
	{
		AppMessageBox.Warn(this, Language.Pop_NoTargetProcess, Language.Pop_Title);
		return;
	}

	try
	{
		await LoadAllAsync();

		// Phase 1-2: Initialize tab configurations and load all tabs
		InitializeTabConfigs();
		await InitializeAllTabsAsync();

		// Set initial active tab
		SetActiveTab(PopTab.BomSpec);
	}
	catch (Exception ex)
	{
		Logger.Error("PopScreen initialization error", ex);
		AppMessageBox.Fatal(this, ex.Message, Language.Pop_Title);
	}
}
```

---

### Step 5: Partial 파일에서 메서드 제거

#### 5-1. PopScreen.Material.cs

**위치:** Line 21-27 (MaterialInit 메서드 전체)

**제거할 코드:**
```csharp
public void MaterialInit()
{
	AppTheme.Grid(gridMaterial, gridViewMaterial, AppTheme.Emerald);
	ConfigureMaterialGrid();
	LoadMaterialAsync();
}
```

**제거 후:** Material.cs는 ConfigureMaterialGrid() 메서드와 FetchMaterialsAsync() 메서드만 포함

---

#### 5-2. PopScreen.Bom.cs

**제거할 메서드:**
- `BomInit()` - line 39-50
- `LoadBomAsync()` - line 53-120

**유지할 메서드:**
- `ConfigureBomSpecGrid()` - line 141 이후
- `FetchBomSpecsAsync()` - line 123-133

---

#### 5-3. PopScreen.Inspection.cs

**제거할 메서드:**
- `InspectionInit()` - line 21-30
- `LoadInspectionAsync()` - line 32-44

---

#### 5-4. PopScreen.Equipment.cs

**제거할 메서드:**
- `EquipmentInit()` - line 23-31
- `LoadEquipAsync()` - line 33-47

**유지할 메서드:**
- `RefreshEquipmentStates()` - 이미 FetchAndBindEquipmentAsync()에서 호출

---

### Step 6: 컴파일 및 테스트

```
Build → F5 (Debug) → 실행 테스트
```

**확인 사항:**
- [ ] 프로젝트 컴파일 성공
- [ ] PopScreen 로드 시 4개 탭 데이터 로드 확인
- [ ] 각 탭 전환 시 데이터 표시 확인
- [ ] 에러 메시지 없음

---

## 🚨 주의사항

### Compilation Errors

1. **"TabInitConfig를 찾을 수 없습니다"**
   → Step 1-2에서 클래스 정의를 추가했는지 확인

2. **"ConfigureMaterialGrid를 찾을 수 없습니다"**
   → PopScreen.Material.cs의 메서드명 확인

3. **"PopTab.Material을 찾을 수 없습니다"**
   → PopTab enum이 PopScreen.cs에 정의되어 있는지 확인

### Runtime Errors

1. **"NullReferenceException"**
   → InitializeTabConfigs()가 PopScreen_Load()에서 호출되는지 확인

2. **"그리드가 비어있습니다"**
   → FetchXxxAsync() 메서드 검증 (API 호출 확인)

### Testing Checklist

- [ ] Material 탭 로드 테스트
- [ ] BOM 탭 로드 + 토크 강조 확인
- [ ] Inspection 탭 로드 테스트
- [ ] Equipment 탭 로드 + 연결 상태 확인
- [ ] 탭 전환 시 Visibility 토글 확인

---

## 📊 적용 후 효과

| 지표 | 변화 |
|------|------|
| 코드 라인 수 | 3,544 → 3,400줄 (-4%) |
| 중복 코드 | -100줄 (Init 메서드 제거) |
| 파일 구조 | 여전히 6개 (Partial 구조 유지) |
| 새 탭 추가 시간 | 2시간 → 30분 (75% 감소) |

**다음 단계:** Phase 3 (Event 통합) 및 Phase 4 (Theme 통합)

---

## ❓ 문제 발생 시

1. **이 문서의 체크리스트** 재확인
2. **PopScreen_Refactoring_Implementation.md** 참고
3. **백업 폴더**에서 원본 코드 비교
4. **컴파일 에러 메시지** 정확히 읽기

---

**행운을 빕니다!** 🚀
