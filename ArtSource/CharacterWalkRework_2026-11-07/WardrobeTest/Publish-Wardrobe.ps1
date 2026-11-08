param([string]$NodeExe='C:/Users/charl/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/bin/node.exe')
$ErrorActionPreference='Stop'
& $NodeExe (Join-Path $PSScriptRoot 'build-layers.cjs')
if($LASTEXITCODE -ne 0){throw 'Layer generation failed'}
$checks=Get-Content (Join-Path $PSScriptRoot 'checks.json') -Raw | ConvertFrom-Json
if(-not $checks.unchangedBodyPixels -or -not $checks.protectedFacesUnchanged -or -not $checks.feetUnchanged){throw 'Protected pixels changed; inspect checks.json before publishing'}
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$target=Join-Path $repo 'MaritimeLegend/Assets/Resources/Sprites/Characters/Character06/WardrobeTest'
New-Item -ItemType Directory -Force -Path $target | Out-Null
foreach($name in @('body-no-head','C2-H1-cloth','C2-H1-head','C2-H1-beads','C5-H7-cloth','C5-H7-head','C5-H7-beads')){
 Copy-Item -LiteralPath (Join-Path $PSScriptRoot "Layers/$name.png") -Destination (Join-Path $target "$name.png") -Force
}
Write-Output "Updated seven runtime layer atlases in $target. Rebuild standalone players to include changes."
