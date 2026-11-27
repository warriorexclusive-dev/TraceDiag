[CmdletBinding()]
param(
    [string]$AppSource,
    [string]$InstallDir,
    [string]$Framework48InstallerPath,
    [string]$Framework48Url,
    [switch]$DesktopShortcut,
    [switch]$NoShortcut,
    [switch]$NoLaunch
)

$ErrorActionPreference = 'Stop'
$Framework48InstallerName = 'dotNetFx48_Full.exe'
if (-not $AppSource) { $AppSource = Join-Path $PSScriptRoot 'App' }
if (-not $InstallDir) { $InstallDir = Join-Path $env:LOCALAPPDATA 'Programs\TraceDiag' }

function Test-Framework48 {
    $keys = @(
        'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full',
        'HKLM:\SOFTWARE\WOW6432Node\Microsoft\NET Framework Setup\NDP\v4\Full'
    )
    foreach ($key in $keys) {
        if (-not (Test-Path -LiteralPath $key)) { continue }
        $release = (Get-ItemProperty -LiteralPath $key -Name Release -ErrorAction SilentlyContinue).Release
        if ($release -ge 528040) { return $true }
    }
    return $false
}

function Test-IsAdministrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Enable-Tls12 {
    try {
        $tls12 = [Net.SecurityProtocolType]3072
        [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor $tls12
    }
    catch { }
}

function Find-Framework48Installer {
    if ($Framework48InstallerPath) {
        if (-not (Test-Path -LiteralPath $Framework48InstallerPath)) {
            throw "Файл '$Framework48InstallerPath' не найден."
        }
        return $Framework48InstallerPath
    }
    $candidates = @(
        (Join-Path $PSScriptRoot $Framework48InstallerName),
        (Join-Path $AppSource $Framework48InstallerName)
    )
    return $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
}

function Get-Framework48Installer {
    $found = Find-Framework48Installer
    if ($found) { return $found }
    if (-not $Framework48Url) {
        throw "Не найден $Framework48InstallerName. Положите его рядом с установщиком или передайте -Framework48Url (альтернативный репозиторий)."
    }
    Enable-Tls12
    $script:Framework48TempInstaller = Join-Path ([IO.Path]::GetTempPath()) $Framework48InstallerName
    Invoke-WebRequest -Uri $Framework48Url -OutFile $script:Framework48TempInstaller -UseBasicParsing
    return $script:Framework48TempInstaller
}

function Remove-Framework48TempInstaller {
    if (-not $script:Framework48TempInstaller) { return }
    Remove-Item -LiteralPath $script:Framework48TempInstaller -Force -ErrorAction SilentlyContinue
    $script:Framework48TempInstaller = $null
}

function Install-Framework48 {
    if (-not (Test-IsAdministrator)) {
        throw 'Для установки .NET Framework 4.8 нужны права администратора.'
    }
    Write-Host 'Установка .NET Framework 4.8 (тихая, с перезапуском при необходимости)...'
    $installer = Get-Framework48Installer
    try {
        $process = Start-Process -FilePath $installer -ArgumentList '/quiet', '/norestart' -Wait -PassThru
        if ($process.ExitCode -ne 0 -and $process.ExitCode -ne 3010) {
            throw "Установщик .NET Framework 4.8 завершился с кодом $($process.ExitCode)."
        }
    }
    finally { Remove-Framework48TempInstaller }
}

function Copy-AppFiles {
    $exe = Join-Path $AppSource 'TraceDiag.exe'
    if (-not (Test-Path -LiteralPath $exe)) {
        throw "В папке '$AppSource' нет TraceDiag.exe. Сначала запустите Publish-TraceDiag.ps1."
    }
    New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null
    Copy-Item -Path (Join-Path $AppSource '*') -Destination $InstallDir -Recurse -Force
}

function New-TraceDiagShortcut([string]$path) {
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($path)
    $shortcut.TargetPath = Join-Path $InstallDir 'TraceDiag.exe'
    $shortcut.IconLocation = Join-Path $InstallDir 'TraceDiag.exe'
    $shortcut.WorkingDirectory = $InstallDir
    $shortcut.Save()
}

if (-not (Test-Framework48)) {
    Install-Framework48
    if (-not (Test-Framework48)) { throw '.NET Framework 4.8 не установлен. Установка прервана.' }
}
Copy-AppFiles

if (-not $NoShortcut) {
    $menu = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs'
    New-Item -ItemType Directory -Force -Path $menu | Out-Null
    New-TraceDiagShortcut (Join-Path $menu 'TraceDiag.lnk')
    if ($DesktopShortcut) {
        $desktop = [Environment]::GetFolderPath('Desktop')
        New-TraceDiagShortcut (Join-Path $desktop 'TraceDiag.lnk')
    }
}

Write-Host "TraceDiag установлен в: $InstallDir"
Write-Host "Консоль: $(Join-Path $InstallDir 'TraceDiag.Console.exe')"
if (-not $NoLaunch) {
    Start-Process -FilePath (Join-Path $InstallDir 'TraceDiag.exe') -WorkingDirectory $InstallDir
}
