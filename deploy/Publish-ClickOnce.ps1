<#
.SYNOPSIS
  TMES POP 클라이언트를 지역별 ClickOnce 패키지로 게시한다.

.DESCRIPTION
  지역: GUMI(구미) · HWASEONG(화성) · YONGIN(용인) · VIETNAM(베트남) · USA(미국) · SEOJIN(서진 Test 서버)

  1) deploy/regions/<Region>/ 의 region.props(배포 서버 주소) + App.config 로 Release 게시본을 만든다.
     배포 서버 주소 · API 주소 · 사업장 코드가 비어 있으면 먼저 물어보고 그 지역 파일에 저장한다.
     값을 미리 넣거나 바꾸기만 하려면 -Configure.
  2) 버전을 따로 주지 않으면 배포 서버에 올라가 있는 최신 버전을 읽어 마지막 자리를 +1 한다
     (서버에 닿지 않으면 이 PC의 이전 게시본 기준) — 게시할 때마다 버전이 올라가야 현장 PC가 업데이트를 받는다.
  3) -CopyTo 를 주면 결과 폴더를 배포 서버의 웹 루트(공유 폴더 등)로 복사한다.

  dotnet build / dotnet msbuild 는 ClickOnce 매니페스트 생성에서 실패하므로 Visual Studio 의 MSBuild.exe 를 쓴다.

.EXAMPLE
  .\deploy\Publish-ClickOnce.ps1 -Region GUMI -Configure
  .\deploy\Publish-ClickOnce.ps1 -Region GUMI
  .\deploy\Publish-ClickOnce.ps1 -Region VIETNAM -Version 1.0.1.0 -CopyTo \\deploy-server\wwwroot\pop
  .\deploy\Publish-ClickOnce.ps1 -Region SEOJIN -CopyTo \\10.200.1.235\C$\TMES\MESClient
  .\deploy\Publish-ClickOnce.ps1 -Region GUMI -DeployBaseUrl https://tmes-gm.texon.co.kr/pop/
#>
[CmdletBinding()]
param(
    # deploy/regions 아래 폴더 이름
    [Parameter(Mandatory = $true)]
    [ValidateSet('GUMI', 'HWASEONG', 'YONGIN', 'VIETNAM', 'USA', 'SEOJIN')]
    [string]$Region,

    # 게시하지 않고 지역 입력값(배포 서버 주소 · API 주소 · 사업장 코드)만 입력/변경한다
    [switch]$Configure,

    # 게시 버전(예: 1.0.3.0). 비우면 서버 최신 버전 + 1
    [string]$Version,

    # region.props 의 배포 서버 주소를 이번 게시에만 덮어쓴다(끝에 / 필수)
    [string]$DeployBaseUrl,

    # 게시 결과를 복사할 배포 서버 폴더(UNC 또는 로컬 경로). 비우면 deploy/out/<Region> 에만 만든다
    [string]$CopyTo,

    # 지정하면 "건너뛰기"가 가능한 선택 업데이트로 게시한다(기본은 필수 업데이트)
    [switch]$OptionalUpdate
)

$ErrorActionPreference = 'Stop'
$repoRoot  = Split-Path -Parent $PSScriptRoot
$regionDir = Join-Path $PSScriptRoot "regions\$Region"
$project   = Join-Path $repoRoot 'src\SEOJIN_TMES.Client\SEOJIN_TMES.Client.csproj'
$appName   = 'SEOJIN_TMES_POP'
$outDir    = Join-Path $PSScriptRoot "out\$Region\"

if (-not (Test-Path (Join-Path $regionDir 'region.props'))) {
    $known = (Get-ChildItem (Join-Path $PSScriptRoot 'regions') -Directory | ForEach-Object Name) -join ', '
    throw "지역 '$Region' 설정이 없다. 사용 가능한 지역: $known"
}

# ── 지역 입력값(배포 서버 주소 · API 주소 · 사업장 코드) ─────────────
# 지역 파일은 주소를 비운 채로 만들어 두고, 처음 게시할 때(또는 -Configure) 물어봐서 파일에 채워 넣는다.
# XML 파서로 다시 쓰면 주석 · 줄바꿈이 흐트러지므로 해당 값 자리만 정규식으로 바꾼다.
$propsPath  = Join-Path $regionDir 'region.props'
$configPath = Join-Path $regionDir 'App.config'
$utf8NoBom  = New-Object Text.UTF8Encoding $false

