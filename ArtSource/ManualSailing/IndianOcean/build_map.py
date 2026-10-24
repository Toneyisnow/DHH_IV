"""Indian Ocean semantic map. Original pixels classify geography; they are never stretched onto relief.
Heights, depths and city locations are authored/inferred game art, not surveyed geographic data.
"""
from pathlib import Path
import json,struct,math,random
import numpy as np
from scipy import ndimage as nd
from PIL import Image
P=Path(__file__).parent;ROOT=P.parents[2];OUT=ROOT/'MaritimeLegend/Assets/Resources/Environment/IndianOcean';OUT.mkdir(parents=True,exist_ok=True)
W,H=1672,941
im=Image.open(ROOT/'Assets/AutoSailing_Map_Pack/01_map/indian_ocean.png').convert('RGB').resize((W,H))
rgb=np.asarray(im,dtype=float)/255;r,g,b=rgb.transpose(2,0,1);yy,xx=np.mgrid[:H,:W]
water=(b>r*1.10)&(b>g*.86)&(b>.12)
water=nd.binary_closing(water,iterations=1,border_value=1)
waterLabels,_=nd.label(water);waterCounts=np.bincount(waterLabels.ravel());water&=waterCounts[waterLabels]>=180
land=~water;labels,n=nd.label(land);counts=np.bincount(labels.ravel());land&=counts[labels]>=10
sdf=nd.gaussian_filter(nd.distance_transform_edt(~land)-nd.distance_transform_edt(land),1.25);land=sdf<0;inside=np.maximum(-sdf,0)
smooth=nd.gaussian_filter(rgb,(8,8,0));rr,gg,bb=smooth.transpose(2,0,1)
aridWest=1/(1+np.exp(np.clip((xx-(690+90*np.sin(yy*.011)))/38,-40,40)))
aridSouth=1/(1+np.exp(np.clip((yy-(440+34*np.sin(xx*.017)))/35,-40,40)))
aridAfrica=(xx<260+45*np.sin(yy*.013))&(yy>450)
desert=land&(((aridWest*aridSouth>.25)&(rr/np.maximum(gg,.01)>1.025+.30*(1-aridWest*aridSouth)))| (aridAfrica&(rr>gg*1.12)))
snow=land&(yy<170+25*np.sin(xx*.02))&(xx>630+22*np.sin(yy*.025))&(nd.gaussian_filter(((r>.68)&(g>.65)&(b>.52)).astype(float),9)>.16)
forest=land&~desert&~snow&((gg>rr*.92)&(gg>bb*1.45))
teal=land&~desert&~snow&~forest
biome=np.where(desert,2,np.where(snow,5,np.where(forest,4,3))).astype(np.uint8)
def broad_noise(seed,scale):
 rng=np.random.default_rng(seed);a=rng.random((math.ceil(H/scale)+2,math.ceil(W/scale)+2))
 return nd.map_coordinates(a,[yy/scale,xx/scale],order=3,mode='reflect')
patch=broad_noise(712,43);fine=broad_noise(317,13)
beachWidth=5+7*np.clip((patch-.20)/.65,0,1)
biome[inside<beachWidth]=1;biome[~land]=0
palette=np.array([[0,0,0],[.84,.71,.36],[.86,.65,.29],[.20,.38,.24],[.19,.31,.075],[.83,.89,.91]])
weights=np.stack([nd.gaussian_filter((biome==i).astype(float),2.2) for i in range(1,6)],axis=2)
color=weights@palette[1:]/np.maximum(weights.sum(axis=2,keepdims=True),1e-6)
color[~land]=palette[1] # beach edge interpolation retains its warm color
noise=.97+.03*np.sin(xx*.17)*np.cos(yy*.21)
color*=noise[:,:,None]
t=np.clip((inside-beachWidth)/9,0,1);t=t*t*(3-2*t)
height=np.where(land,.025*np.minimum(inside,beachWidth)+t*(1.15+.48*(fine-.5)),-.5)
# Patch density controls both dune amplitude and placement, leaving broad quiet stretches.
duneDensity=np.clip((patch-.35)/.36,0,1)
coastRamp=np.clip((inside-2)/5,0,1);coastRamp=coastRamp*coastRamp*(3-2*coastRamp)
angle=.3+.8*broad_noise(214,95);along=xx*.91+yy*.34
windRidges=np.maximum(0,np.sin(along*.22+patch*5+fine*2))**2
sand=(biome==1)|(biome==2)
height+=np.where(land&sand,coastRamp*duneDensity*(.20+1.25*windRidges)*np.where(desert,1.5,.6),0)
color*=1+np.where(sand,(fine-.5)*.16+(windRidges-.5)*duneDensity*.065,0)[:,:,None]

