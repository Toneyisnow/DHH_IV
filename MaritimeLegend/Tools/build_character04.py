"""Original Character 04 rig/wardrobe study. Blender 3.5, no external assets.
Run: blender -b -t 4 --python Tools/build_character04.py -- [--render]
Coordinates: Z up, face toward -Y. Secondary motion is spring-baked, not cloth simulation.
"""
import bpy, math, json, sys
from pathlib import Path
from mathutils import Vector
from math import sin, cos, pi

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT.parent/'ArtSource/Character04'; OUT.mkdir(parents=True,exist_ok=True)
EVID=ROOT/'Documentation/Character04Evidence'; EVID.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
for a in list(bpy.data.actions): bpy.data.actions.remove(a)
sc=bpy.context.scene
sc.render.engine='BLENDER_EEVEE'; sc.eevee.taa_render_samples=24
sc.eevee.use_gtao=True; sc.eevee.gtao_distance=3; sc.eevee.gtao_factor=1.12
sc.render.resolution_x=480; sc.render.resolution_y=640; sc.render.resolution_percentage=100
sc.render.image_settings.file_format='PNG'; sc.render.fps=24
sc.world.color=(.45,.45,.45)
sc.view_settings.view_transform='Standard'; sc.view_settings.look='Medium High Contrast'
sc.view_settings.exposure=0; sc.view_settings.gamma=1

M={}
def mat(name,c):
    m=bpy.data.materials.new(name); m.diffuse_color=(*c,1); m.use_nodes=True;m.use_fake_user=True
    n=m.node_tree.nodes; n.clear(); l=m.node_tree.links
    d=n.new('ShaderNodeBsdfDiffuse'); d.inputs['Color'].default_value=(1,1,1,1)
    rgb=n.new('ShaderNodeShaderToRGB'); l.new(d.outputs[0],rgb.inputs[0])
    ramp=n.new('ShaderNodeValToRGB'); ramp.color_ramp.interpolation='CONSTANT'
    ramp.color_ramp.elements.remove(ramp.color_ramp.elements[1])
    for i,(pos,factor) in enumerate([(0,.48),(.38,.75),(.65,1.0)]):
        e=ramp.color_ramp.elements[0] if i==0 else ramp.color_ramp.elements.new(pos)
        e.position=pos; e.color=(*(v*factor for v in c),1)
    l.new(rgb.outputs[0],ramp.inputs[0]); em=n.new('ShaderNodeEmission');l.new(ramp.outputs[0],em.inputs[0])
    o=n.new('ShaderNodeOutputMaterial');l.new(em.outputs[0],o.inputs[0]); M[name]=m;return m
for name,c in {'skin':(.76,.49,.34),'skinLight':(.9,.66,.48),'lip':(.43,.15,.13),'dark':(.026,.024,.034),
 'hair':(.032,.026,.037),'hairLight':(.09,.065,.07),'ivory':(.88,.81,.65),'red':(.48,.027,.035),
 'navy':(.035,.13,.23),'teal':(.03,.29,.25),'gold':(.71,.43,.13),'leather':(.12,.064,.039),
 'pants':(.065,.074,.09),'white':(.94,.9,.8),'iris':(.23,.1,.035)}.items(): mat(name,c)

# One skeleton standard for every interchangeable part.
ad=bpy.data.armatures.new('ML04_Skeleton'); rig=bpy.data.objects.new('ML04_Rig',ad);sc.collection.objects.link(rig)
bpy.context.view_layer.objects.active=rig;rig.select_set(True);bpy.ops.object.mode_set(mode='EDIT')
def bone(n,h,t,parent=None):
    b=ad.edit_bones.new(n); b.head=h;b.tail=t
    if parent:b.parent=ad.edit_bones[parent]
