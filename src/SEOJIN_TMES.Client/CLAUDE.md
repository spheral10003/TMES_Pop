# SEOJIN_TMES.Client (WinForms UI) — 작업 규칙

DevExpress 26.1 기반 화면 프로젝트. 솔루션 공통 규칙은 [루트 CLAUDE.md](../../CLAUDE.md) 참고.

이 프로젝트는 **View 역할만** 담당한다 — 화면 표시 · 입력 전달 · 설비 통신 호출만 하고,
계산 · 판정 · 집계는 서버가 한다(루트 CLAUDE.md의 "업무 로직은 서버에 있다" 참고).
새 화면 · 컨트롤은 되도록 **DevExpress의 Form/Control**을 사용한다.

## 1. Designer.cs — VS 디자이너를 깨뜨리지 않는다

모든 폼/유저컨트롤은 `X.cs`(로직) + `X.Designer.cs`(구조)로 나뉜다.

**`X.Designer.cs`의 `InitializeComponent()`에 아래를 넣지 말 것.**
VS WinForms 디자이너는 이 메서드를 CodeDom으로 파싱해서 디자인 화면을 그리는데,
아래 구문이 있으면 **디자이너 로드 자체가 예외로 죽는다**(빌드·실행에는 지장 없어서 늦게 발견된다):

- 람다식 `(s, e) => ...`
- 삼항 연산자 `a ? b : c`
- 문자열 보간 `$"..."`
- 객체 이니셜라이저 `new Foo { A = 1 }` → `new Foo(); foo.A = 1;`로 분리
- 커스텀 static 호출 — `AppTheme.*`, `AppIcons.*`, `AppImages.*`, `Strings.*` 전부 해당

Designer에는 **구조 · 도킹 · 크기 · 리터럴 기본값**만 둔다.
색 · 폰트 · 아이콘 · 다국어 문구는 `X.cs`의 `Initialize()` / `ApplyTheme()`에서 런타임에 다시 입힌다.

## 2. 생성자 + Initialize(context) 패턴

디자이너가 폼을 인스턴스화할 수 있어야 하므로 **매개변수 없는 생성자만** 둔다.
의존성(`ClientContext`)과 다국어 문구, 이벤트 배선은 생성 직후 호출하는 `Initialize(context)`에서 처리한다.

```csharp
public LoginForm() { InitializeComponent(); }          // 디자이너용
public void Initialize(ClientContext context) { ... }  // 실제 준비
```

새 화면을 추가할 때도 이 패턴을 그대로 따를 것.

## 3. 도킹 Add 순서를 바꾸지 말 것

WinForms는 **나중에 `Add`된 컨트롤을 먼저 도킹**한다.
그래서 각 Designer의 `Controls.Add(...)` 순서(예: `mainArea`(Fill) → `sidePanel`(Left))를 바꾸면 레이아웃이 무너진다.
Designer 상단 주석에 각 화면의 Add 순서가 적혀 있으니 그대로 유지할 것.

## 4. 색 · 폰트 · 아이콘은 Theme/ 를 거친다

| 무엇 | 어디 |
| --- | --- |
| 색 · 폰트 · 컨트롤 스타일 | [Theme/AppTheme.cs](Theme/AppTheme.cs) |
| 코드로 만드는 SVG 아이콘 | [Theme/AppIcons.cs](Theme/AppIcons.cs) |
| 파일로 들어온 이미지(PNG/SVG) | [Theme/AppImages.cs](Theme/AppImages.cs) |

- 화면 코드에 `Color.FromArgb(...)`를 직접 쓰지 말 것. 톤을 바꿀 때 한 곳만 고쳐서 전체에 반영되는 성질이 깨진다.
- 새 스타일이 필요하면 화면에서 즉석 처리하지 말고 `AppTheme`에 메서드를 추가한다.
- 각 화면은 `IThemedScreen.ApplyTheme()`을 구현해 색을 다시 입힐 수 있게 둔다(테마 전환 시 `MainForm`이 다시 호출한다).
- **화면에 테마 전환 버튼은 두지 않는다.** 다크로 쓰려면 `Program.cs`의 `AppTheme.ApplySkin(true)` 한 곳만 바꾼다.
- **다크 테마 색상 규격** — 배경 · 컨트롤은 `#111A2E`, 글자색은 `#c8d3e0`, 상태표시줄은 `#0b0f1a`를 사용한다.
  `AppTheme.cs`에 새 다크 색상을 추가하거나 기존 색을 손보는 김에 이 값에 맞춘다
  (2026-09 기준 기존 다크 팔레트는 이 값과 다르다).

## 5. DevExpress 실측 함정 (다시 밟지 말 것)

- `PanelControl`에 테두리(`BorderStyles.Simple`)를 켜면 **스킨이 배경을 직접 칠해 `Appearance.BackColor`가 무시된다**
  (흰색을 지정해도 `#F0F0F0`으로 렌더링). 그래서 카드 패널은 `LookAndFeel.Style = Flat`으로 두고
  색·테두리를 직접 지정한다 → `AppTheme.CardPanel`.
- `AccordionControl.Appearance.AccordionControl.BackColor`에 `Color.Transparent`를 주면
  사이드바 그라데이션이 비치지 않고 **검게** 칠해진다. 불투명 색을 지정해야 한다.
- `LabelControl`은 기본이 자동 크기라, 빈 문자열 상태를 거치면 **폭이 0으로 줄어든 채 남는다.**
  아바타 · 배지 · 상태 알약처럼 크기가 고정이어야 하는 라벨은 `AutoSizeMode = None` + 크기 명시
  (`AppTheme.Avatar` / `Badge` / `StatePill`의 인자).