$sharePattern = '(<add key="DocumentShare" value=")([^"]*)(")'
$fields = @(
    @{ Name = 'DeployBaseUrl'; Label = '배포 서버 주소 (예: https://tmes-gm-test.texon.co.kr/pop/)'; Path = $propsPath;  Pattern = '(<DeployBaseUrl[^>]*>)([^<]*)(</DeployBaseUrl>)'; Url = $true }
    @{ Name = 'ApiBaseUrl';    Label = 'API 서버 주소 (예: http://10.200.1.237:8080/)';               Path = $configPath; Pattern = '(<add key="ApiBaseUrl" value=")([^"]*)(")';       Url = $true }
    @{ Name = 'ServerName';    Label = '사업장 코드 corpCd (예: GUM)';                                 Path = $configPath; Pattern = '(<add key="ServerName" value=")([^"]*)(")';       Url = $false }
    # 도면 공유 폴더(DocumentShare JSON). RootPath 는 서버 주소 + 폴더로 나눠 물어보고 \\서버\폴더\ 로 합쳐 저장한다.
    @{ Name = 'ShareServer';   Label = '도면 공유 폴더 서버 주소 (예: 172.16.150.11)';                Path = $configPath; Pattern = $sharePattern; Json = 'RootPath'; Part = 'Server' }
    @{ Name = 'ShareFolder';   Label = '도면 공유 폴더 경로 (예: sjapp\attach)';                     Path = $configPath; Pattern = $sharePattern; Json = 'RootPath'; Part = 'Folder' }
    @{ Name = 'ShareUser';     Label = '도면 공유 폴더 계정 (비우면 현재 윈도우 계정)';             Path = $configPath; Pattern = $sharePattern; Json = 'UserName'; Optional = $true }
    @{ Name = 'SharePassword'; Label = '도면 공유 폴더 비밀번호';                                     Path = $configPath; Pattern = $sharePattern; Json = 'Password'; Optional = $true; Secret = $true }
)

function Get-RawValue($f) {
    $m = [regex]::Match([IO.File]::ReadAllText($f.Path), $f.Pattern)
    if (-not $m.Success) { throw "$($f.Path) 에서 $($f.Name) 자리를 찾지 못했다." }
    [Net.WebUtility]::HtmlDecode($m.Groups[2].Value)
}

function Set-RawValue($f, [string]$value) {
    $text    = [IO.File]::ReadAllText($f.Path)
    $escaped = [Security.SecurityElement]::Escape($value)
    $text    = [regex]::Replace($text, $f.Pattern, { param($m) $m.Groups[1].Value + $escaped + $m.Groups[3].Value }, 1)
    [IO.File]::WriteAllText($f.Path, $text, $utf8NoBom)
}

# DocumentShare 값(JSON)을 항상 같은 네 필드 순서로 다룬다 — 앱의 DocumentShareSettings 가 읽는 형태.
function Get-Share($f) {
    $raw = Get-RawValue $f
    $o   = if ($raw) { $raw | ConvertFrom-Json } else { $null }
    [ordered]@{
        RootPath = "$($o.RootPath)"; UserName = "$($o.UserName)"; Password = "$($o.Password)"; Domain = "$($o.Domain)"
    }
}

function Split-RootPath([string]$root) {
    $m = [regex]::Match($root, '^\\\\([^\\]+)\\?(.*?)\\?$')
    if ($m.Success) { @{ Server = $m.Groups[1].Value; Folder = $m.Groups[2].Value } } else { @{ Server = ''; Folder = '' } }
}

function Get-RegionValue($f) {
    if (-not $f.Json) { return Get-RawValue $f }
    $share = Get-Share $f
    if ($f.Part) { return (Split-RootPath $share.RootPath)[$f.Part] }
    $share[$f.Json]
}

