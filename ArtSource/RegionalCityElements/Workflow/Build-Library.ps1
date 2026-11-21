param([string]$UnityExe='C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe',[switch]$Version2)
$ErrorActionPreference='Stop'
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$project=Join-Path $repo 'MaritimeLegend'
$stage=Join-Path $project '.BuildWorkspace'
$review=Join-Path $repo 'ArtSource/RegionalCityElements/Review'
if($Version2){$review=Join-Path $repo 'ArtSource/RegionalCityElements/V2/Review'}
New-Item -ItemType Directory -Force -Path $stage,$review | Out-Null
foreach($folder in @('Assets','Packages','ProjectSettings')){
  Copy-Item -LiteralPath (Join-Path $project $folder) -Destination $stage -Recurse -Force
}
$method=if($Version2){'MaritimeLegend.Editor.RegionalCityExpansionBuilder.Build'}else{'MaritimeLegend.Editor.RegionalCityElementsBuilder.Build'}
$arguments='-batchmode -force-d3d11 -quit -projectPath "'+$stage+'" -executeMethod '+$method+' -regionalReview "'+$review+'" -logFile "'+$review+'\build.log"'
$process=Start-Process -FilePath $UnityExe -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
if($process.ExitCode -ne 0){throw ('Regional city library failed: '+(Join-Path $review 'build.log'))}
foreach($kind in @('Sprites','Models','Materials','Prefabs')){
  $source=Join-Path $stage ('Assets/Resources/'+$kind+'/Cities')
  $destination=Join-Path $project ('Assets/Resources/'+$kind)
  New-Item -ItemType Directory -Force -Path $destination | Out-Null
  Copy-Item -LiteralPath $source -Destination $destination -Recurse -Force
  Copy-Item -LiteralPath ($source+'.meta') -Destination $destination -Force
}
foreach($relative in @('Assets/Resources/Data/Presentation/Cities','Assets/Scenes/ArtPreview')){
  $source=Join-Path $stage $relative;$destination=Split-Path (Join-Path $project $relative) -Parent
  New-Item -ItemType Directory -Force -Path $destination | Out-Null
  Copy-Item -LiteralPath $source -Destination $destination -Recurse -Force
  Copy-Item -LiteralPath ($source+'.meta') -Destination $destination -Force
}
foreach($folder in @('Assets/Cities/Rendering','Assets/Editor','Assets/Resources/Shaders')){
  Get-ChildItem -LiteralPath (Join-Path $stage $folder) -Filter 'Regional*.meta' -File | ForEach-Object {Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $project $folder) -Force}
}
Write-Output 'REGIONAL_CITY_LIBRARY_COPIED'
