$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$output = Join-Path $projectRoot 'Build'
dotnet publish (Join-Path $projectRoot 'src\Renovice.SemanticSdk.Cli\Renovice.SemanticSdk.Cli.csproj') `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:DebugType=embedded `
    --output $output
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "BUILD PASS -> $output\renovice-semantic.exe"
