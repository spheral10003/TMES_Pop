# 화면 구동 규칙 — 탭은 UserControl, 팝업은 Form

> 이 문서는 **화면을 무엇으로 만들고 어떻게 띄우는지**만 다룬다.
> 디자이너·테마·다국어 등 화면 코드를 쓸 때의 규칙은
> [src/SEOJIN_MEIS.Client/CLAUDE.md](src/SEOJIN_MEIS.Client/CLAUDE.md)에 있다.
> 각 화면이 업무적으로 무엇을 하는지는 [README.md](README.md)에 있다.

---

## 한 줄 규칙

| 어떻게 열리나 | 무엇으로 만드나 | 파일 이름 | 띄우는 법 |
| --- | --- | --- | --- |
| 좌측 메뉴 클릭 → **탭**으로 열림 | `XtraUserControl` | `<화면명>Screen.cs` | `MainForm`이 탭에 얹는다 |
| 화면 안에서 **팝업**으로 뜸 | `XtraForm` | `<화면명>Form.cs` | `using` + `ShowDialog(this)` |

**탭 화면을 `Form`으로 만들면 안 된다.** `MainForm`이 화면을 탭 안에 `Controls.Add()`로
자식 컨트롤처럼 넣기 때문에, `Form`을 넣으면 실행 중에 터진다
(`TopLevel = false`를 먼저 주지 않는 한 자식으로 붙지 않는다).

⚠️ **이 실수는 빌드로 안 걸린다.** `MenuNode.ScreenFactory`의 반환형이 `Func<ClientContext, Control>`이고
`Form`도 `Control`을 상속하므로 컴파일은 통과한다. **메뉴를 눌러야** 드러난다.

---

## 현재 화면 목록

| 탭 화면 (`XtraUserControl`) | 그 화면이 띄우는 팝업 (`XtraForm`) |
| --- | --- |
| `WorkOrderEntry/WorkOrderEntryScreen` | `WorkOrderEntry/WorkEntryConfirmForm` |
| `Pop/PopScreen` | `Pop/MaterialSerialForm`, `Pop/TorqueInputForm`, `Pop/AdminAlarmForm` |
| `WorkStatus/WorkStatusScreen` | `WorkStatus/ItemPickerForm` |
| `WorkIntegratedStatus/WorkIntegratedStatusScreen` | — |
| `ProductionStatus/ProductionStatusScreen` | — |

어느 탭에도 속하지 않는 공용 창(`XtraForm`):

- `LoginForm` · `MainForm` — 탭이 아니라 **앱 셸 자체**다.
- `Notification/NotificationForm` — 상단 종 아이콘에서 뜨는 알림 팝업.
- `UserProfileForm` — 상단 사용자 아이콘에서 뜨는 프로필 팝업.
- `Common/AppMessageForm` — `AppMessageBox`가 내부적으로 쓰는 메시지 창. **직접 쓰지 말 것.**

### 이름이 `Screen`/`Form`이 아닌 예외

`Common/AppMessagePanel`은 `XtraUserControl`이지만 탭 화면이 아니다 —
`AppMessageForm` 안에 얹는 내용 패널이다. 그래서 이름이 `~Panel`이다.
**탭 화면이 아닌 UserControl은 `~Screen`으로 이름 짓지 않는다.**

---

## 탭 화면 만들기

### 1. 클래스

```csharp
public partial class ProductionStatusScreen : XtraUserControl, IThemedScreen
{
    public ProductionStatusScreen() { InitializeComponent(); }   // 디자이너용, 매개변수 없음
    public void Initialize(ClientContext context) { ... }        // 의존성·문구·이벤트 배선
    public void ApplyTheme() { ... }                             // 테마 전환 시 다시 호출된다
}
```

- `IThemedScreen`([Theme/IThemedScreen.cs](src/SEOJIN_MEIS.Client/Theme/IThemedScreen.cs))은 **필수**다.
  구현하지 않으면 테마를 바꿨을 때 그 화면만 옛 색으로 남는다.
- 생성자에 매개변수를 두지 않는 이유와 `Initialize(context)` 패턴은
  [CLAUDE.md 2번](src/SEOJIN_MEIS.Client/CLAUDE.md) 참고.

### 2. 메뉴 등록

[Menu/MenuCatalog.cs](src/SEOJIN_MEIS.Client/Menu/MenuCatalog.cs)에 리프 노드로 등록한다.
**팩토리 안에서 `Initialize(ctx)`까지 호출해 완성된 화면을 돌려준다.**

```csharp
var productionStatus = new MenuNode("ProductionStatusInquiry", () => Strings.Menu_ProductionStatusInquiry, ctx =>
{
    var screen = new ProductionStatusScreen();
    screen.Initialize(ctx);
    return screen;
});
```

- `ScreenFactory`가 `null`인 노드는 대분류(폴더)이고, 화면을 열지 않는다.
- 메뉴 문구를 `Func<string>`으로 두는 이유는 언어 전환 시 즉시 갱신하기 위해서다 —
  문자열을 값으로 박으면 언어를 바꿔도 메뉴가 안 바뀐다.

