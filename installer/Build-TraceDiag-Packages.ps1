[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [string]$Version,
    [switch]$SkipPublish,
    [switch]$SkipDeb
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$buildRoot = Join-Path $PSScriptRoot 'build'
$distRoot = Join-Path $PSScriptRoot 'dist'
$wixDir = Join-Path $PSScriptRoot 'wix'
$guiProject = Join-Path $repoRoot 'TraceDiag\TraceDiag.csproj'
$consoleProject = Join-Path $repoRoot 'TraceDiag.Console\TraceDiag.Console.csproj'
$localizations = Join-Path $repoRoot 'localizations'

if (-not $Version) {
    $projectText = Get-Content -LiteralPath $guiProject -Raw
    $Version = [regex]::Match($projectText, '<Version>([^<]+)</Version>').Groups[1].Value
}
if (-not $Version) { throw 'Cannot detect version from TraceDiag.csproj.' }

function Assert-ExitCode { if ($LASTEXITCODE -ne 0) { throw "External command failed with exit code $LASTEXITCODE." } }

function Publish-Windows {
    param([string]$Platform, [string]$Directory)
    dotnet publish $guiProject -c $Configuration -f net48 -p:Platform=$Platform -p:PlatformTarget=$Platform -o $Directory -v:q
    Assert-ExitCode
    dotnet publish $consoleProject -c $Configuration -f net48 -p:Platform=$Platform -p:PlatformTarget=$Platform -o $Directory -v:q
    Assert-ExitCode
    Copy-Item -LiteralPath $localizations -Destination $Directory -Recurse -Force
}

function Publish-Linux {
    param([string]$Runtime, [string]$Directory)
    dotnet publish $consoleProject -c $Configuration -f net10.0 -r $Runtime --self-contained true `
        -p:PlatformTarget=AnyCPU -o $Directory -v:q
    Assert-ExitCode
    Copy-Item -LiteralPath $localizations -Destination $Directory -Recurse -Force
}

function New-StableGuid {
    param([string]$Name)
    $hash = [Security.Cryptography.SHA1]::Create().ComputeHash([Text.Encoding]::UTF8.GetBytes($Name))
    $bytes = [byte[]]$hash[0..15]
    $bytes[7] = [byte](($bytes[7] -band 0x0f) -bor 0x50)
    $bytes[8] = [byte](($bytes[8] -band 0x3f) -bor 0x80)
    (New-Object System.Guid -ArgumentList (,$bytes)).ToString()
}

function New-WixFragment {
    param([string]$SourceDirectory, [string]$Architecture, [string]$Path)
    $lines = @(
        '<?xml version="1.0" encoding="UTF-8"?>',
        '<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">',
        '  <Fragment>',
        '    <ComponentGroup Id="AppFiles">'
    )
    $files = Get-ChildItem -LiteralPath $SourceDirectory -File -Recurse | Sort-Object FullName
    $root = [IO.Path]::GetFullPath($SourceDirectory).TrimEnd('\', '/') + '\'
    foreach ($file in $files) {
        $relative = $file.FullName.Substring($root.Length).Replace('\', '\')
        $target = if ($relative.StartsWith('localizations\')) { 'LOCALIZATIONSFOLDER' } else { 'INSTALLFOLDER' }
        $suffix = ($relative -replace '[^A-Za-z0-9]', '_')
        $component = New-StableGuid "tracediag|component|$Architecture|$relative"
        $lines += "      <Component Id=`"cmp_$suffix`" Directory=`"$target`" Guid=`"$component`">"
        $lines += "        <File Id=`"fil_$suffix`" Source=`"$relative`" KeyPath=`"yes`" />"
        $lines += '      </Component>'
    }
    $lines += @('    </ComponentGroup>', '  </Fragment>', '</Wix>')
    Set-Content -LiteralPath $Path -Value $lines -Encoding UTF8
}

function Invoke-Wix {
    param([string]$Architecture, [string]$SourceDirectory, [string]$Output)
    $fragment = Join-Path $buildRoot "Files.$Architecture.wxs"
    New-WixFragment -SourceDirectory $SourceDirectory -Architecture $Architecture -Path $fragment
    Push-Location $repoRoot
    try {
        dotnet wix build (Join-Path $wixDir "Product.$Architecture.wxs") $fragment `
            -arch $Architecture -d "Version=$Version" -b $SourceDirectory -o $Output -pdbtype none
        Assert-ExitCode
    }
    finally { Pop-Location }
}

function Convert-ToWslPath {
    param([string]$Path)
    $full = [IO.Path]::GetFullPath($Path)
    '/mnt/' + $full.Substring(0, 1).ToLowerInvariant() + $full.Substring(2).Replace('\', '/')
}

function Invoke-DebBuild {
    param([string]$Architecture, [string]$SourceDirectory, [string]$Output)
    if (-not (Get-Command wsl.exe -ErrorAction SilentlyContinue)) {
        throw 'WSL with dpkg-deb is required to build .deb packages.'
    }
    $script = Convert-ToWslPath (Join-Path $PSScriptRoot 'deb\build-deb.sh')
    $source = Convert-ToWslPath $SourceDirectory
    $target = Convert-ToWslPath $Output
    $command = "bash '$script' '$source' '$Version' '$Architecture' '$target'"
    & wsl.exe -- bash -lc $command
    Assert-ExitCode
}

New-Item -ItemType Directory -Force $buildRoot, $distRoot | Out-Null

if (-not $SkipPublish) {
    Publish-Windows 'x86' (Join-Path $buildRoot 'msi-x86')
    Publish-Windows 'x64' (Join-Path $buildRoot 'msi-x64')
    if (-not $SkipDeb) {
        Publish-Linux 'linux-x64' (Join-Path $buildRoot 'deb-amd64')
        Publish-Linux 'linux-arm64' (Join-Path $buildRoot 'deb-arm64')
    }
}

Invoke-Wix 'x86' (Join-Path $buildRoot 'msi-x86') (Join-Path $distRoot "TraceDiag-$Version-x86.msi")
Invoke-Wix 'x64' (Join-Path $buildRoot 'msi-x64') (Join-Path $distRoot "TraceDiag-$Version-x64.msi")

if (-not $SkipDeb) {
    Invoke-DebBuild 'amd64' (Join-Path $buildRoot 'deb-amd64') (Join-Path $distRoot "tracediag_$Version`_amd64.deb")
    Invoke-DebBuild 'arm64' (Join-Path $buildRoot 'deb-arm64') (Join-Path $distRoot "tracediag_$Version`_arm64.deb")
}

if (-not $SkipPublish) {
    dotnet build (Join-Path $repoRoot 'TraceDiag.Core\TraceDiag.Core.csproj') -c $Configuration `
        -t:Rebuild -p:PlatformTarget=AnyCPU -v:q
    Assert-ExitCode
}

Get-ChildItem -LiteralPath $distRoot -File | Select-Object -ExpandProperty FullName
