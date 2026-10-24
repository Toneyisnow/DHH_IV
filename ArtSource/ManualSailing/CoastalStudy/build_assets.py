"""Independent coastal diorama assets. No source-map pixels or height extrusion."""
import bpy, math, random, json
import numpy as np
from pathlib import Path
from mathutils import Vector
P=Path(__file__).parent;ROOT=P.parents[2]
DEST=ROOT/'MaritimeLegend/Assets/Resources/Models/Environment/CoastalStudy'
CONFIG=ROOT/'MaritimeLegend/Assets/Resources/Environment/CoastalStudy'
DEST.mkdir(parents=True,exist_ok=True);CONFIG.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
random.seed(721)
def material(name,color):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
 p=m.node_tree.nodes['Principled BSDF'];p.inputs['Base Color'].default_value=(*color,1);p.inputs['Roughness'].default_value=1;p.inputs['Specular'].default_value=0
 n=m.node_tree.nodes.new('ShaderNodeVertexColor');n.layer_name='Paint';m.node_tree.links.new(n.outputs['Color'],p.inputs['Base Color']);return m
mats={'ground':material('Earth',(0.55,.39,.13)),'tree':material('Foliage',(.13,.25,.055)),'rock':material('Warm stone',(.42,.32,.18))}
def mesh_object(name,v,f,colors,mat,smooth=False):
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(v,[],f);mesh.update()
 o=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(o);o.data.materials.append(mat)
 col=mesh.color_attributes.new(name='Paint',type='BYTE_COLOR',domain='CORNER')
 for p in mesh.polygons:
  p.use_smooth=smooth
  for l in p.loop_indices:col.data[l].color_srgb=(*colors[mesh.loops[l].vertex_index],1)
 return o
def join(name,objs):
 bpy.ops.object.select_all(action='DESELECT')
 for o in objs:o.select_set(True)
 bpy.context.view_layer.objects.active=objs[0];bpy.ops.object.join();o=objs[0];o.name=name;return o
def primitive_colors(o,base):
 col=o.data.color_attributes.new(name='Paint',type='BYTE_COLOR',domain='CORNER')
 zmin=min(v.co.z for v in o.data.vertices);zmax=max(v.co.z for v in o.data.vertices)
 for p in o.data.polygons:
  shade=random.uniform(.9,1.08)
  for l in p.loop_indices:
   z=o.data.vertices[o.data.loops[l].vertex_index].co.z
   v=shade*(.72+.28*(z-zmin)/max(.01,zmax-zmin))
   col.data[l].color_srgb=(*[min(.95,c*v) for c in base],1)
PINE_SIZES={'Small':.64,'Medium':.86,'Large':1.10}
PINE_PALETTES={
 'Green':[(.12,.25,.045),(.25,.43,.10),(.48,.65,.22)],
 'Teal':[(.045,.23,.14),(.10,.40,.27),(.34,.66,.48)],
 'Yellow':[(.26,.30,.045),(.50,.50,.10),(.78,.71,.25)]}
def pine(name,seed,palette,size):
 random.seed(seed);objs=[]
 bpy.ops.mesh.primitive_cone_add(vertices=9,radius1=.18,radius2=.095,depth=3.8,location=(0,0,1.9));o=bpy.context.object;o.data.materials.append(mats['tree']);primitive_colors(o,(.25,.14,.055));objs.append(o)
 for level in range(5):
  base=.95+level*.67;rad=1.12-level*.17;N=13;v=[];colors=[]
  for ring,t in enumerate([0,.19,.48,.78,1]):
   for j in range(N):
    a=j*math.tau/N;rr=rad*(1-t)*(1+random.uniform(-.12,.12));z=base+t*1.7+(random.uniform(-.12,.10) if ring==0 else 0)
    v.append((rr*math.cos(a),rr*math.sin(a),z))
    # One continuous root-to-tip gradient across all crown tiers.
    height=max(0,min(1,(z-.85)/4.48));segment=min(1,int(height*2));blend=height*2-segment;blend=blend*blend*(3-2*blend)
    q=1+random.uniform(-.025,.025)
    colors.append(tuple((a*(1-blend)+b*blend)*q for a,b in zip(palette[segment],palette[segment+1])))
  f=[(r*N+j,r*N+(j+1)%N,(r+1)*N+(j+1)%N,(r+1)*N+j) for r in range(4) for j in range(N)]
  objs.append(mesh_object('Layered crown',v,f,colors,mats['tree']))
 o=join(name,objs);bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
 for vertex in o.data.vertices:vertex.co*=size
 return o
