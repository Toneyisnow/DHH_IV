from pathlib import Path
import json, subprocess
import numpy as np
from PIL import Image

P = Path(__file__).resolve().parent
G = P / 'generated'
for directory in ['ui', 'menus', 'npc', 'birds', 'reference/crops']:
    (P / directory).mkdir(parents=True, exist_ok=True)
config = json.loads((P / 'scene_config.json').read_text(encoding='utf-8'))
manifest = {'method': 'Built-in ImageGen artwork; FFmpeg lossless crop and atlas packing; Pillow/NumPy used only for alpha inspection and coordinates', 'menus': [], 'ui': [], 'npc': [], 'birds': {}}

def ff(args):
    subprocess.run(['ffmpeg', '-hide_banner', '-loglevel', 'error', '-y', '-filter_complex_threads', '1'] + args, check=True)

def tight(path, rect=None, margin=3):
    a = np.array(Image.open(path).convert('RGBA').getchannel('A'))
    x,y,w,h = rect or [0,0,a.shape[1],a.shape[0]]
    ys,xs = np.where(a[y:y+h,x:x+w] > 128)
    assert len(xs), (path,rect)
    l=max(x,x+int(xs.min())-margin);t=max(y,y+int(ys.min())-margin)
    r=min(x+w,x+int(xs.max())+margin+1);b=min(y+h,y+int(ys.max())+margin+1)
    return [l,t,r-l,b-t]

def crop(source, rect, dest, size=None):
    x,y,w,h=rect
    filters=f'format=rgba,crop={w}:{h}:{x}:{y}:exact=1'
    if size: filters+=f',scale={size[0]}:{size[1]}:flags=neighbor'
    ff(['-i', str(source), '-vf', filters, '-frames:v','1','-update','1',str(dest)])

# Native reference rectangles retain original scenery and text. They are evidence, not cutout sprites.
for name,rect in config['source_regions'].items():
    src=P/'reference'/('trade.png' if name.startswith('trade') else 'menu_visible.png')
    crop(src,rect,P/'reference/crops'/f'{name}.png')
for menu in config['menus']:
    crop(P/'reference/original_nine_menus.png', menu['source_rect'], P/'reference/crops'/f"original_{menu['id']}.png")

menu_source=G/'menu_buttons.png'
mw,mh=Image.open(menu_source).size
for menu in config['menus']:
    i=menu['grid_index'];c=i%3;r=i//3
    x0=round(c*mw/3);x1=round((c+1)*mw/3);y0=round(r*mh/3);y1=round((r+1)*mh/3)
    rect=tight(menu_source,[x0,y0,x1-x0,y1-y0])
    dest=P/'menus'/f"{menu['id']}.png"
    crop(menu_source,rect,dest)
    manifest['menus'].append(dict(menu,file=dest.relative_to(P).as_posix(),source='generated/menu_buttons.png',rect=rect))

ui_regions={
 'sidebar_background':('ui_sidebar',[60,0,400,1285]),
 'label_bar':('ui_sidebar',[510,72,655,123]),
 'data_bar_black':('ui_sidebar',[580,205,512,111]),
 'data_bar_black_alt':('ui_sidebar',[580,333,512,115]),
 'product_row':('ui_sidebar',[510,450,660,126]),
 'product_row_alt':('ui_sidebar',[510,578,660,126]),
 'progress_track':('ui_sidebar',[500,855,680,115]),
 'progress_green':('ui_sidebar',[500,979,680,112]),
 'function_button':('ui_sidebar',[508,1095,675,138]),
 'dialog_shell':('ui_trade_v2',[180,76,1175,710]),
 'cancel_button':('ui_trade_v2',[58,850,393,125]),
 'confirm_button':('ui_trade_v2',[500,850,396,125]),
 'number_input':('ui_trade_v2',[935,850,377,125]),
 'close_button':('ui_trade_v2',[1365,850,116,125]),
}
for name,(source,region) in ui_regions.items():
    rect=tight(G/f'{source}.png',region)
    dest=P/'ui'/f'{name}.png';crop(G/f'{source}.png',rect,dest)
    manifest['ui'].append({'id':name,'file':dest.relative_to(P).as_posix(),'source':f'generated/{source}.png','source_rect':rect,'size':rect[2:]})
crop(G/'ui_frame.png',[0,0,1561,1007],P/'ui/town_frame.png',[1188,774])
manifest['ui'].append({'id':'town_frame','file':'ui/town_frame.png','size':[1188,774],'note':'2x video viewport outer-frame size; transparent center'})

