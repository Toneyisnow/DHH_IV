"""Local Wan half-body proud action trial. Review before publishing."""
import copy
import json
from pathlib import Path
import urllib.request

root = Path(__file__).resolve().parents[2]
art = root/'ArtSource/Character04B/FaceExpressions'
trial = art/'Proud'
trial.mkdir(exist_ok=True)
base = json.loads((art.parent/'idle-384x576-49f-api.json').read_text(encoding='utf-8'))
graph = copy.deepcopy(base)
graph['60'] = {'class_type':'ImageCrop','inputs':{'image':['56',0],'width':704,'height':768,'x':160,'y':0}}
graph['55']['inputs'].update(width=448,height=480,length=65,start_image=['60',0])
graph['6']['inputs']['text'] = (
    'One continuous medium shot, locked stationary camera. The same illustrated female ship captain '
    'performs a clear proud hands-on-hips gesture. At first her hands are in the reference pose. '
    'She lifts and moves both forearms, places both hands firmly on the sides of her waist, '
    'fingers resting on her hips, and opens both elbows outward. Her right hand visibly rises '
    'from the lower edge to her right hip. Her left hand slides from the front belt to her left hip. '
    'Both hands finish planted on her hips in an akimbo pose. She straightens her upper body '
    'and raises her chin slightly with a confident warm closed-mouth smile, corners of lips lifting. '
    'The loose ivory sleeves fold and flutter naturally as her arms move. The red cape gently '
    'swings in response to the gesture and settles. Hold the completed proud pose at the end. '
    'Preserve short black bob haircut, adult female facial identity, red cape, gold trim, '
    'ivory sleeves, black waistcoat, belt and hanging compass. Japanese tactical RPG cel-painted '
    'illustration, crisp hand-painted linework, flat warm gray background, fixed framing.'
)
graph['7']['inputs']['text'] = (
    'camera motion, zoom, pan, camera shake, scene cut, walking, turning around, '
    'waving, crossed arms, clenched fists, extra arms, extra hands, missing fingers, fused fingers, '
    'deformed hands, duplicate person, distorted anatomy, changing outfit, disappearing compass, '
    'flickering, melting clothes, blurry face, photo, 3d render, speech, open mouth, text, watermark'
)
graph['3']['inputs'].update(seed=4200501,steps=24,cfg=5.5)
graph['57']['inputs']['fps'] = 16.0
graph['58']['inputs']['filename_prefix'] = 'Character04B-Face/proud-v1'
graph['61'] = {'class_type':'SaveImage','inputs':{'images':['8',0],'filename_prefix':'Character04B-Face/proud-frames/proud-v1'}}
(trial/'proud-v1-api.json').write_text(json.dumps(graph,indent=2),encoding='utf-8')
req = urllib.request.Request('http://127.0.0.1:8188/prompt',data=json.dumps({'prompt':graph}).encode(),headers={'Content-Type':'application/json'})
with urllib.request.urlopen(req,timeout=60) as response:
    result = json.load(response)
(trial/'proud-v1-submission.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
print(json.dumps(result))
