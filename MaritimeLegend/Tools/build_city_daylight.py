"""Blender 3.5+: blender -b -t 4 --python Tools/build_city_daylight.py.
Original reusable geometry, then an eight-direction/eight-frame sailor render.
Blender Z-up is exported as Unity Y-up; triangle winding is reversed.
"""
import bpy, math, json, random
from pathlib import Path
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Assets/Resources/CityDaylight'
SOURCE = ROOT / 'ArtSource/CityDaylight'
OUT.mkdir(parents=True, exist_ok=True)
SOURCE.mkdir(parents=True, exist_ok=True)
random.seed(17)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
palette = {'plaster':(.76,.68,.49), 'stone':(.53,.55,.49), 'wood':(.23,.12,.065),
           'roof':(.16,.27,.30), 'tile':(.23,.36,.38), 'glass':(.16,.31,.33),
           'brass':(.71,.46,.17), 'canvas':(.76,.31,.16), 'cream':(.93,.82,.55),
           'leaf':(.22,.38,.12), 'leaflight':(.38,.51,.17), 'skin':(.78,.51,.30),
           'coat':(.54,.12,.08), 'trousers':(.13,.19,.24), 'boots':(.12,.08,.055)}
mats={}
for key, rgb in palette.items():
    m=bpy.data.materials.new(key); m.diffuse_color=(*rgb,1); mats[key]=m

def cube(name, p, s, mat, bevel=0):
    bpy.ops.mesh.primitive_cube_add(size=1, location=p)
    o=bpy.context.object; o.name=name; o.scale=s
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    o.data.materials.append(mats[mat])
    if bevel:
        mod=o.modifiers.new('Soft carved edges','BEVEL');mod.width=bevel;mod.segments=1
        bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
    return o

def beam(name,a,b,r,mat):
    d=Vector(b)-Vector(a);o=cube(name,(Vector(a)+Vector(b))/2,(r,r,d.length),mat)
    o.rotation_euler=d.to_track_quat('Z','Y').to_euler();return o

def sphere(name,p,s,mat):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,location=p)
    o=bpy.context.object;o.name=name;o.scale=s;o.data.materials.append(mats[mat]);return o

def roof(w,d,z):
    rise=1.35
    for side in [-1,1]:
        verts=[(side*w/2,-d/2,z),(side*w/2,d/2,z),(side*w/2,0,z+rise)]
        mesh=bpy.data.meshes.new('Plaster gable');mesh.from_pydata(verts,[],[(0,1,2) if side>0 else (2,1,0)]);mesh.materials.append(mats['plaster'])
        gable=bpy.data.objects.new('Closed gable',mesh);bpy.context.scene.collection.objects.link(gable)
        beam('Gable trim',verts[0],verts[2],.13,'wood');beam('Gable trim',verts[1],verts[2],.13,'wood')
    for side in [-1,1]:
        slope=cube('Slate roof',(0,side*d/4,z+rise/2),(w+.8,math.hypot(d/2+.4,rise),.15),'roof')
        slope.rotation_euler.x=-side*math.atan2(rise,d/2+.4)
        for x in range(int((w+.7)/.32)):
            xx=-w/2-.2+x*.32
            beam('Raised tile seam',(xx,0,z+rise+.08),(xx,side*(d/2+.4),z+.04),.055,'tile')
        beam('Eave',( -w/2-.4,side*(d/2+.4),z),(w/2+.4,side*(d/2+.4),z),.19,'wood')
    beam('Ridge',(-w/2-.5,0,z+rise),(w/2+.5,0,z+rise),.22,'tile')

