"""Render the ComfyUI sleeve node directly against original lossless blink frames."""
import json
import sys
import hashlib
import shutil
from pathlib import Path
import av
import numpy as np
from PIL import Image

sys.path.insert(0, 'H:/DHH_IV/ComfyUI')
import folder_paths
folder_paths.set_output_directory('H:/DHH_IV/ComfyUI/output')
from character04_expression_analysis import Character04OriginalBlink, Character04SleeveWind

root = Path(__file__).resolve().parents[2]
art = root/'ArtSource/Character04B/FaceExpressions'
original = Character04OriginalBlink().load()[0]
result, mask = Character04SleeveWind().animate(original, 1.0, 7.0)
frames = (result.numpy()*255).round().clip(0,255).astype(np.uint8)
before = (original.numpy()*255).round().astype(np.uint8)
diff = np.abs(frames.astype(np.int16)-before.astype(np.int16))
report = {'frames':len(frames), 'fps':24,
          'shoulders_max_difference':int(diff[:,:450].max()),
          'hand_max_difference':int(diff[:,525:580,275:390].max()),
          'compass_max_difference':int(diff[:,590:669,298:387].max()),
          'left_cuff_change':int(diff[:,480:530,150:245].max()),
          'right_cuff_change':int(diff[:,550:625,582:626].max()),
          'first_last_difference':int(np.abs(frames[0].astype(int)-frames[-1]).max()),
          'method':'Local 2D white sleeve cuff deformation; shoulders fixed; lower cape and pendant retained.'}
for key in ['shoulders_max_difference','hand_max_difference','compass_max_difference','first_last_difference']:
    assert report[key] == 0, (key,report[key])
assert report['left_cuff_change'] > 0 and report['right_cuff_change'] > 0
video = art/'blink-sleeves-v3.mp4'
with av.open(str(video),'w') as output:
    stream = output.add_stream('libx264',rate=24)
    stream.width,stream.height,stream.pix_fmt = 704,768,'yuv420p'
    stream.options = {'crf':'18'}
    for frame in frames:
        for packet in stream.encode(av.VideoFrame.from_ndarray(frame,format='rgb24')):
            output.mux(packet)
    for packet in stream.encode():
        output.mux(packet)
with av.open(str(video)) as check:
    assert sum(1 for _ in check.decode(video=0)) == 48
for index in [0,12,35,47]:
    Image.fromarray(frames[index]).save(art/f'sleeves-frame-{index:02}.png')
Image.fromarray((mask[0].numpy()*255).astype(np.uint8)).save(art/'sleeves-motion-mask.png')
(art/'sleeves-validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
workflow = json.loads((art/'blink-wind-v2-api.json').read_text(encoding='utf-8'))
workflow['2']['class_type'] = 'Character04SleeveWind'
for node in workflow.values():
    inputs = node['inputs']
    if 'filename_prefix' in inputs:
        inputs['filename_prefix'] = inputs['filename_prefix'].replace('blink-wind-v2','blink-sleeves-v3').replace('cape-pendant-motion-mask','sleeves-motion-mask')
(art/'blink-sleeves-v3-api.json').write_text(json.dumps(workflow,indent=2),encoding='utf-8')
target = root/'MaritimeLegend/Assets/Resources/Videos/Characters/Character04/Character04_01_Idle.mp4'
shutil.copy2(video,target)
digest = hashlib.sha256(video.read_bytes()).hexdigest()
assert hashlib.sha256(target.read_bytes()).hexdigest() == digest
manifest_path = art/'unity-published-assets.json'
manifest = json.loads(manifest_path.read_text(encoding='utf-8'))
for item in manifest:
    if item['unity_resource'].endswith('Character04_01_Idle.mp4'):
        item['source'] = str(video.relative_to(root))
        item['sha256'] = digest
manifest_path.write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
