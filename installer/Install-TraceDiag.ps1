[CmdletBinding()]
param(
    [string]$AppSource,
    [string]$InstallDir,
    [switch]$DesktopShortcut,
    [switch]$NoShortcut,
    [switch]$NoLaunch
)

$ErrorActionPreference = 'Stop'
if (-not $AppSource) { $AppSource = Join-Path $PSScriptRoot 'App' }
if (-not $InstallDir) { $InstallDir = Join-Path $env:LOCALAPPDATA 'Programs\TraceDiag' }
$DotNetInstallDir = Join-Path $env:LOCALAPPDATA 'Microsoft\dotnet'
$Channel = '10.0'

function Get-TargetArchitecture {
    $arch = $env:PROCESSOR_ARCHITECTURE
    if ($arch -eq 'AMD64' -or $env:PROCESSOR_ARCHITEW6432 -eq 'AMD64') { return 'x64' }
    if ($arch -eq 'ARM64' -or $env:PROCESSOR_ARCHITEW6432 -eq 'ARM64') { return 'arm64' }
    return 'x86'
}

function Test-DesktopRuntime {
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($dotnet) {
        $runtimes = & $dotnet.Source --list-runtimes 2>$null
        if ($runtimes | Where-Object { $_ -match '^Microsoft\.WindowsDesktop\.App\s+10\.' }) { return $true }
    }
    $shared = Join-Path $DotNetInstallDir 'shared\Microsoft.WindowsDesktop.App'
    if (-not (Test-Path $shared)) { return $false }
    return [bool](Get-ChildItem -LiteralPath $shared -Directory -Filter '10.*' -ErrorAction SilentlyContinue)
}

function Install-DesktopRuntime {
    $arch = Get-TargetArchitecture
    Write-Host "Установка .NET $Channel Desktop Runtime ($arch) в профиль пользователя..."
    $name = "dotnet-install-$([guid]::NewGuid().ToString('N')).ps1"
    $scriptPath = Join-Path ([IO.Path]::GetTempPath()) $name
    Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile $scriptPath -UseBasicParsing
    Unblock-File -Path $scriptPath -ErrorAction SilentlyContinue
    & $scriptPath -Channel $Channel -Runtime windowsdesktop -Architecture $arch -InstallDir $DotNetInstallDir -NoPath
    Remove-Item -LiteralPath $scriptPath -Force -ErrorAction SilentlyContinue
    $env:DOTNET_ROOT = $DotNetInstallDir
}

function Copy-AppFiles {
    $exe = Join-Path $AppSource 'TraceDiag.exe'
    if (-not (Test-Path $exe)) {
        throw "В папке '$AppSource' нет TraceDiag.exe. Сначала запустите Publish-TraceDiag.ps1."
    }
    New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null
    Copy-Item -Path (Join-Path $AppSource '*') -Destination $InstallDir -Recurse -Force
}

function Get-Launcher {
    $dll = Join-Path $InstallDir 'TraceDiag.dll'
    foreach ($candidate in @((Join-Path $DotNetInstallDir 'dotnet.exe'), (Join-Path $env:ProgramFiles 'dotnet\dotnet.exe'))) {
        if (Test-Path $candidate) { return @{ File = $candidate; Args = "`"$dll`"" } }
    }
    return @{ File = (Join-Path $InstallDir 'TraceDiag.exe'); Args = '' }
}

function New-TraceDiagShortcut([string]$path, $launcher) {
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($path)
    $shortcut.TargetPath = $launcher.File
    $shortcut.Arguments = $launcher.Args
    $shortcut.IconLocation = Join-Path $InstallDir 'TraceDiag.exe'
    $shortcut.WorkingDirectory = $InstallDir
    $shortcut.Save()
}

if (-not (Test-DesktopRuntime)) { Install-DesktopRuntime }
Copy-AppFiles
$launcher = Get-Launcher

if (-not $NoShortcut) {
    $menu = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs'
    New-Item -ItemType Directory -Force -Path $menu | Out-Null
    New-TraceDiagShortcut (Join-Path $menu 'TraceDiag.lnk') $launcher
    if ($DesktopShortcut) {
        $desktop = [Environment]::GetFolderPath('Desktop')
        New-TraceDiagShortcut (Join-Path $desktop 'TraceDiag.lnk') $launcher
    }
}

Write-Host "TraceDiag установлен в: $InstallDir"
if (-not $NoLaunch) {
    Start-Process -FilePath $launcher.File -ArgumentList $launcher.Args -WorkingDirectory $InstallDir
}