function Set-RegionValue($f, [string]$value) {
    if (-not $f.Json) { Set-RawValue $f $value; return }
    $share = Get-Share $f
    if ($f.Part) {
        $parts = Split-RootPath $share.RootPath
        $parts[$f.Part] = $value
        $share.RootPath = if (-not $parts.Server) { '' } elseif ($parts.Folder) { "\\$($parts.Server)\$($parts.Folder)\" } else { "\\$($parts.Server)\" }
    }
    else { $share[$f.Json] = $value }
    Set-RawValue $f (New-Object PSObject -Property $share | ConvertTo-Json -Compress)
}

function Read-RegionValue($f, [string]$current) {
    # 입력이 끝없이 비어 오는 경우(CI처럼 대화형이 아닌 실행)에 멈춰 있지 않도록 횟수를 제한한다.
    for ($try = 1; $try -le 3; $try++) {
        $shown = if ($f.Secret) { '설정됨' } else { $current }
        $hint  = if ($current) { " [현재: $shown, Enter=유지]" } else { '' }
        if ($f.Secret) {
            # 비밀번호는 화면에 남지 않도록 가려서 받는다.
            $secure = Read-Host "  $Region $($f.Label)$hint" -AsSecureString
            $value  = [Runtime.InteropServices.Marshal]::PtrToStringBSTR([Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure))
        }
        else { $value = "$(Read-Host "  $Region $($f.Label)$hint")".Trim() }
        if (-not $value) { $value = $current }
        if (-not $value) {
            if ($f.Optional) { return '' }
            Write-Warning '값이 필요하다.'; continue
        }
        if ($f.Url) {
            if ($value -notmatch '^https?://') { Write-Warning 'http:// 또는 https:// 로 시작해야 한다.'; continue }
            if (-not $value.EndsWith('/')) { $value += '/' }
        }
        if ($f.Part) { $value = $value.Trim('\') }
        return $value
    }
    throw "$Region $($f.Name) 값을 입력받지 못했다 - deploy/regions/$Region 의 파일에 직접 채우거나 -Configure 로 다시 실행할 것."
}

Write-Host "지역 입력값 확인 - $Region"
# 계정 · 비밀번호는 비워 둘 수 있는 값이라 "비어 있으면 묻기" 규칙을 쓰면 게시할 때마다 묻게 된다.
# 공유 폴더 경로를 새로 받을 때(또는 -Configure)만 함께 묻는다.
$askShareAccount = $Configure -or -not (Get-Share $fields[3]).RootPath
foreach ($f in $fields) {
    $current = Get-RegionValue $f
    # -DeployBaseUrl 로 넘긴 값은 이번 게시에만 쓰고 파일에는 남기지 않는다.
    if ($f.Name -eq 'DeployBaseUrl' -and $DeployBaseUrl -and -not $Configure) { continue }
    if ($f.Optional) { if (-not $askShareAccount) { continue } }
    elseif ($current -and -not $Configure) { continue }
    $value = Read-RegionValue $f $current
    if ($value -ne $current) {
        Set-RegionValue $f $value
        $shown = if ($f.Secret) { '****' } else { $value }
        Write-Host "  저장: $($f.Name) = $shown  ($([IO.Path]::GetFileName($f.Path)))"
    }
}
if ($Configure) {
    Write-Host "입력값 저장 완료 - 게시하려면 -Configure 없이 다시 실행한다."
    return
}

if (-not $DeployBaseUrl) { $DeployBaseUrl = Get-RegionValue $fields[0] }
if (-not $DeployBaseUrl.EndsWith('/')) { $DeployBaseUrl += '/' }

# ── 버전 결정 ──────────────────────────────────────────────────
function Get-DeployedVersion([xml]$manifest) {
    [version]$manifest.assembly.assemblyIdentity.version
}

if (-not $Version) {
    $current = $null
    try {
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        $res = Invoke-WebRequest -Uri "$DeployBaseUrl$appName.application" -UseBasicParsing -TimeoutSec 10
        $current = Get-DeployedVersion ([xml]$res.Content)
        Write-Host "배포 서버 최신 버전: $current"
    }
    catch {
        $local = Join-Path $outDir "$appName.application"
        if (Test-Path $local) {
            $current = Get-DeployedVersion ([xml](Get-Content $local -Raw))
            Write-Warning "배포 서버에서 버전을 읽지 못해 이 PC의 이전 게시본($current) 기준으로 올린다. ($($_.Exception.Message))"
        }
        else {
            Write-Warning "배포 서버 · 이전 게시본 모두 없음 - 첫 게시로 보고 1.0.0.1 로 시작한다."
        }
    }
    $Version = if ($current) { "{0}.{1}.{2}.{3}" -f $current.Major, $current.Minor, $current.Build, ($current.Revision + 1) } else { '1.0.0.1' }
}
[void][version]$Version   # 형식 검사(1.2.3.4)

# ── MSBuild 찾기 ───────────────────────────────────────────────
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$msbuild = & $vswhere -latest -prerelease -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (-not $msbuild) { throw 'Visual Studio MSBuild.exe 를 찾지 못했다(vswhere).' }

Write-Host ""
Write-Host "지역        : $Region"
Write-Host "배포 주소   : $DeployBaseUrl"
Write-Host "게시 버전   : $Version $(if ($OptionalUpdate) { '(선택 업데이트)' } else { '(필수 업데이트)' })"
Write-Host "결과 폴더   : $outDir"
Write-Host ""

$msbuildArgs = @(
    $project, '-restore', '-t:Clean;Publish', '-v:m', '-nologo',
    '-p:Configuration=Release',
    "-p:Region=$Region",
    "-p:DeployBaseUrl=$DeployBaseUrl",
    "-p:ApplicationVersion=$Version",
    "-p:PublishDir=$outDir"
)
if (-not $OptionalUpdate) { $msbuildArgs += "-p:MinimumRequiredVersion=$Version" }

& $msbuild @msbuildArgs
if ($LASTEXITCODE -ne 0) { throw "MSBuild 게시 실패(exit $LASTEXITCODE)" }

# ── 결과 정리 ──────────────────────────────────────────────────
# SDK 형식 프로젝트의 ClickOnce 게시는 빌드 산출물(dll · xml · pdb)까지 결과 폴더 루트에 복사한다.
# 현장 PC가 실제로 받는 것은 .application + setup.exe + Application Files\<버전> 뿐이므로 나머지는 지운다
# (pdb · DevExpress xml 문서를 배포 서버에 올릴 이유가 없다).
$keep = @("$appName.application", 'setup.exe', 'Application Files', 'publish.htm')
Get-ChildItem $outDir | Where-Object { $keep -notcontains $_.Name } | Remove-Item -Recurse -Force -Confirm:$false

# 설치 안내 페이지 — 현장 PC 브라우저에서 이 주소를 열어 설치한다.
$installPage = @"
<!doctype html>
<html lang="ko"><head><meta charset="utf-8"><title>TMES POP 설치 ($Region)</title>
<style>body{font-family:'Malgun Gothic',sans-serif;margin:40px;color:#222}a.btn{display:inline-block;padding:12px 24px;background:#1f5fbf;color:#fff;text-decoration:none;border-radius:6px}code{background:#f2f2f2;padding:2px 6px}</style>
</head><body>
<h2>TMES POP 설치 — $Region</h2>
<p>현재 버전: <b>$Version</b></p>
<p><a class="btn" href="setup.exe">설치 (setup.exe)</a></p>
<p>이미 설치된 PC는 프로그램을 실행하면 자동으로 새 버전을 받는다.</p>
<p>setup.exe 가 막힌 환경에서는 Edge 에서 <a href="$appName.application"><code>$appName.application</code></a> 을 직접 연다.</p>
</body></html>
"@
[IO.File]::WriteAllText((Join-Path $outDir 'publish.htm'), $installPage, (New-Object Text.UTF8Encoding $true))

# ── 결과 확인 ──────────────────────────────────────────────────
$versionFolder = Join-Path $outDir ("Application Files\{0}_{1}" -f $appName, $Version.Replace('.', '_'))
foreach ($p in @((Join-Path $outDir "$appName.application"), $versionFolder)) {
    if (-not (Test-Path $p)) { throw "게시 결과가 없다: $p" }
}
$published = Get-DeployedVersion ([xml](Get-Content (Join-Path $outDir "$appName.application") -Raw))
if ($published -ne [version]$Version) { throw "게시된 매니페스트 버전($published)이 요청 버전($Version)과 다르다." }

# ── 배포 서버로 복사 ───────────────────────────────────────────
if ($CopyTo) {
    # 이전 버전 폴더(Application Files\...)는 지우지 않는다 — 업데이트 중이던 PC가 옛 파일을 받을 수 있어야 한다.
    # 루트의 .application 은 버전 폴더가 다 올라간 다음에 바꿔야, 반쯤 올라간 버전을 PC가 받아 가지 않는다.
    robocopy (Join-Path $outDir 'Application Files') (Join-Path $CopyTo 'Application Files') /E /NP /NFL /NDL | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "robocopy 실패(exit $LASTEXITCODE) - Application Files" }
    robocopy $outDir $CopyTo /NP /NFL /NDL | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "robocopy 실패(exit $LASTEXITCODE) - 루트 파일" }
    $global:LASTEXITCODE = 0
    Write-Host "배포 서버 복사 완료: $CopyTo"
}

Write-Host ""
Write-Host "게시 완료: $Region $Version"
Write-Host "설치 페이지: ${DeployBaseUrl}publish.htm"
