"""Import exported solid geometry into an editable Blender source scene (Blender 3.5)."""
import bpy
from pathlib import Path
root = Path(__file__).resolve().parents[1]
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.obj(filepath=str(root / 'Models/NordicHarborLayout.obj'), axis_forward='-Z', axis_up='Y')
bpy.context.scene.unit_settings.system='METRIC'
bpy.context.scene['source_note']='Editable solid geometry and base colors. Production pixel materials and final lighting are in the Unity NordicHarborScene.'
bpy.ops.wm.save_as_mainfile(filepath=str(root / 'NordicHarbor3D.blend'))
