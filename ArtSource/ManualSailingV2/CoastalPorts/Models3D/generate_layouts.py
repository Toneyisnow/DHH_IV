"""Editable architectural layouts for twelve solid, terrain-fitted coastal models."""
import json, pathlib, shutil
root=pathlib.Path(__file__).resolve().parents[4]
out=pathlib.Path(__file__).resolve().parent
res=root/'MaritimeLegend/Assets/Resources/Models/ManualSailing/Ports3D'
styles=[('malacca',3,[.78,.65,.43],[.59,.28,.12]),('manila',1,[.85,.77,.56],[.56,.25,.12]),('batavia',1,[.83,.78,.63],[.62,.25,.13]),('palembang',3,[.64,.46,.27],[.52,.25,.10]),('surabaya',2,[.76,.60,.38],[.65,.30,.13]),('brunei',3,[.70,.53,.29],[.51,.32,.12]),('makassar',2,[.78,.68,.46],[.57,.30,.14]),('manado',2,[.74,.58,.36],[.63,.30,.13]),('ternate',3,[.77,.65,.42],[.59,.26,.11]),('ambon',1,[.82,.75,.56],[.56,.24,.12]),('giadinh',2,[.79,.66,.39],[.61,.33,.13]),('ayutthaya',4,[.84,.72,.45],[.66,.30,.12])]
for i,(name,tiers,wall,roof) in enumerate(styles):
 buildings=[dict(x=0,z=-.22,w=.25,d=.23,h=.25,tiers=tiers,tower=True,stilts=False)]
 for j,(x,z) in enumerate([(-.25,-.12),(.25,-.11),(-.24,.17),(.22,.18),(0,.07),(-.06,.32)]):
  buildings.append(dict(x=x+(i%3-1)*.014,z=z,w=.18+(j%2)*.025,d=.18,h=.12+(j%3)*.025,tiers=2 if tiers>1 and j%3==0 else 1,tower=False,stilts=z>.1 and i in [0,3,5,7,10]))
 if name in ['manila','batavia','makassar','ambon']:
  buildings.append(dict(x=.34,z=-.30,w=.12,d=.14,h=.36,tiers=1,tower=True,stilts=False))
 data=dict(buildings=buildings,wall=wall,roof=roof,palms=4+(i%2))
 file=out/(name+'.json');file.write_text(json.dumps(data,indent=2)+'\n',encoding='utf-8');shutil.copyfile(file,res/file.name)
