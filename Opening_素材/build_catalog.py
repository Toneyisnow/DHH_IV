from pathlib import Path
import base64, json, html
from PIL import Image

ROOT = Path(__file__).resolve().parent
OPEN = ROOT / '01_开场'
SELECT = ROOT / '02_人物选择'

def write(path, text):
    path.write_text(text, encoding='utf-8')

def data(path):
    return 'data:image/png;base64,' + base64.b64encode(path.read_bytes()).decode('ascii')

def svg(body, w=1024, h=256):
    return f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" viewBox="0 0 {w} {h}">{body}</svg>'

for filename, label, size in [('04_remake.svg', 'remake', 96), ('05_press_any_key.svg', 'Press Any Key to Start', 72)]:
    write(OPEN / filename, svg(f'<text x="512" y="152" text-anchor="middle" font-family="Georgia,Times New Roman,serif" font-size="{size}" fill="#2c2118" transform="translate(2,3)">{label}</text><text x="512" y="152" text-anchor="middle" font-family="Georgia,Times New Roman,serif" font-size="{size}" fill="#f6ead2">{label}</text>'))

write(OPEN/'08_dark_background.svg', svg('<defs><radialGradient id="g"><stop stop-color="#423827"/><stop offset="1" stop-color="#061116"/></radialGradient></defs><path fill="url(#g)" d="M0 0H1920V1080H0Z"/>',1920,1080))
write(OPEN/'09_vignette.svg', svg('<defs><radialGradient id="g"><stop offset=".45" stop-color="#000" stop-opacity="0"/><stop offset="1" stop-color="#000" stop-opacity=".72"/></radialGradient></defs><path fill="url(#g)" d="M0 0H1920V1080H0Z"/>',1920,1080))
write(OPEN/'10_soft_glow.svg', svg('<defs><radialGradient id="g"><stop stop-color="#fff4c5" stop-opacity=".9"/><stop offset=".2" stop-color="#ffd37a" stop-opacity=".35"/><stop offset="1" stop-color="#ffc465" stop-opacity="0"/></radialGradient></defs><circle cx="256" cy="256" r="250" fill="url(#g)"/>',512,512))

# Self-contained SVG UI compositions: original PNG data embedded unmodified.
frame = data(SELECT/'05_empty_frame.png')
for index in range(1,8):
    portrait = data(SELECT/f'P{index:02}_portrait.png')
    body = '<defs><clipPath id="portrait"><rect x="308" y="235" width="730" height="674"/></clipPath></defs>'
    body += f'<g clip-path="url(#portrait)"><rect x="308" y="235" width="730" height="674" fill="#d6c19b"/><image href="{portrait}" x="218" y="165" width="910" height="1260" preserveAspectRatio="xMidYMin meet"/></g>'
    body += f'<image href="{frame}" width="1348" height="1167"/>'
    write(SELECT/f'P{index:02}_card.svg',svg(body,1348,1167))

# Runtime contour shader in a portable SVG filter, derived from frame alpha.
filter_body = '<defs><filter id="outline" x="-15%" y="-15%" width="130%" height="130%"><feMorphology in="SourceAlpha" operator="dilate" radius="5" result="wide"/><feComposite in="wide" in2="SourceAlpha" operator="out" result="edge"/><feGaussianBlur in="edge" stdDeviation="9" result="soft"/><feFlood flood-color="#ffcd54"/><feComposite in2="soft" operator="in" result="halo"/><feFlood flood-color="#fff4b3"/><feComposite in2="edge" operator="in" result="line"/><feMerge><feMergeNode in="halo"/><feMergeNode in="line"/></feMerge></filter></defs>'
write(SELECT/'09_hover_outline.svg',svg(filter_body+f'<image href="{frame}" width="1348" height="1167" filter="url(#outline)"/>',1348,1167))

manifest = {'source':'Opening.mp4 + user supplied seven-character reference','method':'Built-in image_gen reconstruction; existing logo copied; typography and UI compositions are SVG','reference_resolution':[726,408], 'portrait_order':['user collage bottom-left','top row 6','top row 4','top row 1','top row 2','top row 5','top row 3'], 'assets':[]}
for folder in [OPEN,SELECT]:
    for path in sorted(folder.iterdir()):
        entry={'path':path.relative_to(ROOT).as_posix(),'bytes':path.stat().st_size}
        if path.suffix=='.png':
            with Image.open(path) as im:
                entry.update(size=list(im.size),mode=im.mode)
                if im.mode=='RGBA':
                    alpha=im.getchannel('A')
                    entry['alpha_range']=list(alpha.getextrema())
                    entry['transparent_pixels']=alpha.histogram()[0]
        manifest['assets'].append(entry)
