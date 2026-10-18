"""Deterministic 2D arm skinning. No generated in-between frames or optical flow."""
import argparse
import json
from pathlib import Path
import av
import cv2
import numpy as np
from PIL import Image
from scipy.spatial import Delaunay

parser = argparse.ArgumentParser()
parser.add_argument('--preview', action='store_true')
args = parser.parse_args()
root = Path(__file__).resolve().parents[2]
art = root/'ArtSource/Character04B/FaceExpressions/Proud/KeyPoseV3'
out = art/'RigAnimation'
out.mkdir(exist_ok=True)
source = np.asarray(Image.open(art/'05-proud-target-v2.png').convert('RGB'),dtype=np.float32)/255
plate = np.asarray(Image.open(art/'05-proud-cleanplate.png').convert('RGB'),dtype=np.float32)/255
h,w = source.shape[:2]
if plate.shape != source.shape:
    plate = cv2.resize(plate,(w,h),interpolation=cv2.INTER_CUBIC).clip(0,1)
assert plate.shape == source.shape

left = [[124,658],[179,637],[276,621],[397,606],[418,640],[407,684],[385,719],[345,768],[344,802],[333,828],[358,865],[424,913],
        [447,902],[480,866],[514,836],[550,815],[578,805],[586,809],[584,816],[558,833],[551,843],[578,840],[599,837],[606,842],[605,849],[591,853],[562,856],[551,867],[579,869],[599,868],[606,873],[605,880],[594,884],[567,885],[550,892],[569,900],[580,901],[586,905],[585,911],[575,916],[549,913],[525,922],[489,945],[465,967],[442,976],[420,973],
        [366,957],[314,939],[275,919],[256,949],[230,967],[206,978],[178,973],[155,960],[131,937],[115,918],[96,893],[81,866],[72,837],[64,811],[57,779],[52,743],[64,728],[83,710],[102,690]]
right = [[881,628],[924,646],[975,676],[1035,705],[1084,737],[1111,766],[1114,786],[1102,823],[1095,847],[1114,874],[1130,899],[1127,921],[1105,954],[1087,979],[1060,996],[1038,996],[1008,982],[974,970],[944,965],[920,967],[903,961],
         [886,944],[865,932],[851,926],[848,918],[855,914],[874,919],[884,921],[878,909],[853,903],[846,896],[847,888],[859,889],[875,894],[871,880],[847,880],[834,875],[832,867],[841,863],[860,867],[853,853],[845,844],[841,834],[845,831],[853,839],[863,846],[877,863],[892,885],[907,909],[923,922],
         [941,911],[971,879],[990,849],[978,821],[960,797],[946,768],[927,740],[909,714],[894,681]]

def mask_for(points):
    mask = np.zeros((h,w),np.uint8)
    cv2.fillPoly(mask,[np.array(points,np.int32)],255)
    return cv2.GaussianBlur(mask.astype(np.float32)/255,(0,0),0.6)

masks = [mask_for(left),mask_for(right)]
# Recover the finger silhouettes from the approved painting, including the
# spaces between fingers. Broad contour polygons alone clipped the fingertips.
hsv=cv2.cvtColor((source*255).astype(np.uint8),cv2.COLOR_RGB2HSV)
skin=((hsv[:,:,0]>3)&(hsv[:,:,0]<27)&(hsv[:,:,1]>55)&(hsv[:,:,1]<180)&(hsv[:,:,2]>205)).astype(np.uint8)
for i,(x0,y0,x1,y1) in enumerate([(425,790,620,990),(825,815,928,980)]):
    cropped=skin[y0:y1,x0:x1]
    number,labels,stats,_=cv2.connectedComponentsWithStats(cropped,8)
    keep=[label for label in range(1,number) if stats[label,cv2.CC_STAT_AREA]>35]
    hand=np.isin(labels,keep).astype(np.uint8)
    hand=cv2.morphologyEx(hand,cv2.MORPH_CLOSE,np.ones((3,3),np.uint8))
    hand=cv2.dilate(hand,np.ones((3,3),np.uint8)).astype(np.float32)
    restored=cv2.GaussianBlur(hand,(0,0),0.5)
    ry,rx=np.mgrid[y0:y1,x0:x1]
    cuff=((rx>=915)&(ry>=920)) if i else ((rx<=440)&(ry>=915))
    restored[cuff]=masks[i][y0:y1,x0:x1][cuff]
    masks[i][y0:y1,x0:x1]=restored
