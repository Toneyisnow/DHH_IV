import bpy, sys
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Documentation/Character04Evidence';OUT.mkdir(exist_ok=True)
sc=bpy.context.scene;rig=bpy.data.objects['ML04_Rig'];cam=sc.camera
def wardrobe(coat,hair,color):
    for col in bpy.data.collections:
        if col.name.startswith(('coat_','hair_')):
            for o in col.objects:
                on=col.name in (coat,hair);o.hide_render=not on;o.hide_set(not on)
                if col.name.startswith('coat_') and o.data.materials and o.data.materials[0].name in ('red','navy','teal'):
                    o.data.materials[0]=bpy.data.materials[color]
def camera(close=False):
    cam.data.ortho_scale=1.5 if close else 3.95
    cam.location=(1.2,-8,3.2) if close else (2,-9,3.3)
    cam.rotation_euler=(Vector((0,0,2.93 if close else 1.77))-cam.location).to_track_quat('-Z','Y').to_euler()
sc.render.resolution_x=400;sc.render.resolution_y=560;sc.eevee.taa_render_samples=16
if '--probe' in sys.argv:
    camera();sc.frame_set(1);sc.render.filepath=str(OUT/'probe.png');bpy.ops.render.render(write_still=True)
else:
    jobs=[('short_turn','coat_short','hair_bob','red','Turn_Linen'),('long_turn','coat_long','hair_long','navy','Turn_Silk_Long'),
    ('bob_head','coat_short','hair_bob','red','HeadTurn_Bob'),('long_head','coat_long','hair_long','navy','HeadTurn_Long'),
    ('gesture','coat_short','hair_bob','red','Gesture'),('walk','coat_short','hair_bob','red','Walk'),('nod','coat_short','hair_bob','red','Nod'),
    ('leather_turn','coat_long','hair_long','teal','Turn_Leather_Long'),
    ('material_linen','coat_long','hair_long','navy','Turn_Linen_Long'),('material_leather','coat_long','hair_long','navy','Turn_Leather_Long')]
    for name,coat,hair,color,action in jobs:
        if '--body-only' in sys.argv and 'head' in name:continue
        wardrobe(coat,hair,color);rig.animation_data.action=bpy.data.actions[action];camera('head' in name)
        dest=OUT/name;dest.mkdir(exist_ok=True)
        for f in range(1,97,4):
            sc.frame_set(f);sc.render.filepath=str(dest/('%03d.png'%f));bpy.ops.render.render(write_still=True)
        print('JOB_DONE',name,flush=True)
