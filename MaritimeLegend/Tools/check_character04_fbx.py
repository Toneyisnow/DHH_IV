import bpy,json
from pathlib import Path
OUT=Path(__file__).resolve().parents[2]/'ArtSource/Character04';results=[]
for name in ['04_short_red_bob','04_long_navy_longhair']:
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=str(OUT/(name+'.fbx')))
    rigs=[o for o in bpy.data.objects if o.type=='ARMATURE'];meshes=[o for o in bpy.data.objects if o.type=='MESH']
    assert len(rigs)==1
    assert len(rigs[0].data.bones)==42
    assert len(bpy.data.actions)>=12
    assert all(any(m.type=='ARMATURE' and m.object==rigs[0] for m in o.modifiers) for o in meshes)
    results.append({'file':name+'.fbx','bones':len(rigs[0].data.bones),'meshes':len(meshes),'actions':len(bpy.data.actions),'skinning_valid':True})
(OUT/'fbx-roundtrip-validation.json').write_text(json.dumps(results,indent=2))
print('FBX_ROUNDTRIP_PASSED',json.dumps(results))
