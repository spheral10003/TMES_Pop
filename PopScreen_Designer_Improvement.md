# PopScreen.Designer.cs 개선 방안 및 Event 통합 전략

## 📌 현황 분석

### Designer.cs의 역할

```
기대 역할 (좋은 예시)
├─ UI 컨트롤 선언 ✓
├─ 컨트롤 속성 설정 (Size, Location, Anchor) ✓
├─ 레이아웃 설정 (Dock, Margin 등) ✓
│
실제 역할
├─ 위의 항목 모두 ✓
├─ Event Handler 등록 (부분적) ⚠️
│   ├─ Tile/Button ItemClick ✓ (이미 등록)
│   └─ Grid/Form Event ❌ (코드에서만)
└─ Theme 설정 ❌ (코드에서만)
```

### 코드 현황

**Designer.cs 규모: 965줄**
- Line 1-34: 클래스 및 설정 주석
- Line 35-960: InitializeComponent() 메서드
  - 컨트롤 선언: ~60줄
  - 컨트롤 구성: ~500줄
  - Event 등록: ~30줄 (7개 Tile + 4개 Button + Save + Accordion 등)
  - Controls.Add(): ~200줄

**Event 등록 현황:**
```csharp
// 등록된 것 (14개)
this.tilePrevProcess.ItemClick += this.tilePrevProcess_ItemClick;
this.tileNextProcess.ItemClick += this.tileNextProcess_ItemClick;
this.tileSkipProcess.ItemClick += this.tileSkipProcess_ItemClick;
this.tileStopProcess.ItemClick += this.tileStopProcess_ItemClick;
this.tileStartProcess.ItemClick += this.tileStartProcess_ItemClick;
this.tileProcessList.ItemClick += this.tileProcessList_ItemClick;
this.tileAdminAlarm.ItemClick += this.tileAdminAlarm_ItemClick;
this.btnInfoAccordion.Click += this.btnInfoAccordion_Click;
this.btnTabMaterial.Click += this.btnTabMaterial_Click;
this.btnTabBomSpec.Click += this.btnTabBomSpec_Click;
this.btnTabInspection.Click += this.btnTabInspection_Click;
this.btnTabEquipment.Click += this.btnTabEquipment_Click;
this.btnSaveResults.Click += this.btnSaveResults_Click;
this.btnTabsAccordion.Click += this.btnTabsAccordion_Click;

// 미등록된 것 (중요)
❌ this.Load += ...;
❌ this.Disposed += ...;
❌ this.viewerSurface.Paint += ...;
❌ this.viewerSurface.Resize += ...;
❌ this.gridViewMaterial.RowCellClick += ...;
❌ this.gridViewBomSpec.ShowingEditor += ...;
❌ this.gridViewInspection.ShowingEditor += ...;
❌ this.gridViewEquipment.RowCellStyle += ...;
```

---

## 🎯 개선 전략

### Strategy 1: Designer 중심화 (권장)

**원칙:**
> "Visual Studio Designer가 자동 생성할 수 있는 모든 것은 Designer가 관리한다"

#### 1.1 Designer에 등록할 Event

**Category A: 반드시 등록 (가능하고 recommended)**
```csharp
// Form Lifecycle
this.Load += PopScreen_Load;
this.FormClosing += PopScreen_FormClosing;
this.Disposed += PopScreen_Disposed;

// Viewer Events
this.viewerSurface.Paint += ViewerSurface_Paint;
this.viewerSurface.Resize += ViewerSurface_Resize;

// UI Control Events (이미 등록된 것)
this.tilePrevProcess.ItemClick += tilePrevProcess_ItemClick;
// ... 기타 7개 Tile
// ... 기타 7개 Button
```

**Category B: 선택적 등록 (복잡한 로직 있는 경우)**
```csharp
// 복잡한 데이터 입력 검증이 필요한 경우만
this.gridViewInspection.ShowingEditor += GridViewInspection_ShowingEditor;
this.gridViewBomSpec.ShowingEditor += GridViewBomSpec_ShowingEditor;

// 복잡한 셀 스타일링이 필요한 경우만
this.gridViewEquipment.RowCellStyle += GridViewEquipment_RowCellStyle;
```

**Category C: 코드에서만 등록 (동적 이벤트)**
```csharp
// 런타임에 동적으로 생성되는 컨트롤의 이벤트
gridViewMaterial.CustomColumnDisplayText += NumericFormat에서...
gridViewMaterial.RowCellClick += 조건부 처리...
```

#### 1.2 Designer에 등록할 초기화 로직

