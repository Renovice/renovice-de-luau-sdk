[CmdletBinding()]
param(
    [int]$ApiDeepContracts = 33
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$inventoryPath = Join-Path $repo 'SOURCE_INVENTORY.json'
$sdkPath = Join-Path $repo 'knowledge\semantic-sdk\semantic-sdk.json'

if (-not (Test-Path -LiteralPath $inventoryPath -PathType Leaf)) {
    throw "Missing source inventory: $inventoryPath"
}
if (-not (Test-Path -LiteralPath $sdkPath -PathType Leaf)) {
    throw "Missing generated Semantic SDK: $sdkPath"
}

$inventory = Get-Content -Raw -LiteralPath $inventoryPath | ConvertFrom-Json
$sdk = Get-Content -Raw -LiteralPath $sdkPath | ConvertFrom-Json

$inventory.assembled_utc = [DateTime]::UtcNow.ToString('o')
$inventory.fresh_verification.api_catalog.deep_contracts = $ApiDeepContracts
$inventory.fresh_verification.semantic_sdk.symbols = $sdk.statistics.symbols
$inventory.fresh_verification.semantic_sdk.deep_contracts = $sdk.statistics.deepContracts
$inventory.fresh_verification.semantic_sdk.catalog_symbols = $sdk.statistics.catalogSymbols
$inventory.fresh_verification.semantic_sdk.corpus_sites = $sdk.statistics.corpusCallSites
$inventory.fresh_verification.semantic_sdk.evidence = $sdk.statistics.evidenceRecords
$inventory.fresh_verification.semantic_sdk.negative_findings = $sdk.statistics.negativeFindings

foreach ($artifact in $inventory.pinned_artifacts) {
    $path = Join-Path $repo $artifact.path
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Pinned artifact is missing: $($artifact.path)"
    }
    $item = Get-Item -LiteralPath $path
    $artifact.bytes = $item.Length
    $artifact.sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
}

$json = $inventory | ConvertTo-Json -Depth 20
$json = $json.Replace("`r`n", "`n") + "`n"
[IO.File]::WriteAllText($inventoryPath, $json, [Text.UTF8Encoding]::new($false))

Write-Host (
    'SOURCE INVENTORY WRITE PASS ' +
    "symbols=$($sdk.statistics.symbols) " +
    "deep=$($sdk.statistics.deepContracts) " +
    "evidence=$($sdk.statistics.evidenceRecords) " +
    "negative=$($sdk.statistics.negativeFindings) " +
    "pinned=$($inventory.pinned_artifacts.Count)"
)