write(ROOT/'manifest.json',json.dumps(manifest,ensure_ascii=False,indent=2))
write(ROOT/'animation_config.json',json.dumps({'note':'Suggested runtime parameters, not extracted original animation data.','globe':{'degrees_per_second':1,'texture':'01_开场/02_world_map_texture.png','seamless_verified':False},'press_any_key':{'period_seconds':1.8,'alpha':[.3,1]},'light_sweep':{'duration_seconds':2.8,'repeat_delay_seconds':9,'blend':'additive','control_points_normalized':[[-.1,.45],[.2,.65],[.58,.32],[.85,.45],[1.1,.3]]},'flame':{'texture':'02_人物选择/04_flame_sheet.png','grid':[4,2],'frames':8,'fps':10,'note':'Generated atlas has noninteger nominal cell dimensions; split using rounded proportional bounds and tune pivots.'},'hover':{'enter_ms':150,'leave_ms':180,'scale':1.02,'outline_asset':'02_人物选择/09_hover_outline.svg'},'table':{'camera_to_plane_degrees':30,'note':'Applies to 3D setup; composed background already has perspective.'}},ensure_ascii=False,indent=2))

items=''
for entry in manifest['assets']:
    path=entry['path']
    items+=f'<a class="tile" href="{path}" target="_blank"><div class="check"><img loading="lazy" src="{path}"></div><span>{html.escape(path.split("/")[-1])}</span></a>'
cards=''.join(f'<div class="card"><img class="portrait" src="02_人物选择/P{i:02}_card.svg"><img class="outline" src="02_人物选择/09_hover_outline.svg"></div>' for i in range(1,8))
page='''<!doctype html><html lang="zh-CN"><meta charset="utf-8"><title>Opening 素材总览</title><style>
body{margin:0;background:#14171b;color:#eadfc8;font:16px system-ui;padding:36px}h1{font-size:30px}p{color:#b8b5ad;max-width:1050px;line-height:1.7}.grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(220px,1fr));gap:18px}.tile{color:inherit;text-decoration:none;background:#24282d;padding:12px;border-radius:10px;overflow:hidden}.check{height:205px;background:repeating-conic-gradient(#666 0 25%,#888 0 50%) 0/20px 20px;display:flex;align-items:center;justify-content:center}.tile img{max-width:100%;max-height:100%;object-fit:contain}.tile span{display:block;font-size:12px;margin-top:12px}.scene{max-width:1050px;aspect-ratio:16/9;background:url('02_人物选择/01_clean_table_background.png') center/cover;position:relative;overflow:hidden}.cards{position:absolute;inset:23% 12% 4%;display:flex;flex-wrap:wrap;justify-content:center;align-content:center;gap:0;width:76%}.card{position:relative;width:24%;aspect-ratio:1348/1167;transition:transform .15s}.card:nth-child(1){margin-left:12%}.card:nth-child(3){margin-right:12%}.card img{width:100%;height:100%;position:absolute;inset:0}.outline{opacity:0;transition:opacity .15s}.card:hover{transform:scale(1.02)}.card:hover .outline{opacity:1}.candle{position:absolute;right:-2%;top:-8%;width:20%}h2{margin-top:40px}
</style><h1>开场与七主角选择 · 素材包</h1><p>PNG 为参照视频 / 用户人物图重建的素材，标题复用已有文件；文字、暗角与卡片组合为 SVG。棋盘格用于检查透明度，不属于图像内容。点击素材查看原文件。</p><h2>卡片组合 / hover 预览</h2><p>下图用于检验素材组合，人物裁切、蜡烛摆位及透视仍可在游戏中微调；不是原视频逐帧复现。</p><div class="scene"><img class="candle" src="02_人物选择/03_candle_holder.png"><div class="cards">'''+cards+'''</div></div><h2>全部素材</h2><div class="grid">'''+items+'</div></html>'
write(ROOT/'素材总览.html',page)
print(f'Catalog created: {len(manifest["assets"])} assets')