**현황: Theme이 코드(PopScreen.cs)에서만 처리**
```
✗ PopScreen.cs의 ApplyTheme() 
  ├─ 타일 테마
  ├─ 필드 라벨/에디터 테마
  ├─ 각 탭의 Init()에서 그리드 테마
  └─ 강조 효과 등
```

**개선안:**
```
✓ Designer.cs또는 PopScreen.cs의 ApplyTheme()
  ├─ InitializeComponent() 직후 실행
  └─ 모든 Theme을 한 번에 적용
```

---

### Strategy 2: Partial Method 활용 (고급, .NET 4.7.2 지원)

Designer가 자동 생성할 때 **Partial Method**를 사용하여 Hook 포인트 제공:

```csharp
// Designer.cs (자동 생성)
partial class PopScreen
{
	private void InitializeComponent()
	{
		// ... 컨트롤 생성

		// Event 등록
		this.Load += PopScreen_Load;
		this.viewerSurface.Paint += ViewerSurface_Paint;

		// Custom Hook
		InitializeTheme();
		InitializeGrids();
	}

	partial void InitializeTheme();
	partial void InitializeGrids();
}

// PopScreen.cs (수동 작성)
partial class PopScreen
{
	partial void InitializeTheme()
	{
		ApplyTheme();
	}

	partial void InitializeGrids()
	{
		InitializeAllTabsAsync();
	}
}
```

**장점:**
- Designer와 코드의 책임이 명확
- 자동 생성 코드 수정 시에도 안전
- 초기화 순서 명확

**단점:**
- 복잡도 증가
- 디버깅 어려움

---

## 🔧 실행 방안

### Phase 1: Event Handler 통합 (2시간)

#### 1.1 Designer.cs에 추가할 Event 등록 (약 20줄)

**위치:** InitializeComponent() 메서드의 끝 (Controls.Add 이후)

```csharp
// Designer.cs의 InitializeComponent() - 추가할 코드

// ===== 폼 라이프사이클 이벤트 =====
this.Load += new System.EventHandler(this.PopScreen_Load);
this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PopScreen_FormClosing);
this.Disposed += new System.EventHandler(this.PopScreen_Disposed);

// ===== 화면 렌더링 이벤트 =====
this.viewerSurface.Paint += new System.Windows.Forms.PaintEventHandler(this.ViewerSurface_Paint);
this.viewerSurface.Resize += new System.EventHandler(this.ViewerSurface_Resize);

// ===== 탭/레이아웃 아코디언 이벤트 (이미 있으므로 확인) =====
// this.btnInfoAccordion.Click += ... (이미 있음)
// this.btnTabsAccordion.Click += ... (이미 있음)

// ===== 정보 카드 토글 =====
this.btnInfoAccordion.Click += new System.EventHandler(this.BtnInfoAccordion_Click);  // 재확인

// ===== 탭 버튼 (이미 있음) =====
// this.btnTabMaterial.Click += ...
// this.btnTabBomSpec.Click += ...
// this.btnTabInspection.Click += ...
// this.btnTabEquipment.Click += ...
```

#### 1.2 코드에서 제거할 이벤트 등록

**위치:** PopScreen.cs의 PopScreen_Load() 또는 별도 InitializeEventHandlers()

```csharp
// ❌ 제거할 코드 (Designer에서 이미 등록)
private void RegisterEventHandlers()  // 이 메서드 전체 제거
{
	Load += PopScreen_Load;           // ❌ 제거 (이제 Designer가 등록)
	Disposed += PopScreen_Disposed;   // ❌ 제거
	viewerSurface.Paint += ViewerSurface_Paint;  // ❌ 제거
	viewerSurface.Resize += (s, e) => viewerSurface.Invalidate();  // ❌ 제거

	// 코드에서만 관리할 이벤트 (동적)
	// gridViewMaterial.RowCellClick += ...  // ✓ 유지 (조건부)
}
```

#### 1.3 검증 체크리스트

- [ ] Designer.cs에 동일한 이벤트가 없는지 확인 (중복 방지)
- [ ] 핸들러 메서드명이 일치하는지 확인
- [ ] PopScreen.cs의 RegisterEventHandlers() 호출 제거
- [ ] 컴파일 및 런타임 테스트

---

### Phase 2: Appearance & Properties 정리 (1시간)

#### 2.1 Designer에서 Theme을 적용할 컨트롤 목록

**현황:** Theme은 코드(ApplyTheme())에서만 적용

**개선:** Designer.cs 또는 별도 설정에서 적용