hole = np.maximum(*masks)
hole = cv2.dilate(hole,np.ones((19,19),np.uint8))
hole = cv2.GaussianBlur(hole,(0,0),2)
base = plate.copy()
# Keep the exact approved face and head, while the new clean plate provides a
# continuous cape behind the full swept arm area (not a patch with moving seams).
base[:550] = source[:550]
for y in range(550,610):
    mix=(y-550)/60
    base[y]=source[y]*(1-mix)+plate[y]*mix

def segment_transform(points,a,b,aa,bb):
    v, vv = b-a, bb-aa
    angle = np.arctan2(vv[1],vv[0])-np.arctan2(v[1],v[0])
    c,s=np.cos(angle),np.sin(angle)
    rot=np.array([[c,-s],[s,c]])
    # Longitudinal stretch affects sleeves only; hand transform below is rigid.
    d=points-a
    along=np.outer(d@v/(v@v),v)
    d += along*(np.linalg.norm(vv)/np.linalg.norm(v)-1)
    return d@rot.T+aa, angle

class Arm:
    def __init__(self,points,mask,index):
        self.index=index
        polygon=np.array(points,np.float32)
        lo=np.floor(polygon.min(0)/24)*24-24
        hi=np.ceil(polygon.max(0)/24)*24+24
        gx,gy=np.meshgrid(np.arange(lo[0],hi[0]+1,24),np.arange(lo[1],hi[1]+1,24))
        self.vertices=np.unique(np.vstack([np.c_[gx.ravel(),gy.ravel()],polygon]),axis=0).astype(np.float32)
        self.triangles=Delaunay(self.vertices).simplices
        self.rgba=np.dstack([source*mask[...,None],mask]).astype(np.float32)
        Image.fromarray(np.dstack([(source*255).astype(np.uint8),(mask*255).astype(np.uint8)])).save(out/('left-arm.png' if index==0 else 'right-arm.png'))
        self.shoulder=np.array([390,637] if index==0 else [920,674],np.float32)
        self.elbow=np.array([168,807] if index==0 else [1080,839],np.float32)
        self.wrist=np.array([426,939] if index==0 else [935,933],np.float32)

    def pose(self,amount,t):
        p=self.vertices
        shoulder=self.shoulder
        if self.index==0:
            elbow=self.elbow+np.array([26,-20])*amount
            wrist=self.wrist+np.array([14,-91])*amount
            extra=np.deg2rad(18)*amount
            handweight=np.clip((p[:,0]-350)/70,0,1)*np.clip((p[:,1]-730)/70,0,1)
        else:
            elbow=self.elbow+np.array([-18,-5])*amount
            wrist=self.wrist+np.array([58,52])*amount
            extra=np.deg2rad(-10)*amount
            handweight=np.clip((1000-p[:,0])/65,0,1)*np.clip((p[:,1]-730)/80,0,1)
        upper,_=segment_transform(p,shoulder,self.elbow,shoulder,elbow)
        lower,angle=segment_transform(p,self.elbow,self.wrist,elbow,wrist)
        # Bind to the nearest bone rather than an upper-arm projection, which
        # incorrectly assigned part of the right upper sleeve to the forearm.
        def distance(a,b):
            v=b-a
            u=np.clip((p-a)@v/(v@v),0,1)
            return np.linalg.norm(p-a-u[:,None]*v,axis=1)
        du=distance(shoulder,self.elbow)
        dl=distance(self.elbow,self.wrist)
        blend=du**3/(du**3+dl**3+1e-5)
        transformed=upper*(1-blend[:,None])+lower*blend[:,None]
        c,s=np.cos(angle+extra),np.sin(angle+extra)
        rigid=(p-self.wrist)@np.array([[c,s],[-s,c]])+wrist
        transformed=transformed*(1-handweight[:,None])+rigid*handweight[:,None]
        # Soft sleeve settling; fingers and shoulder attachments excluded.
        settle=1.6*np.exp(-max(t-1.5,0)*4)*np.sin(max(t-1.5,0)*15)
        transformed[:,0] += settle*(1-handweight)*blend
        return transformed.astype(np.float32)

    def render(self,vertices):
        mx=np.full((h,w),-1,np.float32)
        my=np.full((h,w),-1,np.float32)
        for indices in self.triangles:
            dst=vertices[indices]
            src=self.vertices[indices]
            x0,y0=np.maximum(np.floor(dst.min(0)).astype(int),[0,0])
            x1,y1=np.minimum(np.ceil(dst.max(0)).astype(int)+1,[w,h])
            if x1<=x0 or y1<=y0: continue
            d1,d2=dst[1]-dst[0],dst[2]-dst[0]
            det=d1[0]*d2[1]-d1[1]*d2[0]
            if abs(det)<1e-4: continue
            xx,yy=np.meshgrid(np.arange(x0,x1,dtype=np.float32)-dst[0,0],np.arange(y0,y1,dtype=np.float32)-dst[0,1])
            u=(xx*d2[1]-yy*d2[0])/det
            v=(d1[0]*yy-d1[1]*xx)/det
            inside=(u>=-1e-4)&(v>=-1e-4)&(u+v<=1.0001)
            sx=src[0,0]+u*(src[1,0]-src[0,0])+v*(src[2,0]-src[0,0])
            sy=src[0,1]+u*(src[1,1]-src[0,1])+v*(src[2,1]-src[0,1])
            mx[y0:y1,x0:x1][inside]=sx[inside]
            my[y0:y1,x0:x1][inside]=sy[inside]
        return cv2.remap(self.rgba,mx,my,cv2.INTER_CUBIC,borderMode=cv2.BORDER_CONSTANT).clip(0,1)

