[CmdletBinding()]
param([string]$Configuration = 'Release')

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$windowsDir = Join-Path $PSScriptRoot 'App'
$linuxDir = Join-Path $PSScriptRoot 'ConsoleLinux'
$zipPath = Join-Path $PSScriptRoot 'TraceDiag-install.zip'
$tarPath = Join-Path $PSScriptRoot 'TraceDiag-console-linux-x64.tar.gz'

foreach ($dir in @($windowsDir, $linuxDir)) {
    if (Test-Path -LiteralPath $dir) { Remove-Item -LiteralPath $dir -Recurse -Force }
}

dotnet publish (Join-Path $repoRoot 'TraceDiag\TraceDiag.csproj') -c $Configuration -o $windowsDir
if ($LASTEXITCODE -ne 0) { throw 'Публикация WinForms (net48) завершилась с ошибкой.' }

dotnet publish (Join-Path $repoRoot 'TraceDiag.Console\TraceDiag.Console.csproj') -c $Configuration -f net48 -o $windowsDir
if ($LASTEXITCODE -ne 0) { throw 'Публикация консоли (net48) завершилась с ошибкой.' }

dotnet publish (Join-Path $repoRoot 'TraceDiag.Console\TraceDiag.Console.csproj') -c $Configuration -f net10.0 -r linux-x64 --self-contained true -o $linuxDir
if ($LASTEXITCODE -ne 0) { throw 'Публикация консоли (linux-x64) завершилась с ошибкой.' }

Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'install-console.sh') -Destination $linuxDir

if (Test-Path -LiteralPath $zipPath) { Remove-Item -LiteralPath $zipPath -Force }
$zipItems = @(
    $windowsDir,
    (Join-Path $PSScriptRoot 'Install-TraceDiag.ps1'),
    (Join-Path $PSScriptRoot 'Install-TraceDiag.cmd'),
    (Join-Path $PSScriptRoot 'README-Installer.md')
)
$frameworkInstaller = Join-Path $PSScriptRoot 'dotNetFx48_Full.exe'
if (Test-Path -LiteralPath $frameworkInstaller) { $zipItems += $frameworkInstaller }
Compress-Archive -Path $zipItems -DestinationPath $zipPath -Force

if (Test-Path -LiteralPath $tarPath) { Remove-Item -LiteralPath $tarPath -Force }
Push-Location $linuxDir
try { tar -czf $tarPath . }
finally { Pop-Location }

Write-Host "Готов архив для Windows: $zipPath"
Write-Host "Готов архив консоли для Linux: $tarPath"