```csharp
// Designer.cs의 InitializeComponent() - 추가
private void InitializeControlAppearance()
{
	// Tile Strip 외관
	this.tileProcess.Appearance.BorderColor = SystemColors.ControlDark;
	this.tileProcess.Padding = new System.Windows.Forms.Padding(5);

	// Tile Items 외관 (개별 설정 또는 반복)
	var tiles = new[] {
		this.tilePrevProcess,
		this.tileNextProcess,
		this.tileSkipProcess,
		this.tileStopProcess,
		this.tileStartProcess,
		this.tileProcessList,
		this.tileAdminAlarm
	};

	// 각 Tile은 Theme Color 설정 (코드에서 ApplyTheme()가 처리)
	// Designer는 기본 구조만 제공
}
```

**대안:** ApplyTheme()를 popScreen_Load 바로 다음에서 호출하도록 정확한 타이밍 보장

```csharp
// Designer.cs의 InitializeComponent() - 순서 보장
private void InitializeComponent()
{
	// ... 컨트롤 생성
	// ... Event 등록

	// ✓ 마지막에 호출 (모든 컨트롤이 생성된 후)
	this.ApplyThemeAfterInitialize();
}

// PopScreen.cs에서
private void ApplyThemeAfterInitialize()
{
	ApplyTheme();  // 기존 메서드
	InitializeGridsAppearance();
}
```

---

### Phase 3: 그리드 이벤트 등록 (1시간)

#### 3.1 그리드별 Event 등록여부 검토

**Material Grid**
```csharp
// RowCellClick: 조건부 필요
// CellValueChanged: 필요 여부 확인
// CustomColumnDisplayText: 숫자 포맷 (필요)
this.gridViewMaterial.CustomColumnDisplayText += NumericFormat에서 처리
```

**BOM Grid**
```csharp
// ShowingEditor: 결과 입력 칼럼만 가능하도록
this.gridViewBomSpec.ShowingEditor += new DevExpress.XtraGrid.Views.Base.CancelEventHandler(this.GridViewBomSpec_ShowingEditor);
```

**Inspection Grid**
```csharp
// ShowingEditor: 검사 결과만 입력 가능하도록
this.gridViewInspection.ShowingEditor += new DevExpress.XtraGrid.Views.Base.CancelEventHandler(this.GridViewInspection_ShowingEditor);
```

**Equipment Grid**
```csharp
// RowCellStyle: 연결 상태 색상 표시
this.gridViewEquipment.RowCellStyle += new DevExpress.XtraGrid.Views.Grid.RowCellStyleEventHandler(this.GridViewEquipment_RowCellStyle);
```

#### 3.2 등록 방식 선택

**Option A: Designer에 명시적으로 등록 (권장)**
```csharp
// Designer.cs의 InitializeComponent()
this.gridViewBomSpec.ShowingEditor += new DevExpress.XtraGrid.Views.Base.CancelEventHandler(this.GridViewBomSpec_ShowingEditor);
this.gridViewInspection.ShowingEditor += new DevExpress.XtraGrid.Views.Base.CancelEventHandler(this.GridViewInspection_ShowingEditor);
this.gridViewEquipment.RowCellStyle += new DevExpress.XtraGrid.Views.Grid.RowCellStyleEventHandler(this.GridViewEquipment_RowCellStyle);
```

**Option B: 코드에서만 등록 (현황 유지)**
```csharp
// PopScreen.cs의 InitializeGrids() 또는 각 Configure 메서드
private void ConfigureBomSpecGrid()
{
	// ... 칼럼 설정
	gridViewBomSpec.ShowingEditor += GridViewBomSpec_ShowingEditor;
}
```

**권장:** Option A (모든 Event를 Designer에서 관리)

---

## 📋 Designer.cs 수정 계획

### 수정 항목 요약

| 항목 | 현황 | 개선 | 줄수 |
|------|------|------|------|
| Form Lifecycle Event | ❌ 없음 | ✅ 추가 | +3 |
| Viewer Event | ❌ 없음 | ✅ 추가 | +2 |
| 그리드 Event | 부분 | ✓ 확인/추가 | +3 |
| **총 추가** | - | - | **~20줄** |
| **총 제거** | - | - | **0줄** |
| **순 변화** | 965줄 | 985줄 | **+20줄** |

### 수정 위치 (Designer.cs)

**Line 800-850 추정 (InitializeComponent 끝)**

