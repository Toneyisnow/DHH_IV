"""Reference-inspired dialogue portrait: controlled speech, blinks and head motion."""
import json
import argparse
import math
from pathlib import Path
import urllib.request

parser=argparse.ArgumentParser()
parser.add_argument('--version',default='v2')
parser.add_argument('--mouth-scale',type=float,default=1.7)
args=parser.parse_args()
tag='dialogue-'+args.version
root=Path(__file__).resolve().parents[2]
art=root/'ArtSource/Character04B/DialogueIdle'
base=json.loads((art.parent/'FaceExpressions/smile-animation-api.json').read_text(encoding='utf-8'))
graph={k:v for k,v in base.items() if int(k)<10}
template=base['10']['inputs'].copy()
timeline=[]
last=None
for i in range(120):
    t=i/24
    phase=2*math.pi*i/119
    mouth=sum(a*math.exp(-((t-c)/width)**2) for c,a,width in [(.35,12,.09),(.63,18,.10),(.94,10,.10),(1.25,17,.12),(1.57,13,.10),(1.90,16,.12),(2.24,10,.11),(2.55,18,.10),(2.91,13,.11),(3.23,16,.13),(3.57,10,.10)])
    mouth*=args.mouth_scale
    blink=-15*sum(math.exp(-((t-c)/.065)**2) for c in [.75,3.92])
    # Mouth control also alters pitch internally. Cancel that coupling so the
    # independent low-frequency head motion does not bob at every syllable.
    pitch=.65*math.sin(phase)+.05*mouth
    yaw=.8*math.sin(phase)
    roll=.3*math.sin(2*phase)
    inputs=template.copy()
    inputs.update(aaa=mouth,eee=.16*mouth*max(0,math.sin(t*8)),woo=.19*mouth*max(0,math.cos(t*6)),blink=blink,smile=0.08*(1-math.cos(phase))/2,rotate_pitch=pitch,rotate_yaw=yaw,rotate_roll=roll,motion_link=['3',0])
    key=str(100+i)
    graph[key]={'class_type':'ExpressionEditor','inputs':inputs}
    if last is None:
        last=[key,0]
    else:
        batchkey=str(300+i)
        graph[batchkey]={'class_type':'ImageBatch','inputs':{'image1':last,'image2':[key,0]}}
        last=[batchkey,0]
    timeline.append({'frame':i,'time':t,'mouth_aaa':mouth,'blink':blink,'head_pitch':pitch-.05*mouth,'head_yaw':yaw,'head_roll':roll})
graph['500']={'class_type':'SaveImage','inputs':{'images':last,'filename_prefix':'Character04B-Dialogue/face-frames/'+tag}}
(art/f'{tag}-api.json').write_text(json.dumps(graph,indent=2),encoding='utf-8')
(art/f'{tag}-timeline.json').write_text(json.dumps(timeline,indent=2),encoding='utf-8')
request=urllib.request.Request('http://127.0.0.1:8188/prompt',data=json.dumps({'prompt':graph}).encode(),headers={'Content-Type':'application/json'})
with urllib.request.urlopen(request,timeout=60) as response:
    result=json.load(response)
(art/f'{tag}-submission.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
print(json.dumps(result))
