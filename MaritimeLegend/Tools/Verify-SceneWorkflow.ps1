param([string]$UnityExe='C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe')
$ErrorActionPreference='Stop'
$gameRoot=[IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
& (Join-Path $PSScriptRoot 'Build-Windows.ps1') -UnityExe $UnityExe
$log=Join-Path $gameRoot 'scene-workflow-runtime.log'
$arguments='-batchmode -force-d3d11 -sceneStructureSmoke -logFile "'+$log+'"'
$process=Start-Process -FilePath (Join-Path $gameRoot 'Builds\Windows\MaritimeLegend.exe') -ArgumentList $arguments -WorkingDirectory $gameRoot -WindowStyle Hidden -PassThru
if(-not $process.WaitForExit(120000)){throw 'Scene workflow verification timed out; inspect its isolated process and log.'}
if($process.ExitCode -ne 0 -or -not(Select-String -LiteralPath $log -Pattern 'SCENE_WORKFLOW_PLAYMODE_PASS' -Quiet)){throw ('Scene workflow verification failed: '+$log)}
Write-Output ('Scene workflow verification passed. Evidence: '+(Join-Path $gameRoot 'SceneEvidence'))
