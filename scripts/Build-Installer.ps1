[CmdletBinding()]
param(
    [ValidatePattern('^\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?$')]
    [string]$Version = '0.1.2-beta.1',
    [string]$Configuration = 'Release',
    [string]$InnoSetupCompiler = "$env:ProgramFiles\Inno Setup 7\ISCC.exe",
    [switch]$SkipRestore
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot 'src\NexsusVKVideo.App\NexsusVKVideo.App.csproj'
$publishDirectory = Join-Path $repoRoot "artifacts\publish\$Version"
$installerDirectory = Join-Path $repoRoot 'artifacts\installer'
$installerScript = Join-Path $repoRoot 'installer\NexsusVKVideo.iss'
$applicationPath = Join-Path $publishDirectory 'NexsusVKVideo.App.exe'

if (-not (Test-Path -LiteralPath $InnoSetupCompiler -PathType Leaf)) {
    throw "Inno Setup compiler was not found: $InnoSetupCompiler. Install Inno Setup 7 or pass -InnoSetupCompiler."
}

New-Item -ItemType Directory -Path $publishDirectory -Force | Out-Null
New-Item -ItemType Directory -Path $installerDirectory -Force | Out-Null

$publishArguments = @(
    'publish',
    $projectPath,
    '-c', $Configuration,
    '-r', 'win-x64',
    '--self-contained', 'true',
    '-p:PublishSingleFile=true',
    '-p:IncludeNativeLibrariesForSelfExtract=true',
    '-p:EnableCompressionInSingleFile=true',
    '-p:DebugType=none',
    '-p:DebugSymbols=false',
    '-p:PublishTrimmed=false',
    "-p:Version=$Version",
    "-p:InformationalVersion=$Version",
    '-p:UseSharedCompilation=false',
    '-o', $publishDirectory
)

if ($SkipRestore) {
    $publishArguments += '--no-restore'
}

& dotnet @publishArguments
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

if (-not (Test-Path -LiteralPath $applicationPath -PathType Leaf)) {
    throw "Expected single-file application is missing: $applicationPath"
}

& $InnoSetupCompiler "/DAppVersion=$Version" "/DAppSourceDir=$publishDirectory" "/O$installerDirectory" $installerScript
if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup compilation failed with exit code $LASTEXITCODE."
}

$installerPath = Join-Path $installerDirectory "NexsusVKVideo-Setup-$Version.exe"
if (-not (Test-Path -LiteralPath $installerPath -PathType Leaf)) {
    throw "Expected installer is missing: $installerPath"
}

Write-Host "Installer created: $installerPath"
