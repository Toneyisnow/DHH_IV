$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$root=Split-Path $PSScriptRoot -Parent
$source='C:/Users/charl/.codex/generated_images/01a128e3-154e-7493-a389-499bcb5f49f3'
@('01_map','02_frame','03_ui','04_cities','05_ships/square','05_ships/lateen','05_ships/junk','06_routes','07_currents/frames','08_reference') | ForEach-Object { New-Item -ItemType Directory -Force (Join-Path $root $_) | Out-Null }
$files=@{
'exec-72e6ee50-fb42-435c-9164-29ee44ea4adf.png'='01_map/east_asia.png'
'exec-67889e9b-ad1d-4fca-ad39-7ed529792391.png'='02_frame/frame_instruments.png'
'exec-2729f8d6-95cd-4004-afe7-6286226b75f1.png'='03_ui/atlas.png'
'exec-9df5a5ec-e7ce-440a-8c5e-d96d1b57c998.png'='04_cities/atlas.png'
'exec-a8cb6caf-9106-46f8-b2c7-11170c8fbb1a.png'='05_ships/square/atlas.png'
'exec-64b9c2da-cc41-4ec6-9758-54d787a98667.png'='05_ships/lateen/atlas.png'
'exec-311bad5c-7755-464f-9ed2-3f550d957736.png'='05_ships/junk/atlas.png'
}
foreach($f in $files.GetEnumerator()){Copy-Item -LiteralPath (Join-Path $source $f.Key) -Destination (Join-Path $root $f.Value)}
Copy-Item -LiteralPath 'D:/PlayGround/大航海时代复刻/Screenshots/Screenshot 2026-10-10 173126.png' -Destination (Join-Path $root '08_reference/screenshot.png')
$script:items=[System.Collections.Generic.List[object]]::new()
function Cut-Atlas($relative,$xs,$ys,$names){
 $im=[System.Drawing.Bitmap]::new((Join-Path $root $relative)); $folder=Split-Path $relative -Parent; $i=0
 for($row=0;$row -lt $ys.Count-1;$row++){for($col=0;$col -lt $xs.Count-1;$col++){
  $x=$xs[$col];$y=$ys[$row];$w=$xs[$col+1]-$x;$h=$ys[$row+1]-$y
  $rect=[System.Drawing.Rectangle]::new($x,$y,$w,$h)
  $cut=$im.Clone($rect,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
  $path="$folder/$($names[$i]).png";$cut.Save((Join-Path $root $path),[System.Drawing.Imaging.ImageFormat]::Png)
  $script:items.Add(@{file=$path.Replace('\','/');source=$relative;x=$x;y=$y;width=$w;height=$h;anchor=@(0.5,0.75)})
  $cut.Dispose();$i++
 }};$im.Dispose()
}
Cut-Atlas '03_ui/atlas.png' @(0,560,1120,1680) @(0,276,472,666,936) @('deck','consult','assign_sailors','functions','faction_kurushima','faction_li','go_world','go_southeast_asia','date','enter_port','leave_port','plaque_blank')
Cut-Atlas '04_cities/atlas.png' @(0,550,1080,1536) @(0,500,1024) @('chinese_palace','chinese_walled_port','chinese_pagoda_port','korean_port','japanese_pagoda_port','japanese_castle')
$dirs=@('N','NE','E','SE','S','SW','W','NW')
Cut-Atlas '05_ships/square/atlas.png' @(0,350,735,1195,1536) @(0,512,1024) $dirs
# In the generated lateen atlas the two broadside views point opposite the requested order.
Cut-Atlas '05_ships/lateen/atlas.png' @(0,384,780,1200,1536) @(0,512,1024) @('N','NE','W','SE','S','SW','E','NW')
$junk=[System.Drawing.Bitmap]::new((Join-Path $root '05_ships/junk/atlas.png'));$jw=$junk.Width;$jh=$junk.Height;$junk.Dispose()
Cut-Atlas '05_ships/junk/atlas.png' @(0,440,850,1350,$jw) @(0,440,$jh) $dirs
$script:items | ConvertTo-Json -Depth 6 | Set-Content -Encoding utf8 (Join-Path $root 'atlas_regions.json')
function New-Canvas($w,$h){[System.Drawing.Bitmap]::new($w,$h,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)}
foreach($state in @('normal','selected')){
 $b=New-Canvas 512 32;$g=[System.Drawing.Graphics]::FromImage($b);$g.SmoothingMode='AntiAlias'
 if($state -eq 'selected'){foreach($width in @(12,8,5)){ $p=[System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(25,215,255,255),$width);$g.DrawLine($p,8,16,504,16);$p.Dispose() }}
 $a=if($state -eq 'selected'){255}else{140};$w=if($state -eq 'selected'){2.5}else{1.2}
 $p=[System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb($a,240,255,252),$w);$g.DrawLine($p,8,16,504,16);$p.Dispose();$g.Dispose();$b.Save((Join-Path $root "06_routes/route_$state.png"));$b.Dispose()
}
$b=New-Canvas 128 96;$g=[System.Drawing.Graphics]::FromImage($b);$g.SmoothingMode='AntiAlias'
foreach($w in @(10,6,2)){ $a=if($w -eq 2){255}else{35};$p=[System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb($a,238,255,231),$w);$g.DrawEllipse($p,20,24,88,48);$p.Dispose() };$g.Dispose();$b.Save((Join-Path $root '06_routes/port_selected.png'));$b.Dispose()
# Analytic cubic Bezier, flowing segments move continuously; 32 frames at 16 fps.
function CurvePoint([double]$t,[double]$offset){$u=1-$t;[System.Drawing.PointF]::new([single]($u*$u*$u*24+3*$u*$u*$t*130+3*$u*$t*$t*310+$t*$t*$t*480),[single]($u*$u*$u*224+3*$u*$u*$t*55+3*$u*$t*$t*245+$t*$t*$t*32+$offset))}
$sheet=New-Canvas 4096 1024;$sg=[System.Drawing.Graphics]::FromImage($sheet)
for($f=0;$f -lt 32;$f++){
 $b=New-Canvas 512 256;$g=[System.Drawing.Graphics]::FromImage($b);$g.SmoothingMode='AntiAlias'
 foreach($lane in @(-18,0,18)){for($k=0;$k -lt 3;$k++){for($s=0;$s -lt 40;$s++){
  $t=(($f/32.0)+$k/3.0+$s*0.0035)%1.0;$t2=$t+0.0035
  if($t2 -ge 1){continue};$fade=[Math]::Sin([Math]::PI*$s/40.0);$edge=[Math]::Min(1,[Math]::Min($t,1-$t)*12);$a=[int](175*$fade*$edge)
  $p=[System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb($a,225,248,255),1.3)
  $g.DrawLine($p,(CurvePoint $t $lane),(CurvePoint $t2 $lane));$p.Dispose()
 }}};$g.Dispose();$b.Save((Join-Path $root ('07_currents/frames/current_{0:D2}.png' -f $f)));$sg.DrawImageUnscaled($b,($f%8)*512,[int][Math]::Floor($f/8)*256);$b.Dispose()
};$sg.Dispose();$sheet.Save((Join-Path $root '07_currents/current_sheet.png'));$sheet.Dispose()
Write-Output 'Saved generated artwork, 42 independent sprites, routes, and 32 ocean-current frames.'
