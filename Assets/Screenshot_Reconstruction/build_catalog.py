from pathlib import Path
import json, html, zipfile
P=Path(__file__).resolve().parent
meta=json.loads((P/'image_metadata.json').read_text())
anims=json.loads((P/'animation_frames.json').read_text())
dialogue=json.loads((P/'dialogue.json').read_text(encoding='utf-8'))
descs=[
("外围主框","屏幕四周的窄金属装饰框，暗金色卷草、铆接和花饰。适合屏幕空间 UI，四角固定、边线延伸。生成版中心透明。","[0,0,724,408]"),
("左上角标识","圆形航海仪器徽章叠加金色“大航海時代”、红色 IV 与 HD-2D。截图不能证明它是游戏内部 UI，亦可能是视频叠加标识。生成版字形和徽章比例有差异。","约 [1,2,125,73]"),
("左下头像框","竖向金属边框与四角纹饰；框内暖白背景应置于人物后方。独立生成的是透明框，不包含人物和背景。","约 [17,220,155,188]"),
("人物立绘","黑色短发、齐刘海，黑底金纹衣装、粉色花饰、红色披布和白色袖子。截图只显示半身，本包补绘为半身透明图，面部和衣纹不是原版逐像素复原。","约 [22,230,142,176]"),
("下方空白对话面板","象牙白底、棕金色描边、四角纹饰。应与文字分层。生成版已经去除姓名、台词、指南针和翻页三角。","约 [171,301,429,107]"),
("指南针参考帧","截图底部中央的小圆盘可能是指南针式动态标识，但单帧无法确认其功能、转向与周期。补绘为 4×2 共 8 帧，表盘固定、指针分步旋转；各帧细节仍有轻微漂移。","约 [372,382,31,26]，底部截断"),
("地面纹理","港口地面混合黄褐沙土、浅色不规则石板，近处横向石道清晰。生成版是俯视石板颜色纹理；截图中的透视、阴影与远近尺度应由场景处理。未保证无缝。","主要可见于 [240,161] 到 [645,301] 的不规则区域"),
("主建筑外观","右侧主体：茅草双层坡屋顶、交叉木构饰件、雕花山墙、深棕木墙、二层栏杆、门廊及小遮棚。输出为整栋透明外观，可用作 2D 场景物件；不是模型 UV 贴图。","约 [430,36,166,202]"),
("角色行走参考图","中央角色约 30×45 像素，黑发红衣，黑色外轮廓。补绘为四方向各四帧，行序为下、左、右、上。动作差异偏小、轮廓有变化；不是原游戏动作数据。","约 [347,197,36,46]"),
("港口场景底图","海面与远景船只在后，码头、棕榈和木屋在中，前景物件虚化。生成版去掉人物及 UI，并补全被遮挡区域；透视和遮挡补全部分为推测。","约 [14,14,697,380]，多处被 UI 遮挡"),
("茅草屋顶纹理","独立正视茅草层叠纹理，用于屋面颜色参考。要配合屋顶几何、UV 方向与光照；未提供法线、粗糙度、高度图。未保证无缝。","对应主建筑屋顶"),
("木墙纹理","独立深棕色竖向木板纹理，用于墙面颜色参考；原截图还包含横梁、栏杆和雕花，不能用这张图替代全部构件。未保证无缝。","对应主建筑木墙"),
("翻页提示","对话框右下红色倒三角，与指南针分别处理。单帧无法确认是否闪烁或跳动。生成图有较大透明留白，使用可见范围放置。","约 [558,382,16,12]")
]
cards=[]
for m,d in zip(meta,descs):
 f=m['file']; title,desc,rect=d
 cards.append(f'<article><a href="generated/{f}" target="_blank"><div class="checker"><img loading="lazy" src="generated/{f}" alt="{title}"></div></a><div class="body"><h3>{title}</h3><p>{desc}</p><small>截图范围：{rect}<br>{m["width"]} × {m["height"]} · {"含 Alpha" if m["alpha_range"][0]==0 else "不透明"}<br>可见范围：{m["visible_bounds"]}</small><p><a download href="generated/{f}">{f} ↗</a></p></div></article>')