def mountain(name,seed,earth=False):
 random.seed(seed);nx,ny=47,35;v=[];colors=[]
 peaks=[(-2.5,-.9,7.5,5.8,4.5),(1.0,1.0,9.6 if seed==41 else 8.2,5.4,4),(4,-1.4,5.3,4.4,3.5)]
 for j in range(ny):
  for i in range(nx):
   x=-9+i*18/(nx-1);y=-6+j*12/(ny-1)
   if i not in [0,nx-1] and j not in [0,ny-1]:x+=random.uniform(-.12,.12);y+=random.uniform(-.12,.12)
   if earth:
    r=math.sqrt((x/8.5)**2+(y/5.7)**2)
    z=4.5*max(0,1-r*r)**1.65*(1+.12*math.sin(x*.5)+.07*math.sin(y*.8))
   else:
    z=max(h*max(0,1-((abs((x-px)/rx)**1.25+abs((y-py)/ry)**1.25)**.8))**1.2 for px,py,h,rx,ry in peaks)
    z*=.70*(1+.045*math.sin(x*2.2+y*.7)+.02*math.sin(y*3.7))
   # Bury the flat prototype apron even where the new beach is almost level.
   foot=max(0,min(1,z/1.8));foot=foot*foot*(3-2*foot)
   v.append((x,y,z-.45-1.25*(1-foot)));q=.98+random.uniform(-.012,.012)
   # Altitude palette: warm grass -> earth -> pale crest, independent of lighting.
   stops=[(0,(.39,.46,.23)),(.30,(.59,.46,.20)),(.60,(.78,.62,.25)),(.87,(.94,.79,.36)),(1,(.98,.86,.46))] if not earth else [(0,(.40,.47,.24)),(.22,(.58,.53,.30)),(.65,(.73,.62,.37)),(1,(.83,.73,.48))]
   altitude=min(1,z/(4.5 if earth else .70*max(p[2] for p in peaks)))
   base=stops[-1][1]
   for (lo,c0),(hi,c1) in zip(stops,stops[1:]):
    if altitude<=hi:
     t=max(0,(altitude-lo)/(hi-lo));t=t*t*(3-2*t);base=tuple(a*(1-t)+b*t for a,b in zip(c0,c1));break
   colors.append(tuple(c*q for c in base))
 f=[]
 for j in range(ny-1):
  for i in range(nx-1):
   k=j*nx+i;f.extend([(k,k+1,k+nx),(k+1,k+nx+1,k+nx)])
 return mesh_object(name,v,f,colors,mats['ground' if earth else 'rock'],earth)
knots=[(-100,-12),(-48,-12),(-36,-11),(-27,-17),(-20,-15),(-16,-8),(-12,2),(-3,9),(7,7),(15,0),(23,-5),(31,1),(38,12),(48,16),(100,16)]
assert all(a[0]<b[0] for a,b in zip(knots,knots[1:])), 'Coast profile must be ordered without repeated x coordinates'
def shore(x):
 for i in range(len(knots)-1):
  if knots[i][0]<=x<=knots[i+1][0]:
   x0,z0=knots[i];x1,z1=knots[i+1];t=(x-x0)/(x1-x0)
   m0=(z1-knots[max(0,i-1)][1])/(x1-knots[max(0,i-1)][0]);m1=(knots[min(len(knots)-1,i+2)][1]-z0)/(knots[min(len(knots)-1,i+2)][0]-x0)
   return (2*t**3-3*t*t+1)*z0+(t**3-2*t*t+t)*(x1-x0)*m0+(-2*t**3+3*t*t)*z1+(t**3-t*t)*(x1-x0)*m1+.30*math.sin(x*.81)+.16*math.sin(x*1.73)
 return knots[0 if x<0 else -1][1]
