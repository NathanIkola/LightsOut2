<#
.SYNOPSIS
Builds and deploys the LightsOut2 mod for the VS Code launch configuration.

.DESCRIPTION
Uses RIMWORLD_DIR when set. Otherwise, uses the default Steam installation path under Program Files (x86).
The existing LightsOut2 mod folder in the game's Mods directory is replaced with this repository's mod files.
#>

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $PSScriptRoot 'LightsOut2\LightsOut2\LightsOut2.csproj'
$rimWorldDir = if ($env:RIMWORLD_DIR) {
    $env:RIMWORLD_DIR
} else {
    Join-Path ${env:ProgramFiles(x86)} 'Steam\steamapps\common\RimWorld'
}
$gameExecutable = Join-Path $rimWorldDir 'RimWorld.exe'
$modDestination = Join-Path $rimWorldDir 'Mods\LightsOut2'

if (-not (Test-Path -LiteralPath $gameExecutable -PathType Leaf)) {
    throw "RimWorld.exe was not found at '$gameExecutable'. Set RIMWORLD_DIR to your RimWorld installation folder."
}

dotnet build $projectPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

if (Test-Path -LiteralPath $modDestination) {
    Remove-Item -LiteralPath $modDestination -Recurse -Force
}
New-Item -ItemType Directory -Path $modDestination | Out-Null
foreach ($folder in @('About', 'Assemblies', 'Languages', 'Patches')) {
    Copy-Item -LiteralPath (Join-Path $repoRoot $folder) -Destination $modDestination -Recurse
}
Copy-Item -LiteralPath (Join-Path $repoRoot 'loadFolders.xml') -Destination $modDestination
