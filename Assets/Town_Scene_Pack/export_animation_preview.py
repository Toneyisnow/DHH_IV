from pathlib import Path
import json, subprocess
P=Path(__file__).resolve().parent
M=json.loads((P/'asset_manifest.json').read_text(encoding='utf-8'))
args=['ffmpeg','-hide_banner','-loglevel','error','-y','-filter_complex_threads','1']
for n in M['npc']:
    args+=['-loop','1','-framerate','6','-i',str(P/n['file'])]
filters=[]
for i in range(5):
    filters.append(f"[{i}:v]format=rgba,crop=128:192:x='128*mod(n,4)':y='192*floor(mod(n,64)/8)'[n{i}]")
filters.append(''.join(f'[n{i}]' for i in range(5))+'hstack=inputs=5[out]')
args+=['-filter_complex',';'.join(filters),'-map','[out]','-frames:v','64','-c:v','libwebp_anim','-lossless','1','-loop','0',str(P/'npc_preview.webp')]
subprocess.run(args,check=True)
print('Saved npc_preview.webp: 5 characters, 8 directions, two cycles per direction.')
