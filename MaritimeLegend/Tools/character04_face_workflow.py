"""Build and submit half-body analysis and controlled-expression ComfyUI graphs."""
import argparse
import json
from pathlib import Path
import urllib.request

parser = argparse.ArgumentParser()
parser.add_argument('action', choices=['analysis', 'blink', 'smile', 'nod'])
parser.add_argument('--animate', action='store_true')
parser.add_argument('--amount', type=float)
args = parser.parse_args()
out = Path(__file__).resolve().parents[2] / 'ArtSource' / 'Character04B' / 'FaceExpressions'
out.mkdir(parents=True, exist_ok=True)

def node(kind, **inputs):
    return {'class_type': kind, 'inputs': inputs}

graph = {
    '1': node('LoadImage', image='B-animation-source.png'),
    '2': node('ImageCrop', image=['1', 0], width=704, height=768, x=160, y=0),
    '3': node('Character04FaceAnalysis', image=['2', 0], crop_factor=1.7),
    '4': node('SaveImage', images=['2', 0], filename_prefix='Character04B-Face/half-body-source'),
    '5': node('SaveImage', images=['3', 1], filename_prefix='Character04B-Face/face-detected'),
    '6': node('MaskToImage', mask=['3', 2]),
    '7': node('SaveImage', images=['6', 0], filename_prefix='Character04B-Face/face-edit-mask'),
}
if args.action != 'analysis':
    controls = dict(rotate_pitch=0.0, rotate_yaw=0.0, rotate_roll=0.0, blink=0.0, eyebrow=0.0, wink=0.0, pupil_x=0.0, pupil_y=0.0, aaa=0.0, eee=0.0, woo=0.0, smile=0.0, src_ratio=1.0, sample_ratio=1.0, sample_parts='OnlyExpression', crop_factor=1.7, motion_link=['3', 0])
    control, amount = {'blink': ('blink', -15.0), 'smile': ('smile', 0.7), 'nod': ('rotate_pitch', 6.0)}[args.action]
    controls[control] = args.amount if args.amount is not None else amount
    graph['10'] = node('ExpressionEditor', **controls)
    graph['11'] = node('SaveImage', images=['10', 0], filename_prefix=f'Character04B-Face/{args.action}-target')
    if args.animate:
        command = '0=1:12\n1=4:2\n0=6:23' if args.action == 'blink' else '0=1:8\n1=12:12\n0=12:3'
        graph['12'] = node('AdvancedLivePortrait', retargeting_eyes=0.0, retargeting_mouth=0.0, crop_factor=1.7, turn_on=True, tracking_src_vid=False, animate_without_vid=True, command=command, motion_link=['10', 1])
        graph['13'] = node('CreateVideo', images=['12', 0], fps=24.0)
        graph['14'] = node('SaveVideo', video=['13', 0], filename_prefix=f'Character04B-Face/{args.action}-controlled', format='mp4', codec={'codec': 'h264'})
        # Lossless frames allow checking actual background/clothing immobility before video compression.
        graph['15'] = node('SaveImage', images=['12', 0], filename_prefix=f'Character04B-Face/frames/{args.action}')
tag = args.action + ('-animation' if args.animate else '-analysis')
(out / f'{tag}-api.json').write_text(json.dumps(graph, indent=2), encoding='utf-8')
request = urllib.request.Request('http://127.0.0.1:8188/prompt', data=json.dumps({'prompt': graph}).encode(), headers={'Content-Type': 'application/json'})
try:
    with urllib.request.urlopen(request, timeout=60) as response:
        result = json.load(response)
except urllib.error.HTTPError as error:
    print(error.read().decode())
    raise
(out / f'{tag}-submission.json').write_text(json.dumps(result, indent=2), encoding='utf-8')
print(json.dumps(result))
