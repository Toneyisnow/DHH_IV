"""Add pinned local cape deformation and pendant rotation to original clip 01."""
import json
from pathlib import Path
import urllib.request

out = Path(__file__).resolve().parents[2]/'ArtSource'/'Character04B'/'FaceExpressions'
def node(kind, **inputs):
    return {'class_type': kind, 'inputs': inputs}
graph = {
    '1': node('Character04OriginalBlink'),
    '2': node('Character04CapeWind', images=['1',0], wind_strength=1.0, pendant_degrees=7.0),
    '3': node('CreateVideo', images=['2',0], fps=24.0),
    '4': node('SaveVideo', video=['3',0], filename_prefix='Character04B-Face/blink-wind-v2', format='mp4', codec={'codec':'h264'}),
    '5': node('SaveImage', images=['2',0], filename_prefix='Character04B-Face/wind-frames/blink-wind-v2'),
    '6': node('MaskToImage', mask=['2',1]),
    '7': node('SaveImage', images=['6',0], filename_prefix='Character04B-Face/cape-pendant-motion-mask'),
}
(out/'blink-wind-v2-api.json').write_text(json.dumps(graph,indent=2),encoding='utf-8')
request = urllib.request.Request('http://127.0.0.1:8188/prompt', data=json.dumps({'prompt':graph}).encode(),headers={'Content-Type':'application/json'})
with urllib.request.urlopen(request,timeout=60) as response:
    result=json.load(response)
(out/'blink-wind-v2-submission.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
print(json.dumps(result))
