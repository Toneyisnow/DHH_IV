"""Editable vector HUD art, rasterized with supersampling for Unity's existing LiveGui."""
from pathlib import Path
from PIL import Image,ImageDraw
import math,json
P=Path(__file__).parent;ROOT=P.parents[2];OUT=ROOT/'MaritimeLegend/Assets/Resources/Sprites/UI/ManualSailing/Instruments';OUT.mkdir(parents=True,exist_ok=True)
class Art:
 def __init__(self):self.image=Image.new('RGBA',(1024,1024));self.d=ImageDraw.Draw(self.image);self.svg=[]
 def poly(self,pts,fill,stroke=None,width=1):
  q=[(x*4,y*4) for x,y in pts];self.d.polygon(q,fill=fill)
  if stroke:self.d.line(q+[q[0]],fill=stroke,width=max(1,round(width*4)),joint='curve')
  self.svg.append(f'<polygon points="'+ ' '.join(f'{x:.3f},{y:.3f}' for x,y in pts)+f'" fill="{fill or "none"}" stroke="{stroke or "none"}" stroke-width="{width}" stroke-linejoin="round"/>')
 def line(self,pts,color,width=1):
  self.d.line([(x*4,y*4) for x,y in pts],fill=color,width=max(1,round(width*4)),joint='curve')
  self.svg.append('<polyline points="'+' '.join(f'{x:.3f},{y:.3f}' for x,y in pts)+f'" fill="none" stroke="{color}" stroke-width="{width}" stroke-linecap="round"/>')
 def circle(self,x,y,r,fill,stroke=None,width=1):
  self.d.ellipse(((x-r)*4,(y-r)*4,(x+r)*4,(y+r)*4),fill=fill,outline=stroke,width=max(1,round(width*4)))
  self.svg.append(f'<circle cx="{x}" cy="{y}" r="{r}" fill="{fill or "none"}" stroke="{stroke or "none"}" stroke-width="{width}"/>')
 def save(self,name):
  self.image.resize((512,512),Image.Resampling.LANCZOS).save(OUT/(name+'.png'))
  (P/(name+'.svg')).write_text('<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 256 256">'+''.join(self.svg)+'</svg>',encoding='utf-8')
def curve(a,b,c,d,n=45):
 return [((1-t)**3*a[0]+3*(1-t)**2*t*b[0]+3*(1-t)*t*t*c[0]+t**3*d[0],(1-t)**3*a[1]+3*(1-t)**2*t*b[1]+3*(1-t)*t*t*c[1]+t**3*d[1]) for t in [i/n for i in range(n+1)]]
a=Art()
for inset,color,stroke in [(3,'#0d1512','#0a0c0b'),(6,'#57543a','#b2a477'),(10,'#20271d','#282d22'),(13,'#141c18','#776b49')]:
 k=inset;s=256-k;cut=9;a.poly([(k+cut,k),(s-cut,k),(s,k+cut),(s,s-cut),(s-cut,s),(k+cut,s),(k,s-cut),(k,k+cut)],color,stroke,1.4)
for k in [17,239]:
 for y in [17,239]:a.circle(k,y,2.3,'#c6ac70','#342e1d',.8)
a.line([(23,18),(233,18)],'#847655',.7);a.line([(18,23),(18,230)],'#847655',.7)
# Restrained engraved corner decoration, same art reused in both squares.
for x,y,sx,sy in [(21,21,1,1),(235,21,-1,1),(21,235,1,-1),(235,235,-1,-1)]:
 a.line([(x,y+13*sy),(x,y),(x+13*sx,y)],'#c1ac78',1.1)
