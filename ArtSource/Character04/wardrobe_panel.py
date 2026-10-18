"""Blender: open this file in Text Editor, Run Script, then 3D View > N > ML04.
Also embedded in the .blend. Re-running registration is safe.
The panel selects tested, baked secondary-motion actions; it is not a live physics solver.
"""
import bpy

def apply(self, context):
    rig=bpy.data.objects.get('ML04_Rig')
    if not rig:return
    for col in bpy.data.collections:
        if col.name.startswith(('coat_','hair_')):
            for ob in col.objects:
                on=col.name in (self.coat,self.hair)
                ob.hide_render=not on;ob.hide_set(not on)
                if col.name.startswith('coat_') and ob.type=='MESH' and ob.data.materials and ob.data.materials[0].name in ('red','navy','teal'):
                    ob.data.materials[0]=bpy.data.materials[self.color]
    action=self.motion
    if action=='Turn':action='Turn_'+self.fabric+('_Long' if self.hair=='hair_long' else '')
    if action=='HeadTurn':action='HeadTurn_'+('Bob' if self.hair=='hair_bob' else 'Long')
    rig.animation_data_create();rig.animation_data.action=bpy.data.actions[action]
    rig['outerwear']=self.coat;rig['hairstyle']=self.hair;rig['outerwear_color']=self.color
    context.scene.frame_set(context.scene.frame_current)

class ML04Settings(bpy.types.PropertyGroup):
    coat:bpy.props.EnumProperty(name='Outerwear',items=[('coat_short','Short',''),('coat_long','Long','')],update=apply)
    hair:bpy.props.EnumProperty(name='Hair',items=[('hair_bob','Bob',''),('hair_long','Long','')],update=apply)
    color:bpy.props.EnumProperty(name='Color',items=[('red','Crimson',''),('navy','Navy',''),('teal','Teal','')],update=apply)
    fabric:bpy.props.EnumProperty(name='Fabric response',items=[('Linen','Linen',''),('Silk','Silk',''),('Leather','Stiff leather','')],update=apply)
    motion:bpy.props.EnumProperty(name='Action',items=[(k,k,'') for k in ['Idle','Turn','HeadTurn','Gesture','Nod','Walk']],update=apply)

class ML04_PT_Wardrobe(bpy.types.Panel):
    bl_label='Character 04 - Wardrobe / Motion';bl_idname='ML04_PT_Wardrobe'
    bl_space_type='VIEW_3D';bl_region_type='UI';bl_category='ML04'
    def draw(self,context):
        col=self.layout.column();p=context.scene.ml04
        for key in ['coat','hair','color','fabric','motion']:col.prop(p,key)
        col.label(text='Space: play / pause, frames 1-96')
        col.label(text='Fabric selects baked Turn response.')
        col.label(text='No cloth / hair collision solver yet.')

def register():
    if hasattr(bpy.types.Scene,'ml04'):del bpy.types.Scene.ml04
    for cls in reversed([ML04Settings,ML04_PT_Wardrobe]):
        old=getattr(bpy.types,cls.__name__,None)
        if old:bpy.utils.unregister_class(old)
    for cls in [ML04Settings,ML04_PT_Wardrobe]:bpy.utils.register_class(cls)
    bpy.types.Scene.ml04=bpy.props.PointerProperty(type=ML04Settings)

if __name__=='__main__':register()
