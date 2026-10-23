# TMES POP ClickOnce 배포 가이드

현장 PC에는 **ClickOnce**로 설치하고, 배포 서버(지역별 도메인)에 새 버전을 올리면
PC가 프로그램을 켤 때 새 버전을 확인해 자동으로 업데이트한다.

## 1. 구성

```
deploy/
├─ Publish-ClickOnce.ps1        게시 스크립트 (지역값 입력 · 버전 자동 증가 · 배포 서버 복사)
├─ regions/                     지역 하나 = 폴더 하나
│  ├─ GUMI/      구미
│  ├─ HWASEONG/  화성
│  ├─ YONGIN/    용인
│  ├─ VIETNAM/   베트남
│  └─ USA/       미국
│     ├─ region.props           배포 서버 주소(DeployBaseUrl) · 제품 이름
│     └─ App.config             이 지역 게시본의 SEOJIN_TMES_POP.exe.config
└─ out/<지역>/                   게시 결과(git 제외) — 이 폴더 내용을 배포 서버에 올린다
```

### 지역별 값

| 지역 | 폴더 | 바로가기 이름 | 기본 언어 | 사업장 코드(ServerName) | 배포 서버 주소 · API 주소 |
| --- | --- | --- | --- | --- | --- |
| 구미 | `GUMI` | TMES POP (구미) | ko-KR | `GUM` | 입력 필요 |
| 화성 | `HWASEONG` | TMES POP (화성) | ko-KR | 입력 필요 | 입력 필요 |
| 용인 | `YONGIN` | TMES POP (용인) | ko-KR | 입력 필요 | 입력 필요 |
| 베트남 | `VIETNAM` | TMES POP (베트남) | vi-VN | 입력 필요 | 입력 필요 |
| 미국 | `USA` | TMES POP (미국) | en-US | 입력 필요 | 입력 필요 |

"입력 필요" 값은 **비워 둔 상태**다. 게시 스크립트가 처음 게시할 때 물어보고 그 지역 파일에 저장한다(아래 3번).
세 값 중 하나라도 비어 있으면 게시하지 않는다(빌드도 `DeployBaseUrl`이 비면 오류로 멈춘다).

게시본 App.config는 지역 공통으로 `UseMockData=false`, `LogApiResponseBody=false`,
`DocumentShare`(도면 공유 폴더)는 비움, `Equipments`(설비 목록)는 `[]` — 지역 공유 폴더가 정해지면 해당 App.config에 채운다.

- `SEOJIN_TMES.Client.csproj`는 `-p:Region=<지역>`이 있을 때만 `regions/<지역>/region.props`를 불러와
  배포 주소 · App.config를 바꾼다. Region 없이 빌드(F5, 일반 빌드)하면 지금처럼 개발용 `src/SEOJIN_TMES.Client/App.config`를 쓴다.
- 신뢰 수준은 **완전 신뢰**(`Properties/app.manifest`). 부분 신뢰(LocalIntranet)로 설치되면 Modbus 소켓 · SMB 공유 폴더 접근이 막힌다.

## 2. 배포되는 파일 (솔루션 전체 확인 결과)

ClickOnce 매니페스트가 참조를 따라가며 자동으로 고른다. 게시본 `Application Files\SEOJIN_TMES_POP_<버전>\`에 들어가는 것:

| 구분 | 파일 |
| --- | --- |
| 실행 파일 · 설정 | `SEOJIN_TMES_POP.exe`, `SEOJIN_TMES_POP.exe.config`(지역 App.config), `SEOJIN_TMES_POP.exe.manifest`, `Images\texon.ico` |
| 솔루션 프로젝트 | `SEOJIN_TMES.Client.Common.dll`, `.ApiClient.dll`, `.Equipment.dll`, `.Modbus.dll`, `.Socket.dll`, `ImageBoxEx.dll` |
| 다국어 리소스 | `ko\`, `en\`, `vi\` `SEOJIN_TMES.Client.Common.resources.dll` |
| NuGet | `Newtonsoft.Json.dll` (13.0.3) |
| DevExpress 26.1 (직접 참조 14개) | `Data`, `Data.Desktop`, `Drawing`, `Utils`, `Printing.Core`, `XtraEditors`, `XtraBars`, `XtraLayout`, `XtraGrid`, `XtraTreeList`, `XtraPrinting`, `Pdf.Core`, `Pdf.Drawing`, `BonusSkins` |
| DevExpress 26.1 (간접 의존 3개) | `Office.Core`, `RichEdit.Core`, `Sparkline.Core` — Printing/Pdf가 요구해서 함께 들어간다 |

빠지는 것: `.pdb`, DevExpress `.xml` 문서, `src/SEOJIN_TMES.Client/Dlls/ImageGlass.ImageBox.dll`(어디서도 참조하지 않음 — 지워도 된다).
현장 PC에 DevExpress를 따로 설치할 필요는 없다(dll이 게시본에 포함됨). .NET Framework 4.7.2는 Windows 10(1803)+에 기본 포함.

## 3. 게시하기

Visual Studio(MSBuild.exe)가 설치된 PC의 PowerShell에서 저장소 루트 기준:

```powershell
# ① 주소 입력/변경만(게시 안 함) — 비어 있는 값은 물어보고, 있는 값은 [Enter]로 유지
.\deploy\Publish-ClickOnce.ps1 -Region GUMI -Configure
#   GUMI 배포 서버 주소 (예: https://tmes-gm-test.texon.co.kr/pop/): https://tmes-gm-test.texon.co.kr/pop
#     저장: DeployBaseUrl = https://tmes-gm-test.texon.co.kr/pop/  (region.props)
#   GUMI API 서버 주소 (예: http://10.200.1.237:8080/): ...
#   GUMI 사업장 코드 corpCd (예: GUM) [현재: GUM, Enter=유지]:

# ② 게시 — 버전 자동(배포 서버 최신 버전 + 1), 결과는 deploy\out\GUMI
#    값이 비어 있으면 ①처럼 먼저 물어본 뒤 게시한다.
.\deploy\Publish-ClickOnce.ps1 -Region GUMI

# 결과를 배포 서버 웹 루트(공유 폴더)로 바로 복사
.\deploy\Publish-ClickOnce.ps1 -Region VIETNAM -CopyTo \\<배포서버>\wwwroot\pop

# 버전을 직접 지정(기능 단위 릴리스 때 1.1.0.0 처럼 올리고 싶을 때)
.\deploy\Publish-ClickOnce.ps1 -Region USA -Version 1.1.0.0

# 이번만 다른 도메인으로(파일에는 저장하지 않음)
.\deploy\Publish-ClickOnce.ps1 -Region YONGIN -DeployBaseUrl https://다른도메인/pop/
```

- `-Region`은 `GUMI`, `HWASEONG`, `YONGIN`, `VIETNAM`, `USA` 중 하나(Tab 자동완성). 다른 값은 바로 거부된다.
- 입력한 주소는 `deploy/regions/<지역>/region.props`, `App.config`에 저장되므로 **커밋해 두면** 다른 PC에서도 그대로 게시된다.
  파일을 직접 열어 고쳐도 된다(`[입력 필요]` 주석이 달린 자리).

- `dotnet build`/`dotnet msbuild`로는 ClickOnce 게시가 안 된다(매니페스트 단계 실패) — 스크립트는 vswhere로 VS의 MSBuild.exe를 찾는다.
- 게시 전에 실행 정책이 막혀 있으면: `powershell -ExecutionPolicy Bypass -File .\deploy\Publish-ClickOnce.ps1 -Region GUMI`

### 버전 관리 규칙

- 버전 형식 `주.부.빌드.리비전`. 자동 모드는 **배포 서버의 `SEOJIN_TMES_POP.application`에 적힌 버전**을 읽어 리비전을 +1 한다
  (서버에 닿지 않으면 `deploy\out\<지역>`의 이전 게시본 기준, 둘 다 없으면 `1.0.0.1`).
  → 여러 사람이 게시해도 "서버에 있는 것"이 기준이라 버전이 꼬이지 않는다.
- 기본은 **필수 업데이트**: 게시 버전을 `minimumRequiredVersion`으로 넣어, 옛 버전 PC는 [건너뛰기] 없이 업데이트한다
  (서버 API와 클라이언트 버전이 어긋나지 않게). 선택 업데이트로 올리려면 `-OptionalUpdate`.
- 지역마다 버전은 따로 간다(배포 서버가 다르므로).
- 사이드바 하단의 버전 표시(`AppVersion`)가 ClickOnce 배포 버전을 보여 주므로, 현장에서 업데이트 여부를 눈으로 확인할 수 있다.

### 롤백

옛 버전으로 되돌릴 때도 **버전 번호는 올려서** 다시 게시한다(예: 1.0.0.7이 문제 → 1.0.0.6 소스로 1.0.0.8 게시).
ClickOnce는 낮은 버전으로는 업데이트하지 않는다.

## 4. 도메인 변경 · 지역 추가

도메인/주소가 바뀌면 `.\deploy\Publish-ClickOnce.ps1 -Region <지역> -Configure`로 다시 입력한다
(또는 `region.props`의 `DeployBaseUrl` 한 줄, `App.config`의 `ApiBaseUrl` 한 줄을 직접 수정).

새 지역을 추가할 때:

1. 비슷한 지역 폴더를 복사(예: `deploy/regions/YONGIN` → `deploy/regions/BUSAN`)
2. `region.props`의 `ProductName`, 상단 주석의 지역명 수정 · 주소는 비워 둔다
3. `App.config`의 `DefaultCulture`, `ServerName` 등 지역 값 수정
4. `Publish-ClickOnce.ps1` 상단 `[ValidateSet(...)]`에 폴더 이름 추가
5. `.\deploy\Publish-ClickOnce.ps1 -Region BUSAN -Configure` 로 주소 입력
**주의:** 이미 설치된 PC는 *설치할 때의 주소*로 업데이트를 확인한다. 주소를 바꾸면
① 옛 주소에도 새 주소로 게시한 버전을 한 번 올려 주거나 ② PC에서 제거 후 새 주소로 재설치해야 한다.

## 5. 배포 서버 준비

`deploy\out\<지역>\` 안의 내용(`SEOJIN_TMES_POP.application`, `setup.exe`, `publish.htm`, `Application Files\`)을
`DeployBaseUrl` 경로(예: `https://tmes-gm-test.texon.co.kr/pop/`)에서 보이도록 올린다.
`Application Files\`의 **옛 버전 폴더는 지우지 않는다** — 스크립트의 `-CopyTo`도 버전 폴더를 먼저, `.application`을 마지막에 올린다.

웹 서버에 MIME 타입 등록이 필요하다:

| 확장자 | MIME |
| --- | --- |
| `.application` | `application/x-ms-application` |
| `.manifest` | `application/x-ms-manifest` |
| `.deploy` | `application/octet-stream` |

IIS (`web.config`를 pop 폴더에):

```xml
<configuration>
  <system.webServer>
    <staticContent>
      <remove fileExtension=".application" /><mimeMap fileExtension=".application" mimeType="application/x-ms-application" />
      <remove fileExtension=".manifest" /><mimeMap fileExtension=".manifest" mimeType="application/x-ms-manifest" />
      <remove fileExtension=".deploy" /><mimeMap fileExtension=".deploy" mimeType="application/octet-stream" />
    </staticContent>
  </system.webServer>
