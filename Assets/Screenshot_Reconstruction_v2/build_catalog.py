from pathlib import Path
from PIL import Image
from html.parser import HTMLParser
import json,html,zipfile
P=Path(__file__).resolve().parent
prompts=json.loads((P/'prompts.json').read_text(encoding='utf-8'))
for item in prompts['assets']:
 if item['file']=='08_palm_cluster_shadow.png':item['file']='08_left_palm_shadow.png'
(P/'prompts.json').write_text(json.dumps(prompts,ensure_ascii=False,indent=2),encoding='utf-8')
manifest=json.loads((P/'animation_manifest.json').read_text(encoding='utf-8'))
titles=['主角 · 蒂雅','NPC · 深蓝帽船员','NPC · 浅色头巾工人','NPC · 草帽蓝衣行人','NPC · 棕衣商人']
props=[
('06_foreground_junk_shadow.png','左侧近景帆船','补全画面左边缘裁掉的船体；包含水线暗影和局部倒影。'),
('07_midground_junk_shadow.png','左侧中景货船','包含甲板货物与水线倒影。倒影带有颜色，须在实际海面上检查融合。'),
('08_left_palm_shadow.png','道路左侧棕榈','细长树干与完整叶冠，含半透明地面投影。可复制组合成截图中的树丛。'),
('09_palm_single_shadow.png','道路右侧棕榈','倾斜树干与较宽叶冠，含半透明地面投影。'),
('10_center_hut_shadow.png','中央左侧尖顶茅草屋','补全被树遮住的墙面和门口，附带地面投影。隐藏结构为推测。'),
('11_center_back_hut_shadow.png','中央后方双层茅草屋','小型坡屋顶、木栏杆、上下层门窗，附带地面投影。隐藏结构为推测。')]
description="""本轮素材说明
本包为截图参考重建，使用内置 image_gen 生成与修正。不是从游戏程序中提取的原始资源。原截图为 724×408。
最终交付 12 项：5 套八方向行走图、2 艘帆船、2 种棕榈、2 座中央茅草屋，以及 1920×1080 外围框。

一、外围框
generated/outer_frame_1920x1080.png：精确 1920×1080，RGBA，保留透明中心。重绘了金属花饰、链状边线与顶部卷草。
generated/12_outer_frame_hd_source.png：内置生成工具返回的 1672×941 重绘源图，使用 FFmpeg Lanczos 定尺导出至 1920×1080。
这是重新绘制后的定尺导出，不是游戏官方高清原图；并非原生 1920×1080 生成，边缘仍可能有少量抗锯齿彩边。中央极少像素 Alpha 为 1–2，肉眼近乎透明。

二、主角与 NPC
请使用 atlases/ 下的五张最终图集，每张 512×1536，4 列×8 行，每格 128×192，脚底锚点 (64,180)。
从上到下行顺序：
0 下 S；1 左下 SW；2 左 W；3 左上 NW；4 上 N；5 右上 NE；6 右 E；7 右下 SE。
每行四帧从左向右播放。建议先用 6 fps 试播；这不是原游戏帧率。
主角右上方向单独补绘并合入最终 atlas。generated/01_hero_walk_8dir.png 是未合并补帧的中间源图，请不要拿它替代最终 atlas。
generated/13_hero_ne_walk_patch.png 是右上方向补帧源图。
NPC 名称按外观区分，并非已确认的游戏身份。依据截图位置：
深蓝帽船员约 (297,277)，浅色头巾工人约 (537,242)，草帽蓝衣行人约 (609,244)，棕衣商人约 (633,225)。
没有把所有远景小人逐个当成不同身份；选择了四种清楚的外观。
animation_manifest.json 记录方向、各帧矩形、原始取样坐标和来源。使用 FFmpeg 进行裁格、最近邻缩放及图集打包，未用程序补画角色。
脚底位置已按可见像素下缘对齐，但这不是严格的骨骼脚底，仍可能有滑步、轻微尺寸漂移。NPC 的部分斜向与正向差别较小。四帧动作幅度较小，适合原型试用，未达到最终商用品质动画。

三、场景物件及阴影
两艘帆船、两种棕榈、两座中央茅草屋各自为独立透明 PNG。物件与其阴影在同一张图中，阴影没有单独分层。
棕榈和房屋为半透明地面投影，船为水线暗影/倒影，不把船画成地面落影。
图片中的主体朝向、阴影方向和光照已绘制在图上；如改变场景太阳位置，需要重新制作阴影。
帆船倒影含固定水色，在不同水面可能看出边界；需要在目标海面上检查融合。
单张截图无法恢复被遮挡结构、真实模型、材质贴图或真实运动；所有补全部分为推测。
截图中右侧主草屋沿用第一版，此轮新增的是中央两座不同房屋。

四、文件
preview.html：离线画廊与动画预览。浏览器打开即可；可切换八方向、帧率和物件底色。
atlases/：优先使用的最终五套动画图集。
generated/：场景物件、外围框、生成源图和主角补帧。
prompts.json：生成及修正提示词。
image_metadata.json：尺寸、真实透明通道检查与可见范围。
pack_atlases.py：图集重打包脚本，依赖 Pillow、NumPy 与 FFmpeg。
本版独立保存，第一版素材未覆盖。
"""
(P/'使用说明.txt').write_text(description,encoding='utf-8')
meta=[]
for folder in ['generated','atlases']:
 for f in sorted((P/folder).glob('*.png')):
  im=Image.open(f).convert('RGBA');a=im.getchannel('A');h=a.histogram()
  meta.append({'file':f.relative_to(P).as_posix(),'size':list(im.size),'alpha_range':list(a.getextrema()),'transparent_fraction':round(h[0]/(im.width*im.height),4),'visible_bounds':a.point(lambda v:255 if v>128 else 0).getbbox()})