dark=np.clip((.42-nd.gaussian_filter(g,4))/.34,0,1)
depth=np.where(land,0,np.minimum(4500,.22*np.maximum(sdf,0)**1.65*(.65+.7*dark)))
rgba=np.uint8(np.clip(np.concatenate([color,np.ones((H,W,1))],axis=2),0,1)*255)
with (OUT/'terrain.bytes').open('wb') as f:
 f.write(struct.pack('<II',W,H))
 for data in (height,sdf,depth):f.write(data.astype('<f4').tobytes())
 f.write(rgba.tobytes())
(OUT/'coast-distance.bytes').write_bytes(struct.pack('<II',W,H)+sdf.astype('<f4').tobytes())
def sample(array,x,y):return float(nd.map_coordinates(array,[[y],[x]],order=1)[0])
placements=[];mountains=[];rng=random.Random(221022)
catalog=json.loads((P/"landform-catalog.json").read_text(encoding="utf-8"))
def put(asset,x,y,scale=1,rotation=0,snowy=False):
 placements.append(dict(asset=asset,x=x,y=sample(height,x,y),z=-y,scale=scale,rotation=rotation,snowy=snowy))
# Ranges follow the broad painted ridges. Mountains are individual reusable models.
ranges=[('high',[(53,40),(100,140),(171,292),(235,391)],1.7),('high',[(390,90),(500,153),(644,182)],1.7),('snow',[(706,8),(865,37),(1047,79),(1214,76),(1380,157)],2.6),('high',[(1354,183),(1407,279),(1391,401)],1.8),('high',[(804,300),(844,379),(898,490),(932,548)],1.25),('small',[(862,308),(1009,361),(1038,424)],1.3),('high',[(30,496),(132,585),(156,707),(114,857)],1.6),('high',[(1468,568),(1514,676),(1618,813)],1.5),('small',[(1541,296),(1592,369)],1.1)]
for kind,path,scale in ranges:
 for a,c in zip(path,path[1:]):
  length=math.dist(a,c)
  for step in np.arange(0,length,23 if kind=='snow' else 26):
   t=step/length;x=a[0]*(1-t)+c[0]*t+rng.uniform(-7,7);y=a[1]*(1-t)+c[1]*t+rng.uniform(-7,7)
   if not(3<x<W-3 and 3<y<H-3) or sample(sdf,x,y)>-7:continue
   family='Rolling' if kind=='small' else 'Snow' if kind=='snow' else 'Angular'
   size=rng.choices(['Small','Medium','Large'],[.25,.45,.30])[0]
   asset=family+'_'+size;sc=rng.uniform(.83,1.12)
   rot=math.degrees(math.atan2(c[1]-a[1],c[0]-a[0]));meta=catalog[asset]
   if sample(sdf,x,y)>-max(meta['footY']*sc+2,6):continue
   put(asset,x,y,sc,rot);mountains.append((x,y,sc,asset,rot))
# Extra high escarpments inland from coastal headlands; no raised shoreline shelf.
for x,y,rot in [(210,383,30),(434,231,-30),(1410,316,85),(1445,661,55),(102,626,25),(875,467,70)]:
 if sample(sdf,x,y)<-11:
  put('Cliff_Large',x,y,1,rot);mountains.append((x,y,1,'Cliff_Large',rot))
# Wind-sculpted dunes: denser patches alternate with unoccupied sand flats.
duneCount=0
for y in range(8,H-8,9):
 for x in range(8,W-8,9):
  if biome[y,x] not in (1,2) or inside[y,x]<4.5:continue
  density=duneDensity[y,x]
  if rng.random()>.40*density:continue
  px=x+rng.uniform(-2,2);py=y+rng.uniform(-2,2)
  if sample(sdf,px,py)>-4.5:continue
  put('Dune_Small',px,py,rng.uniform(.55,1.15),math.degrees(angle[y,x])+rng.uniform(-15,15));duneCount+=1