bone('root',(0,0,0),(0,0,.25));bone('pelvis',(0,0,1.42),(0,0,1.75),'root')
bone('spine',(0,0,1.75),(0,0,2.18),'pelvis');bone('chest',(0,0,2.18),(0,0,2.58),'spine')
bone('neck',(0,0,2.58),(0,0,2.83),'chest');bone('head',(0,0,2.83),(0,0,3.43),'neck')
for side,s in [('L',1),('R',-1)]:
    bone('upper_arm.'+side,(s*.39,0,2.54),(s*.54,-.01,2.08),'chest')
    bone('forearm.'+side,(s*.54,-.01,2.08),(s*.59,-.045,1.70),'upper_arm.'+side)
    bone('hand.'+side,(s*.59,-.045,1.70),(s*.60,-.07,1.46),'forearm.'+side)
    bone('thigh.'+side,(s*.19,0,1.60),(s*.20,-.015,.94),'pelvis')
    bone('shin.'+side,(s*.20,-.015,.94),(s*.21,0,.27),'thigh.'+side)
    bone('foot.'+side,(s*.21,0,.27),(s*.21,-.23,.13),'shin.'+side)
secondary={}
for name,x,y,z,length,parent,kind in [('hair.L',.22,.01,3.22,.49,'head','hair'),('hair.R',-.22,.01,3.22,.49,'head','hair'),
 ('hair.back',0,.18,3.20,.76,'head','hair'),('coat.L',.29,.04,1.91,.91,'spine','coat'),('coat.R',-.29,.04,1.91,.91,'spine','coat'),
 ('coat.back',0,.21,1.91,.94,'spine','coat'),('cape.L',.30,.10,2.55,.76,'chest','cape'),('cape.R',-.30,.10,2.55,.76,'chest','cape')]:
    for j in range(3):
        n=name+str(j); bone(n,(x,y,z-length*j/3),(x,y,z-length*(j+1)/3),parent if j==0 else name+str(j-1))
        secondary[n]={'kind':kind,'chain':name,'segment':j}
bpy.ops.object.mode_set(mode='OBJECT');rig.show_in_front=True
parts={}; group='body'
def reg(o,material,weights):
    o.data.materials.append(M[material]); parts.setdefault(group,[]).append(o)
    if isinstance(weights,str):weights=[{weights:1} for v in o.data.vertices]
    for i,w in enumerate(weights):
        for bn,val in w.items():
            vg=o.vertex_groups.get(bn) or o.vertex_groups.new(name=bn);vg.add([i],val,'REPLACE')
    mod=o.modifiers.new('Shared skeleton','ARMATURE');mod.object=rig;o.parent=rig
    for p in o.data.polygons:p.use_smooth=True
    return o
def mesh(name,verts,faces,material,weights):
    me=bpy.data.meshes.new(name);me.from_pydata(verts,[],faces);me.update();o=bpy.data.objects.new(name,me);sc.collection.objects.link(o)
    return reg(o,material,weights)
def ell(name,loc,scale,material,bn):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=24,ring_count=16,location=loc)
    o=bpy.context.object;o.name=name;o.scale=scale;bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    return reg(o,material,bn)
def tube(name,points,radii,material,bn,n=16):
    verts=[];faces=[]
    for k,p in enumerate(points):
        direction=Vector(points[min(k+1,len(points)-1)])-Vector(points[max(0,k-1)])
        q=direction.to_track_quat('Z','Y');r=radii[k];r=(r,r) if isinstance(r,(float,int)) else r
        for j in range(n):verts.append(Vector(p)+q@Vector((r[0]*cos(2*pi*j/n),r[1]*sin(2*pi*j/n),0)))
    for k in range(len(points)-1):
        for j in range(n):a=k*n+j;b=k*n+(j+1)%n;faces.append((a,b,b+n,a+n))
    faces.extend([tuple(reversed(range(n))),tuple((len(points)-1)*n+j for j in range(n))])
    o=mesh(name,verts,faces,material,bn)
    if 'sleeve' in name.lower():
        sub=o.modifiers.new('Tailored smooth surface','SUBSURF');sub.levels=2;sub.render_levels=2
    return o
