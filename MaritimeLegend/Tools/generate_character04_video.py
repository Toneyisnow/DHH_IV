"""Create a reproducible B portrait workflow; optionally queue it on local ComfyUI."""
import argparse
import json
from pathlib import Path
import shutil
import urllib.request

parser = argparse.ArgumentParser()
parser.add_argument('motion', choices=['idle', 'nod', 'smile'])
parser.add_argument('--width', type=int, default=384)
parser.add_argument('--height', type=int, default=576)
parser.add_argument('--frames', type=int, default=49)
parser.add_argument('--steps', type=int, default=20)
parser.add_argument('--seed', type=int, default=42004)
parser.add_argument('--weight-dtype', choices=['default', 'fp8_e4m3fn'], default='fp8_e4m3fn')
parser.add_argument('--submit', action='store_true')
args = parser.parse_args()
source = Path(__file__).resolve().parents[2] / 'ArtSource' / 'Character04B'
comfy = Path('H:/DHH_IV/ComfyUI')
prompts = json.loads((source / 'motion-prompts.json').read_text(encoding='utf-8'))
positive = prompts['style'] + ' ' + prompts['motions'][args.motion]
prefix = f'Character04B/{args.motion}-{args.width}x{args.height}-{args.frames}f-s{args.seed}'

def node(kind, **inputs):
    return {'class_type': kind, 'inputs': inputs}

graph = {
    '37': node('UNETLoader', unet_name='wan2.2_ti2v_5B_fp16.safetensors', weight_dtype=args.weight_dtype),
    '38': node('CLIPLoader', clip_name='umt5_xxl_fp8_e4m3fn_scaled.safetensors', type='wan', device='default'),
    '39': node('VAELoader', vae_name='wan2.2_vae.safetensors'),
    '56': node('LoadImage', image='B-animation-source.png'),
    '55': node('Wan22ImageToVideoLatent', vae=['39', 0], width=args.width, height=args.height, length=args.frames, batch_size=1, start_image=['56', 0]),
    '6': node('CLIPTextEncode', clip=['38', 0], text=positive),
    '7': node('CLIPTextEncode', clip=['38', 0], text=prompts['negative']),
    '48': node('ModelSamplingSD3', model=['37', 0], shift=8.0),
    '3': node('KSampler', model=['48', 0], positive=['6', 0], negative=['7', 0], latent_image=['55', 0], seed=args.seed, steps=args.steps, cfg=5.0, sampler_name='uni_pc', scheduler='simple', denoise=1.0),
    '8': node('VAEDecode', samples=['3', 0], vae=['39', 0]),
    '57': node('CreateVideo', images=['8', 0], fps=24.0),
    '58': node('SaveVideo', video=['57', 0], filename_prefix=prefix, format='mp4', codec={'codec': 'h264'}),
}
workflow = json.loads((source / 'wan5b-official-workflow.json').read_text(encoding='utf-8'))
values = {
    37: ['wan2.2_ti2v_5B_fp16.safetensors', args.weight_dtype],
    38: ['umt5_xxl_fp8_e4m3fn_scaled.safetensors', 'wan', 'default'],
    56: ['B-animation-source.png', 'image'],
    55: [args.width, args.height, args.frames, 1],
    6: [positive], 7: [prompts['negative']],
    3: [args.seed, 'fixed', args.steps, 5, 'uni_pc', 'simple', 1],
    58: [prefix, 'mp4', 'h264'],
}
for item in workflow['nodes']:
    if item['id'] in values:
        item['widgets_values'] = values[item['id']]
    if item['id'] == 56:
        item['mode'] = 0
tag = f'{args.motion}-{args.width}x{args.height}-{args.frames}f'
(source / f'{tag}-workflow.json').write_text(json.dumps(workflow, ensure_ascii=False, indent=2), encoding='utf-8')
(source / f'{tag}-api.json').write_text(json.dumps(graph, ensure_ascii=False, indent=2), encoding='utf-8')
if args.submit:
    shutil.copy2(source / 'B-animation-source.png', comfy / 'input' / 'B-animation-source.png')
    data = json.dumps({'prompt': graph, 'extra_data': {'extra_pnginfo': {'workflow': workflow}}}).encode('utf-8')
    request = urllib.request.Request('http://127.0.0.1:8188/prompt', data=data, headers={'Content-Type': 'application/json'})
    with urllib.request.urlopen(request, timeout=60) as response:
        result = json.load(response)
    (source / f'{tag}-submission.json').write_text(json.dumps(result, indent=2), encoding='utf-8')
    print(json.dumps(result))
else:
    print('Prepared:', tag)