# Minor inland hills, plus a mixed pine canopy sampled from the green regions.
trees=[]
for y in range(6,H-6,4):
 for x in range(6,W-6,4):
  px=x+rng.uniform(-1.6,1.6);py=y+rng.uniform(-1.6,1.6);ix,iy=int(px),int(py)
  if sdf[iy,ix]>-5 or biome[iy,ix] not in (3,4):continue
  chance=.68 if biome[iy,ix]==4 else .22
  if rng.random()>chance or any(((px-mx)/(catalog[asset]["footX"]*sc))**2+((py-my)/(catalog[asset]["footY"]*sc))**2<1 for mx,my,sc,asset,_ in mountains):continue
  trees.append((px,py))
  put('Pine_'+rng.choice(['Green','Green','Teal','Yellow'])+'_'+rng.choice(['Small','Medium','Large']),px,py,rng.uniform(.9,1.25),rng.uniform(0,360))
# Trees encircle a randomized 65-85% of every non-desert mountain's foot sectors.
coverage=[]
for mx,my,sc,asset,rot in mountains:
 if biome[int(my),int(mx)] in (1,2):continue
 sectors=list(range(20));rng.shuffle(sectors);planted=0
 for sector in sectors:
  if planted>=14:break
  a=sector*math.tau/20;dx=catalog[asset]['footX']*sc*math.cos(a);dy=catalog[asset]['footY']*sc*math.sin(a)
  theta=math.radians(rot);px=mx+dx*math.cos(theta)-dy*math.sin(theta);py=my+dx*math.sin(theta)+dy*math.cos(theta)
  if not(3<px<W-3 and 3<py<H-3) or sample(sdf,px,py)>-3:continue
  if any((px-x)**2+(py-y)**2<2.2**2 for x,y in trees):planted+=1;continue
  trees.append((px,py));put('Pine_'+rng.choice(['Green','Green','Teal'])+'_'+rng.choice(['Medium','Large']),px,py,rng.uniform(1,1.3),rng.uniform(0,360));planted+=1
 coverage.append(planted/20)
# Regional Indian ports are map anchors, kept separate from the legacy trading catalog.
ports=[]
for key,name,x,y in [('goa','果阿',825,399),('calicut','卡利卡特',861,466),('cochin','科钦',889,510),('masulipatnam','马苏利帕特南',1018,420)]:
 candidates=(sdf>26)&(sdf<42)&((xx-x)**2+(yy-y)**2<110**2)
 dist=(xx-x)**2+(yy-y)**2;dist=np.where(candidates,dist,np.inf);sy,sx=np.unravel_index(np.argmin(dist),dist.shape)
 assert np.isfinite(dist[sy,sx]);coastdist=np.where(land,(xx-x)**2+(yy-y)**2,np.inf);cy,cx=np.unravel_index(np.argmin(coastdist),dist.shape)
 ports.append(dict(id=key,name=name,x=float(sx/W),y=float(sy/H),landX=float(cx),landZ=float(-cy)))
config=dict(width=W,height=H,placements=placements,ports=ports)
for dest in (P/'layout.json',OUT/'layout.json'):dest.write_text(json.dumps(config,ensure_ascii=False,indent=2),encoding='utf-8')
report=dict(sourceSize=list(im.size),biomeCounts={str(i):int(np.sum(biome==i)) for i in range(6)},mountains=len(mountains),dunes=duneCount,landformTypes={asset:sum(p["asset"]==asset for p in placements) for asset in catalog},trees=len(trees),minimumMountainTreeSectorCoverage=min(coverage),source='painted-map classification and authored ranges; not surveyed elevation/bathymetry')
(P/'generation.json').write_text(json.dumps(report,indent=2));print(report)
Image.fromarray(np.uint8(np.clip(color*np.expand_dims(land,2),0,1)*255)).save(P/'biome-review.png')
Image.fromarray(np.uint8(land*255)).save(P/'coast-mask.png')
Image.fromarray(np.uint8(duneDensity*sand*255)).save(P/'dune-density.png')