def curve(name,points,r,material,bn):return tube(name,points,[r]*len(points),material,bn,8)
def rings(name,profiles,material,bn,n=40):
    # profiles z, width, front depth, back depth, center Y
    vs=[];fs=[];ws=[]
    for z,w,f,b,cy in profiles:
        for j in range(n):
            t=2*pi*j/n;vs.append((w*sin(t),cy-(f if cos(t)>0 else b)*cos(t),z))
            ws.append(bn(z) if callable(bn) else {bn:1})
    for k in range(len(profiles)-1):
        for j in range(n):a=k*n+j;b=k*n+(j+1)%n;fs.append((a,b,b+n,a+n))
    fs.extend([tuple(reversed(range(n))),tuple((len(profiles)-1)*n+j for j in range(n))])
    o=mesh(name,vs,fs,material,ws)
    if name=='Sculpted head':
        sub=o.modifiers.new('Smooth facial planes','SUBSURF');sub.levels=2;sub.render_levels=2
    return o
def torso_weights(z):
    t=max(0,min(1,(z-1.8)/.6));return {'spine':1-t,'chest':t}
def chain_weights(chain,t):
    u=max(0,min(2,t*2));i=min(1,int(u));return {chain+str(i):1-(u-i),chain+str(i+1):u-i}

# Tailored base garment; the covered body has no duplicate surface underneath.
rings('Ivory blouse',[(1.65,.28,.15,.16,0),(1.88,.25,.14,.14,0),(2.12,.28,.18,.16,0),(2.34,.35,.205,.18,0),(2.50,.39,.12,.14,0),(2.60,.23,.105,.10,0)],'ivory',torso_weights)
rings('Black fitted waistcoat',[(1.66,.29,.17,.17,0),(1.86,.255,.16,.155,0),(2.08,.28,.20,.18,0),(2.30,.345,.224,.192,0),(2.46,.365,.15,.17,0),(2.54,.225,.12,.12,0)],'dark',torso_weights)
tube('Neck',[(0,0,2.51),(0,0,2.88)],[.115,.105],'skin','neck')
rings('Standing collar',[(2.56,.132,.12,.12,0),(2.70,.12,.115,.115,0)],'dark','neck')
for s in [-1,1]:
    curve('Collar gold edge',[(s*.12,-.025,2.70),(s*.09,-.10,2.69),(s*.025,-.12,2.63)],.009,'gold','neck')
    curve('Waistcoat piping',[(s*.12,-.16,1.68),(s*.19,-.16,1.86),(s*.24,-.19,2.12),(s*.26,-.195,2.30),(s*.16,-.12,2.53)],.008,'gold','chest')
for z,y in [(2.43,-.166),(2.28,-.23),(2.12,-.207),(1.97,-.18)]:ell('Brass button',(0,y-.012,z),(.022,.013,.022),'gold','chest' if z>2.2 else 'spine')
for side,s in [('L',1),('R',-1)]:
    tube('Puffed sleeve.'+side,[(s*.35,0,2.53),(s*.46,0,2.43),(s*.51,0,2.22),(s*.54,-.01,2.08)],[.12,.16,.135,.095],'ivory','upper_arm.'+side)
    tube('Lower sleeve.'+side,[(s*.54,-.01,2.08),(s*.57,-.025,1.91),(s*.59,-.045,1.74)],[.1,.105,.072],'ivory','forearm.'+side)
    ell('Sleeve elbow.'+side,(s*.54,-.01,2.08),(.095,.095,.10),'ivory','forearm.'+side)
    tube('Leather cuff.'+side,[(s*.58,-.04,1.85),(s*.59,-.045,1.72)],[.083,.077],'leather','forearm.'+side)
    ell('Hand.'+side,(s*.60,-.045,1.64),(.063,.041,.075),'skinLight','hand.'+side)
    for finger in range(4):
        x=s*(.56+finger*.026);z=1.56+abs(finger-1.5)*.012
        tube('Finger.'+side,[(x,-.05,1.62),(x,-.06,z),(x,-.073,z-.055)],[.015,.014,.009],'skinLight','hand.'+side,10)
    ell('Thumb.'+side,(s*.55,-.078,1.65),(.025,.03,.068),'skin','hand.'+side)
    tube('Trousers.'+side,[(s*.17,0,1.65),(s*.19,0,1.44),(s*.20,0,1.13),(s*.20,-.015,.93)],[.17,.17,.125,.105],'pants','thigh.'+side)
    tube('Calf trousers.'+side,[(s*.20,-.015,.94),(s*.21,0,.66)],[.11,.095],'pants','shin.'+side)
    tube('Riding boot.'+side,[(s*.21,0,.78),(s*.21,0,.60),(s*.21,0,.25)],[.125,.115,.085],'leather','shin.'+side)
    tube('Boot cuff.'+side,[(s*.21,0,.8),(s*.21,0,.69)],[.137,.14],'leather','shin.'+side)
    ell('Boot toe.'+side,(s*.21,-.10,.145),(.105,.225,.10),'leather','foot.'+side)
    ell('Boot sole.'+side,(s*.21,-.085,.073),(.112,.23,.026),'dark','foot.'+side)
