$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$root=Split-Path $PSScriptRoot -Parent
$dest=Join-Path $root '04_cities_small_v2'
New-Item -ItemType Directory -Force (Join-Path $dest 'source') | Out-Null
Copy-Item -LiteralPath 'C:/Users/charl/.codex/generated_images/01a128e3-154e-7493-a389-499bcb5f49f3/exec-73e20af5-346a-40d0-b1cc-636b47a446a4.png' -Destination (Join-Path $dest 'source/generated_atlas.png')
$im=[System.Drawing.Bitmap]::new((Join-Path $dest 'source/generated_atlas.png'))
$names=@('chinese_palace','chinese_walled_port','chinese_pagoda_port','korean_port','japanese_pagoda_port','japanese_castle')
$labels=@('中式宫殿','中式城门','中式宝塔','朝鲜门楼','日本塔寺','日本城郭')
$ys=@(0,488,1024);$rows=[System.Collections.Generic.List[object]]::new()
$atlas=[System.Drawing.Bitmap]::new(512,342,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$ag=[System.Drawing.Graphics]::FromImage($atlas)
$preview=[System.Drawing.Bitmap]::new(640,430)
$pg=[System.Drawing.Graphics]::FromImage($preview);$pg.Clear([System.Drawing.Color]::FromArgb(31,49,60))
$font=[System.Drawing.Font]::new('Microsoft YaHei',10)
for($i=0;$i -lt 6;$i++){
 $row=[int][Math]::Floor($i/3);$col=$i%3
 $original=[System.Drawing.Bitmap]::new((Join-Path $root "04_cities/$($names[$i]).png"))
 $ow=$original.Width;$oh=$original.Height;$original.Dispose()
 $w=[int][Math]::Round($ow/3);$h=[int][Math]::Round($oh/3)
 $rect=[System.Drawing.Rectangle]::new($col*512,$ys[$row],512,($ys[$row+1]-$ys[$row]))
 $part=$im.Clone($rect,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $b=[System.Drawing.Bitmap]::new($w,$h,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $g=[System.Drawing.Graphics]::FromImage($b);$g.InterpolationMode='HighQualityBicubic';$g.PixelOffsetMode='HighQuality';$g.CompositingQuality='HighQuality'
 # Uniform scaling preserves landmark proportions inside the requested one-third canvas.
 $scale=[Math]::Min($w/$part.Width,$h/$part.Height)
 $dw=[int][Math]::Round($part.Width*$scale);$dh=[int][Math]::Round($part.Height*$scale)
 $g.DrawImage($part,[System.Drawing.Rectangle]::new([int](($w-$dw)/2),[int](($h-$dh)/2),$dw,$dh));$g.Dispose();$part.Dispose()
 $b.Save((Join-Path $dest "$($names[$i]).png"),[System.Drawing.Imaging.ImageFormat]::Png)
 $ax=@(0,183,360)[$col];$ay=@(0,167)[$row];$ag.DrawImageUnscaled($b,$ax,$ay)
 $pg.DrawImageUnscaled($b,($col*212+[int]((212-$w)/2)),($row*210+5))
 $pg.DrawString("$($labels[$i])  $w x $h",$font,[System.Drawing.Brushes]::White,($col*212+15),($row*210+184))
 $rows.Add(@{file="$($names[$i]).png";label=$labels[$i];originalWidth=$ow;originalHeight=$oh;width=$w;height=$h;atlasRect=@($ax,$ay,$w,$h);alpha=$b.GetPixel(0,0).A})
 $b.Dispose()
}
$ag.Dispose();$atlas.Save((Join-Path $dest 'atlas.png'));$atlas.Dispose()
$pg.Dispose();$font.Dispose();$preview.Save((Join-Path $dest 'preview.png'));$preview.Dispose();$im.Dispose()
@{method='Built-in image_gen simplified repaint, then atlas slicing and one-third export';scale=0.3333333333;rounding='nearest integer pixel';items=$rows} | ConvertTo-Json -Depth 8 | Set-Content -Encoding utf8 (Join-Path $dest 'manifest.json')
$rows | Select-Object label,originalWidth,originalHeight,width,height,alpha | Format-Table
