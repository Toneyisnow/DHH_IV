"""Reusable Indian Ocean landforms. Blender 3.5; geometry and vertex paint, no raster extrusion."""
import bpy, math, random, json
from pathlib import Path
P=Path(__file__).parent;ROOT=P.parents[2]
OUT=ROOT/'MaritimeLegend/Assets/Resources/Models/Environment/IndianOcean';OUT.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
mat=bpy.data.materials.new('Matte painted landforms');mat.diffuse_color=(.67,.47,.20,1)
mat.use_nodes=True;bs=mat.node_tree.nodes.get('Principled BSDF');bs.inputs['Roughness'].default_value=1;bs.inputs['Specular'].default_value=0
paint=mat.node_tree.nodes.new('ShaderNodeVertexColor');paint.layer_name='Paint';mat.node_tree.links.new(paint.outputs['Color'],bs.inputs['Base Color'])
def blend(a,b,t):return tuple(x+(y-x)*max(0,min(1,t)) for x,y in zip(a,b))
def smooth(t):t=max(0,min(1,t));return t*t*(3-2*t)
specs=[]
for family in ['Angular','Rolling','Snow']:
 for size,rx,ry,h,seed in [('Small',6,3.6,4.8,71),('Medium',10,5.5,9.5,37),('Large',15,7.5,16,92)]:
  specs.append((family+'_'+size,family,rx,ry,h*(.44 if family=='Rolling' else 1),seed))
specs += [('Dune_Small','Dune',4.2,2.6,1.65,17),('Cliff_Large','Cliff',13,9,24,49)]
catalog={}
for index,(name,kind,rx,ry,H,seed) in enumerate(specs):
 rng=random.Random(seed);nx,ny=(65,43) if kind!='Dune' else (37,25);verts=[];colors=[]
 for j in range(ny):
  for i in range(nx):
   u=-1+2*i/(nx-1);v=-1+2*j/(ny-1)
   if 0<i<nx-1 and 0<j<ny-1:u+=rng.uniform(-.006,.006);v+=rng.uniform(-.008,.008)
   ridge=.15*math.sin(u*4+seed)+.12*math.sin(u*8)
   if kind in ['Angular','Snow']:
    # Piecewise ridge profile and V-cut ravines preserve hard ridgelines and pointed summits.
    peaks=[(-.63,.57,.39),(-.17,1,.45),(.38,.73,.40),(.73,.43,.25)]
    crest=max(height*max(0,1-abs(u-center)/width) for center,height,width in peaks)
    slope=max(0,1-abs(v-ridge)/(.78-.16*abs(u)))
    h=H*crest*slope
    h*=1-.16*abs(math.sin(u*19+v*6))*smooth(abs(v-ridge)*3)
   elif kind=='Rolling':
    h=H*(.70*math.exp(-((u+.37)/.42)**2-((v+.06)/.72)**2)+.94*math.exp(-((u-.25)/.46)**2-((v-.12)/.59)**2))
    h*=smooth((1-abs(u))/.22)*smooth((1-abs(v))/.24)
   elif kind=='Dune':
    cross=v-.32*(1-u*u);profile=math.exp(-(cross/(.52 if cross<0 else .23))**2)
    h=H*profile*max(0,1-u*u)**1.8*smooth((1-abs(v))/.3)
   else:
    # Irregular mesa rim, steep eroded wall and a separate high rocky summit.
    radius=((u*.94)**2+(v*1.04)**2)**.5;rim=.74+.06*math.sin(math.atan2(v,u)*7)
    wall=smooth((rim-radius)/.09)
    h=H*.69*wall+H*.33*max(0,1-abs(u+.18)/.39-abs(v-.03)/.45)
    h*=1-.055*abs(math.sin(u*38+v*12))*smooth((radius-.35)*3)
   z=h-.18-1.0*(1-smooth(h/.8)) if kind!='Dune' else h-.12
   verts.append((u*rx,v*ry,z));t=max(0,min(1,h/H))
   stops=[(0,(.24,.30,.10)),(.18,(.45,.34,.12)),(.55,(.72,.49,.18)),(1,(.98,.78,.27))]
   if kind=='Rolling':stops=[(0,(.22,.31,.10)),(.35,(.40,.43,.18)),(.75,(.65,.54,.27)),(1,(.84,.69,.36))]
   if kind=='Dune':stops=[(0,(.66,.49,.22)),(.5,(.88,.71,.35)),(1,(.98,.84,.48))]
   if kind=='Cliff':stops=[(0,(.25,.28,.13)),(.25,(.42,.31,.17)),(.7,(.67,.48,.24)),(1,(.91,.73,.39))]
   col=stops[-1][1]
   for (lo,a),(hi,b) in zip(stops,stops[1:]):
    if t<=hi:col=blend(a,b,smooth((t-lo)/(hi-lo)));break
   if kind=='Snow':col=blend(col,(.90,.95,.98),smooth((t-(.52+.045*math.sin(u*27+v*13)))/.14))
   colors.append(col)
 faces=[]
 for j in range(ny-1):
  for i in range(nx-1):
   k=j*nx+i
   faces.extend([(k,k+1,k+nx),(k+1,k+nx+1,k+nx)])
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update()
 obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj);mesh.materials.append(mat)
 col=mesh.color_attributes.new(name='Paint',type='BYTE_COLOR',domain='CORNER')
 for poly in mesh.polygons:
  poly.use_smooth=kind in ['Rolling','Dune']
  for loop in poly.loop_indices:col.data[loop].color_srgb=(*colors[mesh.loops[loop].vertex_index],1)
 bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
 bpy.ops.export_scene.fbx(filepath=str(OUT/(name+'.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_space_transform=True,apply_unit_scale=True,mesh_smooth_type='FACE',bake_anim=False)
 catalog[name]=dict(kind=kind,footX=rx*.80,footY=ry*.79,height=H,vertices=len(verts))
 # Editable comparison board, with exported files retaining origin at their own feet.
 obj.location=((index%3)*38,(index//3)*30,0)
bpy.ops.wm.save_as_mainfile(filepath=str(P/'IndianOcean_Landforms.blend'))
(P/'landform-catalog.json').write_text(json.dumps(catalog,indent=2),encoding='utf-8')
print('INDIAN_LANDFORMS_PASS',len(catalog))