def river_x(z):return 3+math.sin(z*.19)*2.3+math.sin(z*.41)*.5
def beach_width(x):return 5.5+1.1*math.sin(x*.19)+.7*math.sin(x*.43+1)
def land_height(x,z):
 d=max(0,z-shore(x));width=beach_width(x)
 # Broad nearly horizontal tidal apron. The inland rise varies independently.
 t=max(0,min(1,(d-width)/(5.5+1.5*math.sin(x*.17))));ramp=t*t*(3-2*t)
 height=.025*min(d,width)+(1.0+.25*math.sin(x*.16)*math.cos(z*.13))*ramp
 height+=(.13*math.sin(x*.31+z*.12)+.09*math.sin(z*.48-x*.21))*ramp
 if z>5:
  distance=abs(x-river_x(z));width=1.15+.25*math.sin(z*.3)
  channel=max(0,min(1,(distance-width)/1.0));height=-.24+(height+.24)*channel
 return height
vs=[];cs=[];xs=[-100+i*.25 for i in range(801)];ds=[i*.25 for i in range(33)]+[8.5+i*.5 for i in range(224)]
for d in ds:
 for x in xs:
  z=shore(x)+d;vs.append((x,z,land_height(x,z)))
  grass=max(0,min(1,(d-beach_width(x)+.65*math.sin(z*.9+x*.33))/2.5))
  riverbank=abs(x-river_x(z));grass*=min(1,max(0,(riverbank-1.3)/2)) if z>5 else 1
  patch=.5+.5*math.sin(x*.13+math.sin(z*.15))*math.cos(z*.18);grass*=.96+.04*patch
  sand=(.86,.71,.37);green=(.22,.34,.085);q=.97+.03*math.sin(x*1.13)*math.cos(z*1.7)
  wet=.80+.20*min(1,d/.9)
  cs.append(tuple((a*(1-grass)+b*grass)*q*wet for a,b in zip(sand,green)))
fs=[(j*len(xs)+i,j*len(xs)+i+1,(j+1)*len(xs)+i+1,(j+1)*len(xs)+i) for j in range(len(ds)-1) for i in range(len(xs)-1)]
ground=mesh_object('BayGround',vs,fs,cs,mats['ground'],True)
assets={'BayGround':ground,'EarthHill_A':mountain('EarthHill_A',32,True),'RockPeak_A':mountain('RockPeak_A',41),'RockPeak_B':mountain('RockPeak_B',19)}
for color,palette in PINE_PALETTES.items():
 for (size,scale),seed in zip(PINE_SIZES.items(),[11,71,143]):
  name='Pine_'+color+'_'+size;assets[name]=pine(name,seed,palette,scale)
placements=[]
def put(asset,x,z,scale=1,rot=0):placements.append(dict(asset=asset,x=x,y=land_height(x,z),z=z,scale=scale,rotation=rot))
put('RockPeak_A',-26,13,1.0,18);put('RockPeak_B',-33,19,.72,-12);put('RockPeak_B',-18,18,.64,48)
put('EarthHill_A',23,12,1.1,-18);put('EarthHill_A',31,19,.8,50)
put('Pine_Teal_Large',-5,14,1,0);put('Pine_Yellow_Medium',16,5,1,15)
random.seed(940);points=[]
for centerx,centerz,count,radius in [(-29,-2,35,8),(-16,24,23,7.2),(24,24,22,6.4)]:
 for attempt in range(count*30):
  if count<=0:break
  a=random.random()*math.tau;r=radius*math.sqrt(random.random());x=centerx+math.cos(a)*r;z=centerz+math.sin(a)*r
  if z-shore(x)<3.5 or abs(x-river_x(z))<3 or any((x-px)**2+(z-pz)**2<1.8**2 for px,pz in points):continue
  if any(((x-mx)/rx)**2+((z-mz)/rz)**2<1 for mx,mz,rx,rz in [(-26,13,8,6),(-33,19,6,4),(-18,18,5,4),(23,12,9,6),(31,19,7,4)]):continue
  points.append((x,z));color=random.choice(list(PINE_PALETTES));size_draw=random.uniform(.7,1.02)
  size='Small' if size_draw<.80 else 'Medium' if size_draw<.91 else 'Large'
  put('Pine_'+color+'_'+size,x,z,1,random.uniform(0,360));count-=1