def building(kind,w=6,d=4,h=4):
    cube('Stone footing',(0,0,.25),(w+.25,d+.25,.5),'stone',.07)
    cube('Rectangular plaster volume',(0,0,h/2+.3),(w,d,h),'plaster')
    for x in [-w/2,w/2]:
        for y in [-d/2,d/2]:cube('Corner timber',(x,y,h/2+.3),(.19,.19,h),'wood')
    for z in [.6,2.6,h+.3]:cube('Facade beam',(0,-d/2-.04,z),(w,.18,.16),'wood')
    cube('Door',(0,-d/2-.08,1.25),(1.1,.16,2),'wood',.04)
    for x in [-.63,.63]:cube('Door jamb',(x,-d/2-.13,1.25),(.14,.2,2.2),'stone')
    cube('Door lintel',(0,-d/2-.14,2.35),(1.4,.22,.18),'stone')
    for x in [-w*.32,w*.32]:
        for z in ([1.6,3.4] if h>3.7 else [1.6]):
            cube('Window surround',(x,-d/2-.12,z),(1.2,.18,1.2),'wood')
            cube('Blue window',(x,-d/2-.23,z),(1,.05,1),'glass')
            for dx in [-.32,0,.32]:cube('Window mullion',(x+dx,-d/2-.27,z),(.045,.05,1),'cream')
            cube('Window crossbar',(x,-d/2-.28,z),(1,.05,.055),'wood')
            cube('Window sill',(x,-d/2-.3,z-.63),(1.4,.42,.12),'stone')
    for y in [-d*.25,d*.25]:
        cube('Side window',(w/2+.02,y,1.8),(.07,.9,1.1),'glass')
        cube('Side sill',(w/2+.15,y,1.2),(.3,1.1,.12),'stone')
    roof(w,d,h+.4)
    cube('Chimney',(w*.30,d*.18,h+1.2),(.65,.65,2),'stone',.04)
    if kind.startswith('Market'):
        for i in range(8):
            aw=cube('Striped market canopy',(-w/2+(i+.5)*w/8,-d/2-1,2.65),(w/8,2,.09),'canvas' if (i+(kind=='MarketB'))%2 else 'cream')
            aw.rotation_euler.x=.13
        for x in [-w/2+.15,w/2-.15]:cube('Canopy upright',(x,-d/2-1.8,1.35),(.12,.12,2.7),'wood')
        for x in [-w*.3,w*.3]:
            cube('Market counter',(x,-d/2-1.05,.65),(1.6,.8,1.3),'wood')
            for i in range(5):sphere('Produce',(x-.6+i*.3,-d/2-1.05,1.4),(.17,.17,.16),'brass' if kind=='MarketA' else 'leaflight')
    if kind=='TownHall':
        cube('Civic tower',(0,d*.12,h+1),(2.1,2.1,3),'plaster')
        cube('Clock',(0,d*.12-1.08,h+1.3),(1.3,.08,1.3),'cream')
        beam('Clock hand',(0,d*.12-1.15,h+1.3),(0,d*.12-1.15,h+1.8),.07,'wood')
        beam('Clock hand',(0,d*.12-1.15,h+1.3),(.38,d*.12-1.15,h+1.3),.07,'wood')
        cube('Tower crown',(0,d*.12,h+2.6),(2.5,2.5,.3),'roof')
    if kind=='Tavern':
        beam('Sign bracket',(w*.37,-d/2,3.1),(w*.37,-d/2-1.2,3.1),.12,'wood')
        cube('Inn sign',(w*.37,-d/2-1.1,2.65),(.75,.16,.65),'brass',.06)
    if kind=='Shipyard':
        cube('Shipyard open door',(0,-d/2-.2,1.5),(2.4,.1,2.5),'boots')
        beam('Shipwright crane',(w/2+1,0,0),(w/2+1,0,5),.3,'wood')
        beam('Crane jib',(w/2+1,0,4.7),(w/2+1,-3,4.7),.25,'wood')
        beam('Crane rope',(w/2+1,-2.8,4.7),(w/2+1,-2.8,1.2),.04,'wood')

def export(name,objects):
    # Bake and combine by material: few draw calls, genuine geometry and shadows.
    groups={}
    for o in objects:
        if o.type!='MESH':continue
        mesh=o.data;mesh.calc_loop_triangles(); key=o.data.materials[0].name
        group=groups.setdefault(key,{'vertices':[],'triangles':[],'color':dict(zip('rgba',(*palette[key],1)))})
        offset=len(group['vertices'])
        for v in mesh.vertices:
            p=o.matrix_world@v.co;group['vertices'].append({'x':round(p.x,5),'y':round(p.z,5),'z':round(p.y,5)})
        for tri in mesh.loop_triangles:
            group['triangles'] += [offset+tri.vertices[i] for i in [0,2,1]]
    (OUT/(name+'.json')).write_text(json.dumps({'parts':list(groups.values())},separators=(',',':')))
    collection=bpy.data.collections.new(name);bpy.context.scene.collection.children.link(collection)
    for o in objects:
        for c in list(o.users_collection):c.objects.unlink(o)
        collection.objects.link(o)
    collection.hide_render=True;collection.hide_viewport=True

for kind,w,d,h in [('MarketA',6,4,4),('MarketB',6,4,3),('TownHall',8,5,5),('Shipyard',7,5,3.8),('Tavern',6,4,4.7),('House',5,4,3.5)]:
    before=set(bpy.data.objects);building(kind,w,d,h);bpy.context.view_layer.update();export(kind,set(bpy.data.objects)-before)
