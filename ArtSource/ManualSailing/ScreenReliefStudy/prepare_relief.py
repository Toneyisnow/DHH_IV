"""Single-screen art study, semantic relief in original registered map coordinates."""
from pathlib import Path
import json
import numpy as np
from PIL import Image,ImageDraw
from scipy.ndimage import gaussian_filter,distance_transform_edt,maximum_filter,label
P=Path(__file__).parent
im=Image.open(P/'source-region.png').convert('RGB')
a=np.asarray(im).astype(float)/255
h,w=a.shape[:2];yy,xx=np.mgrid[:h,:w];r,g,b=a.transpose(2,0,1)
water=(b>r*1.2)&(b>g*.9)
ocean=water&(g>b*.65)&(r<.16)
river=water&~ocean
land=~water
inland=distance_transform_edt(~ocean)
base=np.clip(inland/20,0,1)*.8
base[water]=0
# Authored ridge controls follow the painted mountain crests, not pixel brightness.
ridge_points=[(301,103,5),(368,44,10),(398,7,13),(453,82,8),(508,27,16),(548,144,10),(626,116,15),(592,226,10),(638,289,13),(661,352,4)]
mountain=np.zeros((h,w))
for p,q in zip(ridge_points[:-1],ridge_points[1:]):
    ax,ay,az=p;bx,by,bz=q;dx,dy=bx-ax,by-ay
    t=np.clip(((xx-ax)*dx+(yy-ay)*dy)/(dx*dx+dy*dy),0,1)
    dist=np.hypot(xx-ax-t*dx,yy-ay-t*dy)
    z=az+(bz-az)*t
    mountain=np.maximum(mountain,z*np.maximum(0,1-dist/65)**1.6)
mountain*=np.clip(distance_transform_edt(~water)/12,0,1)
# Limit crown detection to painted forest stands, excluding mountains and grass.
maskim=Image.new('L',(w,h));draw=ImageDraw.Draw(maskim)
for polygon in [[(0,0),(300,0),(250,100),(70,90),(0,130)],[(160,135),(290,100),(400,86),(530,140),(512,280),(392,334),(210,335),(150,250)],[(510,328),(640,310),(704,344),(704,420),(572,443),(500,399)],[(289,416),(355,393),(414,447),(419,485),(315,480)]]:
    draw.polygon(polygon,fill=255)
forest=(np.asarray(maskim)>0)&(g>r*.86)&(g>b*1.6)&(b<.27)&~water
score=gaussian_filter((g-b)*forest,1.3)
peaks=(score==maximum_filter(score,size=13))&(score>.11)&forest
ys,xs=np.where(peaks)
crown=np.zeros((h,w));trees=[]
for x,y in zip(xs,ys):
    radius=5.5+3*np.clip(score[y,x],0,.5)
    height=1.3+3*min(score[y,x],.5)
    d=np.sqrt(((xx-x)/radius)**2+((yy-y)/(radius*1.2))**2)
    crown=np.maximum(crown,height*np.maximum(0,1-d)**.65)
    trees.append([int(x),int(y),float(height)])
crown*=gaussian_filter(forest.astype(float),.7)
heights=base+mountain+crown
# River beds are cut through the relief. River and ocean surfaces remain at datum.
channel=gaussian_filter(river.astype(float),.7)
heights*=1-np.clip(channel*1.2,0,1)
heights[ocean]=0
heights=gaussian_filter(heights,.55).astype('float32')
np.save(P/'height.npy',heights)
# Simple portable binary input for Blender's bundled Python.
import struct
(P/'height.bin').write_bytes(struct.pack('<II',w,h)+heights.tobytes())
Image.fromarray(np.uint8(np.clip(heights/17,0,1)*255)).save(P/'height-review.png')
(P/'study.json').write_text(json.dumps(dict(source='Assets/AutoSailing_Map_Pack/01_map/southeast_asia.png',crop_original_pixels=[300,185,476,325],texture_source='Existing registered 4x Real-ESRGAN map',texture_size=[w,h],size_world=[176,140],ridge_controls=ridge_points,crown_count=len(trees),peak_height=float(heights.max()),note='Authored stylized relief; not surveyed elevation. Forest crowns are relief geometry, not separate tree assets.'),indent=2))
print('PREPARED',w,h,'crowns',len(trees),'peak',heights.max())
