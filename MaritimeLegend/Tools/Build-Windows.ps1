param([string]$UnityExe = 'C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe', [switch]$RunChecks, [switch]$CityArtPreview)
$ErrorActionPreference = 'Stop'
$gameRoot = [IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$stageRoot = [IO.Path]::GetFullPath((Join-Path $gameRoot '.BuildWorkspace'))
if (-not $stageRoot.StartsWith($gameRoot + [IO.Path]::DirectorySeparatorChar)) { throw 'Invalid staging directory' }
New-Item -ItemType Directory -Force -Path $stageRoot | Out-Null
foreach ($folder in @('Assets','Packages','ProjectSettings')) {
  $stageFolder = [IO.Path]::GetFullPath((Join-Path $stageRoot $folder))
  if (-not $stageFolder.StartsWith($stageRoot + [IO.Path]::DirectorySeparatorChar)) { throw 'Invalid staging target' }
  # A merge-copy retains deleted/renamed scripts and can compile duplicate classes.
  if (Test-Path -LiteralPath $stageFolder) { Remove-Item -LiteralPath $stageFolder -Recurse -Force }
  Copy-Item -LiteralPath (Join-Path $gameRoot $folder) -Destination $stageRoot -Recurse -Force
}
$method = if ($CityArtPreview) { 'MaritimeLegend.Editor.CityArtSetup.BuildPreview' } else { 'MaritimeLegend.Editor.BuildGame.Build' }
$releaseFolder = if ($CityArtPreview) { 'Builds\CityPreview' } else { 'Builds\Windows' }
$buildLog = Join-Path $gameRoot $(if ($CityArtPreview) { 'daylight-preview-build.log' } else { 'build-isolated.log' })
$buildArguments = '-batchmode -nographics -quit -projectPath "' + $stageRoot + '" -executeMethod ' + $method + ' -logFile "' + $buildLog + '"'
$buildProcess = Start-Process -FilePath $UnityExe -ArgumentList $buildArguments -WindowStyle Hidden -PassThru -Wait
if ($buildProcess.ExitCode -ne 0) { throw ('Unity build failed. See ' + $buildLog) }
$release = Join-Path $gameRoot $releaseFolder
New-Item -ItemType Directory -Force -Path $release | Out-Null
Get-ChildItem -LiteralPath (Join-Path $stageRoot $releaseFolder) | Where-Object { $_.Name -ne 'Evidence' -and $_.Name -notlike '*DoNotShip*' } | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination $release -Recurse -Force }
if (-not $CityArtPreview) { Copy-Item -LiteralPath (Join-Path $stageRoot 'Documentation\TestResults.txt') -Destination (Join-Path $gameRoot 'Documentation\TestResults.txt') -Force }
if ($RunChecks) {
  $runLog = Join-Path $gameRoot $(if ($CityArtPreview) { 'daylight-runtime.log' } else { 'runtime-ui.log' })
  $smokeFlag = if ($CityArtPreview) { '-cityArtSmoke' } else { '-maritimeSmoke' }
  $passMarker = if ($CityArtPreview) { 'CITY_ART_PLAYMODE_PASS' } else { 'RUNTIME_SMOKE_COMPLETE' }
  $runArguments = '-batchmode -force-d3d11 ' + $smokeFlag + ' -logFile "' + $runLog + '"'
  $runProcess = Start-Process -FilePath (Join-Path $release 'MaritimeLegend.exe') -ArgumentList $runArguments -WorkingDirectory $gameRoot -WindowStyle Hidden -PassThru -Wait
  if (-not (Select-String -LiteralPath $runLog -Pattern $passMarker -Quiet)) { throw ('Runtime checks incomplete. See ' + $runLog) }
  if (Select-String -LiteralPath $runLog -Pattern 'FAILED|NOT_FOUND|Exception:' -Quiet) { throw ('Runtime check failed. See ' + $runLog) }
}
Write-Output ('Ready: ' + (Join-Path $release 'MaritimeLegend.exe'))