rings('Leather waist belt',[(1.79,.272,.175,.17,0),(1.88,.269,.175,.17,0)],'leather','spine')
curve('Belt buckle',[(-.065,-.187,1.79),(.065,-.187,1.79),(.065,-.187,1.88),(-.065,-.187,1.88),(-.065,-.187,1.79)],.013,'gold','spine')
ell('Compass case',(.25,-.215,1.64),(.085,.022,.085),'gold','pelvis');ell('Compass face',(.25,-.241,1.64),(.067,.008,.067),'ivory','pelvis')
curve('Compass needle',[(.22,-.254,1.61),(.28,-.254,1.67)],.009,'red','pelvis')
ell('Belt pouch',(-.30,-.01,1.64),(.10,.12,.15),'leather','pelvis')

# Custom ring-built head: chin/jaw/cheek/temple profiles, front plane toward -Y.
rings('Sculpted head',[(2.80,.037,.078,.065,-.026),(2.85,.105,.135,.10,-.015),(2.94,.165,.167,.14,0),(3.05,.197,.178,.17,0),(3.18,.202,.17,.183,0),(3.31,.18,.14,.17,0),(3.40,.10,.082,.11,0),(3.43,.012,.018,.018,0)],'skinLight','head',48)
for s in [-1,1]:
    ell('Ear',(s*.197,.0,3.035),(.032,.033,.068),'skin','head')
    # Almond eye meshes sit on the face and avoid protruding sphere eyeballs.
    cx=s*.081; eyez=3.095
    v=[(cx,-.183,eyez)]; count=24
    for i in range(count):
        a=2*pi*i/count;x=.063*cos(a);z=.027*sin(a)*(0.85+.15*abs(sin(a)))
        v.append((cx+x,-.177+abs(x)*.08,eyez+z+s*x*.08))
    mesh('Almond eye',v,[(0,i+1,(i+1)%count+1) for i in range(count)],'white','head')
    ell('Iris',(cx,-.188,eyez),(.024,.007,.025),'iris','head');ell('Pupil',(cx,-.195,eyez),(.011,.003,.018),'dark','head')
    ell('Eye catchlight',(cx-.008,-.200,eyez+.009),(.006,.002,.006),'white','head')
    curve('Upper eyelid',[(cx-.061,-.185,eyez-.004),(cx-.035,-.189,eyez+.021),(cx,-.19,eyez+.028),(cx+.036,-.187,eyez+.02),(cx+.06,-.18,eyez+.003)],.006,'dark','head')
    curve('Eyebrow',[(cx-.06,-.181,3.16),(cx-.025,-.183,3.178),(cx+.024,-.181,3.175),(cx+.055,-.17,3.163)],.009,'hair','head')
    ell('Earring',(s*.214,-.008,2.95),(.023,.012,.033),'gold','head')