before=set(bpy.data.objects)
beam('Tree trunk',(0,0,0),(.1,0,3.7),.38,'wood')
for i in range(15):
    a=i*2.4;r=1.3 if i<10 else .7;z=3.5+(i%3)*.55
    p=(math.cos(a)*r,math.sin(a)*r,z)
    beam('Branch',(0,0,2),p,.13,'wood');sphere('Faceted olive canopy',p,(1.15,1.05,.9),'leaf' if i%3 else 'leaflight')
bpy.context.view_layer.update();export('Tree',set(bpy.data.objects)-before)
before=set(bpy.data.objects)
for i in range(24):
    z=-3+(i+.5)*.25;y=math.sin((i+.5)/24*math.pi)*1.05
    cube('Bridge stone tread',(0,z,y-.12),(3,.27,.24),'stone')
    for x in [-1.55,1.55]:
        cube('Bridge parapet',(x,z,y+.3),(.22,.27,.6),'stone')
        cube('Parapet cap',(x,z,y+.64),(.3,.27,.10),'cream')
bpy.context.view_layer.update();export('Bridge',set(bpy.data.objects)-before)
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'DaylightKit.blend'))

# Articulated, rendered sailor. Each column is a direction, each row a gait frame.
scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE';scene.eevee.taa_render_samples=16
scene.render.resolution_x=128;scene.render.resolution_y=160;scene.render.resolution_percentage=100
scene.render.film_transparent=True;scene.view_settings.view_transform='Standard';scene.view_settings.look='Medium High Contrast'
bpy.ops.object.camera_add(location=(0,-7,4.2));cam=bpy.context.object;cam.rotation_euler=(Vector((0,0,1))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=2.65;scene.camera=cam
bpy.ops.object.light_add(type='AREA',location=(-3,-4,7));bpy.context.object.data.energy=450;bpy.context.object.data.size=5
scene.world.color=(.5,.5,.5)
frames=SOURCE/'walker_frames';frames.mkdir(exist_ok=True)
for frame in range(8):
    for direction in range(8):
        before=set(bpy.data.objects);phase=frame*math.tau/8
        sphere('Coat',(0,0,1.12),(.31,.20,.41),'coat')
        cube('Belt',(0,0,.91),(.55,.36,.10),'wood')
        sphere('Head',(0,-.01,1.72),(.22,.21,.27),'skin')
        sphere('Nose',(0,-.21,1.71),(.065,.09,.07),'skin')
        for side in [-1,1]:sphere('Eye',(side*.085,-.195,1.79),(.027,.025,.028),'boots')
        cube('Neck scarf',(0,-.03,1.48),(.28,.3,.12),'cream',.03)
        cube('Hair',(0,.08,1.83),(.38,.29,.21),'wood',.05)
        cube('Sailor hat',(0,0,1.99),(.61,.47,.12),'cream',.05)
        cube('Hat crown',(0,0,2.08),(.38,.32,.17),'cream',.05)
        for sign in [-1,1]:
            stride=math.sin(phase)*sign*.31
            hip=(sign*.14,0,.91);knee=(sign*.14,stride*.6,.51);foot=(sign*.14,stride,.14+max(0,stride)*.22)
            beam('Trouser thigh',hip,knee,.22,'trousers');beam('Trouser calf',knee,foot,.18,'trousers')
            cube('Boot',(foot[0],foot[1]-.06,foot[2]-.035),(.22,.34,.21),'boots',.03)
            shoulder=(sign*.3,0,1.4);elbow=(sign*.37,-stride*.6,1.06);hand=(sign*.34,-stride,.85)
            beam('Sleeve',shoulder,elbow,.18,'coat');beam('Cuff',elbow,hand,.14,'cream');sphere('Hand',hand,(.09,.09,.1),'skin')
        objects=set(bpy.data.objects)-before
        # Atlas order: down, down-left, left, up-left, up, up-right, right, down-right.
        from mathutils import Matrix
        turn=Matrix.Rotation(-direction*math.pi/4,4,'Z')
        bpy.context.view_layer.update()
        for o in objects:o.matrix_world=turn@o.matrix_world
        scene.render.filepath=str(frames/f'{frame}_{direction}.png');bpy.ops.render.render(write_still=True)
        for o in objects:bpy.data.objects.remove(o,do_unlink=True)
print('CITY_DAYLIGHT_BLENDER_COMPLETE')
