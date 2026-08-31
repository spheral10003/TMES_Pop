# SEOJIN_MEIS.Client.ApiClient — 작업 규칙

서버와의 REST 통신 전담(Token 인증). 솔루션 공통 규칙은 [루트 CLAUDE.md](../../CLAUDE.md) 참고.
3중 오류 처리 구조의 설명은 [README의 API 오류 처리](../../README.md#api-오류-처리--앱이-죽지-않고-안내-창만-띄운다).

## 1. API 버전은 상수 한 곳에서만

모든 엔드포인트는 [ApiClientBase.cs](ApiClientBase.cs)의 `ApiPrefix`(`"api/v1.0/"`)를 접두사로 쓴다.

**각 클라이언트에 버전 문자열을 직접 적지 말 것.** API 개정 시 이 상수 한 곳만 바꾸면 전체에 반영된다.

```csharp
// 이렇게
var url = ApiPrefix + "workorders/search";
// 이러면 안 된다
var url = "api/v1.0/workorders/search";
```

## 2. 실패는 ApiException으로만 올린다

화면은 전부 `catch (ApiException)` 하나로 안내 창을 띄운다.
그 계약이 깨지면 예외가 `async void` 핸들러를 타고 올라가 앱이 중단된다.

- `HttpRequestException` · DNS · SSL 등 **응답을 못 받은 예외**는 [LoggingHandler.cs](LoggingHandler.cs)에서
  `ApiException(StatusCode=0)`으로 변환된다.
- HttpClient는 **타임아웃도 `TaskCanceledException`으로 던진다.** [ApiClientBase.cs](ApiClientBase.cs)에서
  **호출자가 준 원본 토큰**이 취소되지 않았으면 타임아웃으로 보고 `ApiException`으로 바꾼다
  (핸들러 안에서는 링크 토큰이라 구분할 수 없다). 호출자가 실제로 취소한 경우는 오류가 아니므로 그대로 통과시킨다.
- 통신 실패와 서버가 준 오류는 `ApiException.IsTransportFailure`(= `StatusCode == 0`)로 구분한다.
  로그인 화면이 이 값으로 "서버에 연결할 수 없습니다" 안내를 따로 띄운다.

새 API 메서드를 추가할 때 **예외를 삼키지 말 것.** `catch { return null; }`은 금지 —
화면이 "결과 0건"과 "서버 장애"를 구분할 수 없게 된다.

## 3. 401은 여기서 1회만 재시도한다

[AuthTokenHandler.cs](AuthTokenHandler.cs)가 401을 받으면 `RefreshToken`으로 **1회** 재시도한다.
그 이상의 판단(재로그인 유도, 세션 종료)은 **UI 계층 책임**이다.

핸들러에 재시도 루프나 재로그인 창을 넣지 말 것 — 통신 계층이 화면을 알게 되는 순간 경계가 무너진다.

## 4. 로그에 바디를 남기지 않는다

`LoggingHandler`는 모든 호출의 **URL · Method · 응답 상태코드 · 소요시간(ms)**을 자동으로 남긴다.

**요청/응답 바디는 남기지 않는다** — 비밀번호·토큰·개인정보가 섞인다.
디버깅 때문에 임시로 바디를 찍었다면 커밋 전에 반드시 지울 것.

## 5. 인터페이스와 구현을 함께 둔다

`I*ApiClient` 인터페이스가 이 프로젝트의 공개 계약이다. 화면은 인터페이스만 본다
(`ClientContext.ApiClients` → `ApiClientFactory`).

새 API를 추가할 때:

1. `I<업무>ApiClient`에 메서드 추가 — 반환은 DTO(`SEOJIN_MEIS.Client.Common.Models`)
2. 구현에서 `ApiPrefix` + 상대 경로 사용
3. `ApiClientFactory`에 노출
4. 경로·필드명이 서버 스펙 확정 전 **가정값**이라면 주석에 그렇게 적어 둘 것
   (지금 `users/me`가 그런 상태다 — 나중에 무엇을 확인해야 하는지 알 수 있어야 한다)
