"""Publish the visually reviewed proud action as an explicitly marked prototype."""
import hashlib
import json
from pathlib import Path
import shutil

# Rejected by the user: never republish the Wan trial as a game asset.
raise SystemExit('05 Wan prototype was rejected. Use the new key-pose workflow; do not publish proud-smile-v2.mp4.')

root = Path(__file__).resolve().parents[2]
art = root/'ArtSource/Character04B/FaceExpressions'
source = art/'Proud/proud-smile-v2.mp4'
relative = 'Videos/Characters/Character04/Character04_05_Proud.mp4'
target = root/'MaritimeLegend/Assets/Resources'/relative
shutil.copy2(source,target)
digest = hashlib.sha256(source.read_bytes()).hexdigest()
assert hashlib.sha256(target.read_bytes()).hexdigest() == digest
manifest_path = art/'unity-published-assets.json'
manifest = json.loads(manifest_path.read_text(encoding='utf-8'))
manifest = [item for item in manifest if item['unity_resource'] != relative]
manifest.append({'source':str(source.relative_to(root)),'unity_resource':relative,'sha256':digest,'status':'motion prototype; hand/accessory detail drift remains; not seamless-looping'})
manifest_path.write_text(json.dumps(manifest,indent=2),encoding='utf-8')
preview = art/'preview.html'
html = preview.read_text(encoding='utf-8')
card = '<article><video src="Proud/proud-smile-v2.mp4" poster="Proud/proud-smile-v2-frame-00.png" controls muted playsinline preload="metadata"></video><h2>05 · 自豪（动作样片）</h2><p>双手移向腰侧，双肘外展；衣袖和斗篷随动作摆动，笑容渐渐出现。448 × 480 · 16 fps · 约 4 秒。结尾保持叉腰，不循环。</p><p>手指、罗盘和腰带等细节仍有漂移，待精修。<a href="Proud/proud-smile-v2-contact-sheet.jpg">关键帧</a></p></article>'
marker = '</div>\n<h2>先分析，再驱动</h2>'
assert marker in html
if '05 · 自豪' not in html:
    html = html.replace(marker,card+marker,1)
html = html.replace('AdvancedLivePortrait · 704 × 768 · 24 fps · 每段 2 秒。','01–04：AdvancedLivePortrait · 704 × 768 · 24 fps · 每段 2 秒。05：Wan 身体动作 + AdvancedLivePortrait 笑容 · 448 × 480 · 16 fps · 约 4 秒。')
html = html.replace('四段均为 0（无损帧）','01–04 均为 0（无损帧）；05 不循环').replace('<dd>48 帧 / 2 秒</dd>','<dd>01–04：48 帧 / 2 秒；05：65 帧 / 约 4 秒</dd>')
html = html.replace('没有三维布料或头发物理模拟。','05 的手臂和衣物由视频模型生成，再逐帧跟踪脸部添加笑容。所有片段均没有三维布料或头发物理模拟。')
preview.write_text(html,encoding='utf-8')
notes = '''05 自豪：动作样片
当前文件：proud-smile-v2.mp4，448×480，16fps，65帧，4.0625秒。
流程：半身原图 → 本地 Wan 2.2 TI2V 5B 生成双手叉腰和衣物运动 → AdvancedLivePortrait 逐帧跟踪面部，渐进添加 smile=0.85。
实际观察：双手从原手位移向腰侧，双肘外展；衣袖和斗篷随动作变化；嘴角上扬并略微露齿。
剩余问题：部分手指形状不稳定，腰带、罗盘、装饰和身体轮廓有生成漂移；人物仍有小幅姿态摆动。未达到精修资源标准。
结尾保留叉腰笑容；不是无缝循环，播放时应关闭 Loop。
本地模型和临时输出仍在 H:/DHH_IV，未添加模型环境。
Wan 本次生成耗时约15分50秒。对应 API 工作流、提交记录、运行历史和关键帧在此文件夹。
Unity 测试副本：MaritimeLegend/Assets/Resources/Videos/Characters/Character04/Character04_05_Proud.mp4。
视频已解码检查、复制并校验哈希；未在 Unity 场景中测试。
复现脚本：MaritimeLegend/Tools/character04_proud_workflow.py、character04_proud_smile.py、review_character04_proud.py。
'''
(art/'Proud/使用说明.txt').write_text(notes,encoding='utf-8')
print('Published prototype and updated preview with 05.')
