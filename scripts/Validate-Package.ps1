$ErrorActionPreference = 'Stop'
$kitRoot = Split-Path $PSScriptRoot -Parent
$required = @(
 'README.md','AGENTS.md','START_HERE_FOR_CODEX.md','CODEX_WORKFLOW.md',
 'CODEX_TOKEN_OPTIMIZATION.md','NexsusVKVideo_Development_Prompt.md',
 'SECURITY.md','RELEASE_PROCESS.md','docs/ARCHITECTURE.md','docs/MODULES.md',
 'docs/ROADMAP.md','docs/CODING_STYLE.md','docs/DESIGN_IMPLEMENTATION_GUIDE.md',
 'assets/mockups/MainWindow_Mockup.png','assets/branding/README.md',
 '.github/workflows/build.yml','.github/copilot-instructions.md',
 'changes/README.md','src/README.md','tests/README.md'
)
foreach ($item in $required) {
 $kitPath = Join-Path $kitRoot $item
 if (!(Test-Path -LiteralPath $kitPath -PathType Leaf)) { throw "Missing: $item" }
 if ((Get-Item -LiteralPath $kitPath).Length -eq 0) { throw "Empty: $item" }
}
$logo = Join-Path $kitRoot 'assets/branding/a_sleek_high_resolution_app_icon_logo_design_on.png'
if (!(Test-Path -LiteralPath $logo)) { throw 'Required approved logo is missing.' }
foreach ($item in @('NexsusVKVideo.sln', 'global.json')) {
 $kitPath = Join-Path $kitRoot $item
 if (!(Test-Path -LiteralPath $kitPath -PathType Leaf)) { throw "Required after stage 0: $item" }
}
if (!(Get-ChildItem -Path (Join-Path $kitRoot 'src') -Recurse -Filter *.csproj)) { throw 'Required after stage 0: source project.' }
if (!(Get-ChildItem -Path (Join-Path $kitRoot 'tests') -Recurse -Filter *.csproj)) { throw 'Required after stage 0: test project.' }
Write-Host "Validated $($required.Count) required package entries."
