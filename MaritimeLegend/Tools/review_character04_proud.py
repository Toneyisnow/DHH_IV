"""Collect the local proud action trial without promoting unreviewed art."""
import json
import argparse
from pathlib import Path
import shutil
import urllib.request
import av
from PIL import Image, ImageDraw

root = Path(__file__).resolve().parents[2]
art = root/'ArtSource/Character04B/FaceExpressions/Proud'
parser = argparse.ArgumentParser()
parser.add_argument('--name', default='proud-v1')
parser.add_argument('--frames-node', default='61')
args = parser.parse_args()
name = args.name
submission = json.loads((art/f'{name}-submission.json').read_text())
pid = submission['prompt_id']
with urllib.request.urlopen(f'http://127.0.0.1:8188/history/{pid}',timeout=30) as response:
    history = json.load(response)
if pid not in history:
    print('Still running')
    raise SystemExit(0)
record = history[pid]
(art/f'{name}-history.json').write_text(json.dumps(record,indent=2),encoding='utf-8')
if record['status']['status_str'] == 'error':
    print(json.dumps(record['status']))
    raise SystemExit(1)
output = Path('H:/DHH_IV/ComfyUI/output')
paths = [output/item['subfolder']/item['filename'] for item in record['outputs'][args.frames_node]['images']]
video = sorted((output/'Character04B-Face').glob(f'{name}_*.mp4'))[-1]
shutil.copy2(video,art/f'{name}.mp4')
with av.open(str(video)) as container:
    frames = list(container.decode(video=0))
assert len(frames) == 65
sheet = Image.new('RGB',(448*3,504*3),'#282828')
draw = ImageDraw.Draw(sheet)
for slot,index in enumerate([0,8,16,24,32,40,48,56,64]):
    frame = Image.open(paths[index]).convert('RGB')
    frame.save(art/f'{name}-frame-{index:02}.png')
    x,y = slot%3*448,slot//3*504
    sheet.paste(frame,(x,y))
    draw.text((x+8,y+482),f'{index:02} / {index/16:.1f}s',fill='white')
sheet.save(art/f'{name}-contact-sheet.jpg',quality=93)
print('Collected 65 frames, 448x480, 16fps. Visual review required.')
