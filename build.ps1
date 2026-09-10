[CmdletBinding()]
param(
    [string]$Cxx,
    [switch]$SkipSemanticSdk,
    [switch]$SkipAbilityBehaviorSelfTest,
    [switch]$SkipVerification
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false

$repo = [IO.Path]::GetFullPath($PSScriptRoot)
$buildDirectory = Join-Path $repo 'work\build'
$binDirectory = Join-Path $repo 'bin'
New-Item -ItemType Directory -Force -Path $buildDirectory, $binDirectory | Out-Null

function Resolve-CxxCompiler {
    param([string]$Requested)
    $candidates = @()
    if (-not [string]::IsNullOrWhiteSpace($Requested)) { $candidates += $Requested }
    if (-not [string]::IsNullOrWhiteSpace($env:CXX)) { $candidates += $env:CXX }
    $onPath = Get-Command g++.exe -ErrorAction SilentlyContinue
    if ($null -ne $onPath) { $candidates += $onPath.Source }
    $candidates += 'C:\msys64\ucrt64\bin\g++.exe'

    foreach ($candidate in $candidates | Select-Object -Unique) {
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            return [IO.Path]::GetFullPath($candidate)
        }
        $command = Get-Command $candidate -ErrorAction SilentlyContinue
        if ($null -ne $command) { return $command.Source }
    }
    throw 'No g++.exe was found. Pass -Cxx, set CXX, add g++ to PATH, or install MSYS2 UCRT64.'
}

function Invoke-NativeChecked {
    param(
        [Parameter(Mandatory)][string]$Program,
        [Parameter()][string[]]$Arguments = @(),
        [Parameter(Mandatory)][string]$Label
    )
    & $Program @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$Label failed with exit code $LASTEXITCODE." }
}

function Expect-NativeReject {
    param([string]$Program, [string[]]$Arguments, [string]$Label)
    & $Program @Arguments *> $null
    if ($LASTEXITCODE -eq 0) { throw "$Label was incorrectly accepted." }
    Write-Host "REJECT PASS $Label"
}

$compiler = Resolve-CxxCompiler $Cxx
Write-Host "CXX $compiler"
Invoke-NativeChecked $compiler @('--version') 'C++ compiler version check'

$common = @('-O2', '-std=c++17', '-Wall', '-Werror', '-static', '-static-libgcc', '-static-libstdc++', '-Wl,--no-insert-timestamp')
$derecompCandidate = Join-Path $buildDirectory 'derecomp.new.exe'
$namecrackCandidate = Join-Path $buildDirectory 'namecrack.new.exe'
$catalogCandidate = Join-Path $buildDirectory 'wf_api_catalog.new.exe'
$checkerCandidate = Join-Path $buildDirectory 'wf_api_check.new.exe'

Invoke-NativeChecked $compiler ($common + @('-o', $derecompCandidate, (Join-Path $repo 'src\main.cpp'))) 'derecomp build'
Invoke-NativeChecked $compiler ($common + @('-o', $namecrackCandidate, (Join-Path $repo 'src\namecrack.cpp'))) 'namecrack build'
Invoke-NativeChecked $compiler ($common + @('-Wextra', '-o', $catalogCandidate, (Join-Path $repo 'tools\warframe_api\wf_api_catalog.cpp'))) 'API catalog build'
Invoke-NativeChecked $compiler ($common + @('-Wextra', '-o', $checkerCandidate, (Join-Path $repo 'tools\warframe_api\wf_api_check.cpp'))) 'API checker build'

# derecomp resolves both Luau executables beside its own executable. Stage the
# pinned frontend/runtime before testing the candidate rather than accidentally
# falling back to a machine-specific installation.
Copy-Item -LiteralPath (Join-Path $binDirectory 'luau-compile.exe') -Destination (Join-Path $buildDirectory 'luau-compile.exe') -Force
Copy-Item -LiteralPath (Join-Path $binDirectory 'luau.exe') -Destination (Join-Path $buildDirectory 'luau.exe') -Force

Invoke-NativeChecked $derecompCandidate @('transcode-global-selftest') 'global lowering self-test'
Invoke-NativeChecked $derecompCandidate @('closure-index-selftest') 'closure index self-test'
Invoke-NativeChecked $derecompCandidate @('semantic-ir-selftest') 'Semantic IR self-test'
Invoke-NativeChecked $derecompCandidate @('semantic-ir-lowering-selftest') 'Semantic IR lowering self-test'
Invoke-NativeChecked $derecompCandidate @('semantic-ir-readable-selftest') 'readable naming self-test'
Invoke-NativeChecked $derecompCandidate @('closure-map', (Join-Path $repo 'cert\cert_all.spawn.lua_B'), (Join-Path $buildDirectory 'closure-map-smoke.tsv')) 'closure map smoke test'

