"""Compose a restrained character-only breathing offset and encode the dialogue loop."""
import json
import argparse
import math
from pathlib import Path
import urllib.request
import av
import cv2
import numpy as np
from PIL import Image,ImageDraw

parser=argparse.ArgumentParser()
parser.add_argument('--version',default='v2')
args=parser.parse_args()
tag='dialogue-'+args.version
root=Path(__file__).resolve().parents[2]
art=root/'ArtSource/Character04B/DialogueIdle'
pid=json.loads((art/f'{tag}-submission.json').read_text())['prompt_id']
with urllib.request.urlopen(f'http://127.0.0.1:8188/history/{pid}',timeout=30) as response:
    history=json.load(response)
if pid not in history:
    print('Still running')
    raise SystemExit(0)
record=history[pid]
(art/f'{tag}-history.json').write_text(json.dumps(record,indent=2),encoding='utf-8')
assert record['status']['status_str']=='success',record['status']
paths=[Path('H:/DHH_IV/ComfyUI/output')/item['subfolder']/item['filename'] for item in record['outputs']['500']['images']]
assert len(paths)==120
frames=[np.asarray(Image.open(p).convert('RGB')) for p in paths]
h,w=frames[0].shape[:2]
yy,xx=np.mgrid[:h,:w].astype(np.float32)
silhouette=np.zeros((h,w),np.float32)
outline=[(357,15),(416,12),(461,38),(483,79),(501,142),(487,194),(449,220),(474,258),(519,303),(548,365),(589,450),(623,530),(650,626),(662,768),(70,768),(72,653),(82,577),(69,499),(81,440),(106,376),(152,320),(213,266),(274,245),(276,222),(240,208),(219,174),(221,118),(243,72),(291,39)]
cv2.fillPoly(silhouette,[np.array(outline,np.int32)],1)
silhouette=cv2.GaussianBlur(silhouette,(0,0),8)
# Fade the displacement to zero at the bottom edge, keeping the waist anchored.
weight=silhouette*np.clip((h-yy)/240,0,1)
source=np.asarray(Image.open(art.parent/'FaceExpressions/half-body-source.png').convert('RGB'))
body_diff=max(int(np.abs(f[320:].astype(int)-source[320:].astype(int)).max()) for f in frames)
assert body_diff==0
encoded=[]
selected=[0,12,18,30,48,66,86,94,119]
sheet=Image.new('RGB',(3*352,3*404),(30,30,30))
draw=ImageDraw.Draw(sheet)
with av.open(str(art/f'dialogue-idle-{args.version}.mp4'),'w') as video:
    stream=video.add_stream('libx264',rate=24)
    stream.width=w; stream.height=h; stream.pix_fmt='yuv420p'
    stream.options={'crf':'14','preset':'slow'}
    for i,frame in enumerate(frames):
        phase=2*math.pi*i/119
        dy=-2.6*(1-math.cos(phase))/2
        dx=.45*math.sin(phase)
        out=cv2.remap(frame,(xx-weight*dx).astype(np.float32),(yy-weight*dy).astype(np.float32),cv2.INTER_CUBIC,borderMode=cv2.BORDER_REFLECT_101)
        encoded.append(out)
        if i in selected:
            Image.fromarray(out).save(art/f'{tag}-frame-{i:03}.png')
            j=selected.index(i); x=j%3*352; y=j//3*404
            sheet.paste(Image.fromarray(out).resize((352,384)),(x,y))
            draw.text((x+5,y+384),f'{i/24:.2f}s',fill='white')
        for packet in stream.encode(av.VideoFrame.from_ndarray(out,format='rgb24')):video.mux(packet)
    for packet in stream.encode():video.mux(packet)
sheet.save(art/f'{tag}-contact.jpg',quality=95)
endpoint=int(np.abs(encoded[0].astype(int)-encoded[-1].astype(int)).max())
background=max(int(np.abs(f[:,:50].astype(int)-encoded[0][:,:50].astype(int)).max()) for f in encoded)
assert endpoint==0 and background==0
with av.open(str(art/f'dialogue-idle-{args.version}.mp4')) as check:
    assert sum(1 for _ in check.decode(video=0))==120
report={'width':w,'height':h,'frames':120,'fps':24,'duration_seconds':5,'face_stage_body_max_difference':body_diff,'first_last_max_difference':endpoint,'far_left_background_max_difference':background,'body_vertical_max_px':2.6,'body_horizontal_max_px':.45,'blinks_seconds':[.75,3.92],'audio':'none; speech-like mouth motion, not audio-aligned lip sync','method':'fixed source + explicit face controls + low-amplitude body displacement; no video diffusion'}
(art/f'{tag}-validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
