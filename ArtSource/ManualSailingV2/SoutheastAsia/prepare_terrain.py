"""Stylized SEA heightfield from existing regional illustration; not surveyed elevation.
Keeps source image unchanged. World +Z points north (image top).
"""
from pathlib import Path
import numpy as np
from PIL import Image
from scipy import ndimage as nd
import struct,json
ROOT=Path(__file__).resolve().parents[3]
OUT=ROOT/'MaritimeLegend/Assets/Resources/Models/Environment/SoutheastAsiaV2'
OUT.mkdir(parents=True,exist_ok=True)
W,H=1254,706
rgb=np.asarray(Image.open(ROOT/'Assets/AutoSailing_Map_Pack/01_map/southeast_asia.png').convert('RGB').resize((W,H)),dtype=np.float32)/255
r,g,b=rgb.transpose(2,0,1)
water=(b>r*1.12)&(b>g*.90)
water=nd.binary_closing(water,iterations=1,border_value=1)
land=~water
labels,n=nd.label(land);counts=np.bincount(labels.ravel());land&=counts[labels]>=9
# Remove tiny painted shadow holes while retaining rivers/straits with real width.
holes=nd.binary_fill_holes(land)&~land
labels,n=nd.label(holes);counts=np.bincount(labels.ravel());land|=holes&(counts[labels]<15)
scale=960/W
sdf=(nd.distance_transform_edt(land)-nd.distance_transform_edt(~land))*scale
inside=np.maximum(sdf,0)
y,x=np.mgrid[:H,:W]
coast=nd.gaussian_filter(sdf,.65)
ridge=nd.gaussian_filter(((r>g*1.08)&(g>b*1.12)&(r>.32)).astype(float),2.0)
light=nd.gaussian_filter(((r>.69)&(g>.64)&(b>.48)).astype(float),1.8)
noise=(np.sin(x*.031+y*.017)+np.cos(y*.046-x*.022))*.22+.5
ramp=np.clip(inside/7,0,1);ramp=ramp*ramp*(3-2*ramp)
height=np.where(coast>0,np.minimum(inside*.22,1.5)+ramp*(1.1+ridge*10+light*6+np.maximum(noise,0)*3),np.maximum(coast*.75,-32))
# Higher, deeply inland ridges only; ordinary coastal peaks remain warm yellow.
height+=np.where(land,np.clip((inside-18)/24,0,1)*ridge*9,0)
height=nd.gaussian_filter(height,.7).astype('<f4')
forest=nd.gaussian_filter(((g>r*.94)&(g>b*1.5)&(r<.55)).astype(float),2)
forest*=land
with (OUT/'terrain.bytes').open('wb') as f:
 f.write(struct.pack('<IIff',W,H,960,540))
 for a in (height,coast.astype('<f4'),forest.astype('<f4')):f.write(a.astype('<f4').tobytes())
report={'source':'Assets/AutoSailing_Map_Pack/01_map/southeast_asia.png','style_reference':'Assets/AutoSailing_Map_Pack/01_map/east_asia.png','grid':[W,H],'world':[960,540],'land_fraction':float(land.mean()),'height_max':float(height.max()),'deep_summit_rule':'height > 15 AND inland distance > 18; smooth transitions','elevation':'authored illustration-derived relief, not surveyed geography'}
(Path(__file__).parent/'generation.json').write_text(json.dumps(report,indent=2))
print(report)