# Sample each authored mountain's foot contour at equal arc lengths, then grow
# irregular pine groups until at least half of that perimeter has tree cover.
mountains=[p for p in placements if p['asset'].startswith(('RockPeak','EarthHill'))]
def foot_contour(p):
 vertices=np.array([tuple(v.co) for v in assets[p['asset']].data.vertices])
 rim=vertices[vertices[:,2]>.2,:2];angles=np.arctan2(rim[:,1],rim[:,0]);radii=np.linalg.norm(rim,axis=1)
 outline=[]
 for angle in np.linspace(-math.pi,math.pi,96,endpoint=False):
  delta=np.abs(np.arctan2(np.sin(angles-angle),np.cos(angles-angle)))
  r=np.max(radii[delta<.14]);outline.append((math.cos(angle)*r,math.sin(angle)*r))
 outline=np.asarray(outline)*p['scale'];rotation=math.radians(-p['rotation']);c,s=math.cos(rotation),math.sin(rotation)
 outline=outline@np.array([[c,s],[-s,c]])+np.array([p['x'],p['z']])
 closed=np.vstack([outline,outline[0]]);length=np.r_[0,np.cumsum(np.linalg.norm(np.diff(closed,axis=0),axis=1))]
 samples=np.linspace(0,length[-1],96,endpoint=False)
 return np.stack([np.interp(samples,length,closed[:,axis]) for axis in range(2)],axis=1)
contours=[foot_contour(p) for p in mountains]
def inside_polygon(x,z,polygon):
 inside=False
 for (ax,az),(bx,bz) in zip(polygon,np.roll(polygon,-1,axis=0)):
  if (az>z)!=(bz>z) and x<(bx-ax)*(z-az)/(bz-az)+ax:inside=not inside
 return inside
def foot_coverage(contour):
 covered=np.zeros(len(contour),dtype=bool)
 for tree in placements:
  if not tree['asset'].startswith('Pine_'):continue
  # Crown footprint plus a narrow 0.65-unit contact band around the foot line.
  reach=1.12*PINE_SIZES[tree['asset'].split('_')[-1]]+.65
  covered|=np.sum((contour-[tree['x'],tree['z']])**2,axis=1)<reach*reach
 return float(np.mean(covered))
random.seed(221026);coverage_report=[]
for index,(mountain,contour) in enumerate(zip(mountains,contours)):
 order=list(range(len(contour)));random.shuffle(order);added=0
 for attempt in range(480):
  if foot_coverage(contour)>=.63:break
  foot=contour[order[attempt%len(order)]];outward=foot-[mountain['x'],mountain['z']];outward/=np.linalg.norm(outward)
  x,z=foot+outward*random.uniform(.35,.80)
  if z-shore(x)<2.8 or (z>5 and abs(x-river_x(z))<2.8):continue
  if any(inside_polygon(x,z,other) for j,other in enumerate(contours) if j!=index):continue
  if any((x-p['x'])**2+(z-p['z'])**2<1.55**2 for p in placements if p['asset'].startswith('Pine_')):continue
  color=random.choice(['Green','Green','Green','Teal','Yellow']);size=random.choice(['Medium','Large','Large'])
  put('Pine_'+color+'_'+size,float(x),float(z),1,random.uniform(0,360));added+=1
 ratio=foot_coverage(contour)
 assert ratio>=.5, 'Mountain foot forest coverage below half: '+str((index,ratio))
 coverage_report.append(dict(asset=mountain['asset'],x=mountain['x'],z=mountain['z'],coverage=round(ratio,4),addedTrees=added))
