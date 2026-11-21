from pathlib import Path
import shutil,json
ROOT=Path(__file__).resolve().parents[2];HERE=Path(__file__).resolve().parent
PREVIEW=ROOT/'ArtSource/RegionalSeasV2/UnityPreview';TARGET=ROOT/'MaritimeLegend'
assert (HERE/'Review/result.txt').read_text().startswith('PASS '),'Successful player verification required'
assert (HERE/'Review/atlas-checks.txt').read_text().startswith('PASS '),'Shared-edge verification required'
def copy(rel):
    src=PREVIEW/rel;dst=TARGET/rel;assert src.exists(),src;dst.parent.mkdir(parents=True,exist_ok=True)
    if src.is_dir():shutil.copytree(src,dst,dirs_exist_ok=True)
    else:shutil.copy2(src,dst)
    meta=Path(str(src)+'.meta')
    if meta.exists():shutil.copy2(meta,Path(str(dst)+'.meta'))
copy(Path('Assets/Resources/Environment/StreamedSeasV2'));copy(Path('Assets/Scenes/StreamedWorldSailing.unity'))
for folder in ['Assets/Sailing','Assets/Editor','Assets/Core/Sailing']:
    for p in (PREVIEW/folder).glob('*Stream*.cs.meta'):copy(p.relative_to(PREVIEW))
# Keep the authoritative generated data; copy importer GUIDs, never stale preview payloads.
for p in (TARGET/'Assets/Resources/Data/StreamedSeasV2').rglob('*'):
    if p.is_file() and p.suffix!='.meta':
        meta=PREVIEW/p.relative_to(TARGET);meta=Path(str(meta)+'.meta')
        if meta.exists():shutil.copy2(meta,Path(str(p)+'.meta'))
for p in [TARGET/'Assets/Resources/Data/StreamedSeasV2',*(TARGET/'Assets/Resources/Data/StreamedSeasV2').rglob('*')]:
    if p.is_dir():
        meta=Path(str(PREVIEW/p.relative_to(TARGET))+'.meta')
        if meta.exists():shutil.copy2(meta,Path(str(p)+'.meta'))
print('Published verified streamed-world shell, scene, scripts metadata and atlas metadata.')