mesh('Nose', [(-.024,-.17,3.105),(.024,-.17,3.105),(-.032,-.184,3.008),(.032,-.184,3.008),(0,-.237,3.027),(0,-.177,2.997)],[(0,1,4),(0,4,2),(1,3,4),(2,4,5),(4,3,5)],'skinLight','head')
curve('Upper lip',[(-.045,-.163,2.947),(-.02,-.177,2.95),(0,-.18,2.945),(.02,-.177,2.95),(.045,-.163,2.947)],.007,'lip','head')
curve('Lower lip',[(-.032,-.166,2.938),(0,-.177,2.930),(.032,-.166,2.938)],.006,'lip','head')

# Replaceable hair shell and sculpted strands with pinned roots and weighted tips.
def haircap():
    vs=[];fs=[];n=48;m=12
    for k in range(m+1):
        for j in range(n):
            a=2*pi*j/n; end=1.40 if cos(a)>.4 else 2.0
            p=.03+(end-.03)*k/m
            vs.append((.218*sin(p)*sin(a),-.197*sin(p)*cos(a)+.017,3.16+.305*cos(p)))
    for k in range(m):
        for j in range(n):a=k*n+j;b=k*n+(j+1)%n;fs.append((a,b,b+n,a+n))
    return mesh('Hair fitted scalp',vs,fs,'hair','head')
def lock(name,pts,width,chain=None,material='hair'):
    vs=[];fs=[];ws=[];n=8
    for i,p in enumerate(pts):
        t=i/(len(pts)-1); w=width*(.8+.3*sin(pi*t))*(1-.85*t**5)
        for j in range(n):
            a=2*pi*j/n;vs.append((p[0]+w*cos(a),p[1]+w*.43*sin(a),p[2]))
            ws.append({'head':1} if chain is None or i==0 else chain_weights(chain,t))
    for i in range(len(pts)-1):
        for j in range(n):a=i*n+j;b=i*n+(j+1)%n;fs.append((a,b,b+n,a+n))
    return mesh(name,vs,fs,material,ws)
for style,length in [('hair_bob',.42),('hair_long',.81)]:
    group=style;haircap()
    for i in range(7):
        x=-.175+i*.053
        lock('Swept fringe',[(x*.7,-.105,3.43),(x,-.19,3.32),(x+.035,-.205,3.20),(x+.055,-.192,3.15+abs(x)*.24)],.044,None,'hairLight' if i==2 else 'hair')
    for s in [-1,1]:
        chain='hair.L' if s>0 else 'hair.R'
        for i in range(3):
            y=-.07+i*.07
            lock('Side lock',[(s*.17,y,3.35),(s*.224,y-.01,3.16),(s*.23,y,3.16-length*.45),(s*.22,y+.02,3.16-length)],.047,chain,'hairLight' if i==1 else 'hair')
    for i in range(7):
        x=-.16+i*.053;y=.17-abs(x)*.2
        lock('Back lock',[(x,y,3.34),(x*1.25,y+.04,3.14),(x*1.35,y+.06,3.13-length*.5),(x*1.27,y+.09,3.13-length)],.047,'hair.back')

# Open outer coat skirts and shoulder cape: cloth surfaces have actual thickness.
def cloth(name,rows,material,chain):
    vs=[];fs=[];ws=[]
    for k,row in enumerate(rows):
        for p in row:vs.append(p);ws.append(chain_weights(chain,k/(len(rows)-1)))
    n=len(rows[0])
    for k in range(len(rows)-1):
        for j in range(n-1):a=k*n+j;fs.append((a,a+1,a+n+1,a+n))
    o=mesh(name,vs,fs,material,ws);solid=o.modifiers.new('Tailored fabric thickness','SOLIDIFY');solid.thickness=.012
    return o