$census = Join-Path $repo 'knowledge\research\DE LUAU TRANSLATOR\NATIVE API AND LIVE CANDIDATE CENSUS\result_consumption_sites.tsv'
$contracts = Join-Path $repo 'api\warframe\contracts.tsv'
$catalog = Join-Path $repo 'api\warframe\selected_catalog.tsv'
Invoke-NativeChecked $catalogCandidate @((Join-Path $repo 'api\warframe\selection_seeds.tsv'), $census, $contracts, $catalog) 'API catalog generation'

$apiTests = Join-Path $repo 'tools\warframe_api\tests'
Invoke-NativeChecked $checkerCandidate @($contracts, (Join-Path $repo 'cert\warframe_api\src\wf_radial_numeric_damage_callback.luau')) 'Mallet callback contract test'
Invoke-NativeChecked $checkerCandidate @($contracts, (Join-Path $apiTests 'valid_catalog_contracts.luau')) 'catalog contract test'
Invoke-NativeChecked $checkerCandidate @($contracts, (Join-Path $apiTests 'valid_gravity_contracts.luau'), '--strict-unknown') 'gravity contract test'
Invoke-NativeChecked $checkerCandidate @($contracts, (Join-Path $apiTests 'valid_sanitizer_strings.luau'), '--strict-unknown') 'string sanitizer test'
Expect-NativeReject $checkerCandidate @($contracts, (Join-Path $apiTests 'invalid_contracts.luau')) 'invalid callback/method arities'
Expect-NativeReject $checkerCandidate @($contracts, (Join-Path $apiTests 'invalid_getlocalplayeravatar_arity.luau')) 'invalid GetLocalPlayerAvatar arity'
Expect-NativeReject $checkerCandidate @($contracts, (Join-Path $apiTests 'invalid_addgravity_arity.luau')) 'invalid AddGravityMultiplier arity'
Expect-NativeReject $checkerCandidate @($contracts, (Join-Path $apiTests 'invalid_removegravity_arity.luau')) 'invalid RemoveGravityMultiplier arity'
Expect-NativeReject $checkerCandidate @($contracts, (Join-Path $apiTests 'invalid_catalog_contracts.luau')) 'invalid high-confidence catalog arities'
Invoke-NativeChecked $checkerCandidate @($contracts, (Join-Path $apiTests 'replacement_same_stock_calls.luau'), '--baseline', (Join-Path $apiTests 'baseline_stock_overloads.luau'), '--strict-unknown') 'replacement baseline subtraction test'
Expect-NativeReject $checkerCandidate @($contracts, (Join-Path $apiTests 'replacement_introduced_unknown.luau'), '--baseline', (Join-Path $apiTests 'baseline_stock_overloads.luau'), '--strict-unknown') 'replacement-introduced unknown call'

Copy-Item -LiteralPath $derecompCandidate -Destination (Join-Path $binDirectory 'derecomp.exe') -Force
Copy-Item -LiteralPath $namecrackCandidate -Destination (Join-Path $binDirectory 'namecrack.exe') -Force
Copy-Item -LiteralPath $catalogCandidate -Destination (Join-Path $binDirectory 'wf_api_catalog.exe') -Force
Copy-Item -LiteralPath $checkerCandidate -Destination (Join-Path $binDirectory 'wf_api_check.exe') -Force

if (-not $SkipSemanticSdk) {
    & (Join-Path $repo 'semantic-sdk\build.ps1')
    if ($LASTEXITCODE -ne 0) { throw "Semantic SDK build failed with exit code $LASTEXITCODE." }
    $semantic = Join-Path $repo 'semantic-sdk\Build\renovice-semantic.exe'
    Invoke-NativeChecked $semantic @('selftest', '--workspace', $repo) 'Semantic SDK self-test'
    Invoke-NativeChecked $semantic @('build', '--workspace', $repo) 'Semantic SDK generation'
    Invoke-NativeChecked $semantic @('validate', '--workspace', $repo) 'Semantic SDK validation'
}

if (-not $SkipAbilityBehaviorSelfTest) {
    $abilityProject = Join-Path $repo 'tools\ability_behavior\AbilityBehaviorCatalog.csproj'
    Invoke-NativeChecked 'dotnet' @('build', $abilityProject, '--configuration', 'Release', '--nologo') 'ability behavior tool build'
    $abilityDll = Join-Path $repo 'tools\ability_behavior\bin\Release\net9.0\AbilityBehaviorCatalog.dll'
    Invoke-NativeChecked 'dotnet' @($abilityDll, 'self-test') 'ability behavior tool self-test'
}

if (-not $SkipVerification) {
    & (Join-Path $repo 'verify.ps1') -SkipChecksums
    if ($LASTEXITCODE -ne 0) { throw "Repository verification failed with exit code $LASTEXITCODE." }
}

Write-Host 'BUILD PASS: native tools, API catalog/checker, Semantic SDK, and local verification completed.'