a.save('instrument_frame')
a=Art()
hull=curve((128,23),(160,48),(179,92),(175,151))+curve((175,151),(174,174),(168,196),(162,215))+[(94,215)]+curve((94,215),(87,196),(82,174),(81,151))+curve((81,151),(77,92),(96,48),(128,23))
a.poly(hull,'#39291a','#100f0c',4)
deck=curve((128,35),(154,61),(168,96),(166,150))+[(154,203),(102,203),(90,150)]+curve((90,150),(88,96),(102,61),(128,35))
a.poly(deck,'#ad8450','#d6b071',2)
for y,w in [(73,20),(88,27),(104,32),(120,36),(136,37),(152,35),(169,31),(187,27)]:
 a.line([(128-w,y),(128+w,y)],'#705332',1.7);a.line([(129-w,y+2),(127+w,y+2)],'#c59c60',.7)
a.poly([(104,178),(152,178),(148,199),(108,199)],'#59442b','#ddba78',1.8)
a.line([(128,28),(128,60)],'#e6c88c',2.3)
a.circle(128,128,9,'#30251a','#d8b77d',2);a.circle(128,128,4,'#c7a369')
a.save('ship_top')
areas=[]
for state,bulge in enumerate([1.5,19,39,64]):
 a=Art();start=(46,128);end=(210,128)
 front=curve(start,(72,128-bulge*1.15),(184,128-bulge*1.15),end)
 back=curve(end,(175,134+bulge*.13),(81,134+bulge*.13),start)
 a.poly(front+back,'#eadfc2','#524b36',1.6)
 if state:
  a.poly(curve(start,(79,126-bulge*.75),(176,126-bulge*.75),end)+curve(end,(178,128-bulge*.25),(78,128-bulge*.25),start),'#fff5d6')
  for x in [78,103,128,153,178]:
   u=(x-46)/164;top=128-bulge*3.45*u*(1-u)
   a.line(curve((x,top+2),(x-3,top+bulge*.25),(x-3,126),(x,131)), '#c5b997',.75)
 a.line([(43,128),(213,128)],'#35291c',3.4);a.line([(43,127),(213,127)],'#b59861',1.2)
 a.circle(128,128,3.2,'#8d6b3d','#2b2217',1)
 a.save('sail_'+str(state));areas.append(sum(px[3]>128 for px in a.image.getdata()))
a=Art();cx=cy=128
# Eight turned wooden spokes and handles. A brass end cap marks the upper handle.
for i in range(8):
 angle=math.radians(i*45);d=(math.sin(angle),-math.cos(angle));v=(math.cos(angle),math.sin(angle))
 def p(r,w):return (128+d[0]*r+v[0]*w,128+d[1]*r+v[1]*w)
 a.poly([p(13,-4),p(89,-4),p(93,-7),p(104,-5),p(109,0),p(104,5),p(93,7),p(89,4),p(13,4)],'#a9824c','#30271c',2)
 a.line([p(28,-1),p(89,-1)],'#dfbf7c',1.8)
 a.line([p(93,-5),p(93,5)],'#55422b',2)
a.circle(128,128,76,None,'#282119',15);a.circle(128,128,76,None,'#b28b55',10);a.circle(128,128,76,None,'#e0bc7f',2)
a.circle(128,128,69,None,'#655033',1.4);a.circle(128,128,82,None,'#d4af72',1)
for i in range(8):
 ang=math.radians(i*45);a.circle(128+math.sin(ang)*76,128-math.cos(ang)*76,3,'#f0d699','#57442c',1)
a.circle(128,128,20,'#4b3924','#d8b578',3);a.circle(128,128,13,'#ba965c','#30271a',1.7);a.circle(128,128,6,'#f1d795','#6c5331',1.2)
a.circle(128,24,3.7,'#dfb15d','#513f26',1)
a.save('helm')
assert all(a<b for a,b in zip(areas,areas[1:])),areas
(P/'art-report.json').write_text(json.dumps(dict(sailSilhouetteAreas=areas,canvas=[512,512],source='editable vector paths; supersampled raster export'),indent=2))
print('INSTRUMENT_ART_PASS',areas)
