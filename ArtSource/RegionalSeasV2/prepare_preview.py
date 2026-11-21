"""Isolated Unity build with original asset GUID dependency closure; no main project recovery changes."""
from pathlib import Path
import shutil,json,re,hashlib
ROOT=Path(__file__).resolve().parents[2];HERE=Path(__file__).resolve().parent
PROJECT=HERE/'UnityPreview';ASSETS=ROOT/'MaritimeLegend/Assets'
def copy(rel):
    src=ASSETS/rel;dst=PROJECT/'Assets'/rel;dst.parent.mkdir(parents=True,exist_ok=True)
    if src.is_dir():shutil.copytree(src,dst,dirs_exist_ok=True)
    else:shutil.copy2(src,dst)
    meta=Path(str(src)+'.meta')
    if meta.exists():shutil.copy2(meta,Path(str(dst)+'.meta'))
index={}
for meta in ASSETS.rglob('*.meta'):
    m=re.search(r'^guid: ([a-f0-9]{32})',meta.read_text(errors='ignore'),re.M)
    if m:index[m[1]]=Path(str(meta)[:-5])
pending=[ASSETS/'Resources/Environment/SoutheastAsiaV2/Runtime.prefab',ASSETS/'Resources/Materials/SoutheastAsiaV2/Clouds.mat'];seen=set()
while pending:
    path=pending.pop()
    if path in seen or not path.is_file():continue
    seen.add(path);copy(path.relative_to(ASSETS))
    if path.suffix in ['.prefab','.asset','.mat']:
        pending.extend(index[g] for g in re.findall(r'guid: ([a-f0-9]{32})',path.read_text(errors='ignore')) if g in index)
for folder in ['Core','Resources/Models/Environment/RegionalSeasV2']:copy(Path(folder))
for excluded in ['Simulation/WorldSession.cs','Localization/LocalizationService.cs']:
    p=PROJECT/'Assets/Core'/excluded
    if p.exists():p.unlink()
for path in ASSETS.glob('Sailing/ManualSeaV2*.cs'):copy(path.relative_to(ASSETS))
for name in ['RegionalSeaV2Review','ManualSeaPortCatalog','CoastalCityMesh']:
    paths=list(ASSETS.rglob(name+'.cs'));assert len(paths)==1,(name,paths);copy(paths[0].relative_to(ASSETS))
for name in ['ManualSeaV2Study','SoutheastAsiaV2Study','RegionalSeaV2Builder']:copy(Path('Editor')/(name+'.cs'))
copy(Path('Sailing/CoastNavigation.cs'))
instruments=(ASSETS/'Sailing/SailingInstruments.cs').read_text(encoding='utf-8')
math=instruments[instruments.index(' public static class SeaInstrumentMath'):instruments.index(' public partial class MaritimeGame')]
(PROJECT/'Assets/SeaInstrumentMath.cs').write_text('using UnityEngine; using MaritimeLegend.Domain; namespace MaritimeLegend {\n'+math+'}\n')
for path in (ASSETS/'Resources/Shaders').glob('ManualSeaV2*'):copy(path.relative_to(ASSETS))
for folder in ['junk','square','lateen']:copy(Path('Resources/Sprites/ManualSailing')/folder)
(PROJECT/'Assets/PreviewUiStub.cs').write_text('namespace MaritimeLegend { public static class LiveGui { public static UnityEngine.Camera Camera => null; } }\n')
(PROJECT/'Packages').mkdir(exist_ok=True)
(PROJECT/'Packages/manifest.json').write_text(json.dumps({'dependencies':{f'com.unity.modules.{m}':'1.0.0' for m in ['imgui','jsonserialize','physics','imageconversion']}}))
(PROJECT/'ProjectSettings').mkdir(exist_ok=True)
for name in ['ProjectVersion.txt','QualitySettings.asset','GraphicsSettings.asset']:
    shutil.copy2(ROOT/'MaritimeLegend/ProjectSettings'/name,PROJECT/'ProjectSettings'/name)
settings=(ROOT/'MaritimeLegend/ProjectSettings/ProjectSettings.asset').read_text(encoding='utf-8').replace('activeInputHandler: 2','activeInputHandler: 0')
(PROJECT/'ProjectSettings/ProjectSettings.asset').write_text(settings)
# Baseline receipt detects accidental edits to any approved source dependency.
receipt={str(p.relative_to(ROOT)):hashlib.sha256(p.read_bytes()).hexdigest() for p in sorted(seen)}
(HERE/'Review/baseline-hashes.json').write_text(json.dumps(receipt,indent=2))
print(PROJECT, len(seen),'approved dependencies copied')


