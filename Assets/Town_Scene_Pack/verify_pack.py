from pathlib import Path
import json, re, shutil, subprocess
from PIL import Image
P=Path(__file__).resolve().parent
M=json.loads((P/'asset_manifest.json').read_text(encoding='utf-8'))
C=json.loads((P/'scene_config.json').read_text(encoding='utf-8'))
assert len(M['menus'])==9
assert len(M['ui'])==15
assert len(M['npc'])==5
assert all(len(n['frames'])==32 for n in M['npc'])
files=[a['file'] for k in ['menus','ui','npc'] for a in M[k]]+[M['birds']['file']]+[a['file'] for a in C['layers']]
assert all((P/f).exists() for f in files)
assert all(Image.open(P/n['file']).size==(512,1536) for n in M['npc'])
assert Image.open(P/M['birds']['file']).size==(1024,512)
for file in files:
    im=Image.open(P/file)
    if file.endswith('scene_clean.png'):continue
    assert im.mode=='RGBA',(file,im.mode)
    assert im.getchannel('A').getextrema()[0]==0,file
    assert im.getchannel('A').getextrema()[1]>=250,file
for n in M['npc']:
    for frame in n['frames']:
        x,y,w,h=frame['rect']
        assert 0<=x<x+w<=512 and 0<=y<y+h<=1536
html=(P/'preview.html').read_text(encoding='utf-8')
refs=re.findall(r'(?:src|href)="([^"]+)"',html)
missing=[r for r in refs if not any(s in r for s in ['#','{','http']) and not (P/r).exists()]
assert not missing,missing
node=shutil.which('node')
js=re.findall(r'<script>(.*?)</script>',html,re.S)[0]
if node:
    check=P/'preview_script_check.js';check.write_text(js,encoding='utf-8')
    subprocess.run([node,'--check',str(check)],check=True)
    subprocess.run([node,'--check',str(P/'preview_data.js')],check=True)
    check.unlink()
result={'menus':9,'ui':15,'npc_characters':5,'npc_frames':160,'seagull_frames':8,'png_files':len(list(P.rglob('*.png'))),'all_references_exist':True,'rgba_transparency_verified':True,'javascript_syntax_checked':bool(node),'browser_interaction_test':'not completed: local page unavailable through browser tool'}
(P/'validation_summary.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(result,ensure_ascii=False,indent=2))