arms=[Arm(left,masks[0],0),Arm(right,masks[1],1)]
fps,count=24,72
indices=[0,12,24,36,48,71] if args.preview else range(count)
selected=[]
video=None
if not args.preview:
    video=av.open(str(out/'05-proud-rig-v1.mp4'),'w')
    stream=video.add_stream('libx264',rate=fps)
    stream.width=w+(w%2)
    stream.height=h+(h%2)
    stream.pix_fmt='yuv420p'
    stream.options={'crf':'14','preset':'slow'}
face_diff=0
torso_diff=0
compass_diff=0
base_pixels=(base*255).round().astype(np.uint8)
gy,gx=np.mgrid[:h,:w].astype(np.float32)
cape=mask_for([[145,1040],[215,1010],[277,1080],[270,1170],[235,1265],[209,1280],[158,1180]])
cape*=np.clip((gy-1000)/240,0,1)
for index in indices:
    t=index/fps
    progress=np.clip((t-.15)/1.35,0,1)
    ease=progress**3*(10-15*progress+6*progress**2)
    wave=3.0*np.sin(np.pi*np.clip((t-.25)/1.7,0,1))**2*np.sin(t*7)
    frame=cv2.remap(base,(gx-cape*wave).astype(np.float32),gy,cv2.INTER_CUBIC,borderMode=cv2.BORDER_REFLECT_101)
    for arm in arms:
        rgba=arm.render(arm.pose(1-ease,t))
        frame=frame*(1-rgba[...,3:])+rgba[...,:3]
    pixels=(frame.clip(0,1)*255).round().astype(np.uint8)
    face_diff=max(face_diff,int(np.abs(pixels[:450].astype(int)-(source[:450]*255).round().astype(int)).max()))
    torso_diff=max(torso_diff,int(np.abs(pixels[600:1100,660:810].astype(int)-base_pixels[600:1100,660:810].astype(int)).max()))
    compass_diff=max(compass_diff,int(np.abs(pixels[1000:1140,505:650].astype(int)-base_pixels[1000:1140,505:650].astype(int)).max()))
    if index in [0,12,24,36,48,71]:
        Image.fromarray(pixels).save(out/f'frame-{index:02}.png')
        selected.append(pixels)
    if video:
        padded=cv2.copyMakeBorder(pixels,0,h%2,0,w%2,cv2.BORDER_REPLICATE)
        for packet in stream.encode(av.VideoFrame.from_ndarray(padded,format='rgb24')): video.mux(packet)
    print('frame',index,flush=True)
if video:
    for packet in stream.encode():video.mux(packet)
    video.close()
assert face_diff==0
print('fixed-region checks',face_diff,torso_diff,compass_diff,flush=True)
assert torso_diff==0 and compass_diff==0
sheet=Image.new('RGB',(3*400,2*437))
for i,pixels in enumerate(selected): sheet.paste(Image.fromarray(pixels).resize((400,437)),((i%3)*400,(i//3)*437))
sheet.save(out/'contact-sheet.jpg',quality=95)
Image.fromarray((base*255).round().astype(np.uint8)).save(out/'fixed-body.png')
(out/'rig-validation.json').write_text(json.dumps({'fps':fps,'frames':count,'width':w+(w%2),'height':h+(h%2),'face_max_difference':face_diff,'central_torso_max_difference':torso_diff,'compass_max_difference':compass_diff,'method':'2D shoulder/elbow/wrist skinning with rigid hand texture; generated clean plate only; no generated intermediate frames','scope':'raised-hands approach to hips; smile held from key art; not a full original-pose transition','rendered_full_video':not args.preview},indent=2),encoding='utf-8')
