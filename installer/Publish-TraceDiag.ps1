[CmdletBinding()]
param([string]$Configuration = 'Release')

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$windowsDir = Join-Path $PSScriptRoot 'App'
$linuxDir = Join-Path $PSScriptRoot 'ConsoleLinux'
$linuxArmDir = Join-Path $PSScriptRoot 'ConsoleLinux-arm64'
$zipPath = Join-Path $PSScriptRoot 'TraceDiag-install.zip'
$tarPath = Join-Path $PSScriptRoot 'TraceDiag-console-linux-x64.tar.gz'
$tarArmPath = Join-Path $PSScriptRoot 'TraceDiag-console-linux-arm64.tar.gz'

foreach ($dir in @($windowsDir, $linuxDir, $linuxArmDir)) {
    if (Test-Path -LiteralPath $dir) { Remove-Item -LiteralPath $dir -Recurse -Force }
}

dotnet publish (Join-Path $repoRoot 'TraceDiag\TraceDiag.csproj') -c $Configuration -o $windowsDir
if ($LASTEXITCODE -ne 0) { throw 'Публикация WinForms (net48) завершилась с ошибкой.' }

dotnet publish (Join-Path $repoRoot 'TraceDiag.Console\TraceDiag.Console.csproj') -c $Configuration -f net48 -o $windowsDir
if ($LASTEXITCODE -ne 0) { throw 'Публикация консоли (net48) завершилась с ошибкой.' }

dotnet publish (Join-Path $repoRoot 'TraceDiag.Console\TraceDiag.Console.csproj') -c $Configuration -f net10.0 -r linux-x64 --self-contained true -o $linuxDir
if ($LASTEXITCODE -ne 0) { throw 'Публикация консоли (linux-x64) завершилась с ошибкой.' }

dotnet publish (Join-Path $repoRoot 'TraceDiag.Console\TraceDiag.Console.csproj') -c $Configuration -f net10.0 -r linux-arm64 --self-contained true -o $linuxArmDir
if ($LASTEXITCODE -ne 0) { throw 'Публикация консоли (linux-arm64) завершилась с ошибкой.' }

$localizations = Join-Path $repoRoot 'localizations'
$dnsConfig = Join-Path $repoRoot 'dns-servers.json'
foreach ($dir in @($windowsDir, $linuxDir, $linuxArmDir)) {
    Copy-Item -LiteralPath $localizations -Destination $dir -Recurse -Force
    Copy-Item -LiteralPath $dnsConfig -Destination $dir -Force
}

foreach ($dir in @($linuxDir, $linuxArmDir)) {
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'install-console.sh') -Destination $dir
}

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

foreach ($pair in @(@($linuxDir, $tarPath), @($linuxArmDir, $tarArmPath))) {
    $source = $pair[0]
    $target = $pair[1]
    if (Test-Path -LiteralPath $target) { Remove-Item -LiteralPath $target -Force }
    Push-Location $source
    try { tar -czf $target . }
    finally { Pop-Location }
}

Write-Host "Готов архив для Windows: $zipPath"
Write-Host "Готов архив консоли для Linux (amd64): $tarPath"
Write-Host "Готов архив консоли для Linux (arm64): $tarArmPath"
