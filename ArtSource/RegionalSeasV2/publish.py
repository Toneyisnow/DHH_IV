"""Publish verified map assets only, preserving their Unity GUIDs."""
from pathlib import Path
import hashlib,json,shutil
ROOT=Path(__file__).resolve().parents[2];HERE=Path(__file__).resolve().parent
PREVIEW=HERE/'UnityPreview';TARGET=ROOT/'MaritimeLegend'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
receipt=json.loads((HERE/'Review/baseline-hashes.json').read_text())
for name,digest in receipt.items():assert sha(ROOT/name)==digest, f'Approved dependency was modified: {name}'
assert (HERE/'Review/result.txt').read_text().startswith('PASS:'), 'Player render verification required'
def copy(rel):
    src=PREVIEW/rel;dst=TARGET/rel;assert src.exists(),src;dst.parent.mkdir(parents=True,exist_ok=True)
    if src.is_dir():shutil.copytree(src,dst,dirs_exist_ok=True)
    else:shutil.copy2(src,dst)
    meta=Path(str(src)+'.meta')
    if meta.exists():shutil.copy2(meta,Path(str(dst)+'.meta'))
for category in ['Models/Environment','Materials','Textures','Environment']:
    copy(Path('Assets/Resources')/category/'RegionalSeasV2')
for rel in ['Assets/Sailing/RegionalSeaV2Review.cs.meta','Assets/Editor/RegionalSeaV2Builder.cs.meta','Assets/Scenes/RegionalSeaV2Review.unity']:copy(Path(rel))
shutil.copy2(PREVIEW/'RegionalReview/geometry.txt',HERE/'Review/geometry.txt')
records=json.loads((HERE/'Review/generation.json').read_text())
catalog={'version':1,'status':'regional_art_review','renderer':'ManualSeaV2Runtime','georeferenced':False,'streamingReady':False,'regions':[{'id':r['id'],'prefab':f"Environment/RegionalSeasV2/{r['id']}/Runtime",'terrain':f"Models/Environment/RegionalSeasV2/{r['id']}/terrain",'reviewUV':r['review_uv']} for r in records]}
out=TARGET/'Assets/Resources/Data/RegionalSeasV2.json';out.write_text(json.dumps(catalog,indent=2))
(HERE/'Review/publication.txt').write_text(f'PASS: {len(receipt)} original dependencies unchanged; seven regional assets and review scene published.\n')
print('Published 7 regional map assets; approved SEA dependencies unchanged.')
