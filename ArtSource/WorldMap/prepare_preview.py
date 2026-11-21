"""Small isolated Unity render check, compiling the same production world sources.
Never opens or modifies the main editor's scene recovery data or settings.
"""
from pathlib import Path
import shutil,json
ROOT=Path(__file__).resolve().parents[2]
HERE=Path(__file__).resolve().parent
PROJECT=HERE/'UnityPreview'
ASSETS=ROOT/'MaritimeLegend/Assets'
def copy(rel):
    src=ASSETS/rel;dst=PROJECT/'Assets'/rel;dst.parent.mkdir(parents=True,exist_ok=True)
    if src.is_dir():shutil.copytree(src,dst,dirs_exist_ok=True)
    else:shutil.copy2(src,dst)
for folder in ['Core','Resources/Data','Resources/Localization']:copy(folder)
# Exclude the live application's scene runner; its domain executor is still compiled.
for excluded in ['Simulation/WorldSession.cs','Localization/LocalizationService.cs']:
    runner=PROJECT/'Assets/Core'/excluded
    if runner.exists():runner.unlink()
for file in ['WorldSeaAssets','WorldSeaRuntime','WorldChunkRepository','WorldChunkGeometry','WorldSeaDemo','ManualSeaV2Camera','ManualSeaV2Ship','ManualSeaV2Environment']:
    copy(f'Sailing/{file}.cs')
copy('Editor/WorldSeaIntegration.cs');copy('Tests/EditMode/Editor/WorldSailingChecks.cs')
for folder in ['junk','square','lateen']:copy('Resources/Sprites/ManualSailing/'+folder)
for file in ['WorldSeaLand.shader','WorldSeaWater.shader','ManualSeaV2Ship.shader','ManualSeaV2Sky.shader','ManualSeaV2SkyColor.cginc']:
    copy('Resources/Shaders/'+file)
(PROJECT/'Assets/PreviewUiStub.cs').write_text('namespace MaritimeLegend { public static class LiveGui { public static UnityEngine.Camera Camera => null; } }\n')
(PROJECT/'Packages').mkdir(exist_ok=True)
(PROJECT/'Packages/manifest.json').write_text(json.dumps({'dependencies':{'com.unity.modules.imgui':'1.0.0','com.unity.modules.jsonserialize':'1.0.0','com.unity.modules.physics':'1.0.0','com.unity.modules.imageconversion':'1.0.0'}}))
(PROJECT/'ProjectSettings').mkdir(exist_ok=True)
shutil.copy2(ROOT/'MaritimeLegend/ProjectSettings/ProjectVersion.txt',PROJECT/'ProjectSettings/ProjectVersion.txt')
print(PROJECT)
