param([string]$UnityExe='C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe')
$ErrorActionPreference='Stop'
$gameRoot=[IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
& (Join-Path $PSScriptRoot 'Build-Windows.ps1') -UnityExe $UnityExe
$log=Join-Path $gameRoot 'city-art-runtime.log'
$arguments='-batchmode -force-d3d11 -cityArtSmoke -logFile "'+$log+'"'
$process=Start-Process -FilePath (Join-Path $gameRoot 'Builds\Windows\MaritimeLegend.exe') -ArgumentList $arguments -WorkingDirectory $gameRoot -WindowStyle Hidden -PassThru
if(-not $process.WaitForExit(120000)){throw 'City art runtime test timed out; inspect its isolated process and log.'}
if($process.ExitCode -ne 0 -or -not(Select-String -LiteralPath $log -Pattern 'CITY_ART_PLAYMODE_PASS' -Quiet)){throw ('City art test failed: '+$log)}
Write-Output ('City art verified. Runtime screenshots: '+(Join-Path $gameRoot 'CityArtEvidence'))
