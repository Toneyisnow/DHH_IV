import bpy, struct
from pathlib import Path
root=Path(__file__).resolve().parents[3]
out=Path(__file__).parent
raw=(out/'relief.bytes').read_bytes();nx,ny=struct.unpack_from('<II',raw);heights=struct.unpack_from('<'+str(nx*ny)+'f',raw,8)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
verts=[(x*1672/(nx-1),y*941/(ny-1),heights[y*nx+x]) for y in range(ny) for x in range(nx)]
faces=[(y*nx+x,y*nx+x+1,(y+1)*nx+x+1,(y+1)*nx+x) for y in range(ny-1) for x in range(nx-1)]
mesh=bpy.data.meshes.new('SEA true relief');mesh.from_pydata(verts,[],faces);mesh.update()
obj=bpy.data.objects.new('Southeast Asia - editable relief',mesh);bpy.context.collection.objects.link(obj)
uv=mesh.uv_layers.new(name='Map UV')
for poly in mesh.polygons:
 for loop in poly.loop_indices:
  v=mesh.vertices[mesh.loops[loop].vertex_index].co;uv.data[loop].uv=(v.x/1672,1-v.y/941)
mat=bpy.data.materials.new('Original approved SEA art');mat.use_nodes=True;mat.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=1;mat.node_tree.nodes['Principled BSDF'].inputs['Specular'].default_value=0
tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(root/'Assets/AutoSailing_Map_Pack/01_map/southeast_asia.png'));tex.image.pack()
mat.node_tree.links.new(tex.outputs['Color'],mat.node_tree.nodes['Principled BSDF'].inputs['Base Color']);obj.data.materials.append(mat)
bpy.context.view_layer.objects.active=obj;obj.select_set(True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'SoutheastAsiaRelief.blend'))
print('RELIEF_BLEND_SAVED',len(verts),len(faces))