- `SimpleButton.ImageOptions.Image`는 **원본 크기로 그려진다**(SVG의 `SvgImageSize`에 해당하는 것이 없다).
  PNG 아이콘은 반드시 `AppImages.Icon(...)`으로 크기를 맞춰 받을 것.
- 자동 크기 라벨을 가운데 정렬할 때는 **부모에 `Add`한 뒤** 위치를 잡아야 한다(그 전에는 `Width`가 확정되지 않는다).

## 6. async — await 뒤에는 화면이 사라졌을 수 있다

팝업은 `using (var form = new XForm())`으로 띄우므로 **닫는 즉시 `Dispose`된다.**
조회가 `Load`에서 시작되면 응답이 늦게 도착할 수 있고, 그때 없어진 화면을 만지면 `ObjectDisposedException`이 난다
(특히 `AppMessageBox.Warn(this, ...)` → `ShowDialog(owner)`가 Dispose된 owner의 `Handle`을 건드린다).

**`await` 뒤 모든 분기(성공 · catch · finally)에서 `IsDisposed`를 확인할 것.**
탭으로 열리는 화면(UserControl)도 조회 중 탭을 닫으면 같은 상황이 된다.

```csharp
var result = await _context.ApiClients.XApiClient.GetAsync(...);
if (IsDisposed || Disposing) return;   // 사용자가 이미 닫았다
grid.DataSource = result;
```

`AppMessageBox`가 Dispose된 소유자를 걸러 주긴 하지만(화면 가운데로 띄운다) **그건 마지막 방어선**이다.
없어진 화면의 오류를 뒤늦게 띄우는 게 옳은지는 각 화면이 판단해야 한다 —
[Forms/Notification/NotificationForm.cs](Forms/Notification/NotificationForm.cs)의 `IsClosed` 사용례 참고.

## 7. 문자열은 전부 Languages/ 을 거친다

화면에 보이는 문구를 코드에 직접 쓰지 말 것(디자이너의 리터럴 기본값은 예외 — `Initialize()`에서 덮어쓴다).

문자열 키를 추가할 때는 **네 곳을 함께** 갱신한다:

1. `Languages/Language.resx` — 중립/기본(한국어)
2. `Languages/Language.en.resx` — 영어
3. `Languages/Language.vi.resx` — 베트남어
4. `Languages/Language.cs` — 강타입 속성(VS의 resx 코드생성을 쓰지 않고 수동 작성)

세 resx의 **키 집합과 선언 순서를 동일하게** 유지한다(빠진 키는 조용히 한국어로 대체되어 발견이 늦다).

언어를 추가할 때는 세 가지가 모두 있어야 한다 —
`Forms/MainForm.cs`의 `Languages` 배열 한 줄 + `Language.{두 글자 코드}.resx` + `Images/flag/{두 글자 코드}.png`.
번역 리소스 없이 항목만 넣으면 골라도 한국어로 되돌아가 고장처럼 보인다.

## 8. 이미지 리소스 규약

원본은 `Images/`에 두고, **`SEOJIN_TMES.Client.csproj`에 `EmbeddedResource` + `LogicalName`으로 등록**한다.
exe만 복사해도 그림이 따라가고, 현장에서 파일이 지워져 깨지는 일이 없다.

```xml
<EmbeddedResource Include="Images\bell-regular.png">
  <LogicalName>SEOJIN_TMES.Client.Images.bell-regular.png</LogicalName>
</EmbeddedResource>
```

- `LogicalName`을 **반드시 명시**할 것. 자동 생성되는 리소스 이름은 폴더명 변환 규칙에 따라 달라져 예측이 어렵다.
- 코드에서는 `AppImages`를 통해서만 꺼낸다(`Assembly.GetManifestResourceStream` 직접 호출 금지).
  크기 조정·색 적용·캐시가 그 안에서 처리된다.
- 파일 이름을 바꿔도 `LogicalName`은 그대로 두면 코드를 고칠 필요가 없다.

## 9. 새 화면 추가 절차

1. `Forms/<업무명>/<화면명>Screen.cs` + `.Designer.cs` 생성 — 위 1~3번 규칙 준수
2. `XtraUserControl` + `IThemedScreen` 구현, `Initialize(ClientContext)` 제공
3. 문구는 `Strings`에 키 추가(7번), 색은 `ApplyTheme()`에서 `AppTheme` 호출(4번)
4. `Menu/MenuCatalog.cs`에 메뉴 노드 등록
5. 조회는 `try/catch (ApiException)` + `await` 뒤 `IsDisposed` 확인(6번)
6. 서버가 아직 없으면 `Mock/`에 목업을 두고 `UseMockData`로 분기

기존 화면 중 [Forms/ProductionStatus/ProductionStatusScreen.cs](Forms/ProductionStatus/ProductionStatusScreen.cs)가
가장 단순한 표준형이다 — 조회 조건 카드 + 결과 그리드 카드. 새 조회 화면은 이 배치를 따를 것.

## 10. 람다식 · static 사용

- **람다식은 최대한 줄인다.** 이벤트 핸들러 등은 되도록 이름 있는 메서드로 뺀다
  (1번 규칙의 Designer.cs 제약과는 별개로, `.cs` 로직 파일에도 적용되는 일반 스타일 방침).
- **`Core/` 를 제외하고 static 사용을 최소화한다.** 화면 · Forms 코드에서는 인스턴스 필드 · 의존성 주입으로 상태를 들고 다니고,
  정적 헬퍼는 `Core/`(예: [Core/Global.cs](Core/Global.cs), [Core/Setting.cs](Core/Setting.cs))에만 모은다.
