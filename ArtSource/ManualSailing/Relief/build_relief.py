"""Derive editable stylized relief and navigation SDF from the supplied SEA art.
Not measured elevation: mountains are inferred from inland distance and painted ridge tones.
Run with Python + numpy/scipy/Pillow. All data uses top-left map coordinates.
"""
from pathlib import Path
import numpy as np
from scipy import ndimage as nd
from PIL import Image
import struct, json
root=Path(__file__).resolve().parents[3]
out=Path(__file__).parent
runtime=root/'MaritimeLegend/Assets/Resources/Models/ManualSailing'
im=Image.open(root/'Assets/AutoSailing_Map_Pack/01_map/southeast_asia.png').convert('RGB')
rgb=np.asarray(im,dtype=float)/255
r,g,b=rgb.transpose(2,0,1)
water=(b>r*1.13)&(b>g*.91)
water=nd.binary_closing(water,iterations=2,border_value=1)
land=nd.binary_fill_holes(~water)
labels,n=nd.label(land);counts=np.bincount(labels.ravel());land &= counts[labels]>=14
# Keep narrow straits: remove noise, do not broadly dilate the land mask.
sdf=nd.distance_transform_edt(~land)-nd.distance_transform_edt(land)
inside=np.maximum(-sdf,0)
# Flat sea-level littoral; relief only starts inland and never creates a raised island slab.
ridge=nd.gaussian_filter(np.clip((r-.38)*2,0,1)*np.clip((r-g+.1)*3,0,1),.85)
inland=np.maximum(inside-4,0)
ramp=np.clip(inland/18,0,1);ramp=ramp*ramp*(3-2*ramp)
height=np.where(land,ramp*(.10*np.minimum(inland,25)+ridge*7),-1.2)
# Cap slopes to protect the hand-painted texture against vertical stretching.
for _ in range(12):
 for axis in (0,1):
  for shift in (-1,1):
   neighbor=np.roll(height,shift,axis=axis)
   height=np.where(land,np.minimum(height,neighbor+.48),height)
height=np.where(land,np.maximum(height,0),-1.2)
height[land & (inside<=4)]=0
# Model bathymetry from distance offshore and the original painted deep-water tones.
# Values are illustrative metres, not a surveyed DEM.
offshore=np.maximum(sdf,0)
dark=np.clip((.40-nd.gaussian_filter(g,3))/.32,0,1)
depth=np.where(land,0,np.minimum(4500,.18*offshore**1.65*(.65+.70*dark))).astype('<f4')
for path in [out/'bathymetry.bytes',runtime/'bathymetry.bytes']:
 path.write_bytes(struct.pack('<II',im.width,im.height)+depth.tobytes())
Image.fromarray(np.uint8(np.clip(np.log1p(depth)/np.log(4501),0,1)*255)).save(out/'depth-review.png')
assert np.max(height[land & (inside<=4)])==0
assert np.max(height)<10
assert np.percentile(depth[~land],90)>np.percentile(depth[~land],10)
# Sample a dense grid, preserving coast accuracy separately at original resolution.
nx,ny=837,471
xs=np.linspace(0,im.width-1,nx);ys=np.linspace(0,im.height-1,ny)
gy,gx=np.meshgrid(ys,xs,indexing='ij')
heights=nd.map_coordinates(height,[gy,gx],order=1).astype('<f4')
for path in [out/'relief.bytes',runtime/'relief.bytes']:
 path.write_bytes(struct.pack('<II',nx,ny)+heights.tobytes())
for path in [out/'coast-distance.bytes',runtime/'coast-distance.bytes']:
 path.write_bytes(struct.pack('<II',im.width,im.height)+sdf.astype('<f4').tobytes())
Image.fromarray((land*255).astype('uint8')).save(out/'land-mask.png')
Image.fromarray(np.uint8(np.clip(height/height.max(),0,1)*255)).save(out/'height-review.png')
# Review: original art plus coast and a conservative 14px fleet clearance line.
review=np.array(im);edge=np.abs(sdf)<1.5;review[edge]=[255,211,66];review[(sdf>13)&(sdf<15)]=[255,90,90]
Image.fromarray(review).save(out/'coast-review.png')
meta={'sourceSize':im.size,'grid':[nx,ny],'landFraction':float(land.mean()),'maximumHeight':float(height.max()),'shoreHeight':0,'maximumDepth':float(depth.max()),'depthSource':'inferred distance + painted water tone, not surveyed','clearancePixels':14,'elevation':'stylized inferred relief, not geographic DEM','collision':'full-resolution signed Euclidean distance field, positive in water'}
(out/'generation.json').write_text(json.dumps(meta,indent=2))
print(meta)
