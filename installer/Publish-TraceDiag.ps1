[CmdletBinding()]
param([string]$Configuration = 'Release')

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$appDir = Join-Path $PSScriptRoot 'App'
$zipPath = Join-Path $PSScriptRoot 'TraceDiag-install.zip'

if (Test-Path $appDir) { Remove-Item -LiteralPath $appDir -Recurse -Force }
dotnet publish (Join-Path $repoRoot 'TraceDiag\TraceDiag.csproj') -c $Configuration -o $appDir --no-self-contained
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish завершился с ошибкой.' }

if (Test-Path $zipPath) { Remove-Item -LiteralPath $zipPath -Force }
$items = @(
    $appDir,
    (Join-Path $PSScriptRoot 'Install-TraceDiag.ps1'),
    (Join-Path $PSScriptRoot 'Install-TraceDiag.cmd'),
    (Join-Path $PSScriptRoot 'README-Installer.md')
)
Compress-Archive -Path $items -DestinationPath $zipPath -Force
Write-Host "Готов архив для чистого ПК: $zipPath"
