from pathlib import Path
import shutil
ROOT=Path(__file__).resolve().parents[2];HERE=Path(__file__).resolve().parent
# Reuse the already isolated original-renderer project and its shader import cache.
PROJECT=ROOT/'ArtSource/RegionalSeasV2/UnityPreview';ASSETS=ROOT/'MaritimeLegend/Assets'
def copy(rel):
    src=ASSETS/rel;dst=PROJECT/'Assets'/rel;dst.parent.mkdir(parents=True,exist_ok=True)
    if src.is_dir():shutil.copytree(src,dst,dirs_exist_ok=True)
    else:shutil.copy2(src,dst)
    meta=Path(str(src)+'.meta')
    if meta.exists():shutil.copy2(meta,Path(str(dst)+'.meta'))
copy(Path('Core'))
for excluded in ['Simulation/WorldSession.cs','Localization/LocalizationService.cs']:
    p=PROJECT/'Assets/Core'/excluded
    if p.exists():p.unlink()
for name in ['StreamedSeaTerrain','StreamedSeaRuntime','StreamedSeaNavigation','StreamedSeaReview','StreamedSeaPorts','ManualSeaV2Runtime','CoastNavigation']:copy(Path('Sailing')/(name+'.cs'))
copy(Path('Editor/StreamedSeaBuilder.cs'));copy(Path('Resources/Data/StreamedSeasV2'))
for name in ['ManualSeaV2Exploration','ManualSeaV2Land','ManualSeaV2Water','CoastalCity3D']:copy(Path('Resources/Shaders')/(name+'.shader'))
copy(Path('Resources/Environment/SoutheastAsiaV2/ports.json'));copy(Path('Resources/Models/ManualSailing/Ports3D'))
print(PROJECT)
