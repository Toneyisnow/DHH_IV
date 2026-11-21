"""Remove only this generator's superseded deep-ocean chunk files, never other assets."""
from pathlib import Path
import json
ROOT=Path(__file__).resolve().parents[2]
relative=Path('Assets/Resources/Data/StreamedSeasV2')
manifest=json.loads((ROOT/'MaritimeLegend'/relative/'atlas.json').read_text(encoding='utf-8'))
removed=0
for project in [ROOT/'MaritimeLegend',ROOT/'ArtSource/RegionalSeasV2/UnityPreview']:
    folder=(project/relative/'Chunks').resolve()
    assert folder.is_relative_to(ROOT.resolve())
    for key,ocean in enumerate(manifest['ocean']):
        if not ocean:continue
        file=folder/f'{key%72:02d}_{key//72:02d}.bytes'
        for p in [file,Path(str(file)+'.meta')]:
            assert p.resolve().parent==folder
            if p.exists():p.unlink();removed+=1
print('Removed',removed,'obsolete generated ocean payloads/metas')
