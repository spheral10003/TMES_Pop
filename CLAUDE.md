# SEOJIN_TMES.Client — 작업 규칙

WinForms(.NET Framework 4.7.2) + DevExpress 26.1 기반 MES POP 클라이언트.

이 파일과 각 프로젝트의 `CLAUDE.md`는 **코드를 고칠 때 지켜야 할 규칙**만 담는다.
시스템이 무엇이고 각 화면이 어떻게 동작하는지에 대한 설명은 [README.md](README.md)에 있다.

## 가장 중요한 규칙: 업무 로직은 서버에 있다

수량 계산 · 합격/불합격 판정 · 집계 · 상태 전이는 **전부 서버**가 한다.
클라이언트가 하는 일은 **화면 표시 · 입력 전달 · 설비 Modbus 통신**뿐이다.

클라이언트에서 합계를 내거나 판정하는 코드를 새로 만들지 말 것 — 서버가 내려준 값을 그대로 보여 준다.
화면에 값이 안 맞으면 먼저 서버 응답을 확인한다.

## 빌드

**`dotnet build`를 쓰지 말 것.** ClickOnce 매니페스트 생성 단계(`GenerateApplicationManifest`)에서
`System.ArgumentException: Partial trust is not supported`로 실패한다 —
.NET SDK의 MSBuild가 net472 ClickOnce `app.manifest`를 처리하지 못하는 것이며, C# 코드 문제가 아니다.

컴파일·산출물 확인용(매니페스트 생성만 건너뛴다):

```bash
dotnet msbuild src/SEOJIN_TMES.Client/SEOJIN_TMES.Client.csproj -t:Build -p:GenerateManifests=false -v:m -nologo
```

배포까지 포함한 정식 빌드는 Visual Studio, 또는 VS에 포함된 MSBuild.exe로:

```bash
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" SEOJIN_TMES.sln
```

앱이 실행 중(디버깅 중)이면 컴파일은 끝나고 `bin` 복사 단계에서 파일 잠금(`MSB3027`)으로 실패한다.
디버깅을 멈추고 다시 빌드할 것 — 코드 오류로 착각하지 말 것.

## 프로젝트 경계

```
SEOJIN_TMES.Client ──┬─→ SEOJIN_TMES.Client.ApiClient ──┐
                     ├─→ SEOJIN_TMES.Client.Equipment ──┼─→ SEOJIN_TMES.Client.Common
                     └──────────────────────────────────┘
```

- **역방향 참조 금지.** 아래 세 프로젝트는 `SEOJIN_TMES.Client`를 모른다.
- **Common / ApiClient / Equipment에 WinForms · DevExpress 참조를 넣지 말 것.**
  현재 세 프로젝트의 UI 의존은 0이며, 이 경계 덕분에 화면 없이 통신·설비 코드를 띄워 확인할 수 있다.

## 공통 규약

- **주석 · 문서 · 커밋 메시지는 한국어.** 주석은 "무엇을"보다 **"왜"**를 남긴다(주변 코드의 밀도·어투를 따를 것).
- **로그는 `AppLog`만.** `Console.WriteLine` / `Debug.WriteLine` 직접 호출 금지.
- **사용자에게 띄우는 창은 `AppMessageBox`만.** `MessageBox` / `XtraMessageBox` 직접 호출 금지.
- 환경마다 달라지는 값(서버 주소, 설비 IP/포트, 공유 폴더, 타임아웃, 폴링 주기)은 코드에 박지 않고
  `src/SEOJIN_TMES.Client/App.config`에 둔다. 새 설정을 넣을 때는 App.config 주석에 **의미와 기본값**을 함께 적는다.
- `Mock/`은 `App.config`의 `UseMockData=true`일 때만 쓰인다. 운영 경로에 목업 분기를 새로 만들지 말 것.
- 솔루션에서 **공통으로 쓰는 메서드 · 클래스는 되도록 `SEOJIN_TMES.Client.Common`에 모아서 관리**한다.
  프로젝트 경계(위 참고)상 Common은 다른 세 프로젝트가 공유하는 최하위 계층이므로, 여러 프로젝트에서 쓰일 코드는
  처음부터 Common에 두거나 나중에 발견하면 그쪽으로 옮긴다.

## 프로젝트별 규칙

| 프로젝트 | 규칙 파일 |
| --- | --- |
| WinForms UI (화면 · 테마 · 다국어) | [src/SEOJIN_TMES.Client/CLAUDE.md](src/SEOJIN_TMES.Client/CLAUDE.md) |
| 화면 구동 방식 (탭=UserControl · 팝업=Form) | [SCREEN_GUIDE.md](SCREEN_GUIDE.md) |
| 공용 계약 · 설정 · 로깅 | [src/SEOJIN_TMES.Client.Common/CLAUDE.md](src/SEOJIN_TMES.Client.Common/CLAUDE.md) |
| REST API 클라이언트 | [src/SEOJIN_TMES.Client.ApiClient/CLAUDE.md](src/SEOJIN_TMES.Client.ApiClient/CLAUDE.md) |
| 설비 Modbus-TCP | [src/SEOJIN_TMES.Client.Equipment/CLAUDE.md](src/SEOJIN_TMES.Client.Equipment/CLAUDE.md) |