for style,hem in [('coat_short',1.43),('coat_long',.94)]:
    group=style
    # Three panels leave room for the legs; each has a separate motion chain.
    for sector,a0,a1 in [('L',.60,2.05),('back',2.05,4.23),('R',4.23,5.68)]:
        rows=[]
        for k in range(7):
            t=k/6;z=1.93*(1-t)+hem*t;r=.29+.10*t;dep=.19+.08*t
            row=[]
            for j in range(13):
                a=a0+(a1-a0)*j/12;flute=.009*sin(j*pi/2)*t
                row.append(((r+flute)*sin(a),-(dep+flute)*cos(a),z+.035*sin(pi*j/12)*t))
            rows.append(row)
        cloth('Outer coat '+sector,rows,'red','coat.'+sector)
        # Wide gold hem ribbon weighted identically to panel.
        trim=[[(x,y-.002,z+.018) for x,y,z in rows[-1]],[(x,y-.003,z-.005) for x,y,z in rows[-1]]]
        ob=cloth('Gold hem '+sector,trim,'gold','coat.'+sector)
        for vg in list(ob.vertex_groups):ob.vertex_groups.remove(vg)
        vg=ob.vertex_groups.new(name='coat.'+sector+'2');vg.add(list(range(len(ob.data.vertices))),1,'REPLACE')
    for s,side in [(1,'L'),(-1,'R')]:
        rows=[]
        for k in range(7):
            t=k/6;row=[]
            for j in range(13):
                a=-.10+(pi+.2)*j/12;x=s*(.07+(.37+.05*t)*sin(a));y=.02-(.18+.13*t)*cos(a)
                row.append((x,y,2.63-.50*t-.08*sin(a)))
            rows.append(row)
        cloth('Shoulder mantle '+side,rows,'red','cape.'+side)
    ell('Mantle clasp',(0,-.17,2.56),(.06,.022,.05),'gold','chest')

group='stage'
# Stage objects are deliberately outside the character collections and exports.
def stage_mat(name,c):
    m=bpy.data.materials.new(name);m.diffuse_color=(*c,1);return m
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,.015));floor=bpy.context.object;floor.name='Studio ground';floor.data.materials.append(stage_mat('Warm studio',(.19,.22,.25)))
def area(name,loc,power,size):
    bpy.ops.object.light_add(type='AREA',location=loc);o=bpy.context.object;o.name=name;o.data.energy=power;o.data.size=size;o.rotation_euler=(Vector((0,0,1.8))-o.location).to_track_quat('-Z','Y').to_euler()
area('Key softbox',(-3,-4,6),450,4);area('Fill',(3,-2,4),140,3);area('Rim',(1,3,5),650,3)
bpy.ops.object.camera_add(location=(4,-9,3.6));cam=bpy.context.object;cam.name='Portrait camera';cam.data.type='ORTHO';cam.data.ortho_scale=4.1;cam.rotation_euler=(Vector((0,0,1.78))-cam.location).to_track_quat('-Z','Y').to_euler();sc.camera=cam

# Put parts in named collections so visibility toggles and export selections are clear.
for name,objects in parts.items():
    col=bpy.data.collections.new(name);sc.collection.children.link(col)
    for o in objects:
        for c in list(o.users_collection):c.objects.unlink(o)
        col.objects.link(o)
def wardrobe(coat,hair,color):
    for g,objects in parts.items():
        visible=not (g.startswith('coat_') or g.startswith('hair_')) or g in (coat,hair)
        for o in objects:
            o.hide_render=not visible;o.hide_set(not visible)
            if g.startswith('coat_') and o.data.materials and o.data.materials[0].name in ('red','navy','teal'):
                o.data.materials[0]=M[color]
    rig['outerwear']=coat;rig['hairstyle']=hair;rig['outerwear_color']=color

# Baked damped springs driven by changes in root/head rotation.
# The adjustable stiffness/damping profiles express fabric differences; no collision solver.
profiles={'linen':{'hair':(95,15,.025),'coat':(75,14,.016),'cape':(65,12,.022)},
          'silk':{'hair':(52,9,.045),'coat':(38,8,.035),'cape':(32,7,.045)},
          'leather':{'hair':(95,15,.025),'coat':(160,23,.006),'cape':(150,22,.008)}}
