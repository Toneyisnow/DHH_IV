"""Author coastal placements against the shipped navigation mask, not guessed water.
All anchors must connect to the same open ocean with >= 10 navigation pixel clearance.
Original generated RGBA files are copied without altering image pixels.
"""
from pathlib import Path
import json, struct, shutil, re, uuid
import numpy as np
from scipy import ndimage as nd
from scipy.spatial import cKDTree
from PIL import Image
ROOT=Path(__file__).resolve().parents[3]
HERE=Path(__file__).resolve().parent
RES=ROOT/'MaritimeLegend/Assets/Resources'
# Approximate illustration positions, not geographic projection coordinates.
PORTS=[('malacca','马六甲',6,.184,.47),('manila','马尼拉',-1,.681,.16),
 ('batavia','巴达维亚',-1,.29,.775),('palembang','巨港',-1,.235,.65),
 ('surabaya','泗水',-1,.425,.81),('brunei','文莱',-1,.44,.46),
 ('makassar','望加锡',-1,.572,.686),('manado','万鸦老',-1,.745,.562),
 ('ternate','德尔纳特',-1,.776,.6),('ambon','安汶',-1,.806,.695),
 ('giadinh','嘉定',-1,.275,.365),('ayutthaya','大城',-1,.168,.273)]
with (RES/'Environment/SoutheastAsiaV2/coast-distance.bytes').open('rb') as f:
 nw,nh=struct.unpack('<II',f.read(8));nav=np.frombuffer(f.read(),dtype='<f4').reshape(nh,nw)
with (RES/'Models/Environment/SoutheastAsiaV2/terrain.bytes').open('rb') as f:
 tw,th,ww,hh=struct.unpack('<IIff',f.read(16));height=np.frombuffer(f.read(tw*th*4),dtype='<f4').reshape(th,tw);coast=np.frombuffer(f.read(tw*th*4),dtype='<f4').reshape(th,tw)
gy,gx=np.mgrid[:nh,:nw]
edge=np.minimum(np.minimum(gx,nw-1-gx),np.minimum(gy,nh-1-gy))
# CoastNavigation.Distance also limits travel at the map edges.
nav=np.minimum(nav,edge)
safe=nav>=10
safe[:2]=False;safe[-2:]=False;safe[:,:2]=False;safe[:,-2:]=False
labels,count=nd.label(safe);sizes=np.bincount(labels.ravel());sizes[0]=0;ocean=labels==sizes.argmax()
sy,sx=np.where(ocean&(nav>=18)&(nav<=23));water=np.column_stack((sx/nw*ww,sy/nh*hh));tree=cKDTree(water)
ly,lx=np.where((coast>=.6)&(coast<=2.2)&(height>=.2)&(height<=2.8))
land=np.column_stack((lx/(tw-1)*ww,ly/(th-1)*hh));gap,nearest=tree.query(land)
eligible=(gap<19)&(gap>7)
land,lx,ly,nearest,gap=land[eligible],lx[eligible],ly[eligible],nearest[eligible],gap[eligible]
out=[];checks=[]
for ident,name,game_id,u,v in PORTS:
 score=np.sum((land-np.array([u*ww,v*hh]))**2,axis=1)+gap**2*.12
 # Prefer an unobstructed gently descending approach, no inland lake or cliff behind quay.
 chosen=None
 for j in np.argsort(score)[:2000]:
  a=land[j];b=water[nearest[j]];t=np.linspace(0,1,40);line=a[:,None]*(1-t)+b[:,None]*t
  terrain=nd.map_coordinates(height,[line[1]/hh*(th-1),line[0]/ww*(tw-1)],order=1)
  if np.max(terrain-height[ly[j],lx[j]])>.7:continue
  chosen=j;break
 if chosen is None:raise RuntimeError('No coastal site '+name)
 j=chosen;wi=nearest[j];p=land[j];spawn=water[wi];y=float(height[ly[j],lx[j]])+.06
 row=dict(id=ident,name=name,gamePortId=next(p["id"] for p in json.loads((RES/"Data/Cities/ports.json").read_text(encoding="utf-8-sig"))["ports"] if p["name"]==name),x=float(p[0]-ww/2),y=y,z=float(hh/2-p[1]),spawnX=float(sx[wi]/nw),spawnY=float(sy[wi]/nh),art='Sprites/ManualSailing/PortsV2/'+ident,height=13.,cropX=0.,cropY=0.,cropW=1.,cropH=1.,aspect=1.)
 source=HERE/'Generated'/f'{ident}.png'
 if source.exists():
  im=Image.open(source);assert im.mode=='RGBA',ident+' needs alpha';alpha=im.getchannel('A');bounds=alpha.point(lambda a:255 if a>24 else 0).getbbox();assert bounds and alpha.getextrema()[0]==0
  x0,y0,x1,y1=bounds;w,h=im.size;row.update(cropX=x0/w,cropY=1-y1/h,cropW=(x1-x0)/w,cropH=(y1-y0)/h,aspect=(x1-x0)/(y1-y0))
  target=RES/'Sprites/ManualSailing/PortsV2'/source.name;target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(source,target)
  meta=Path(str(target)+'.meta')
  if not meta.exists():
   template=(RES/'Sprites/ManualSailing/Ports/malacca.png.meta').read_text()
   template=re.sub(r'guid: [a-f0-9]+','guid: '+uuid.uuid4().hex,template,count=1).replace('maxTextureSize: 2048','maxTextureSize: 1024')
   meta.write_text(template)
 out.append(row);checks.append(dict(id=ident,name=name,clearance=float(nav[sy[wi],sx[wi]]),ocean_component=int(sizes.argmax()),shore_distance=float(gap[j]),placement_error=float(np.linalg.norm(p-[u*ww,v*hh])),image_ready=source.exists()))
target=RES/'Environment/SoutheastAsiaV2/ports.json';target.write_text(json.dumps(dict(ports=out),ensure_ascii=False,indent=2),encoding='utf-8')
report=dict(navigation_grid=[nw,nh],minimum_route_clearance=10,connected_ocean_pixels=int(ocean.sum()),ports=checks,all_ports_share_open_ocean=True)
(HERE/'reachability.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
cards=''.join(f'<article><h2>{p["name"]}</h2><a href="Generated/{p["id"]}.png"><img src="Generated/{p["id"]}.png"></a><p>海上接近点：{p["spawnX"]:.4f}, {p["spawnY"]:.4f}</p><a href="Review/port-{i}.png">游戏画面</a></article>' for i,p in enumerate(out))
(HERE/'preview.html').write_text('<!doctype html><meta charset="utf-8"><title>东南亚十二港口</title><style>body{background:#142b31;color:#eadfbd;font:16px system-ui;padding:30px}main{display:grid;grid-template-columns:repeat(3,1fr);gap:22px}article{background:#29444a;border-radius:12px;padding:20px}img{width:100%;aspect-ratio:1;object-fit:contain}a{color:#e6c775}h2{margin:0}</style><h1>东南亚十二港口 · 2.5D 城市图案</h1><p>独立透明建筑群 / 全部港口连接同一可航行海域 / 新出海初始镜头 10</p><a href="Review/departure.png">初始出海画面</a><main>'+cards+'</main>',encoding='utf-8')
print(json.dumps(report,ensure_ascii=False,indent=2))
