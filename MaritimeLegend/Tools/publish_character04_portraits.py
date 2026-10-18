"""Validate local motion and copy current portrait assets to Unity Resources."""
import hashlib
import json
from pathlib import Path
import shutil
import av
import numpy as np
from PIL import Image

root = Path(__file__).resolve().parents[2]
art = root/'ArtSource/Character04B/FaceExpressions'
render = Path('H:/DHH_IV/ComfyUI/output/Character04B-Face')
original_paths = [render/'frames'/f'blink_{n:05}_.png' for n in range(1,49)]
new_paths = sorted((render/'wind-frames').glob('blink-wind-v2_*.png'))[-48:]
assert len(new_paths) == 48
original = [np.asarray(Image.open(p).convert('RGB'), dtype=np.int16) for p in original_paths]
frames = [np.asarray(Image.open(p).convert('RGB'), dtype=np.int16) for p in new_paths]
yy, xx = np.mgrid[:768,:704]
clasp = (xx-320)**2+(yy-296)**2 < 35**2
def max_diff(region):
    return max(int(np.abs(a-b)[region].max()) for a,b in zip(frames,original))
report = {
    'frame_count': len(frames), 'fps': 24,
    'face_difference_vs_original_01': max_diff(np.s_[:240,:,:]),
    'compass_disk_difference_vs_original_01': max_diff(np.s_[590:669,298:387,:]),
    'hand_difference_vs_original_01': max_diff(np.s_[510:590,246:402,:]),
    'cape_clasp_difference_vs_original_01': max_diff(clasp),
    'far_left_background_difference_vs_original_01': max_diff(np.s_[:,0:60,:]),
    'cape_change_max': max_diff(np.s_[330:440,75:260,:]),
    'pendant_change_max': max_diff(np.s_[675:727,318:354,:]),
    'first_last_max_difference': int(np.abs(frames[0]-frames[-1]).max()),
    'method': 'authored local 2D cape deformation and pendant pivot rotation; no full-body generation or 3D cloth simulation',
}
for key in ['face_difference_vs_original_01','compass_disk_difference_vs_original_01','hand_difference_vs_original_01','cape_clasp_difference_vs_original_01','far_left_background_difference_vs_original_01','first_last_max_difference']:
    assert report[key] == 0, (key, report[key])
assert report['cape_change_max'] > 0 and report['pendant_change_max'] > 0
video = sorted(render.glob('blink-wind-v2_*.mp4'))[-1]
with av.open(str(video)) as container:
    decoded = list(container.decode(video=0))
    assert len(decoded) == 48
shutil.copy2(video, art/'blink-wind-v2.mp4')
shutil.copy2(sorted(render.glob('cape-pendant-motion-mask_*.png'))[-1], art/'cape-pendant-motion-mask.png')
for index in [0,12,35,47]:
    shutil.copy2(new_paths[index], art/f'wind-frame-{index:02}.png')
(art/'wind-validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')

resources = root/'MaritimeLegend/Assets/Resources'
assets = {
    'blink-wind-v2.mp4': 'Videos/Characters/Character04/Character04_01_Idle.mp4',
    'smile.mp4': 'Videos/Characters/Character04/Character04_02_Smile.mp4',
    'nod.mp4': 'Videos/Characters/Character04/Character04_03_Nod.mp4',
    'half-body-source.png': 'Textures/Characters/Character04/Portrait.png',
}
manifest = []
for filename, relative in assets.items():
    target = resources/relative
    target.parent.mkdir(parents=True,exist_ok=True)
    shutil.copy2(art/filename,target)
    digest = hashlib.sha256(target.read_bytes()).hexdigest()
    assert digest == hashlib.sha256((art/filename).read_bytes()).hexdigest()
    manifest.append({'source': str((art/filename).relative_to(root)), 'unity_resource': relative, 'sha256': digest})
(art/'unity-published-assets.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
print('Published and hash-verified', len(manifest), 'assets.')