### 3. 탭이 열리는 흐름

`MainForm.OpenOrFocusTab`([Forms/MainForm.cs:639](src/SEOJIN_MEIS.Client/Forms/MainForm.cs:639))이 하는 일:

```
메뉴 클릭
  → 이미 열린 탭이면 그 탭을 활성화하고 끝  (같은 화면이 두 번 열리지 않는다)
  → 아니면 ScreenFactory(ctx) 로 화면 생성
  → Dock = Fill, contentHost.Controls.Add(화면)
  → INavigatingScreen 이면 NavigateRequested 배선
  → 탭 칩 생성 후 활성화
```

- **탭 중복 방지는 `MenuNode.Key`로 한다.** 새 화면의 Key는 기존과 겹치지 않게 짓는다.
- 탭을 닫으면 `CloseTab`이 화면을 `Dispose()`한다 — 화면은 다음에 열 때 **새로 만들어진다.**
  탭을 닫았다 열면 상태가 초기화되는 것이 정상 동작이다.

### 4. 같은 탭에서 다른 화면으로 넘어가기

새 탭을 열지 않고 탭 내용만 바꾸려면 `INavigatingScreen`을 구현한다
([Forms/Common/INavigatingScreen.cs](src/SEOJIN_MEIS.Client/Forms/Common/INavigatingScreen.cs)).

```csharp
public event EventHandler<ScreenNavigationEventArgs> NavigateRequested;

// [이동] 버튼에서
NavigateRequested?.Invoke(this, new ScreenNavigationEventArgs(popScreen, $"POP - {workOrderNo}"));
```

- 실제 사용처: `WorkOrderEntryScreen`(작업지시 조회) → `PopScreen`. 탭 제목이 `POP - {작업지시번호}`로 바뀐다.
- **화면이 탭을 직접 조작하지 않는다.** 요청만 올리고 `MainForm.SwapTabContent`가 교체·제목 갱신·
  이전 화면 `Dispose`까지 처리한다.
- 교체는 `BeginInvoke`로 한 박자 미뤄 실행된다 — 요청을 보낸 화면을 그 이벤트 처리 도중에
  `Dispose`하면 안 되기 때문이다.

---

## 팝업 만들기

### 1. 클래스

```csharp
public partial class ItemPickerForm : XtraForm
{
    public ItemPickerForm() { InitializeComponent(); }
    public void Initialize(ClientContext context, string keyword) { ... }
    public ItemInfo SelectedItem { get; private set; }   // 결과는 속성으로 돌려준다
}
```

- 기반 클래스는 `Form`이 아니라 **`XtraForm`**이다(DevExpress 스킨이 적용되어야 한다).
- 탭 화면과 달리 `IThemedScreen`은 구현하지 않는다 — 팝업은 뜰 때마다 새로 만들어지므로
  그 시점의 테마가 이미 반영된다.

### 2. 띄우기

```csharp
using (var picker = new ItemPickerForm())
{
    picker.Initialize(_context, txtItemCode.Text?.Trim());
    picker.CenterOn(this);
    if (picker.ShowDialog(this) != DialogResult.OK || picker.SelectedItem == null)
        return;

    txtItemCode.Text = picker.SelectedItem.ItemCode;
}
```

- **`using`으로 감싼다.** 닫는 즉시 `Dispose`된다.
- **`ShowDialog(this)`** — 소유자를 반드시 넘긴다. 넘기지 않으면 팝업이 다른 창 뒤로 숨을 수 있다.
- 결과는 `DialogResult`로 성공 여부를, **속성으로 값을** 돌려받는다.
  팝업이 호출한 화면을 직접 건드리지 않는다.
- 값을 꺼내 쓰는 코드는 `using` 블록 **안에** 둔다 — 밖으로 나가면 이미 `Dispose`된 뒤다.

### 3. `CenterOn` — 공용 메서드가 아니다

`CenterOn(owner)`는 확장 메서드나 공통 기반 클래스가 아니라 **각 폼이 스스로 정의한 메서드**다.
새 팝업에 그대로 붙여 넣지 않으면 호출부에서 컴파일 오류가 난다.

```csharp
public void CenterOn(IWin32Window owner)
{
    var ownerForm = (owner as Control)?.FindForm();
    var bounds = ownerForm != null ? ownerForm.Bounds : Screen.PrimaryScreen.WorkingArea;

    Location = new Point(
        bounds.Left + ((bounds.Width - Width) / 2),
        bounds.Top + ((bounds.Height - Height) / 2));
}
```

- 현재 7개 폼(`AppMessageForm` · `NotificationForm` · `AdminAlarmForm` · `MaterialSerialForm` ·
  `TorqueInputForm` · `WorkEntryConfirmForm` · `ItemPickerForm`)에 **같은 코드가 복제**되어 있다.
- `UserProfileForm`만 이 메서드가 없어 `ShowDialog(this)`로만 뜬다(WinForms 기본 위치).
  의도한 예외라기보다 빠진 것에 가깝다.
