[CmdletBinding()]
param(
    [switch]$SkipChecksums,
    [switch]$SkipSemanticValidation
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$repo = [IO.Path]::GetFullPath($PSScriptRoot)
$run = Join-Path $repo ('work\verify\' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ'))
New-Item -ItemType Directory -Force -Path $run | Out-Null

$parseFailures = @()
$powershellFiles = Get-ChildItem -LiteralPath $repo -Recurse -File -Filter '*.ps1' | Where-Object {
    $_.FullName -notlike "$repo\work\*" -and
    $_.FullName -notlike "$repo\semantic-sdk\Build\*" -and
    $_.FullName -notlike '*\bin\*' -and $_.FullName -notlike '*\obj\*'
}
foreach ($file in $powershellFiles) {
    $tokens = $null; $errors = $null
    [void][Management.Automation.Language.Parser]::ParseFile($file.FullName, [ref]$tokens, [ref]$errors)
    foreach ($error in $errors) { $parseFailures += "$($file.FullName):$($error.Extent.StartLineNumber): $($error.Message)" }
}
if ($parseFailures.Count -ne 0) { throw "PowerShell parse failures:`n$($parseFailures -join "`n")" }
Write-Host "PASS PowerShell syntax files=$($powershellFiles.Count)"

function Invoke-NativeChecked {
    param([string]$Program, [string[]]$Arguments, [string]$Label)
    & $Program @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$Label failed with exit code $LASTEXITCODE." }
    Write-Host "PASS $Label"
}

function Expect-NativeReject {
    param([string]$Program, [string[]]$Arguments, [string]$Label)
    & $Program @Arguments *> $null
    if ($LASTEXITCODE -eq 0) { throw "$Label was incorrectly accepted." }
    Write-Host "PASS $Label rejected"
}

$required = @(
    'bin\derecomp.exe', 'bin\luau-compile.exe', 'bin\luau.exe',
    'bin\namecrack.exe', 'bin\wf_api_catalog.exe', 'bin\wf_api_check.exe',
    'data\namebase_verified.tsv', 'data\name_map.json',
    'api\warframe\contracts.tsv', 'api\warframe\selected_catalog.tsv',
    'knowledge\research\DE LUAU TRANSLATOR\NATIVE API AND LIVE CANDIDATE CENSUS\result_consumption_sites.tsv',
    'knowledge\native-analysis\registry\registry.json',
    'knowledge\semantic-sdk\semantic-sdk.json', 'knowledge\semantic-sdk\symbols.tsv',
    'knowledge\semantic-sdk\manifest.json'
)
foreach ($relative in $required) {
    $path = Join-Path $repo $relative
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Required file is missing: $relative" }
}
Write-Host "PASS required files=$($required.Count)"

if (-not $SkipChecksums) {
    $inventoryPath = Join-Path $repo 'SOURCE_INVENTORY.json'
    if (-not (Test-Path -LiteralPath $inventoryPath -PathType Leaf)) { throw 'SOURCE_INVENTORY.json is missing.' }
    $inventory = Get-Content -LiteralPath $inventoryPath -Raw | ConvertFrom-Json
    foreach ($artifact in $inventory.pinned_artifacts) {
        $path = Join-Path $repo ([string]$artifact.path).Replace('/', '\')
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Pinned artifact missing: $($artifact.path)" }
        $actual = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
        if ($actual -ne $artifact.sha256) { throw "Pinned artifact hash mismatch: $($artifact.path) expected=$($artifact.sha256) actual=$actual" }
    }
    Write-Host "PASS pinned hashes=$($inventory.pinned_artifacts.Count)"

    $manifestPath = Join-Path $repo 'MANIFEST.sha256'
    if (Test-Path -LiteralPath $manifestPath -PathType Leaf) {
        & python (Join-Path $repo 'tools\update_manifest.py') --verify --repo $repo
        if ($LASTEXITCODE -ne 0) { throw 'Tracked Git-blob manifest verification failed.' }
    } else {
        Write-Host 'PASS tracked Git-blob manifest skipped during initial assembly'
    }
}

$derecomp = Join-Path $repo 'bin\derecomp.exe'
Invoke-NativeChecked $derecomp @('transcode-global-selftest') 'global lowering self-test'
Invoke-NativeChecked $derecomp @('closure-index-selftest') 'closure index self-test'
Invoke-NativeChecked $derecomp @('semantic-ir-selftest') 'Semantic IR self-test'
Invoke-NativeChecked $derecomp @('semantic-ir-lowering-selftest') 'Semantic IR lowering self-test'
Invoke-NativeChecked $derecomp @('semantic-ir-readable-selftest') 'readable naming self-test'
Invoke-NativeChecked $derecomp @('closure-map', (Join-Path $repo 'cert\cert_all.spawn.lua_B'), (Join-Path $run 'closure-map.tsv')) 'closure map smoke test'
Invoke-NativeChecked $derecomp @('de-roundtrip', (Join-Path $repo 'cert\cert_all.spawn.lua_B')) 'exact DE container round trip'

$source0 = Join-Path $repo 'tests\hello.luau'
$bytecode0 = Join-Path $run 'smoke.0.lua_B'
$source1 = Join-Path $run 'smoke.1.luau'
$bytecode1 = Join-Path $run 'smoke.1.lua_B'
$source2 = Join-Path $run 'smoke.2.luau'
$bytecode2 = Join-Path $run 'smoke.2.lua_B'
Invoke-NativeChecked $derecomp @('recompile', $source0, $bytecode0) 'source compilation smoke'
Invoke-NativeChecked $derecomp @('decompile-mod', $bytecode0, $source1) 'first decompile smoke'
Invoke-NativeChecked $derecomp @('recompile', $source1, $bytecode1) 'first rebuilt bytecode smoke'
Invoke-NativeChecked $derecomp @('decompile-mod', $bytecode1, $source2) 'second decompile smoke'
Invoke-NativeChecked $derecomp @('recompile', $source2, $bytecode2) 'second rebuilt bytecode smoke'
if ((Get-FileHash -LiteralPath $source1 -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath $source2 -Algorithm SHA256).Hash) {
    throw 'Compiler-closed smoke source drifted between cycle 1 and cycle 2.'
}
if ((Get-FileHash -LiteralPath $bytecode1 -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath $bytecode2 -Algorithm SHA256).Hash) {
    throw 'Compiler-closed smoke bytecode drifted between cycle 1 and cycle 2.'
}
Write-Host 'PASS compiler-closed smoke source=equal bytecode=equal'

$contracts = Join-Path $repo 'api\warframe\contracts.tsv'
$checker = Join-Path $repo 'bin\wf_api_check.exe'
$apiTests = Join-Path $repo 'tools\warframe_api\tests'
Invoke-NativeChecked $checker @($contracts, (Join-Path $apiTests 'valid_catalog_contracts.luau')) 'valid API contracts'
Invoke-NativeChecked $checker @($contracts, (Join-Path $apiTests 'valid_gravity_contracts.luau'), '--strict-unknown') 'valid strict API contracts'
Expect-NativeReject $checker @($contracts, (Join-Path $apiTests 'invalid_contracts.luau')) 'invalid API contracts'
Expect-NativeReject $checker @($contracts, (Join-Path $apiTests 'replacement_introduced_unknown.luau'), '--baseline', (Join-Path $apiTests 'baseline_stock_overloads.luau'), '--strict-unknown') 'introduced unknown API call'

$wrapperSource = Join-Path $run 'wrapper.luau'
$wrapperBytecode = Join-Path $run 'wrapper.lua_B'
& (Join-Path $repo 'renovice.ps1') decompile (Join-Path $repo 'cert\warframe_api\de\wf_radial_numeric_damage_callback.lua_B') $wrapperSource
if ($LASTEXITCODE -ne 0) { throw 'renovice.ps1 decompile wrapper failed.' }
& (Join-Path $repo 'renovice.ps1') recompile $wrapperSource $wrapperBytecode
if ($LASTEXITCODE -ne 0) { throw 'renovice.ps1 recompile wrapper failed.' }
$wrapperApiOutput = & (Join-Path $repo 'renovice.ps1') api-check (Join-Path $apiTests 'valid_gravity_contracts.luau') --strict-unknown 2>&1
if ($LASTEXITCODE -ne 0) { throw "renovice.ps1 API wrapper failed:`n$($wrapperApiOutput -join "`n")" }
if (($wrapperApiOutput -join "`n") -notmatch 'strict_unknown=yes') { throw 'renovice.ps1 did not forward --strict-unknown.' }
Write-Host 'PASS convenience wrapper decompile/recompile/strict API forwarding'

if (-not $SkipSemanticValidation) {
    $semantic = Join-Path $repo 'semantic-sdk\Build\renovice-semantic.exe'
    if (Test-Path -LiteralPath $semantic -PathType Leaf) {
        Invoke-NativeChecked $semantic @('validate', '--workspace', $repo) 'Semantic SDK structural validation'
    } else {
        $manifest = Get-Content -LiteralPath (Join-Path $repo 'knowledge\semantic-sdk\manifest.json') -Raw | ConvertFrom-Json
        foreach ($output in $manifest.outputs) {
            $path = Join-Path $repo ('knowledge\semantic-sdk\' + [string]$output.path)
            $actual = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
            if ($actual -ne $output.sha256) { throw "Semantic SDK output hash mismatch: $($output.path)" }
        }
        Write-Host 'PASS Semantic SDK published-output hashes (run build.ps1 for full structural validation)'
    }
}

$fidelity = Join-Path $run 'readable-smoke.fidelity.luau'
$readable = Join-Path $run 'readable-smoke.readable.luau'
$names = Join-Path $run 'readable-smoke.names.tsv'
$calls = Join-Path $run 'readable-smoke.calls.tsv'
Invoke-NativeChecked $derecomp @(
    'semantic-ir-render-module-readable', (Join-Path $repo 'cert\warframe_api\warframe_api\de\wf_radial_numeric_damage_callback.lua_B'),
    $fidelity, $readable, $names,
    '--semantic-sdk', (Join-Path $repo 'knowledge\semantic-sdk\symbols.tsv'),
    '--call-map', $calls
) 'readable/API coordinated render'
foreach ($path in @($fidelity, $readable, $names, $calls)) {
    if ((Get-Item -LiteralPath $path).Length -eq 0) { throw "Readable output is empty: $path" }
}
Invoke-NativeChecked $derecomp @('recompile', $fidelity, (Join-Path $run 'readable-smoke.fidelity.lua_B')) 'fidelity twin recompilation'
Invoke-NativeChecked $derecomp @('recompile', $readable, (Join-Path $run 'readable-smoke.readable.lua_B')) 'readable view recompilation'

$trackedBytecode = @()
if (Test-Path -LiteralPath (Join-Path $repo '.git') -PathType Container) {
    $trackedBytecode = @(git -C $repo ls-files '*.lua_B')
    if ($LASTEXITCODE -ne 0) { throw 'Could not enumerate tracked bytecode fixtures.' }
    $outside = @($trackedBytecode | Where-Object { $_ -notlike 'cert/*' })
    if ($outside.Count -ne 0) { throw "Game/corpus bytecode is tracked outside cert/: $($outside -join ', ')" }
    $fixtureBytes = 0L
    foreach ($relative in $trackedBytecode) { $fixtureBytes += (Get-Item -LiteralPath (Join-Path $repo $relative)).Length }
    Write-Host "PASS tracked bytecode fixtures=$($trackedBytecode.Count) bytes=$fixtureBytes outside_cert=0"
}

$brokenLinks = @()
$markdown = Get-ChildItem -LiteralPath $repo -Recurse -File -Filter '*.md' | Where-Object {
    $_.FullName -notlike "$repo\work\*" -and $_.FullName -notlike '*\bin\*' -and $_.FullName -notlike '*\obj\*'
}
foreach ($file in $markdown) {
    $text = Get-Content -LiteralPath $file.FullName -Raw
    foreach ($match in [regex]::Matches($text, '\[[^\]]*\]\(([^)]+)\)')) {
        $target = $match.Groups[1].Value
        if ($target -match '^(https?|mailto|#)' -or $target -match '^<') { continue }
        $clean = ($target -split '#')[0]
        if ([string]::IsNullOrWhiteSpace($clean)) { continue }
        $candidate = Join-Path $file.DirectoryName ([Uri]::UnescapeDataString($clean))
        if (-not (Test-Path -LiteralPath $candidate)) { $brokenLinks += "$($file.FullName): $target" }
    }
}
if ($brokenLinks.Count -ne 0) { throw "Broken local documentation links:`n$($brokenLinks -join "`n")" }
Write-Host "PASS local documentation links files=$($markdown.Count)"

Write-Host "VERIFY PASS output=$run"