def sheet_rects(path,rows,cols):
    im=Image.open(path).convert('RGBA');a=np.array(im.getchannel('A'))
    yy=np.where((a>128).sum(axis=1)>20)[0];groups=[]
    for y in yy:
        if not groups or y-groups[-1][-1]>12: groups.append([int(y)])
        else: groups[-1].append(int(y))
    assert len(groups)==rows,(path.name,'row groups',len(groups),[(g[0],g[-1]) for g in groups])
    edges=[0]+[(groups[i][-1]+groups[i+1][0])//2 for i in range(rows-1)]+[im.height]
    rects=[]
    for r in range(rows):
        for c in range(cols):
            x0=round(c*im.width/cols);x1=round((c+1)*im.width/cols)
            rects.append(tight(path,[x0,edges[r],x1-x0,edges[r+1]-edges[r]],2))
    return rects

def pack(records, dest, cell, cols, sizes, offsets):
    sources=list(dict.fromkeys(rec['source'] for rec in records));args=[];filters=[]
    for source in sources:args+=['-i',str(G/source)]
    for si,source in enumerate(sources):
        ix=[i for i,r in enumerate(records) if r['source']==source]
        filters.append(f'[{si}:v]format=rgba,split={len(ix)}'+''.join(f'[s{i}]' for i in ix))
    for i,rec in enumerate(records):
        x,y,w,h=rec['rect'];dw,dh=sizes[i];px,py=offsets[i];cw,ch=cell
        assert dw+px<=cw and dh+py<=ch and px>=0 and py>=0,(dest.name,i,dw,dh,px,py)
        filters.append(f'[s{i}]crop={w}:{h}:{x}:{y}:exact=1,scale={dw}:{dh}:flags=neighbor,pad={cw}:{ch}:{px}:{py}:color=black@0[c{i}]')
    layout='|'.join(f'{i%cols*cell[0]}_{i//cols*cell[1]}' for i in range(len(records)))
    filters.append(''.join(f'[c{i}]' for i in range(len(records)))+f'xstack=inputs={len(records)}:layout={layout}:fill=black@0,format=rgba[out]')
    ff(args+['-filter_complex',';'.join(filters),'-map','[out]','-frames:v','1','-update','1',str(dest)])

for path in sorted(G.glob('npc_*.png')):
    if path.stem.endswith('_fix'):continue
    rects=sheet_rects(path,8,4);records=[{'source':path.name,'rect':rect} for rect in rects]
    scale=168/max(r[3] for r in rects)
    sizes=[(round(r[2]*scale),round(r[3]*scale)) for r in rects]
    if path.stem=='npc_purple_townsfolk':
        patch='npc_purple_nw_fix.png';pr=sheet_rects(G/patch,1,4);ps=168/max(r[3] for r in pr)
        for i in range(4):
            records[12+i]={'source':patch,'rect':pr[i]}
            sizes[12+i]=(round(pr[i][2]*ps),round(pr[i][3]*ps))
    offsets=[((128-w)//2,180-h) for w,h in sizes]
    dest=P/'npc'/f'{path.stem}.png'
    pack(records,dest,[128,192],4,sizes,offsets)
    frames=[]
    for i,rec in enumerate(records):
        frames.append({'direction':config['animation']['directions'][i//4],'phase':i%4,'rect':[i%4*128,i//4*192,128,192],'source':rec})
    manifest['npc'].append({'id':path.stem,'file':dest.relative_to(P).as_posix(),'size':[512,1536],'cell':[128,192],'feet_anchor':[64,180],'fps':6,'frames':frames})
    print('Packed',path.stem,flush=True)

# For bird animation, align the beak/body, not the changing wing envelope.
bird=G/'seagull_flight.png';bw,bh=Image.open(bird).size
br=[]
for i in range(8):
    c=i%4;r=i//4;x0=round(c*bw/4);x1=round((c+1)*bw/4);y0=round(r*bh/2);y1=round((r+1)*bh/2)
    br.append(tight(bird,[x0,y0,x1-x0,y1-y0],2))
# Measured approximate beak bases in source atlas. Shared scale preserves body dimensions.
beaks=[(112,316),(550,316),(1000,316),(1445,320),(114,705),(555,710),(1005,711),(1450,725)]
scale=.46
records=[{'source':bird.name,'rect':r} for r in br]
sizes=[(round(r[2]*scale),round(r[3]*scale)) for r in br]
offsets=[(round(55-(beaks[i][0]-r[0])*scale),round(170-(beaks[i][1]-r[1])*scale)) for i,r in enumerate(br)]
pack(records,P/'birds/seagull_atlas.png',[256,256],4,sizes,offsets)
manifest['birds']={'file':'birds/seagull_atlas.png','cell':[256,256],'fps':10,'frame_order':[0,1,2,3,4,6,5,7],'frames':[{'rect':[i%4*256,i//4*256,256,256],'source':r} for i,r in enumerate(records)]}

(P/'asset_manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
(P/'preview_data.js').write_text('window.TOWN_CONFIG='+json.dumps(config,ensure_ascii=False)+';\nwindow.TOWN_ASSETS='+json.dumps(manifest,ensure_ascii=False)+';',encoding='utf-8')
meta=[]
for folder in ['generated','ui','menus','npc','birds']:
    for p in sorted((P/folder).glob('*.png')):
        im=Image.open(p).convert('RGBA');a=np.array(im.getchannel('A'))
        meta.append({'file':p.relative_to(P).as_posix(),'size':list(im.size),'alpha_range':[int(a.min()),int(a.max())],'transparent_fraction':round(float((a==0).mean()),4)})
(P/'verification.json').write_text(json.dumps({'menu_count':len(manifest['menus']),'npc_count':len(manifest['npc']),'npc_frame_count':sum(len(n['frames']) for n in manifest['npc']),'bird_frame_count':8,'images':meta},ensure_ascii=False,indent=2),encoding='utf-8')
print('Pack complete:',len(manifest['menus']),'menus,',len(manifest['npc'])*32,'NPC frames, 8 bird frames',flush=True)