- 팝업이 더 늘어나면 공통 헬퍼로 뽑는 편이 낫다. 지금은 **복제가 현재 관례**이므로
  새 팝업도 이 메서드를 포함시킨다.

### 4. 창틀 — 제목 줄은 헤더가 대신한다

팝업은 Windows 제목 줄을 쓰지 않는다. `Initialize()` 끝에서 `DialogChrome.Apply(this, headerPanel)`를
부르면 제목 줄이 사라지고, 화면 안의 색 띠 헤더가 그 역할을 이어받는다.

```csharp
AppTheme.DialogHeader(headerPanel, AppTheme.Indigo);
DialogChrome.Apply(this, headerPanel);   // 제목 줄 제거 + 헤더를 끌어 창 이동
```

제목 줄을 걷어낸 이유는 두 가지다.

1. **제목이 두 번 나왔다.** `Text`와 헤더의 `lblTitle`에 같은 문구가 들어가, 제목 줄과 색 띠가
   위아래로 겹쳐 **창이 두 번 뜬 것처럼** 보였다.
2. **모달 중에 창이 흔들렸다.** 팝업이 떠 있을 때 뒤쪽 화면을 누르면 Windows가 제목 줄을 흔들어
   알린다(표준 동작). 전체 화면 POP에서는 고장처럼 읽힌다 — 흔들 제목 줄이 없으면 일어나지 않는다.

- 닫기는 각 팝업의 **[닫기] 버튼과 Esc**(`CancelButton`)가 맡는다.
  버튼이 하나뿐인 공용 메시지 창만 헤더 안의 X를 쓴다(`AppMessagePanel.ShowCloseButton`).
- 창 이동은 헤더와 그 안의 아이콘·제목 라벨을 끌면 된다. 좌표를 직접 계산하지 않고
  `WM_NCLBUTTONDOWN`으로 OS에 맡기므로 제목 줄을 잡고 끌 때와 동작이 같다.
- `Text`는 그대로 둔다 — 화면에는 안 보이지만 접근성·작업 표시줄이 쓴다.

### 5. 사용자에게 알림을 띄울 때


**`MessageBox` / `XtraMessageBox`를 직접 부르지 말 것**(루트 [CLAUDE.md](CLAUDE.md) 공통 규약).
`AppMessageBox`만 쓴다. 그 안에서 `AppMessageForm`이 뜬다.

---

## 공통 주의 — `await` 뒤에는 화면이 사라졌을 수 있다

탭 화면과 팝업 **양쪽 모두** 해당한다.

- 팝업: 사용자가 닫으면 `using`이 즉시 `Dispose`한다.
- 탭 화면: 조회 중 탭을 닫으면 `CloseTab`이 `Dispose`한다.

그 뒤에 도착한 응답으로 화면을 만지면 `ObjectDisposedException`이 난다.
**`await` 뒤의 모든 분기(성공 · catch · finally)에서 확인한다.**

```csharp
var result = await _context.ApiClients.XApiClient.GetAsync(...);
if (IsDisposed || Disposing) return;   // 사용자가 이미 닫았다
grid.DataSource = result;
```

자세한 배경과 사례는 [CLAUDE.md 6번](src/SEOJIN_MEIS.Client/CLAUDE.md) 참고.

---

## 새 화면 추가 체크리스트

**탭 화면**

- [ ] `Forms/<업무명>/<화면명>Screen.cs` + `.Designer.cs` 생성
- [ ] `XtraUserControl` + `IThemedScreen` 구현, 매개변수 없는 생성자 + `Initialize(ClientContext)`
- [ ] 문구는 `Strings`에 키 추가(3개 resx + `Strings.cs`), 색은 `ApplyTheme()`에서 `AppTheme` 호출
- [ ] `MenuCatalog`에 노드 등록 — **Key가 기존과 겹치지 않는지 확인**
- [ ] 팩토리 안에서 `Initialize(ctx)`까지 호출
- [ ] 조회는 `try/catch (ApiException)` + `await` 뒤 `IsDisposed` 확인
- [ ] 메뉴를 눌러 실제로 탭이 열리는지 확인 (컴파일만으로는 검증되지 않는다)

**팝업**

- [ ] `Forms/<업무명>/<화면명>Form.cs` + `.Designer.cs` 생성
- [ ] `XtraForm` 상속, 매개변수 없는 생성자 + `Initialize(...)`
- [ ] 결과는 `DialogResult` + 속성으로 반환
- [ ] `CenterOn(IWin32Window)` 메서드 포함(공용 메서드가 아니다 — 폼마다 정의한다)
- [ ] 호출부는 `using` + `CenterOn(this)` + `ShowDialog(this)`
- [ ] `await`를 쓴다면 `IsDisposed` 확인

표준형으로 삼을 화면:
[Forms/ProductionStatus/ProductionStatusScreen.cs](src/SEOJIN_MEIS.Client/Forms/ProductionStatus/ProductionStatusScreen.cs)
(조회 조건 카드 + 결과 그리드 카드 — 가장 단순한 조회 화면).
