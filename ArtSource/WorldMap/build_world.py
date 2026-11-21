"""Reproducible coarse world coastline, not surveyed elevation or a nautical chart.
Natural Earth 1:50m land (public domain); source zip remains in Source/.
No model/runtime downloads or extra Python environment required.
"""
from pathlib import Path
import base64, hashlib, json, struct, zipfile
import numpy as np
from PIL import Image, ImageDraw
from scipy.ndimage import distance_transform_edt

ROOT = Path(__file__).resolve().parents[2]
HERE = Path(__file__).resolve().parent
OUT = ROOT / 'MaritimeLegend/Assets/Resources/Data/WorldMap'
OUT.mkdir(parents=True, exist_ok=True)
(OUT/'Chunks').mkdir(exist_ok=True)
W, H, N, SCALE = 4608, 2048, 64, 32
STEP = 360*SCALE/W
source = HERE/'Source/ne_50m_land.zip'
with zipfile.ZipFile(source) as z:
    data = z.read(next(n for n in z.namelist() if n.endswith('.shp')))
mask = Image.new('1', (W, H+1))
draw = ImageDraw.Draw(mask)
offset = 100
while offset < len(data):
    _, size = struct.unpack_from('>ii', data, offset)
    record = data[offset+8:offset+8+size*2]; offset += 8+size*2
    kind, = struct.unpack_from('<i', record)
    if kind == 0: continue
    assert kind == 5, kind
    parts, count = struct.unpack_from('<ii', record, 36)
    starts = list(struct.unpack_from('<'+'i'*parts, record, 44))+[count]
    points = np.frombuffer(record, dtype='<f8', count=count*2, offset=44+parts*4).reshape(-1,2)
    for a,b in zip(starts,starts[1:]):
        ring=points[a:b]
        # Shapefile outer rings are clockwise; interior rings are counterclockwise.
        area=np.sum(ring[:-1,0]*ring[1:,1]-ring[1:,0]*ring[:-1,1])
        draw.polygon([((lon+180)*W/360,(lat+80)*H/160) for lon,lat in ring], fill=1 if area<0 else 0)
land=np.asarray(mask,dtype=bool)
# Periodic halo wider than the retained distance range. Latitude edge extends,
# so an air wall is never interpreted as coastline.
pad=64
padded=np.pad(np.pad(land,((0,0),(pad,pad)),mode='wrap'),((pad,pad),(0,0)),mode='edge')
sdf=(distance_transform_edt(~padded)-distance_transform_edt(padded))[pad:-pad,pad:-pad]*STEP
sdf=np.clip(sdf,-80,80).astype('<f4')
del padded
chunks=[]
for cy in range(32):
    for cx in range(72):
        xs=(np.arange(cx*N-1,cx*N+N+2)%W)
        ys=np.clip(np.arange(cy*N-1,cy*N+N+2),0,H)
        tile=sdf[np.ix_(ys,xs)].copy()
        ocean=bool(np.all(tile>=79.99))
        key=f'Chunks/{cx:02d}_{cy:02d}'
        if not ocean:
            payload=struct.pack('<iiii',0x574D5031,cx,cy,N+3)+tile.tobytes()
            (OUT/(key+'.bytes')).write_bytes(payload)
        chunks.append({'x':cx,'y':cy,'ocean':ocean,'key':'' if ocean else 'Data/WorldMap/'+key})

coords=[(-9.14,38.72),(-6.3,36.6),(12.33,45.44),(29.92,31.2),(28.98,41.01),(73.82,15.49),
        (102.25,2.19),(121.0,30.2),(120.98,14.6),(106.85,-6.12),(104.76,-2.99),(112.75,-7.25),
        (115.0,4.9),(119.41,-5.14),(124.84,1.49),(127.38,.79),(128.18,-3.7),(106.75,10.7),(100.58,14.35)]
names=json.loads((ROOT/'MaritimeLegend/Assets/Resources/Data/Cities/ports.json').read_text(encoding='utf-8'))['ports']
ports=[]
for p,(lon,lat) in zip(names,coords):
    gx=(lon+180)*W/360; gy=(lat+80)*H/160
    candidates=[]
    for dy in range(-90,91):
        for dx in range(-90,91):
            ix=int(round(gx))+dx; iy=int(round(gy))+dy
            if 0<=iy<=H and sdf[iy,ix%W]>=8:
                candidates.append(((ix-gx)**2+(iy-gy)**2,ix,iy))
    _,ix,iy=min(candidates)
    ports.append({'id':p['id'],'name':p['name'],'x':float(ix*STEP%(360*SCALE)),
                  'y':float(iy*STEP),'longitude':lon,'latitude':lat})
ocean_bits=bytearray(288)
for i,c in enumerate(chunks):
    if c['ocean']:ocean_bits[i//8]|=1<<(i%8)
manifest={'version':1,'worldId':'world_v1','unitsPerDegree':SCALE,'chunkCells':N,'columns':72,'rows':32,
          'source':'Natural Earth 1:50m land, public domain; stylized relief; coarse coastal berths',
          'oceanBits':base64.b64encode(ocean_bits).decode(),'ports':ports}
(OUT/'world.json').write_text(json.dumps(manifest,ensure_ascii=False,separators=(',',':')),encoding='utf-8')
# Review image is an authoring artifact, not a runtime global high-resolution texture.
rgb=np.zeros((H+1,W,3),np.uint8);rgb[:]=[19,95,139];rgb[land]=[137,161,102]
review=Image.fromarray(rgb).transpose(Image.Transpose.FLIP_TOP_BOTTOM).resize((1440,640))
review.save(HERE/'Review/world-overview.png')
report={'sourceSha256':hashlib.sha256(source.read_bytes()).hexdigest(),'chunks':len(chunks),
        'oceanChunks':sum(c['ocean'] for c in chunks),'sampleStep':STEP,'ports':len(ports),
        'bytes':sum(p.stat().st_size for p in OUT.rglob('*') if p.is_file()),
        'license':'https://www.naturalearthdata.com/about/terms-of-use/',
        'download':'https://naturalearth.s3.amazonaws.com/50m_physical/ne_50m_land.zip'}
(HERE/'Review/generation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