(P/'mountain-forest-coverage.json').write_text(json.dumps(dict(method='96 equal-arc foot samples; crown radius plus 0.65-unit contact band',mountains=coverage_report),indent=2))
print('MOUNTAIN_FOREST_COVERAGE',coverage_report)
# Paint merged forest floor from the actual tree layout, not circular decals.
# Compact kernels overlap into continuous clearings; preserve pale shore/river banks.
coords=np.asarray(vs,dtype=np.float32);gx=coords[:,0];gz=coords[:,1]
density=np.zeros(len(vs),dtype=np.float32)
for p in placements:
 if p['asset'].startswith('Pine_'):
  r=4.3*PINE_SIZES[p['asset'].split('_')[-1]];distance=((gx-p['x'])**2+(gz-p['z'])**2)/(r*r)
  density+=np.maximum(0,1-distance)**2
def smooth_mask(value,lo,hi):
 t=np.clip((value-lo)/(hi-lo),0,1);return t*t*(3-2*t)
grain=np.sin(gx*1.13+np.sin(gz*.73))*np.cos(gz*.91)
forest=smooth_mask(density+grain*.06,.12,.85)
inland=np.repeat(np.asarray(ds),len(xs))
forest*=smooth_mask(inland,1.7,3.4)
river_distance=np.abs(gx-(3+np.sin(gz*.19)*2.3+np.sin(gz*.41)*.5))
forest*=np.where(gz>5,smooth_mask(river_distance,1.65,2.8),1)
shade=np.clip(density*.28,0,1)
forest_color=np.array((.17,.27,.06))[None,:]*(1-shade[:,None])+np.array((.12,.21,.04))[None,:]*shade[:,None]
forest_color*=1+grain[:,None]*.035
paint=np.asarray(cs)*(1-forest[:,None])+forest_color*forest[:,None]
col=ground.data.color_attributes['Paint']
for loop in ground.data.loops:col.data[loop.index].color_srgb=(*paint[loop.vertex_index],1)
print('FOREST_FLOOR_PAINT',int(np.count_nonzero(forest>.8)),'dense vertices, tree-driven coverage')
for name,o in assets.items():
 bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o
 # Joining retains first object's origin; bake it so exported bases are at ground level.
 bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
 bpy.ops.export_scene.fbx(filepath=str(DEST/(name+'.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_space_transform=True,apply_unit_scale=True,use_mesh_modifiers=True,mesh_smooth_type='FACE',bake_anim=False,path_mode='AUTO')
data=dict(seed=940,shoreMinX=-100,shoreMaxX=100,shore=[shore(-100+i*200/1023) for i in range(1024)],placements=placements)
for path in [P/'layout.json',CONFIG/'layout.json']:path.write_text(json.dumps(data,indent=2))
# Editable source scene: keep prototypes in hidden collection; instantiate from layout.
library=bpy.data.collections.new('Reusable Asset Library');bpy.context.scene.collection.children.link(library)
for name,o in assets.items():
 if name=='BayGround':continue
 for c in list(o.users_collection):c.objects.unlink(o)
 library.objects.link(o)
library.hide_render=True;library.hide_viewport=True
for p in placements:
 o=assets[p['asset']].copy();o.data=assets[p['asset']].data;bpy.context.scene.collection.objects.link(o);o.location=(p['x'],p['z'],p['y']);o.scale=(p['scale'],)*3;o.rotation_euler.z=math.radians(-p['rotation'])
bpy.ops.wm.save_as_mainfile(filepath=str(P/'CoastalStudy.blend'))
bpy.ops.object.select_all(action='DESELECT');ground.select_set(True)
for o in bpy.context.scene.collection.objects:
 if o.type=='MESH':o.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(P/'CoastalStudy_Landscape.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_space_transform=True,bake_anim=False)
(P/'asset-report.json').write_text(json.dumps({name:dict(vertices=len(o.data.vertices),faces=len(o.data.polygons)) for name,o in assets.items()},indent=2))
print('COASTAL_ASSETS_PASS',len(placements),'placements',len(assets),'assets')
