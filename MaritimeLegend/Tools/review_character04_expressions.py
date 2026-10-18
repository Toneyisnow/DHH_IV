"""Measure local-motion stability from lossless plugin output; collect artifacts."""
import json
from pathlib import Path
import shutil
import av
import numpy as np
from PIL import Image

source = Path('H:/DHH_IV/ComfyUI/output/Character04B-Face')
out = Path(__file__).resolve().parents[2] / 'ArtSource' / 'Character04B' / 'FaceExpressions'
for stem in ['half-body-source', 'face-detected', 'face-edit-mask']:
    shutil.copy2(sorted(source.glob(stem + '_*.png'))[-1], out / (stem + '.png'))
shutil.copy2(source / 'face-analysis.json', out / 'face-analysis.json')
report = {}
for action in ['blink', 'smile', 'nod']:
    video = sorted(source.glob(action + '-controlled_*.mp4'))[-1]
    shutil.copy2(video, out / (action + '.mp4'))
    shutil.copy2(sorted(source.glob(action + '-target_*.png'))[-1], out / (action + '-target.png'))
    paths = sorted((source/'frames').glob(action + '_*.png'))[-48:]
    images = [np.asarray(Image.open(p).convert('RGB'), dtype=np.int16) for p in paths]
    first = images[0]
    body_differences = [int(np.abs(frame[320:] - first[320:]).max()) for frame in images]
    face_differences = [float(np.abs(frame[55:225, 298:469] - first[55:225, 298:469]).mean()) for frame in images]
    with av.open(str(video)) as container:
        stream = container.streams.video[0]
        decoded = list(container.decode(video=0))
        metadata = dict(width=stream.width, height=stream.height, frames=len(decoded), fps=float(stream.average_rate))
    report[action] = {
        **metadata, 'lossless_frames': len(paths),
        'max_body_pixel_difference_vs_first_frame': max(body_differences),
        'body_test_region': 'all rows y >= 320, includes shoulders/cape, torso, sleeves, belt, hands, compass and background',
        'max_face_mean_absolute_difference_vs_first_frame': max(face_differences),
        'first_last_max_difference': int(np.abs(images[-1] - first).max()),
        'peak_face_change_frame': int(np.argmax(face_differences)),
    }
    for label, index in [('start', 0), ('peak', int(np.argmax(face_differences))), ('end', len(paths)-1)]:
        shutil.copy2(paths[index], out / f'{action}-{label}.png')
(out/'motion-validation.json').write_text(json.dumps(report, indent=2), encoding='utf-8')
print(json.dumps(report, indent=2))