</configuration>
```

nginx:

```nginx
location /pop/ {
    types {
        application/x-ms-application application;
        application/x-ms-manifest    manifest;
        application/octet-stream     deploy exe;
    }
    add_header Cache-Control "no-cache";   # .application 이 캐시되면 PC가 새 버전을 못 본다
}
```

## 6. 현장 PC

- **최초 설치:** 브라우저로 `https://<도메인>/pop/publish.htm` → [설치(setup.exe)].
  바탕화면 · 시작 메뉴에 `TMES POP (구미)` 같은 바로가기가 생긴다.
- **업데이트:** 바로가기로 실행하면 창이 뜨기 전에 서버 버전을 확인하고 새 버전이 있으면 받은 뒤 실행한다.
  POP는 하루 종일 켜 두므로, 게시 후에는 프로그램을 한 번 껐다 켜야 반영된다.
- exe를 직접 실행하면 ClickOnce 업데이트가 동작하지 않는다 — 반드시 바로가기(또는 .application)로 실행.

## 7. 알려진 주의사항

1. **화면에서 바꿔 exe.config에 저장하는 값은 업데이트 때 초기화된다.**
   ClickOnce는 버전마다 새 폴더에 설치하고 게시본의 exe.config로 덮는다. 현재 해당되는 곳:
   - `Core/Setting.cs` `SaveCulture` — 로그인 화면에서 고른 언어(`DefaultCulture`)
   - `Core/Devices.cs` `Save` — 설비 목록(`Equipments`)
   → `UserPreferences`(%APPDATA%)처럼 사용자 폴더로 옮겨야 업데이트 후에도 유지된다.
2. **게시된 App.config는 배포 서버에서 누구나 내려받을 수 있다**(`SEOJIN_TMES_POP.exe.config.deploy`).
   지역 App.config의 `DocumentShare`에 비밀번호를 넣지 말 것 — 도메인 계정 접근(UserName/Password 비움)을 쓰거나
   배포 서버를 사내망으로 제한한다. (개발용 `src/SEOJIN_TMES.Client/App.config`에는 지금 평문 비밀번호가 있다.)
3. **매니페스트 서명 안 함**(`SignManifests=false`). 설치 때 "게시자를 알 수 없음" 경고가 뜬다.
   코드 서명 인증서가 있으면 `SignManifests=true` + `ManifestCertificateThumbprint`를 넣는다.
   한 번 서명 방식을 정하면 바꿀 때 기존 PC가 업데이트를 거부하니 처음에 정할 것.
4. **같은 PC에 두 지역(또는 TEST/PROD)을 동시에 설치할 수 없다**(앱 ID가 같다). 한 PC는 한 지역 기준.
5. 화성 · 용인 · 베트남 · 미국의 **사업장 코드(ServerName)**는 아직 모른다 — 서버 쪽 corpCd를 확인해 입력할 것.
