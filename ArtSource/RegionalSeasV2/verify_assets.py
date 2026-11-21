"""Content regression checks for the published regional art assets."""
from pathlib import Path
import json,hashlib,re,uuid
import numpy as np
ROOT=Path(__file__).resolve().parents[2];HERE=Path(__file__).resolve().parent
RES=ROOT/'MaritimeLegend/Assets/Resources'
records=json.loads((HERE/'Review/generation.json').read_text())
checks=[]
baseline=RES/'Models/Environment/SoutheastAsiaV2'
generated=RES/'Models/Environment/RegionalSeasV2/southeast_asia'
meshes=list(baseline.glob('Terrain-*.asset'));assert len(meshes)==360
for p in meshes:assert p.read_bytes()==(generated/p.name).read_bytes(),p.name
checks.append('SEA: 360/360 near/far terrain meshes byte-identical to approved baseline')
assert (baseline/'terrain.bytes').read_bytes()==(generated/'terrain.bytes').read_bytes()
checks.append('SEA: height/coast/forest field byte-identical to approved baseline')
for record in records:
    region=record['id'];folder=RES/'Models/Environment/RegionalSeasV2'/region
    assert hashlib.sha256((folder/'terrain.bytes').read_bytes()).hexdigest()==record['terrain_sha256']
    coast=np.frombuffer((folder/'coast-distance.bytes').read_bytes(),dtype='<f4',offset=8).reshape(941,1672)
    assert np.isfinite(coast).all()
    x,y=record['review_uv']
    for dx,dy in [(0,0),(-.01,.018),(.01,.036)]:assert coast[round((y+dy)*941),round((x+dx)*1672)]>9
    prefab=RES/'Environment/RegionalSeasV2'/region/'Runtime.prefab';assert prefab.exists()
    water=(RES/'Materials/RegionalSeasV2'/region/'Water.mat').read_text()
    for name,value in [('_WaveLength','26'),('_WaveStrength','0.3'),('_WaveSpeed','0.65')]:
        assert re.search(r'- '+name+r': '+re.escape(value)+r'\s',water), (region,name)
    assert 'WorldSea' not in water
checks.append('7 regions: valid data, safe three-ship review anchorages, original water parameters')
assert len(list((HERE/'Review').glob('*-Sunny.png')))==7
for sky in ['Sunny','Sunset','Night']:
    for r in records:assert (HERE/'Review'/f"{r['id']}-{sky}.png").stat().st_size>100000
checks.append('21 full-resolution player screenshots present')
# Data-only manifest is new; deterministic metadata is kept after first creation.
manifest=RES/'Data/RegionalSeasV2.json';meta=Path(str(manifest)+'.meta')
if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nTextScriptImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n')
(HERE/'Review/asset-checks.txt').write_text('\n'.join('PASS '+c for c in checks)+'\n')
print('\n'.join(checks))
