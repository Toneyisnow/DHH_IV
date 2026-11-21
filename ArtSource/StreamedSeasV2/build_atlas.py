"""Bake the seven authored fields into shared global samples, with periodic halos.
Only data is baked here: production still uses the original SEA renderer/materials.
"""
from pathlib import Path
import json,struct,hashlib
import numpy as np
from scipy.ndimage import map_coordinates, distance_transform_edt
from PIL import Image
ROOT=Path(__file__).resolve().parents[2];HERE=Path(__file__).resolve().parent
RES=ROOT/'MaritimeLegend/Assets/Resources';OUT=RES/'Data/StreamedSeasV2'
OUT.mkdir(exist_ok=True);(OUT/'Chunks').mkdir(exist_ok=True)
N=128;SIDE=N+3;STEP=160/N
config=json.loads((HERE/'registration.json').read_text())
regions=[]
for r in config['regions']:
    p=RES/'Models/Environment/RegionalSeasV2'/r['id']/'terrain.bytes'
    raw=p.read_bytes();w,h,_,_=struct.unpack_from('<IIff',raw)
    regions.append((r,np.frombuffer(raw,dtype='<f4',offset=16).reshape(3,h,w)))
    r['sourceSha256']=hashlib.sha256(raw).hexdigest()
# Coarse global geography only fills areas outside the authored illustrations.
old=json.loads((RES/'Data/WorldMap/world.json').read_text(encoding='utf-8'))
base=np.full((2049,4609),80,np.float32)
for p in (RES/'Data/WorldMap/Chunks').glob('*.bytes'):
    raw=p.read_bytes();_,cx,cy,side=struct.unpack_from('<4i',raw)
    a=np.frombuffer(raw,dtype='<f4',offset=16).reshape(side,side)
    base[cy*64:cy*64+65,cx*64:cx*64+65]=a[1:66,1:66]
base[:,-1]=base[:,0]
def field(x,z):
    x=np.mod(x,11520);z=np.clip(z,0,5120)
    d=map_coordinates(base,[z/2.5,x/2.5],order=1,mode='nearest')
    inland=np.maximum(-d,0);ramp=np.clip(inland/7,0,1);ramp=ramp*ramp*(3-2*ramp)
    relief=np.minimum(inland*.22,1.5)+ramp*(2.2+(np.sin(x*.031+z*.017)+np.cos(z*.046-x*.022)+2)*2)
    height=np.where(d<0,relief,np.maximum(-d*.75,-32));forest=np.zeros_like(height)
    for r,a in regions:
        west,south,east,north=r['bounds'];u=(x/32-180-west)/(east-west);v=(north-(z/32-80))/(north-south)
        edge=np.minimum.reduce([u,1-u,v,1-v]);weight=np.clip(edge/.12,0,1);weight=weight*weight*(3-2*weight)
        if not np.any(weight):continue
        coords=[np.clip(v,0,1)*(a.shape[1]-1),np.clip(u,0,1)*(a.shape[2]-1)]
        height=height*(1-weight)+map_coordinates(a[0],coords,order=1,mode='nearest')*weight
        forest=forest*(1-weight)+map_coordinates(a[2],coords,order=1,mode='nearest')*weight
    return height,forest
def main():
    ocean=[];overview=np.zeros((1024,2304,3),np.uint8);overview[:]=[19,110,164]
    count=0
    for cy in range(32):
      for cx in range(72):
        # Signed clearance derived from the final blended coastline, with a 40-unit halo.
        yy,xx=np.mgrid[-33:N+34,-33:N+34];x=(cx*N+xx)*STEP;z=(cy*N+yy)*STEP
        height,forest=field(x,z);land=height>=0
        coast=(distance_transform_edt(land)-distance_transform_edt(~land))*STEP
        coast=np.clip(coast,-32,32)
        arrays=np.stack([height,coast,forest])[:,32:-32,32:-32].astype('<f4')
        empty=bool(np.all(arrays[0]<=-31.99));ocean.append(empty)
        if not empty:
            (OUT/'Chunks'/f'{cx:02d}_{cy:02d}.bytes').write_bytes(struct.pack('<4i',0x53563232,cx,cy,SIDE)+arrays.tobytes());count+=1
        else:
            obsolete=OUT/'Chunks'/f'{cx:02d}_{cy:02d}.bytes'
            if obsolete.exists():obsolete.unlink()
            meta=Path(str(obsolete)+'.meta')
            if meta.exists():meta.unlink()
        tile=arrays[0,1:129:4,1:129:4];colors=overview[cy*32:cy*32+32,cx*32:cx*32+32];colors[tile>=0]=[207,181,112]
      print('row',cy+1,'/ 32',flush=True)
    ports=[]
    for p in old['ports']:
        px=(p['longitude']+180)*32;pz=(p['latitude']+80)*32
        yy,xx=np.mgrid[-128:129,-128:129];x=px+xx*STEP;z=pz+yy*STEP
        h,_=field(x,z);safe=distance_transform_edt(h<0)*STEP>14
        score=np.where(safe,xx*xx+yy*yy,np.inf);iy,ix=np.unravel_index(np.argmin(score),score.shape)
        assert np.isfinite(score[iy,ix]),p
        ports.append(dict(p,x=float(x[iy,ix]%11520),y=float(np.clip(z[iy,ix],0,5120))))
    manifest={'version':2,'worldId':'regional_world_v2','ocean':ocean,'ports':ports,'regions':config['regions']}
    (OUT/'atlas.json').write_text(json.dumps(manifest,ensure_ascii=False,separators=(',',':')),encoding='utf-8')
    Image.fromarray(overview[::-1]).save(HERE/'Review/atlas.png')
    (HERE/'Review/generation.json').write_text(json.dumps({'chunks':2304,'landChunks':count,'sampleStep':STEP,'source':'seven authored regional heightfields + coarse global gap fill','registration':config},indent=2))
if __name__=='__main__':main()