def smooth(t):return t*t*(3-2*t)
def pulse(t,a,b,c,d):
    if t<a or t>d:return 0
    if t<b:return smooth((t-a)/(b-a))
    if t<c:return 1
    return 1-smooth((t-c)/(d-c))
def make_action(name,profile='linen',long_hair=False):
    act=bpy.data.actions.new(name);rig.animation_data_create();rig.animation_data.action=act
    state={n:[0,0] for n in secondary};prev={'root':0,'head':0}; max_angles={k:0 for k in ('hair','coat','cape')}
    for f in range(1,97):
        t=(f-1)/95
        for p in rig.pose.bones:p.rotation_mode='XYZ';p.rotation_euler=(0,0,0);p.location=(0,0,0)
        rootang=headang=0
        if name.startswith('Turn'):rootang=1.9*pulse(t,.08,.38,.48,.78)
        elif name.startswith('Head'):headang=.9*pulse(t,.08,.30,.45,.72)
        elif name.startswith('Gesture'):
            v=pulse(t,.05,.3,.6,.9);rig.pose.bones['upper_arm.L'].rotation_euler.z=-.65*v;rig.pose.bones['forearm.L'].rotation_euler.x=-.9*v
            rig.pose.bones['head'].rotation_euler.x=.11*v
        elif name.startswith('Walk'):
            phase=t*4*pi
            for side,s in [('L',1),('R',-1)]:
                rig.pose.bones['thigh.'+side].rotation_euler.x=.32*sin(phase)*s
                rig.pose.bones['shin.'+side].rotation_euler.x=-.32*max(0,-sin(phase)*s)
                rig.pose.bones['upper_arm.'+side].rotation_euler.x=-.25*sin(phase)*s
            rig.pose.bones['pelvis'].location.y=.018*(1-cos(phase*2));rootang=.06*sin(phase)
        elif name.startswith('Nod'):rig.pose.bones['head'].rotation_euler.x=.22*sin(t*4*pi)*sin(pi*t)
        rig.pose.bones['root'].rotation_euler.y=rootang # root bone local Y is vertical Z
        rig.pose.bones['head'].rotation_euler.y=headang
        rig.pose.bones['chest'].rotation_euler.x=.012*sin(t*2*pi)
        velocities={'root':(rootang-prev['root'])*24,'head':(rootang+headang-prev['head'])*24}
        prev={'root':rootang,'head':rootang+headang}
        for n,meta in secondary.items():
            response='silk' if long_hair else 'linen'
            k,d,amp=profiles[response if meta['kind']=='hair' else profile][meta['kind']];drive=velocities['head' if meta['kind']=='hair' else 'root']
            target=-drive*amp*(1+meta['segment']*.22);x,v=state[n]
            for sub in range(4):
                dt=1/96;v+=(k*(target-x)-d*v)*dt;x+=v*dt
            x=max(-.32,min(.32,x));state[n]=[x,v]
            rig.pose.bones[n].rotation_euler.z=x
            rig.pose.bones[n].rotation_euler.x=abs(x)*.24
            max_angles[meta['kind']]=max(max_angles[meta['kind']],abs(x)*180/pi)
        for p in rig.pose.bones:
            p.keyframe_insert('rotation_euler',frame=f,group=p.name);p.keyframe_insert('location',frame=f,group=p.name)
    for fc in act.fcurves:
        for kp in fc.keyframe_points:kp.interpolation='LINEAR'
    act.use_fake_user=True
    return act,max_angles
actions={};metrics={}
for name,profile in [('Idle','linen'),('Turn_Linen','linen'),('Turn_Silk','silk'),('Turn_Leather','leather'),('HeadTurn_Bob','linen'),('HeadTurn_Long','silk'),('Gesture','linen'),('Nod','linen'),('Walk','linen')]:
    actions[name],metrics[name]=make_action(name,profile,name=='HeadTurn_Long')
