"""Track the generated gesture and add a gradual smile with AdvancedLivePortrait."""
import json
from pathlib import Path
import shutil
import urllib.request

root = Path(__file__).resolve().parents[2]
art = root/'ArtSource/Character04B/FaceExpressions'
trial = art/'Proud'
shutil.copy2(trial/'proud-v1.mp4','H:/DHH_IV/ComfyUI/input/Character04-proud-v1.mp4')
graph = json.loads((art/'smile-animation-api.json').read_text(encoding='utf-8'))
graph['30'] = {'class_type':'LoadVideo','inputs':{'file':'Character04-proud-v1.mp4'}}
graph['31'] = {'class_type':'GetVideoComponents','inputs':{'video':['30',0]}}
graph['10']['inputs']['smile'] = 0.85
graph['12']['inputs'].update(src_images=['31',0],tracking_src_vid=True,animate_without_vid=False,command='0=1:3\n1=20:41')
graph['13']['inputs']['fps'] = 16.0
graph['14']['inputs']['filename_prefix'] = 'Character04B-Face/proud-smile-v2'
graph['15']['inputs']['filename_prefix'] = 'Character04B-Face/proud-frames/proud-smile-v2'
(trial/'proud-smile-v2-api.json').write_text(json.dumps(graph,indent=2),encoding='utf-8')
for endpoint, data in [('free',{'unload_models':True,'free_memory':True}),('prompt',{'prompt':graph})]:
    req = urllib.request.Request('http://127.0.0.1:8188/'+endpoint,data=json.dumps(data).encode(),headers={'Content-Type':'application/json'})
    with urllib.request.urlopen(req,timeout=60) as response:
        payload = response.read()
    if endpoint == 'prompt':
        result = json.loads(payload)
        (trial/'proud-smile-v2-submission.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
        print(json.dumps(result))
