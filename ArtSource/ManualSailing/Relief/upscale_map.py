"""Registered 4x map super-resolution; overlap blending, then shared-pixel gutters.
Run with the existing H:/DHH_IV/ComfyUI/.venv runtime. No geographic regeneration.
"""
from pathlib import Path
import hashlib, json
import numpy as np
from PIL import Image
import torch
from spandrel import ModelLoader

ROOT = Path(__file__).resolve().parents[3]
OUT = Path(__file__).parent / 'Upscale'
OUT.mkdir(exist_ok=True)
DEST = ROOT / 'MaritimeLegend/Assets/Resources/Textures/ManualSailing/Tiles'
DEST.mkdir(parents=True, exist_ok=True)
MODEL = Path('H:/DHH_IV/ComfyUI/models/upscale_models/RealESRGAN_x4plus.pth')
source = Image.open(ROOT / 'Assets/AutoSailing_Map_Pack/01_map/southeast_asia.png').convert('RGB')
src = np.asarray(source).astype(np.float32) / 255
h, w = src.shape[:2]
scale, core, overlap, gutter = 4, 256, 32, 8
model = ModelLoader().load_from_state_dict(torch.load(MODEL, map_location='cpu', weights_only=True)).eval().cuda()
canvas = np.zeros((h*scale,w*scale,3), np.float32)
weights = np.zeros((h*scale,w*scale,1), np.float32)
with torch.inference_mode():
    for y in range(0,h,core):
        for x in range(0,w,core):
            x0,y0=max(0,x-overlap),max(0,y-overlap)
            x1,y1=min(w,x+core+overlap),min(h,y+core+overlap)
            tensor=torch.from_numpy(src[y0:y1,x0:x1].copy()).permute(2,0,1).unsqueeze(0).cuda()
            result=model(tensor).squeeze(0).permute(1,2,0).clamp(0,1).cpu().numpy()
            # Feather the extra context; adjacent tiles contribute to the same pixels.
            wx=np.ones(result.shape[1],np.float32);wy=np.ones(result.shape[0],np.float32)
            n=overlap*scale
            if x0>0: wx[:n]=np.linspace(.001,1,n)
            if x1<w: wx[-n:]=np.linspace(1,.001,n)
            if y0>0: wy[:n]=np.linspace(.001,1,n)
            if y1<h: wy[-n:]=np.linspace(1,.001,n)
            weight=(wy[:,None]*wx[None,:])[...,None]
            canvas[y0*scale:y1*scale,x0*scale:x1*scale]+=result*weight
            weights[y0*scale:y1*scale,x0*scale:x1*scale]+=weight
            print(f'upscaled {x},{y}',flush=True)
assert weights.min()>0
pixels=np.round(canvas/weights*255).clip(0,255).astype(np.uint8)
full=Image.fromarray(pixels)
full.save(OUT/'southeast_asia_4x.png')
padded=np.pad(pixels,((gutter*scale,(core*4-h+gutter)*scale),(gutter*scale,(core*7-w+gutter)*scale),(0,0)),mode='edge')
for ty in range(4):
    for tx in range(7):
        tile=padded[ty*core*scale:(ty*core+core+2*gutter)*scale,tx*core*scale:(tx*core+core+2*gutter)*scale]
        Image.fromarray(tile).convert('RGBA').save(DEST/f'map_{ty*7+tx:02}.png')
# Compare at identical scale, without a misleading change in visible area.
box=(525,495,685,615)
review=Image.new('RGB',(1280,480))
review.paste(source.crop(box).resize((640,480),Image.Resampling.BILINEAR),(0,0))
review.paste(full.crop(tuple(v*4 for v in box)),(640,0))
review.save(OUT/'comparison.png')
(OUT/'manifest.json').write_text(json.dumps(dict(source_size=[w,h],output_size=full.size,scale=scale,grid=[7,4],tile_size=1088,gutter=32,model=str(MODEL),model_sha256=hashlib.sha256(MODEL.read_bytes()).hexdigest(),model_source='https://github.com/xinntao/Real-ESRGAN/releases/tag/v0.1.0'),indent=2))
print('UPSCALE_PASS',flush=True)