for profile in ['linen','silk','leather']:
    name='Turn_'+profile.title()+'_Long'
    actions[name],metrics[name]=make_action(name,profile,True)
rig['motion_note']='Secondary bones use damped spring baking driven by angular velocity. Not collision-aware cloth/hair simulation.'
rig['wardrobe_note']='body + one coat collection + one hair collection. Color is independent. Same skeleton across all variants.'
rig['material_profiles']=json.dumps(profiles)
panel_path=OUT/'wardrobe_panel.py'
if panel_path.exists():bpy.data.texts.load(str(panel_path))
sc.frame_start=1;sc.frame_end=96
wardrobe('coat_short','hair_bob','red');rig.animation_data.action=actions['Idle'];sc.frame_set(1)
# Validate all skin weights and referenced bones, before exporting any artifacts.
checks={'objects':sum(len(v) for v in parts.values()),'bones':len(ad.bones),'actions':list(actions),'max_secondary_degrees':metrics,'errors':[]}
for objects in parts.values():
    for o in objects:
        for v in o.data.vertices:
            total=sum(g.weight for g in v.groups)
            if abs(total-1)>1e-4:checks['errors'].append(o.name+': unnormalized vertex '+str(v.index));break
        for vg in o.vertex_groups:
            if vg.name not in ad.bones:checks['errors'].append(o.name+': missing bone '+vg.name)
(OUT/'validation.json').write_text(json.dumps(checks,indent=2),encoding='utf-8')
assert not checks['errors'],checks['errors']
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Character04_WardrobeMotion.blend'))
# FBX contains actual rig, meshes, selected wardrobe and all actions; no studio objects.
for variant,coat,hair,col in [('04_short_red_bob','coat_short','hair_bob','red'),('04_long_navy_longhair','coat_long','hair_long','navy')]:
    wardrobe(coat,hair,col);bpy.ops.object.select_all(action='DESELECT');rig.select_set(True)
    for g,objects in parts.items():
        if g in ('body',coat,hair):
            for o in objects:o.select_set(True)
    bpy.context.view_layer.objects.active=rig
    bpy.ops.export_scene.fbx(filepath=str(OUT/(variant+'.fbx')),use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,axis_forward='-Z',axis_up='Y')
wardrobe('coat_short','hair_bob','red');rig.animation_data.action=actions['Idle'];sc.frame_set(1)
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Character04_WardrobeMotion.blend'))
print('CHARACTER04_BUILT',json.dumps(checks),flush=True)
if '--render' in sys.argv:
    jobs=[('short_turn','coat_short','hair_bob','red','Turn_Linen'),('long_turn','coat_long','hair_long','navy','Turn_Silk'),
          ('bob_head','coat_short','hair_bob','red','HeadTurn_Bob'),('long_head','coat_long','hair_long','navy','HeadTurn_Long'),
          ('gesture','coat_short','hair_bob','red','Gesture'),('walk','coat_short','hair_bob','red','Walk'),('nod','coat_short','hair_bob','red','Nod')]
    # Render at 12 fps while preserving the original four-second action duration.
    for name,coat,hair,col,act in jobs:
        wardrobe(coat,hair,col);rig.animation_data.action=actions[act]
        close='head' in name
        cam.data.ortho_scale=1.65 if close else 4.1
        target=Vector((0,0,2.88 if close else 1.78));cam.location=(1.6,-8,3.35) if close else (3,-9,3.3)
        cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler()
        dest=EVID/name;dest.mkdir(exist_ok=True)
        for frame in range(1,97,2):
            sc.frame_set(frame);sc.render.filepath=str(dest/('%03d.png'%frame));bpy.ops.render.render(write_still=True)
        print('RENDER_JOB_DONE',name,flush=True)
    wardrobe('coat_short','hair_bob','red');rig.animation_data.action=actions['Idle'];sc.frame_set(1)
print('CHARACTER04_COMPLETE',flush=True)
