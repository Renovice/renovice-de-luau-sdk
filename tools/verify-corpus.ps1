[CmdletBinding()]
param(
    [string]$Corpus,
    [ValidateRange(1, 100000)][int]$Sample = 360,
    [ValidateRange(1, 64)][int]$Workers = 4,
    [switch]$RunReleaseGates,
    [ValidateRange(1, 100000)][int]$GateStructure = 300,
    [ValidateRange(1, 100000)][int]$GateBehavior = 150,
    [switch]$RequireOriginalByteIdentity
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if ([string]::IsNullOrWhiteSpace($Corpus)) { $Corpus = $env:RENOVICE_CORPUS }
if ([string]::IsNullOrWhiteSpace($Corpus)) { throw 'Pass -Corpus or set RENOVICE_CORPUS to an external directory containing .lua_B files.' }
$Corpus = [IO.Path]::GetFullPath($Corpus)
if (-not (Test-Path -LiteralPath $Corpus -PathType Container)) { throw "Corpus directory does not exist: $Corpus" }
$corpusCount = @(Get-ChildItem -LiteralPath $Corpus -File -Filter '*.lua_B').Count
if ($corpusCount -lt $Sample) { throw "Corpus has $corpusCount .lua_B files, fewer than requested sample $Sample." }

$pythonCommand = Get-Command python.exe -ErrorAction SilentlyContinue
if ($null -eq $pythonCommand) { $pythonCommand = Get-Command py.exe -ErrorAction SilentlyContinue }
if ($null -eq $pythonCommand) { throw 'Python 3 was not found on PATH.' }
$python = $pythonCommand.Source
$run = Join-Path $repo ('work\corpus-verification\' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ'))
New-Item -ItemType Directory -Force -Path $run | Out-Null

$saved = @{}
Get-ChildItem Env:RENOVICE* | ForEach-Object { $saved[$_.Name] = $_.Value }
try {
    Get-ChildItem Env:RENOVICE* | ForEach-Object { Remove-Item -LiteralPath ('Env:' + $_.Name) }
    $env:RENOVICE_CORPUS = $Corpus
    $env:RENOVICE_DECOMPILE_MODE = 'decompile-mod'
    $idempotenceArgs = @((Join-Path $repo 'cert\idempotence.py'), [string]$Sample, '--workers', [string]$Workers, '--decompile-mode', 'decompile-mod', '--json-out', (Join-Path $run 'idempotence.json'))
    if ($RequireOriginalByteIdentity) { $idempotenceArgs += '--require-original-byte-identity' }
    & $python @idempotenceArgs
    if ($LASTEXITCODE -ne 0) { throw "Idempotence gate failed with exit code $LASTEXITCODE. Evidence: $run" }

    if ($RunReleaseGates) {
        & $python (Join-Path $repo 'cert\gates.py') ([string]$GateStructure) ([string]$GateBehavior) '--decompile-mode' 'decompile-mod' '--json-out' (Join-Path $run 'release-gates.json')
        if ($LASTEXITCODE -ne 0) { throw "Release gates failed with exit code $LASTEXITCODE. Evidence: $run" }
    }

    $record = [ordered]@{
        schema_version = 1
        result = 'PASS'
        utc = [DateTime]::UtcNow.ToString('o')
        corpus = $Corpus
        corpus_file_count = $corpusCount
        sample = $Sample
        mode = 'decompile-mod'
        workers = $Workers
        require_original_byte_identity = [bool]$RequireOriginalByteIdentity
        release_gates_run = [bool]$RunReleaseGates
        derecomp_sha256 = (Get-FileHash -LiteralPath (Join-Path $repo 'bin\derecomp.exe') -Algorithm SHA256).Hash
        luau_compile_sha256 = (Get-FileHash -LiteralPath (Join-Path $repo 'bin\luau-compile.exe') -Algorithm SHA256).Hash
        idempotence_report = 'idempotence.json'
        release_report = if ($RunReleaseGates) { 'release-gates.json' } else { $null }
    }
    $record | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $run 'run.json') -Encoding utf8NoBOM
    Write-Host "CORPUS VERIFY PASS denominator=$Sample/$corpusCount mode=decompile-mod output=$run"
} finally {
    Get-ChildItem Env:RENOVICE* | ForEach-Object { Remove-Item -LiteralPath ('Env:' + $_.Name) }
    foreach ($entry in $saved.GetEnumerator()) { Set-Item -LiteralPath ('Env:' + $entry.Key) -Value $entry.Value }
}
