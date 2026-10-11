from pathlib import Path
from PIL import Image
import numpy as np
import json, subprocess
P=Path(__file__).resolve().parent
G=P/'generated'; A=P/'atlases'; A.mkdir(exist_ok=True)
directions=['S','SW','W','NW','N','NE','E','SE']
def rects(file,rows,cols):
 im=Image.open(G/file)
 a=np.array(im.getchannel('A'))
 yy=np.where((a>128).sum(axis=1)>15)[0]
 groups=[]
 for y in yy:
  if not groups or y-groups[-1][-1]>10: groups.append([int(y)])
  else: groups[-1].append(int(y))
 assert len(groups)==rows,(file,groups)
 edges=[0]+[(groups[i][-1]+groups[i+1][0])//2 for i in range(rows-1)]+[im.height]
 out=[]
 for r in range(rows):
  for c in range(cols):
   x0=round(c*im.width/cols); x1=round((c+1)*im.width/cols)
   y0=edges[r]; y1=edges[r+1]
   ys,xs=np.where(a[y0:y1,x0:x1]>128)
   l=max(x0,x0+int(xs.min())-3); t=max(y0,y0+int(ys.min())-3)
   rr=min(x1,x0+int(xs.max())+4); bb=min(y1,y0+int(ys.max())+4)
   out.append([l,t,rr-l,bb-t])
 return out
manifest={'directions':directions,'frames_per_direction':4,'cell':[128,192],'feet_anchor':[64,180],'note':'Built-in generated artwork, normalized atlas packing using FFmpeg. Inferred motion; review foot sliding and subtle directions.','characters':[]}
files=sorted(G.glob('0[1-5]_*walk*.png'))
patch='13_hero_ne_walk_patch.png'; pr=rects(patch,2,2)
for path in files:
 rs=rects(path.name,8,4)
 records=[{'source':path.name,'rect':r} for r in rs]
 if path.name.startswith('01'):
  for i in range(4):records[20+i]={'source':patch,'rect':pr[i]}
 scale=168/max(r[3] for r in rs)
 pscale=168/max(r[3] for r in pr)
 sources=list(dict.fromkeys(v['source'] for v in records))
 args=['ffmpeg','-hide_banner','-loglevel','error','-y']
 for source in sources: args+=['-i',str(G/source)]
 filters=[]
 labels={}
 for si,source in enumerate(sources):
  indices=[i for i,v in enumerate(records) if v['source']==source]
  labels.update({i:f's{i}' for i in indices})
  filters.append(f'[{si}:v]split={len(indices)}'+''.join(f'[s{i}]' for i in indices))
 for i,v in enumerate(records):
  x,y,w,h=v['rect'];sc=pscale if v['source']==patch else scale
  dw=max(1,round(w*sc));dh=max(1,round(h*sc))
  assert dw<=128 and dh<=180,(path.name,dw,dh)
  filters.append(f'[s{i}]crop={w}:{h}:{x}:{y},scale={dw}:{dh}:flags=neighbor,format=rgba,pad=128:192:(ow-iw)/2:180-ih:color=black@0[c{i}]')
 layout='|'.join(f'{(i%4)*128}_{(i//4)*192}' for i in range(32))
 filters.append(''.join(f'[c{i}]' for i in range(32))+f'xstack=inputs=32:layout={layout}:fill=black@0,format=rgba[out]')
 target=A/path.name
 args+=['-filter_complex',';'.join(filters),'-map','[out]','-frames:v','1','-update','1',str(target)]
 subprocess.run(args,check=True)
 ch={'file':'atlases/'+path.name,'source_sheet':'generated/'+path.name,'frames':[]}
 for i,v in enumerate(records):
  ch['frames'].append({'direction':directions[i//4],'phase':i%4,'rect':[(i%4)*128,(i//4)*192,128,192],'source':v})
 manifest['characters'].append(ch)
 print('Packed',target.name,flush=True)
(P/'animation_manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
meta=[]
for folder in [G,A]:
 for f in sorted(folder.glob('*.png')):
  im=Image.open(f).convert('RGBA');a=im.getchannel('A');h=a.histogram()
  meta.append({'file':f.relative_to(P).as_posix(),'size':list(im.size),'alpha_range':list(a.getextrema()),'transparent_fraction':round(h[0]/(im.width*im.height),4),'visible_bounds':a.point(lambda v:255 if v>128 else 0).getbbox()})
(P/'image_metadata.json').write_text(json.dumps(meta,indent=2),encoding='utf-8')
im=Image.open(G/'outer_frame_1920x1080.png')
assert im.size==(1920,1080)
print('Frame verified:',im.size,im.mode, 'center alpha',im.getchannel('A').crop((100,100,1820,980)).getextrema())

