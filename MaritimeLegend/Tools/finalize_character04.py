import bpy,json,itertools
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT.parent/'ArtSource/Character04'
for ob in bpy.data.objects:
    if 'trousers' in ob.name.lower():
        for mod in list(ob.modifiers):
            if mod.type=='SUBSURF':ob.modifiers.remove(mod)
for name,c in [('navy',(.035,.13,.23)),('teal',(.03,.29,.25))]:
    m=bpy.data.materials.get(name)
    if m is None:
        m=bpy.data.materials['red'].copy();m.name=name;m.diffuse_color=(*c,1)
        ramp=next(n for n in m.node_tree.nodes if n.type=='VALTORGB')
        for e,f in zip(ramp.color_ramp.elements,[.48,.75,1]):e.color=(*(v*f for v in c),1)
    m.use_fake_user=True
for m in bpy.data.materials:m.use_fake_user=True
path=OUT/'wardrobe_panel.py'
old=bpy.data.texts.get(path.name)
if old:bpy.data.texts.remove(old)
bpy.data.texts.load(str(path))
scope={'__name__':'__main__'};exec(compile(path.read_text(),str(path),'exec'),scope)
p=bpy.context.scene.ml04;rig=bpy.data.objects['ML04_Rig'];results=[]
for coat,hair,color,fabric in itertools.product(['coat_short','coat_long'],['hair_bob','hair_long'],['red','navy','teal'],['Linen','Silk','Leather']):
    p.coat=coat;p.hair=hair;p.color=color;p.fabric=fabric;p.motion='Turn'
    scope['apply'](p,bpy.context)
    visible=[col.name for col in bpy.data.collections if col.name.startswith(('coat_','hair_')) and any(not o.hide_render for o in col.objects)]
    assert sorted(visible)==sorted([coat,hair]),visible
    assert rig.animation_data.action.name=='Turn_'+fabric+('_Long' if hair=='hair_long' else '')
    results.append([coat,hair,color,fabric])
p.coat='coat_short';p.hair='hair_bob';p.color='red';p.fabric='Linen';p.motion='Idle'
scope['apply'](p,bpy.context);bpy.context.scene.frame_set(1)
for screen in bpy.data.screens:
    for area in screen.areas:
        if area.type=='VIEW_3D':
            area.spaces.active.region_3d.view_perspective='CAMERA'
            area.spaces.active.shading.type='MATERIAL'
        elif area.type=='TEXT_EDITOR':area.spaces.active.text=bpy.data.texts[path.name]
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Character04_WardrobeMotion.blend'))
(OUT/'wardrobe-validation.json').write_text(json.dumps({'combinations_passed':len(results),'combinations':results},indent=2))
print('WARDROBE_COMBINATIONS_PASSED',len(results))
def hair_curves(action):
    return [[round(p.co.y,7) for p in fc.keyframe_points] for fc in bpy.data.actions[action].fcurves if 'hair.' in fc.data_path]
assert hair_curves('Turn_Linen')==hair_curves('Turn_Silk')==hair_curves('Turn_Leather')
assert hair_curves('Turn_Linen_Long')==hair_curves('Turn_Silk_Long')==hair_curves('Turn_Leather_Long')
print('HAIR_FABRIC_INDEPENDENCE_PASSED')
(OUT/'independence-validation.json').write_text(json.dumps({'hair_unchanged_across_fabric':True,'bob_and_long_checked':True},indent=2))
for name,coat,hair,color in [('04_short_red_bob','coat_short','hair_bob','red'),('04_long_navy_longhair','coat_long','hair_long','navy')]:
    p.coat=coat;p.hair=hair;p.color=color;scope['apply'](p,bpy.context)
    bpy.ops.object.select_all(action='DESELECT');rig.select_set(True)
    for col in bpy.data.collections:
        if col.name in ('body',coat,hair):
            for ob in col.objects:ob.select_set(True)
    bpy.context.view_layer.objects.active=rig
    bpy.ops.export_scene.fbx(filepath=str(OUT/(name+'.fbx')),use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,axis_forward='-Z',axis_up='Y')
print('FINAL_FBX_EXPORTED')
