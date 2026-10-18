"""Generate and publish single/double nods through local AdvancedLivePortrait."""
import copy
import hashlib
import json
from pathlib import Path
import shutil
import time
import urllib.request
import av
import numpy as np
from PIL import Image

root = Path(__file__).resolve().parents[2]
art = root/'ArtSource/Character04B/FaceExpressions'
output = Path('H:/DHH_IV/ComfyUI/output')
base = json.loads((art/'nod-animation-api.json').read_text(encoding='utf-8'))
variants = [
    ('nod-single-v2', '0=1:5\n1=10:0\n0=12:20', 'Character04_03_Nod.mp4'),
    ('nod-double-v1', '0=1:5\n1=10:0\n2=4:0\n1=4:0\n0=12:12', 'Character04_04_DoubleNod.mp4'),
]
reports = {}
manifest_path = art/'unity-published-assets.json'
manifest = json.loads(manifest_path.read_text(encoding='utf-8'))
for name, command, unity_name in variants:
    graph = copy.deepcopy(base)
    graph['12']['inputs']['command'] = command
    if 'double' in name:
        graph['20'] = copy.deepcopy(graph['10'])
        graph['20']['inputs']['rotate_pitch'] = 4.0
        graph['20']['inputs']['motion_link'] = ['10',1]
        graph['12']['inputs']['motion_link'] = ['20',1]
    graph['14']['inputs']['filename_prefix'] = f'Character04B-Face/{name}'
    graph['15']['inputs']['filename_prefix'] = f'Character04B-Face/frames/{name}'
    (art/f'{name}-api.json').write_text(json.dumps(graph,indent=2),encoding='utf-8')
    request = urllib.request.Request('http://127.0.0.1:8188/prompt',data=json.dumps({'prompt':graph}).encode(),headers={'Content-Type':'application/json'})
    with urllib.request.urlopen(request,timeout=60) as response:
        submitted = json.load(response)
    (art/f'{name}-submission.json').write_text(json.dumps(submitted,indent=2),encoding='utf-8')
    prompt_id = submitted['prompt_id']
    print(name, prompt_id, flush=True)
    deadline = time.monotonic()+600
    while time.monotonic() < deadline:
        with urllib.request.urlopen(f'http://127.0.0.1:8188/history/{prompt_id}',timeout=30) as response:
            history = json.load(response)
        if prompt_id in history:
            record = history[prompt_id]
            if record['status']['status_str'] == 'error':
                raise RuntimeError(record['status'])
            if record['status']['completed']:
                break
        time.sleep(2)
    else:
        raise TimeoutError(prompt_id)
    (art/f'{name}-history.json').write_text(json.dumps(record,indent=2),encoding='utf-8')
    paths = [output/item['subfolder']/item['filename'] for item in record['outputs']['15']['images']]
    frames = np.stack([np.asarray(Image.open(p).convert('RGB'),dtype=np.int16) for p in paths])
    assert len(frames) == 48
    static_difference = int(np.abs(frames[:,320:]-frames[0,320:]).max())
    endpoint_difference = int(np.abs(frames[0]-frames[-1]).max())
    assert static_difference == 0 and endpoint_difference == 0
    # Peak frame 15 is immediately followed by recovery; double nod peaks again at 23.
    assert int(np.abs(frames[16]-frames[15]).max()) > 0
    if 'double' in name:
        assert int(np.abs(frames[24]-frames[23]).max()) > 0
    video = sorted((output/'Character04B-Face').glob(f'{name}_*.mp4'))[-1]
    with av.open(str(video)) as container:
        assert sum(1 for _ in container.decode(video=0)) == 48
    shutil.copy2(video,art/f'{name}.mp4')
    for index in ([0,15,19,23,35] if 'double' in name else [0,15,16,27]):
        Image.fromarray(frames[index].astype(np.uint8)).save(art/f'{name}-frame-{index:02}.png')
    target = root/'MaritimeLegend/Assets/Resources/Videos/Characters/Character04'/unity_name
    shutil.copy2(video,target)
    digest = hashlib.sha256(target.read_bytes()).hexdigest()
    assert digest == hashlib.sha256((art/f'{name}.mp4').read_bytes()).hexdigest()
    relative = f'Videos/Characters/Character04/{unity_name}'
    manifest = [item for item in manifest if item['unity_resource'] != relative]
    manifest.append({'source':str((art/f'{name}.mp4').relative_to(root)), 'unity_resource':relative, 'sha256':digest})
    reports[name] = {'frames':48,'fps':24,'command':command,'pitch_targets_degrees':[0,6,4,6,0] if 'double' in name else [0,6,0], 'peak_hold_frames':0,'lower_body_max_difference':static_difference,'first_last_max_difference':endpoint_difference}
    print(name, 'validated and published', flush=True)
manifest_path.write_text(json.dumps(manifest,indent=2),encoding='utf-8')
(art/'nod-variants-validation.json').write_text(json.dumps(reports,indent=2),encoding='utf-8')