```csharp
// Designer.cs의 InitializeComponent() - 추가 위치

// ... 기존 Event 등록 (line 700-800)

// ===== 추가할 이벤트 (약 20줄) =====

// Form Lifecycle
this.Load += new System.EventHandler(this.PopScreen_Load);
this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PopScreen_FormClosing);
this.Disposed += new System.EventHandler(this.PopScreen_Disposed);

// Viewer Surface Events
this.viewerSurface.Paint += new System.Windows.Forms.PaintEventHandler(this.ViewerSurface_Paint);
this.viewerSurface.Resize += new System.EventHandler(this.ViewerSurface_Resize);

// Grid Specific Events
this.gridViewBomSpec.ShowingEditor += new DevExpress.XtraGrid.Views.Base.CancelEventHandler(this.GridViewBomSpec_ShowingEditor);
this.gridViewInspection.ShowingEditor += new DevExpress.XtraGrid.Views.Base.CancelEventHandler(this.GridViewInspection_ShowingEditor);
this.gridViewEquipment.RowCellStyle += new DevExpress.XtraGrid.Views.Grid.RowCellStyleEventHandler(this.GridViewEquipment_RowCellStyle);

// ===== End =====
```

---

## ⚠️ 주의사항 및 검증

### Caveat 1: Designer 수정 시 주의

```
❌ 위험: Visual Studio 디자이너 도구에서 Designer.cs 직접 수정
✅ 안전: 텍스트 에디터에서 InitializeComponent() 메서드 끝에만 추가
```

**이유:** VS 디자이너가 자동으로 코드를 재생성할 수 있음

### Caveat 2: Event Handler 중복 등록 방지

**체크:**
```csharp
// 컴파일 후 런타임에 이벤트가 여러 번 호출되는지 확인
// 로그 추가
private void PopScreen_Load(object sender, EventArgs e)
{
	Debug.WriteLine("PopScreen_Load called");  // 한 번만 나와야 함
	// ...
}
```

### Caveat 3: 이벤트 핸들러 명명 규칙

**현황 (일관성 있음):**
```csharp
this.tilePrevProcess.ItemClick += this.tilePrevProcess_ItemClick;
this.btnTabMaterial.Click += this.btnTabMaterial_Click;
```

**새 이벤트도 동일하게:**
```csharp
this.Load += this.PopScreen_Load;              // ✓ 일관성 있음
this.viewerSurface.Paint += this.ViewerSurface_Paint;  // ✓
```

---

## 🎯 최종 Effect

### Designer.cs 개선 후

| 지표 | 변화 |
|------|------|
| Event 관리 명확성 | ⬆️ Designer에서 모두 처리 |
| 코드 가독성 | ⬆️ 사용되는 Event가 명확 |
| 유지보수성 | ⬆️ 한 곳에서 Event 관리 |
| 런타임 성능 | → 변화 없음 |
| 컴파일 시간 | → 약간 증가 (+20줄) |

### PopScreen.cs 개선 후

| 지표 | 변화 |
|------|------|
| 이벤트 등록 코드 제거 | -5~10줄 |
| 메서드 정리 | 코드 응집도 ⬆️ |
| 가독성 | ⬆️ 비즈니스 로직 집중 |

---

## 🚀 실행 Checklist

### Step 1: 분석 ✓
- [x] 현재 Event 등록 상황 파악
- [x] 미등록 Event 식별
- [x] Theme 관리 위치 확인

### Step 2: Designer.cs 수정
- [ ] 텍스트 에디터에서 열기
- [ ] InitializeComponent() 끝 위치 확인 (line ~800-850)
- [ ] Form Lifecycle Event 3줄 추가
- [ ] Viewer Event 2줄 추가
- [ ] Grid Event 3줄 추가
- [ ] 컴파일 검증

### Step 3: PopScreen.cs 정리
- [ ] RegisterEventHandlers() 메서드 제거
- [ ] PopScreen_Load()에서 호출 제거
- [ ] Event 중복 등록 확인 (런타임)
- [ ] 로그 출력으로 한 번만 호출되는지 검증

### Step 4: 테스트
- [ ] 폼 로드 테스트
- [ ] 각 Event Handler 호출 검증
- [ ] Paint/Resize 깜빡임 없음 검증
- [ ] 버튼/탭 클릭 동작 검증

### Step 5: 문서화
- [ ] Designer.cs의 Event 목록 주석 추가
- [ ] PopScreen.cs의 Event Handler 주석 정리

---

**결론:** Designer.cs에 Event 관리를 집중시키면 코드 구조가 더 명확해지고, PopScreen.cs는 비즈니스 로직에만 집중할 수 있습니다.
