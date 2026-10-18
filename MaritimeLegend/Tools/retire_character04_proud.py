"""Archive the rejected Unity copy and remove it from current previews."""
import json
import re
from pathlib import Path
import shutil

root = Path(__file__).resolve().parents[2]
art = root/'ArtSource/Character04B/FaceExpressions'
archive = art/'Proud/RejectedUnityCopy'
archive.mkdir(exist_ok=True)
for filename in ['Character04_05_Proud.mp4','Character04_05_Proud.mp4.meta']:
    source = root/'MaritimeLegend/Assets/Resources/Videos/Characters/Character04'/filename
    destination = archive/filename
    assert source.resolve().is_relative_to(root) and destination.resolve().is_relative_to(root)
    if source.exists():
        if destination.exists():
            raise RuntimeError('Refusing to overwrite archived file')
        shutil.move(str(source),str(destination))
manifest_path = art/'unity-published-assets.json'
manifest = json.loads(manifest_path.read_text(encoding='utf-8'))
manifest = [item for item in manifest if not item['unity_resource'].endswith('Character04_05_Proud.mp4')]
manifest_path.write_text(json.dumps(manifest,indent=2),encoding='utf-8')
preview = art/'preview.html'
html = preview.read_text(encoding='utf-8')
html = re.sub(r'<article><video src="Proud/proud-smile-v2\.mp4".*?</article>', '', html, flags=re.S)
html = html.replace('05：Wan 身体动作 + AdvancedLivePortrait 笑容 · 448 × 480 · 16 fps · 约 4 秒。','05 旧版已撤下，正在重做。')
html = html.replace('；05 不循环','').replace('；05：65 帧 / 约 4 秒','')
html = html.replace('05 的手臂和衣物由视频模型生成，再逐帧跟踪脸部添加笑容。','')
preview.write_text(html,encoding='utf-8')
(art/'Proud/REJECTED.txt').write_text('旧版 proud-v1 / proud-smile-v2 已被用户否定：模糊、抖动、帧间不连贯、手指结构错误。仅保留历史记录，禁止作为游戏正式资源发布。Unity 副本及其 meta 已移入 RejectedUnityCopy。下一版采用高清目标姿势与可控关键帧方案。',encoding='utf-8')
print('Rejected 05 archived; current preview and manifest updated.')