report='''截图组件分析与素材说明
本包全部 PNG 由内置 image_gen 参照截图重建，不是从游戏资源文件提取的原始素材，也不是原截图的无损切片。
原截图显示为 724×408；实际源文件尺寸见下方检查记录。坐标按 724×408 显示参考系，以左上为原点，格式为 x,y,width,height。所有位置为人工估计。
截图展示的风格：低分辨率人物精灵，较高细节场景，暖色光照和前后景虚化。仅凭截图不能证明底层是 3D、预渲染背景还是多层 2D。
建议图层顺序：港口背景/地面 → 建筑与场景物件 → 按脚底 Y 值排序的人物 → 前景遮挡 → 外围框 → 标识 → 对话面板 → 头像底色/立绘/头像框 → 姓名/台词 → 指南针/翻页提示。
'''

for _,d in zip(meta,descs):
 report+='\n'+d[0]+'\n'+d[1]+'\n截图位置：'+d[2]+'\n'
report+='''
文字层：
姓名：蒂雅·李
台词：統治東南亞的庫恩和普雷依拉。我們李家和阿歌特商會好像被捲入兩者之間的戰爭了。
这是人工读图转录，未经原脚本核对。姓名的小字号及间隔符应以原图/原脚本最终核对。
推荐文本独立渲染，以支持本地化、换行和打字动画；不把文字烧录到面板图片中。

交付与验证：
generated/：13 张重建 PNG。UI、人物、建筑、精灵图已检查存在真实 Alpha；黑色预览区不是黑色背景图片。
image_metadata.json：实际尺寸、透明比例和 alpha>128 的可见范围，范围为 left,top,right,bottom，右/下不包含。
animation_frames.json：基于 alpha>128 测得每格主体的 x,y,width,height。弱透明边缘可能位于范围外。人物和指南针图并非严格等格对齐，优先使用逐帧坐标。
preview.html：素材画廊、原截图对照和参考帧播放，可在浏览器本地打开，无网络依赖。
dialogue.json：姓名、台词与参考布局。
prompts.json：内置 image_gen 实际生成提示词。
动画预览以各帧脚底居中作临时对齐；建议人物 6–8 fps、指南针 4–8 fps 仅作试播值，非原游戏帧率。人物帧仍需修正步幅与轮廓漂移。
生成纹理虽然要求可平铺，但未通过严格边缘连续性检查；不可宣称已经无缝。
图像不是原画精确复原；透明边缘可能有浅色/彩色毛边，正式使用前应在目标背景上复核。
截图之外的角色方向、遮挡区细节、材质纹理与指南针运动均为重建推测。目录内的视频未参与本次重建。
'''
(P/'分析与使用说明.txt').write_text(report,encoding='utf-8')
page='''<!doctype html><html lang="zh-CN"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>航海港口 · 组件素材包</title><style>
*{box-sizing:border-box}body{margin:0;background:#141c21;color:#f0e7d6;font:16px/1.75 "Microsoft YaHei",sans-serif}main{max-width:1300px;margin:auto;padding:40px 24px}h1{font-size:36px;margin:0}h2{margin-top:40px}h3{margin:0;color:#dfbd80}p{margin:10px 0}a{color:#e5c68f}small,.muted{color:#aeb8bb}header{border-bottom:1px solid #4c5353;padding-bottom:24px}.tag{color:#e4bb72;letter-spacing:3px;font-size:12px}.notice{background:#263138;padding:18px 24px;border-left:3px solid #bc985c;margin:24px 0}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(280px,1fr));gap:20px}article{background:#202a30;border:1px solid #38444a;border-radius:10px;overflow:hidden}.body{padding:18px}.checker{background-color:#899092;background-image:conic-gradient(#737a7d 25%,transparent 0 50%,#737a7d 0 75%,transparent 0);background-size:24px 24px}.checker img{display:block;width:100%;height:255px;object-fit:contain}.source{width:100%;max-width:1000px;border:1px solid #576367}.player{display:flex;flex-wrap:wrap;gap:24px;align-items:center}.player canvas{background:#35434b;border-radius:8px}button,select,input{font:inherit}button,select{padding:7px 14px;background:#33434c;color:white;border:1px solid #6b777a;border-radius:6px}pre{white-space:pre-wrap;overflow-wrap:anywhere;background:#202a30;padding:24px;font:15px/1.9 "Microsoft YaHei",sans-serif}footer{margin:40px 0;color:#aeb8bb}
</style><main><header><div class="tag">SCREENSHOT → ASSETS / 13 PNG</div><h1>航海港口 · 分层素材重建</h1><p>外围 UI、人物立绘、场景纹理与动画参考帧</p></header>
<div class="notice"><strong>这是参照截图的 AI 重建素材包。</strong>包含补绘与推测，并非游戏原始资源。动画是参考序列，纹理未保证无缝，详细限制见下方说明。</div>
<p><a href="分析与使用说明.txt">完整分析</a> · <a href="dialogue.json">对话文字</a> · <a href="image_metadata.json">尺寸与透明范围</a> · <a href="animation_frames.json">逐帧坐标</a> · <a href="prompts.json">生成提示词</a></p>
<h2>原截图对照</h2><img class="source" src="reference.png" alt="原截图">
<h2>动画参考预览</h2><p class="muted">逐帧主体区域试播；人物脚底居中对齐。帧率可调，不能代表原游戏运动。当前素材动作幅度较小。</p><div class="player"><canvas id="walk" width="200" height="300"></canvas><canvas id="compass" width="180" height="180"></canvas><div><p><select id="direction" aria-label="行走方向"><option value="0">朝下</option><option value="1">朝左</option><option value="2">朝右</option><option value="3">朝上</option></select></p><p><label>帧率 <input id="fps" type="range" min="1" max="12" value="6"><span id="rate">6</span> fps</label></p><button id="toggle">暂停</button></div></div>
<h2>独立 PNG 画廊</h2><p class="muted">棋盘格显示透明区域。点击图片打开原始生成尺寸；可见范围用于去除外部留白。</p><section class="grid">'''+''.join(cards)+'''</section><h2>组件解析与接入说明</h2><pre>'''+html.escape(report)+'''</pre><footer>生成方式：内置 image_gen。保留原始生成 PNG，未将重新绘制内容伪称为原始游戏资产。</footer></main><script>
const data='''+json.dumps(anims)+''';
const walk=new Image(), compass=new Image();walk.src="generated/09_walk_sheet.png";compass.src="generated/06_compass_sheet.png";
let paused=false,step=0,last=0;const fps=document.querySelector("#fps");fps.oninput=()=>document.querySelector("#rate").textContent=fps.value;
document.querySelector("#toggle").onclick=e=>{paused=!paused;e.target.textContent=paused?"继续":"暂停"};
function draw(id,img,rect,scale,bottom){if(!img.complete||!img.naturalWidth)return;const c=document.getElementById(id),ctx=c.getContext("2d");ctx.clearRect(0,0,c.width,c.height);ctx.imageSmoothingEnabled=id!=="walk";const [x,y,w,h]=rect;ctx.drawImage(img,x,y,w,h,(c.width-w*scale)/2,bottom-h*scale,w*scale,h*scale)}
function tick(t){if(!paused&&t-last>1000/Number(fps.value)){step++;last=t}let row=Number(document.querySelector("#direction").value);draw("walk",walk,data["09_walk_sheet.png"].frames[row*4+step%4].rect,.9,285);draw("compass",compass,data["06_compass_sheet.png"].frames[step%8].rect,.5,168);requestAnimationFrame(tick)}requestAnimationFrame(tick);
</script></html>'''
(P/'preview.html').write_text(page,encoding='utf-8')
with zipfile.ZipFile(P.parent/'Screenshot_Reconstruction.zip','w',zipfile.ZIP_DEFLATED) as z:
 for f in P.rglob('*'):
  if f.is_file(): z.write(f,Path(P.name)/f.relative_to(P))
print('Catalog and ZIP created; PNG count:',len(meta))

