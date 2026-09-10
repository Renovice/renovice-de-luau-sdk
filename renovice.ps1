[CmdletBinding()]
param(
    [Parameter(Mandatory, Position = 0)]
    [ValidateSet('decompile', 'recompile', 'readable', 'closure-map', 'api-check', 'sdk-query', 'verify', 'verify-corpus')]
    [string]$Command,
    [Parameter(Position = 1)][string]$Source,
    [Parameter(Position = 2)][string]$Destination,
    [Parameter(ValueFromRemainingArguments = $true)][string[]]$Extra
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = [IO.Path]::GetFullPath($PSScriptRoot)
$derecomp = Join-Path $repo 'bin\derecomp.exe'

function Require-Value([string]$Value, [string]$Name) {
    if ([string]::IsNullOrWhiteSpace($Value)) { throw "$Name is required for $Command." }
}

switch ($Command) {
    'decompile' {
        Require-Value $Source 'input .lua_B path'; Require-Value $Destination 'output .luau path'
        & $derecomp decompile-mod ([IO.Path]::GetFullPath($Source)) ([IO.Path]::GetFullPath($Destination))
    }
    'recompile' {
        Require-Value $Source 'input .luau path'; Require-Value $Destination 'output .lua_B path'
        & $derecomp recompile ([IO.Path]::GetFullPath($Source)) ([IO.Path]::GetFullPath($Destination))
    }
    'readable' {
        Require-Value $Source 'input .lua_B path'; Require-Value $Destination 'output prefix'
        $prefix = [IO.Path]::GetFullPath($Destination)
        $directory = Split-Path -Parent $prefix
        if (-not [string]::IsNullOrWhiteSpace($directory)) { New-Item -ItemType Directory -Force -Path $directory | Out-Null }
        & $derecomp semantic-ir-render-module-readable ([IO.Path]::GetFullPath($Source)) `
            ($prefix + '.fidelity.luau') ($prefix + '.readable.luau') ($prefix + '.names.tsv') `
            --semantic-sdk (Join-Path $repo 'knowledge\semantic-sdk\symbols.tsv') `
            --call-map ($prefix + '.calls.tsv') @Extra
    }
    'closure-map' {
        Require-Value $Source 'input .lua_B path'; Require-Value $Destination 'output .tsv path'
        & $derecomp closure-map ([IO.Path]::GetFullPath($Source)) ([IO.Path]::GetFullPath($Destination))
    }
    'api-check' {
        Require-Value $Source 'input .luau path'
        $checkerArguments = @()
        if (-not [string]::IsNullOrWhiteSpace($Destination)) { $checkerArguments += $Destination }
        if ($null -ne $Extra) { $checkerArguments += $Extra }
        & (Join-Path $repo 'bin\wf_api_check.exe') (Join-Path $repo 'api\warframe\contracts.tsv') ([IO.Path]::GetFullPath($Source)) @checkerArguments
    }
    'sdk-query' {
        Require-Value $Source 'query term'
        $semantic = Join-Path $repo 'semantic-sdk\Build\renovice-semantic.exe'
        if (-not (Test-Path -LiteralPath $semantic -PathType Leaf)) { throw 'Build the Semantic SDK first with .\build.ps1.' }
        & $semantic query $Source --workspace $repo
    }
    'verify' {
        $forward = @($Source, $Destination) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
        if ($null -ne $Extra) { $forward += $Extra }
        & (Join-Path $repo 'verify.ps1') @forward
    }
    'verify-corpus' {
        $forward = @($Source, $Destination) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
        if ($null -ne $Extra) { $forward += $Extra }
        & (Join-Path $repo 'tools\verify-corpus.ps1') @forward
    }
}
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
