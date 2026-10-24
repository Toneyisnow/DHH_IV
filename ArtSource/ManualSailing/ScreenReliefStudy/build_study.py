import bpy,struct,json,math
from pathlib import Path
from mathutils import Vector
P=Path(__file__).parent
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
data=(P/'height.bin').read_bytes();w,h=struct.unpack_from('<II',data);zs=struct.unpack_from('<'+str(w*h)+'f',data,8)
verts=[((x/(w-1)-.5)*176,(.5-y/(h-1))*140,zs[y*w+x]) for y in range(h) for x in range(w)]
faces=[(y*w+x,(y+1)*w+x,(y+1)*w+x+1,y*w+x+1) for y in range(h-1) for x in range(w-1)]
mesh=bpy.data.meshes.new('Screen relief dense surface');mesh.from_pydata(verts,[],faces);mesh.update()
obj=bpy.data.objects.new('ScreenRelief_Mountains_Forest_Riverbeds',mesh);bpy.context.collection.objects.link(obj)
uv=mesh.uv_layers.new(name='Registered_Map_UV')
for poly in mesh.polygons:
    poly.use_smooth=True
    for loop in poly.loop_indices:
        index=mesh.loops[loop].vertex_index
        uv.data[loop].uv=((index%w)/(w-1),1-(index//w)/(h-1))
mat=bpy.data.materials.new('Matte_Painted_Map');mat.use_nodes=True
bsdf=mat.node_tree.nodes.get('Principled BSDF');bsdf.inputs['Roughness'].default_value=1;bsdf.inputs['Specular'].default_value=0
tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(P/'source-region.png'));tex.interpolation='Linear'
mat.node_tree.links.new(tex.outputs['Color'],bsdf.inputs['Base Color']);obj.data.materials.append(mat)
# Keep base thickness separate from sea level; this is a finite review tile.
bpy.ops.mesh.primitive_cube_add(size=1,location=(0,0,-1.05));base=bpy.context.object;base.name='Review_Tile_Base';base.dimensions=(176,140,2)
bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
base_mat=bpy.data.materials.new('Base_Matte');base_mat.diffuse_color=(.06,.08,.07,1);base.data.materials.append(base_mat)
export_objects=[obj,base]
bpy.ops.object.select_all(action='DESELECT')
for o in export_objects:o.select_set(True)
bpy.context.view_layer.objects.active=obj
bpy.ops.export_scene.fbx(filepath=str(P/'ScreenRelief.fbx'),use_selection=True,object_types={'MESH'},use_mesh_modifiers=True,mesh_smooth_type='FACE',use_tspace=True,axis_forward='-Z',axis_up='Y',path_mode='COPY',embed_textures=True,bake_anim=False)
# Reimport the actual deliverable into a clean scene, then render that imported FBX.
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.fbx(filepath=str(P/'ScreenRelief.fbx'))
objects=[o for o in bpy.context.scene.objects if o.type=='MESH']
terrain=next(o for o in objects if o.name.startswith('ScreenRelief_Mountains'))
assert len(terrain.data.vertices)==w*h
assert len(terrain.data.uv_layers)>0
assert all(math.isfinite(v.co.z) for v in terrain.data.vertices)
material=terrain.data.materials[0]
assert any(n.type=='TEX_IMAGE' and n.image and n.image.has_data for n in material.node_tree.nodes)
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=32;scene.cycles.use_denoising=True
scene.render.resolution_x=1600;scene.render.resolution_y=1200;scene.render.resolution_percentage=100
scene.world.color=(.6,.6,.6)
scene.view_settings.view_transform='Standard';scene.view_settings.look='Medium High Contrast';scene.view_settings.exposure=0;scene.view_settings.gamma=1
bpy.ops.object.camera_add(location=(0,-205,225));camera=bpy.context.object;camera.name='Preview_45deg';camera.rotation_euler=(Vector((0,0,1))-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=213;scene.camera=camera
bpy.ops.object.light_add(type='AREA',location=(-100,-90,220));light=bpy.context.object;light.data.energy=1800;light.data.size=170;light.rotation_euler=(Vector((0,0,0))-light.location).to_track_quat('-Z','Y').to_euler()
# Use soft ambient light to preserve the illustration's existing shadows.
world=scene.world;world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.68,.72,.78,1);world.node_tree.nodes['Background'].inputs[1].default_value=.8
scene.render.image_settings.file_format='PNG'
for image in bpy.data.images:
    if image.source=='FILE' and image.has_data:image.pack()
bpy.ops.wm.save_as_mainfile(filepath=str(P/'ScreenRelief.blend'))
scene.render.filepath=str(P/'preview-textured.png');bpy.ops.render.render(write_still=True)
clay=bpy.data.materials.new('Geometry inspection clay');clay.use_nodes=True
clay.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(.46,.53,.50,1)
clay.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=1
scene.view_layers[0].material_override=clay
scene.render.filepath=str(P/'preview-geometry.png');bpy.ops.render.render(write_still=True)
(P/'validation.json').write_text(json.dumps(dict(fbx_reimport=True,vertices=len(terrain.data.vertices),polygons=len(terrain.data.polygons),uv_layers=len(terrain.data.uv_layers),embedded_texture_loaded=True,rendered_from_reimport=True),indent=2))
print('FBX_ROUNDTRIP_PASS',len(terrain.data.vertices),len(terrain.data.polygons))
