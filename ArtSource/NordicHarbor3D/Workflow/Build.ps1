param([string]$UnityExe='C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe',[switch]$Player)
$ErrorActionPreference='Stop'
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$project=Join-Path $repo 'MaritimeLegend'
$source=Join-Path $repo 'ArtSource/NordicHarbor3D'
$stage=Join-Path $project '.BuildWorkspace'
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'NordicHarbor3DBuilder.cs') -Destination (Join-Path $project 'Assets/Editor/NordicHarbor3DBuilder.cs') -Force
New-Item -ItemType Directory -Force -Path $stage | Out-Null
foreach($folder in @('Assets','Packages','ProjectSettings')){Copy-Item -LiteralPath (Join-Path $project $folder) -Destination $stage -Recurse -Force}
$method=if($Player){'MaritimeLegend.Editor.NordicHarbor3DBuilder.BuildWithPlayer'}else{'MaritimeLegend.Editor.NordicHarbor3DBuilder.Build'}
$arguments='-batchmode -force-d3d11 -quit -projectPath "'+$stage+'" -executeMethod '+$method+' -nordicSource "'+$source+'" -logFile "'+$source+'\Review\build.log"'
$process=Start-Process -FilePath $UnityExe -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
if($process.ExitCode -ne 0){throw 'Nordic harbor build failed; see Review/build.log'}
foreach($kind in @('Models','Materials','Prefabs')){
 $relative='Assets/Resources/'+$kind+'/Cities/Nordic/Harbor3D'
 $src=Join-Path $stage $relative;$parent=Split-Path (Join-Path $project $relative) -Parent
 New-Item -ItemType Directory -Force -Path $parent | Out-Null
 Copy-Item -LiteralPath $src -Destination $parent -Recurse -Force
 Copy-Item -LiteralPath ($src+'.meta') -Destination $parent -Force
}
foreach($relative in @('Assets/Scenes/NordicHarborScene.unity','Assets/Scenes/NordicHarborScene.unity.meta')){Copy-Item -LiteralPath (Join-Path $stage $relative) -Destination (Join-Path $project $relative) -Force}
foreach($folder in @('Assets/Cities/Rendering','Assets/Editor','Assets/Resources/Shaders')){Get-ChildItem -LiteralPath (Join-Path $stage $folder) -Filter 'Nordic*.meta' -File | ForEach-Object {Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $project $folder) -Force}}
if($Player){
 $destination=Join-Path $project 'Builds/NordicHarbor3D';New-Item -ItemType Directory -Force -Path $destination | Out-Null
 Get-ChildItem -LiteralPath (Join-Path $stage 'Builds/NordicHarbor3D') | Where-Object {$_.Name -notlike '*DoNotShip*'} | ForEach-Object {Copy-Item -LiteralPath $_.FullName -Destination $destination -Recurse -Force}
 $runArgs='-batchmode -force-d3d11 -nordicHarborSmoke -nordicReview "'+$source+'\Review" -logFile "'+$source+'\Review\runtime.log"'
 $run=Start-Process -FilePath (Join-Path $destination 'NordicHarbor.exe') -ArgumentList $runArgs -WindowStyle Hidden -WorkingDirectory $project -PassThru -Wait
 if($run.ExitCode -ne 0 -or -not (Select-String -LiteralPath (Join-Path $source 'Review/runtime.log') -Pattern 'NORDIC_HARBOR_RUNTIME_PASS' -Quiet)){throw 'Nordic standalone runtime check failed'}
}
Write-Output 'NORDIC_HARBOR_3D_COPIED'