(P/'image_metadata.json').write_text(json.dumps(meta,indent=2),encoding='utf-8')
cc=[]
for i,(ch,title) in enumerate(zip(manifest['characters'],titles)):
 cc.append(f'<article><h3>{title}</h3><canvas id="actor{i}" width="256" height="384" aria-label="{title}八方向动画"></canvas><p><a href="{ch["file"]}" download>下载最终八方向图集</a></p><small>512×1536 · 32 帧 · 单格 128×192</small></article>')
pc=[]
for f,title,note in props:
 w,h=Image.open(P/'generated'/f).size
 pc.append(f'<article><a href="generated/{f}" target="_blank"><div class="stage"><img src="generated/{f}" alt="{title}"></div></a><h3>{title}</h3><p>{note}</p><small>{w}×{h} · RGBA</small><p><a download href="generated/{f}">下载 PNG ↗</a></p></article>')
page="""<!doctype html><html lang="zh-CN"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>航海港口 · 素材扩展 V2</title><style>
*{box-sizing:border-box}body{margin:0;background:#151d22;color:#efe8da;font:16px/1.75 "Microsoft YaHei",sans-serif}main{max-width:1320px;padding:36px 24px;margin:auto}h1{margin:0;font-size:36px}h2{margin:40px 0 14px}h3{color:#dfbd80;margin:14px 0 4px}p{margin:8px 0}a{color:#e5c68f}small,.muted{color:#aebbbf}.tag{letter-spacing:3px;color:#c9a76c;font-size:12px}.note{background:#25323a;border-left:3px solid #c9a76c;padding:16px 20px;margin:24px 0}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(220px,1fr));gap:18px}.props{grid-template-columns:repeat(auto-fit,minmax(340px,1fr))}article{background:#202b31;border:1px solid #39464d;border-radius:10px;padding:18px;overflow:hidden}canvas{width:100%;max-width:220px;display:block;margin:auto;background:#37464f;image-rendering:pixelated}.stage{background-color:#82908f;background-image:conic-gradient(#697877 25%,transparent 0 50%,#697877 0 75%,transparent 0);background-size:24px 24px}.stage img{display:block;width:100%;height:350px;object-fit:contain}.frame{overflow:auto}.frame img{width:100%;height:auto;display:block}.frame.zoom img{width:1920px;max-width:none}.controls{display:flex;flex-wrap:wrap;gap:12px;align-items:center;position:sticky;top:0;padding:14px;background:#26343def;z-index:2;border-radius:8px}button,select,input{font:inherit}button,select{padding:7px 12px;color:#efe8da;background:#36454e;border:1px solid #758187;border-radius:5px;cursor:pointer}button.active{background:#876b3e}.source{max-width:724px;width:100%}pre{font:15px/1.9 "Microsoft YaHei",sans-serif;white-space:pre-wrap;overflow-wrap:anywhere;background:#202b31;padding:22px}footer{margin:30px 0;color:#aebbbf}
</style><main><header><div class="tag">ASSET EXPANSION / V2</div><h1>八方向人物与港口物件</h1><p>5 套行走图 · 6 件带阴影场景物件 · 1080p 外围框</p></header>
<div class="note">基于截图补绘的重建素材。行走图为动作参考，斜向、步幅与脚底仍需在实际场景中检验；遮挡部分和阴影形状含推测。</div>
<p><a href="使用说明.txt">使用说明</a> · <a href="animation_manifest.json">动画坐标</a> · <a href="image_metadata.json">图片参数</a> · <a href="prompts.json">生成提示词</a></p>
<h2>外围框 · 1920×1080</h2><p>重绘纹饰后定尺导出，透明中心。可按 1:1 查看实际像素。</p><p><button id="zoom">按 1:1 查看</button> <a href="generated/outer_frame_1920x1080.png" download>下载 1920×1080 PNG</a></p>
<div class="frame stage" id="frame"><img src="generated/outer_frame_1920x1080.png" alt="1920×1080 高清外围框"></div>
<h2>八方向行走 · 最终图集预览</h2><div class="controls"><label>方向 <select id="dir" aria-label="方向"><option value="0">↓ 下</option><option value="1">↙ 左下</option><option value="2">← 左</option><option value="3">↖ 左上</option><option value="4">↑ 上</option><option value="5">↗ 右上</option><option value="6">→ 右</option><option value="7">↘ 右下</option></select></label><label>帧率 <input type="range" id="fps" min="1" max="12" value="6"><span id="rate">6</span></label><button id="toggle">暂停</button><button id="next">下一帧</button><span id="phase">帧 1 / 4</span></div>
<p class="muted">统一 128×192 格子，脚底锚点 (64,180)。主角右上方向已使用单独补绘帧。最终图集都在 atlases 目录。</p><div class="grid">"""+''.join(cc)+"""</div>
<h2>独立场景物件</h2><p>阴影与主体位于同一 PNG；换底色可检查半透明效果和边缘。</p><p><label>查看背景 <select id="bg"><option value="checker">棋盘格</option><option value="#e1d0a8">沙土地面</option><option value="#38686e">海水色</option><option value="#ffffff">白色</option><option value="#111820">深色</option></select></label></p>
<div class="grid props">"""+''.join(pc)+"""</div><h2>原截图</h2><img class="source" src="reference.png" alt="原截图"><h2>规格与限制</h2><pre>"""+html.escape(description)+"""</pre><footer>内置 image_gen 重建；FFmpeg 仅用于精确尺寸导出及图集排列。旧版文件保留。</footer></main><script>
const chars="""+json.dumps(manifest['characters'])+""";
const images=chars.map(c=>{const i=new Image();i.src=c.file;return i});
let step=0,last=0,paused=false;const dir=document.getElementById("dir"),fps=document.getElementById("fps");
fps.oninput=()=>document.getElementById("rate").textContent=fps.value;
document.getElementById("toggle").onclick=e=>{paused=!paused;e.target.textContent=paused?"继续":"暂停"};
document.getElementById("next").onclick=()=>{paused=true;step++;document.getElementById("toggle").textContent="继续"};
document.getElementById("zoom").onclick=e=>{const f=document.getElementById("frame");f.classList.toggle("zoom");e.target.textContent=f.classList.contains("zoom")?"适应页面":"按 1:1 查看"};
document.getElementById("bg").onchange=e=>{document.querySelectorAll(".stage").forEach(s=>{s.style.backgroundImage=e.target.value==="checker"?"":"none";s.style.backgroundColor=e.target.value==="checker"?"":e.target.value})};
function tick(t){if(!paused&&t-last>1000/Number(fps.value)){step++;last=t}const phase=step%4;document.getElementById("phase").textContent="帧 "+(phase+1)+" / 4";images.forEach((im,k)=>{if(!im.complete||!im.naturalWidth)return;const c=document.getElementById("actor"+k),ctx=c.getContext("2d");ctx.clearRect(0,0,c.width,c.height);ctx.imageSmoothingEnabled=false;ctx.drawImage(im,phase*128,Number(dir.value)*192,128,192,0,0,256,384)});requestAnimationFrame(tick)}requestAnimationFrame(tick);
</script></html>"""
(P/'preview.html').write_text(page,encoding='utf-8')
class Check(HTMLParser):
 def handle_starttag(self,tag,attrs):
  for key,val in attrs:
   if key in ['href','src']:assert (P/val).exists(),val
Check().feed(page)
for ch in manifest['characters']:
 im=Image.open(P/ch['file']);assert im.size==(512,1536);assert im.mode=='RGBA'
assert Image.open(P/'generated/outer_frame_1920x1080.png').size==(1920,1080)
zip_path=P.parent/'Screenshot_Reconstruction_v2.zip'
with zipfile.ZipFile(zip_path,'w',zipfile.ZIP_DEFLATED) as z:
 for f in P.rglob('*'):
  if f.is_file():z.write(f,Path(P.name)/f.relative_to(P))
with zipfile.ZipFile(zip_path) as z:assert z.testzip() is None
print('Verified 5 atlases (160 frames), 6 props, 1920x1080 frame, local links and ZIP.')
print('ZIP MB:',round(zip_path.stat().st_size/1048576,2))

