param([switch]$Version2)
$ErrorActionPreference='Stop'
$review=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../Review'))
if($Version2){$review=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../V2/Review'))}
$regions=@('Europe','Nordic','Mediterranean','Africa','Asia','SouthAmerica')
foreach($region in $regions){
  $label=if($region -eq 'SouthAmerica'){'South America'}else{$region}
  $filter="drawtext=fontfile='C\:/Windows/Fonts/arial.ttf':text='$label':x=20:y=16:fontsize=28:fontcolor=white:shadowcolor=black:shadowx=1:shadowy=1,format=yuv420p"
  & ffmpeg -hide_banner -loglevel error -y -framerate 12 -i (Join-Path $review ($region+'-%02d.png')) -vf $filter -c:v libx264 -crf 18 -movflags +faststart (Join-Path $review ($region+'.mp4'))
  if($LASTEXITCODE -ne 0){throw ('Video encode failed: '+$region)}
}
$arguments=@('-hide_banner','-loglevel','error','-y')
foreach($region in $regions){$arguments+=@('-i',(Join-Path $review ($region+'.mp4')))}
$filter='[0:v]scale=512:384[a];[1:v]scale=512:384[b];[2:v]scale=512:384[c];[3:v]scale=512:384[d];[4:v]scale=512:384[e];[5:v]scale=512:384[f];[a][b][c][d][e][f]xstack=inputs=6:layout=0_0|512_0|1024_0|0_384|512_384|1024_384[out]'
$arguments+=@('-filter_complex',$filter,'-map','[out]','-c:v','libx264','-crf','18','-pix_fmt','yuv420p','-movflags','+faststart',(Join-Path $review 'SixStyles.mp4'))
& ffmpeg @arguments
if($LASTEXITCODE -ne 0){throw 'Six-style video encode failed'}
& ffmpeg -hide_banner -loglevel error -y -i (Join-Path $review 'SixStyles.mp4') -frames:v 1 (Join-Path $review 'SixStyles.png')
if($LASTEXITCODE -ne 0){throw 'Overview extraction failed'}
Write-Output 'REGIONAL_CITY_VIDEOS_READY'
