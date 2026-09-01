# SEOJIN_MEIS.Client.Common — 작업 규칙

서버·클라이언트 공용 계약(DTO), 실행 설정, 로깅, 비밀번호 해시, 문서 공유 폴더 접근.
솔루션 공통 규칙은 [루트 CLAUDE.md](../../CLAUDE.md) 참고.

## 이 프로젝트는 UI를 모른다

**WinForms · DevExpress 참조를 넣지 말 것.** 현재 UI 의존이 0이며, 이 경계 덕분에
화면 없이 콘솔/스크립트에서 통신·설정·로깅을 띄워 확인할 수 있다.

- 여기 있는 코드는 `MessageBox`를 띄우지 않는다. 실패는 **예외로 올리고** 안내는 화면이 결정한다.
- 문화권에 의존하는 서식(`ToString()` 기본 오버로드 등)을 쓰지 말 것 —
  서버로 나가는 값은 항상 `CultureInfo.InvariantCulture`를 명시한다.

## Models/ — DTO는 서버 계약이다

- **직렬화 필드명이 서버 스펙과 정확히 일치해야 한다.** 기본 직렬화는 PascalCase라
  서버가 camelCase를 요구하면 `[JsonProperty]`로 고정한다(예: `LoginRequest.orgNo/password/salt`).
  필드명을 "정리"하려고 이름을 바꾸면 인증·조회가 조용히 실패한다.
- 응답이 `{ "data": ... }`처럼 한 겹 싸여 오는 경우가 있다 → `ApiDataResponse<T>`를 쓴다.
  이 서버는 **봉투를 쓰는 곳과 안 쓰는 곳이 섞여 있고 Swagger 선언도 일관되지 않다.**
  봉투 유무를 잘못 짚으면 예외 없이 **모든 값이 null**이 되어 발견이 늦다(로그인 토큰·사용자 프로필에서 겪었다).
  확실하지 않으면 `ApiClientBase.GetDataAsync<T>()`를 쓰면 두 형태를 모두 받아 준다.
- DTO에 계산 로직을 넣지 말 것. 판정·집계는 서버 몫이며, 표시용 파생 값이 필요하면
  읽기 전용 속성 하나로 끝내고 그 이유를 주석에 남긴다.

## Configuration/ — 새 설정을 추가할 때

`AppSettings`에 속성 하나를 추가하는 것으로 끝나지 않는다. **세 곳을 함께** 갱신할 것:

1. [Configuration/AppSettings.cs](Configuration/AppSettings.cs) — 속성 + 기본값 + 용도 주석
2. `src/SEOJIN_MEIS.Client/Composition/AppSettingsLoader.cs` — App.config에서 읽어 채우는 코드
3. `src/SEOJIN_MEIS.Client/App.config` — 실제 값 + **의미와 기본값을 설명하는 주석**

로더가 값을 못 읽어도 앱은 떠야 한다 — 파싱 실패 시 속성의 기본값을 그대로 쓴다(`TryParse` 패턴 유지).

`UserPreferences`는 사용자 단말에 남는 편의 설정(아이디 저장 등)이다.
**비밀번호·토큰을 여기에 저장하지 말 것.**

## Logging/AppLog — 앱을 죽이지 않는 것이 최우선

앱 전체의 유일한 로깅 지점이다(`Console.WriteLine` 직접 호출 금지).
한 줄이 콘솔 · Trace · 파일 세 곳에 동시에 남는다.

파일 정책 — 첫 기록 때 자동으로 열리며 별도 초기화 호출이 필요 없다:

- **일별 생성** : `logs\meis-client-yyyyMMdd.log`
- **7일 압축** : 7일이 지난 로그를 7일 단위로 묶어 `logs\archive\meis-client-{시작}_{끝}.zip`으로 옮기고 원본 삭제
- **30일 보관** : 압축본·원본 모두 30일이 지나면 삭제
- 위치는 실행 파일 옆 `logs`, 쓸 수 없으면 `%LOCALAPPDATA%\MEIS\logs`

고칠 때 지켜야 할 것:

- **파일 쪽 실패는 절대 밖으로 던지지 말 것.** 권한 없음·디스크 가득 등이 나면
  파일 기록만 끄고(`_fileLogDisabled`) 콘솔·Trace는 계속한다.
- **정리·압축 코드 안에서 `AppLog`를 호출하지 말 것** — 자기 자신을 다시 부르는 꼴이 된다. `Trace.WriteLine`만 쓴다.
- 7일 묶음 경계는 고정 기준일(`BucketEpoch`)로 계산한다. "마지막 압축 시각"을 파일에 저장하는 방식으로 바꾸지 말 것 —
  그 상태 파일이 사라지면 묶음이 어긋난다.
- 파일 이름 형식(`meis-client-yyyyMMdd`)을 바꾸면 정리·압축이 자기 파일을 못 찾는다. 상수와 함께 바꿀 것.

## Security/PasswordHash — 평문은 서버로 나가지 않는다

로그인은 nonce 방식이라 `SHA256(SHA256(평문 비밀번호) + salt)`를 보낸다
(안쪽 해시 결과는 **소문자 16진수 64자 문자열**이고, 거기에 salt를 문자열로 이어붙여 다시 해시한다).
화면은 평문만 넘기고 해시는 이 클래스 안에서 끝난다 — **화면이나 API 클라이언트에서 해시를 직접 계산하지 말 것.**

해시 조합 순서가 서버와 다르면 인증이 통과하지 않는다. 다르면 `PasswordHash.ForLogin` **한 줄만** 고친다.

## Storage/ — 도면 공유 폴더(SMB)

- `RootPath`가 비어 있으면 **파일 접근을 시도하지 않는다**(화면은 "공유 폴더 미설정" 안내만 표시).
  이 조기 반환을 없애면 미설정 환경에서 매번 네트워크 타임아웃을 기다리게 된다.
- 계정을 명시하지 않으면 현재 윈도우 계정으로 접근한다(권장). 명시하면 `WNetAddConnection2`로 연결한다.
- `IDisposable`이다. 연결을 열었으면 반드시 해제할 것(`Program.cs`의 `finally`에서 처리 중).
