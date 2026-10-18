"""Download the specific weights used by AdvancedLivePortrait, entirely to H:."""
from concurrent.futures import ThreadPoolExecutor
import hashlib
import json
from pathlib import Path
import urllib.request

root = Path('H:/DHH_IV/ComfyUI/models')
repo = 'Kijai/LivePortrait_safetensors'
with urllib.request.urlopen(f'https://huggingface.co/api/models/{repo}/tree/main') as response:
    entries = json.load(response)
files = [(f'https://huggingface.co/{repo}/resolve/main/{x["path"]}', root/'liveportrait'/x['path'], x.get('lfs', {}).get('oid')) for x in entries if x['path'].endswith('.safetensors')]
files.append(('https://huggingface.co/Bingsu/adetailer/resolve/main/face_yolov8n.pt', root/'ultralytics'/'face_yolov8n.pt', None))

def fetch(item):
    url, target, expected = item
    target.parent.mkdir(parents=True, exist_ok=True)
    if target.exists():
        print('Exists:', target.name, flush=True)
        return
    partial = target.with_suffix('.part')
    with urllib.request.urlopen(url, timeout=120) as response, partial.open('wb') as output:
        while chunk := response.read(4*1024*1024):
            output.write(chunk)
    with partial.open('rb') as source:
        digest = hashlib.file_digest(source, 'sha256').hexdigest()
    if expected and digest != expected:
        raise RuntimeError('Checksum mismatch: ' + target.name)
    partial.replace(target)
    print('Downloaded:', target.name, digest, flush=True)

with ThreadPoolExecutor(max_workers=3) as pool:
    list(pool.map(fetch, files))
