param([string]$UnityExe = 'C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe', [switch]$RunChecks)
$ErrorActionPreference = 'Stop'
$gameRoot = [IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$stageRoot = [IO.Path]::GetFullPath((Join-Path $gameRoot '.BuildWorkspace'))
if (-not $stageRoot.StartsWith($gameRoot + [IO.Path]::DirectorySeparatorChar)) { throw 'Invalid staging directory' }
New-Item -ItemType Directory -Force -Path $stageRoot | Out-Null
foreach ($folder in @('Assets','Packages','ProjectSettings')) { Copy-Item -LiteralPath (Join-Path $gameRoot $folder) -Destination $stageRoot -Recurse -Force }
$buildLog = Join-Path $gameRoot 'build-isolated.log'
$buildArguments = '-batchmode -nographics -quit -projectPath "' + $stageRoot + '" -executeMethod MaritimeLegend.Editor.BuildGame.Build -logFile "' + $buildLog + '"'
$buildProcess = Start-Process -FilePath $UnityExe -ArgumentList $buildArguments -WindowStyle Hidden -PassThru -Wait
if ($buildProcess.ExitCode -ne 0) { throw ('Unity build failed. See ' + $buildLog) }
$release = Join-Path $gameRoot 'Builds\Windows'
New-Item -ItemType Directory -Force -Path $release | Out-Null
Get-ChildItem -LiteralPath (Join-Path $stageRoot 'Builds\Windows') | Where-Object { $_.Name -ne 'Evidence' -and $_.Name -notlike '*DoNotShip*' } | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination $release -Recurse -Force }
Copy-Item -LiteralPath (Join-Path $stageRoot 'Documentation\TestResults.txt') -Destination (Join-Path $gameRoot 'Documentation\TestResults.txt') -Force
if ($RunChecks) {
  $runLog = Join-Path $gameRoot 'runtime-ui.log'
  $runArguments = '-batchmode -force-d3d11 -maritimeSmoke -logFile "' + $runLog + '"'
  $runProcess = Start-Process -FilePath (Join-Path $release 'MaritimeLegend.exe') -ArgumentList $runArguments -WindowStyle Hidden -PassThru -Wait
  if (-not (Select-String -LiteralPath $runLog -Pattern 'RUNTIME_SMOKE_COMPLETE' -Quiet)) { throw ('Runtime checks incomplete. See ' + $runLog) }
  if (Select-String -LiteralPath $runLog -Pattern 'FAILED|NOT_FOUND|Exception:' -Quiet) { throw ('Runtime check failed. See ' + $runLog) }
}
Write-Output ('Ready: ' + (Join-Path $release 'MaritimeLegend.exe'))
