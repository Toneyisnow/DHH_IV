from pathlib import Path
import json,struct
import numpy as np
ROOT=Path(__file__).resolve().parents[2];HERE=Path(__file__).resolve().parent
OUT=ROOT/'MaritimeLegend/Assets/Resources/Data/StreamedSeasV2';manifest=json.loads((OUT/'atlas.json').read_text(encoding='utf-8'))
cache={};checks=0;maximum=0
def data(x,y):
    key=y*72+x
    if key not in cache:
        if manifest['ocean'][key]:
            a=np.zeros((3,131,131),np.float32);a[:2]=-32
        else:
            raw=(OUT/'Chunks'/f'{x:02d}_{y:02d}.bytes').read_bytes()
            assert struct.unpack_from('<4i',raw)==(0x53563232,x,y,131)
            a=np.frombuffer(raw,dtype='<f4',offset=16).reshape(3,131,131)
            assert np.isfinite(a).all()
        cache[key]=a
    return cache[key]
for y in range(32):
    for x in range(72):
        a=data(x,y);b=data((x+1)%72,y)
        for channel in range(3):
            delta=np.abs(a[channel,1:130,129]-b[channel,1:130,1]);maximum=max(maximum,float(delta.max()))
            assert np.max(delta)<.011,(x,y,channel,'east',float(delta.max()))
            checks+=129
        # Halo gradients must agree too: no normal/lighting seam between separately loaded meshes.
        assert np.allclose(a[0,1:130,130]-a[0,1:130,128],b[0,1:130,2]-b[0,1:130,0],atol=.011)
        if y<31:
            b=data(x,y+1)
            assert np.allclose(a[:,129,1:130],b[:,1,1:130],atol=.011),(x,y,'north')
            assert np.allclose(a[0,130,1:130]-a[0,128,1:130],b[0,2,1:130]-b[0,0,1:130],atol=.011)
            checks+=387
    # Bound verifier memory as well as runtime memory.
    cache={k:v for k,v in cache.items() if k//72>=y}
for p in manifest['ports']:
    x,y=p['x'],p['y'];a=data(int(x/160),min(31,int(y/160)))
    ix=int(round((x%160)/1.25))+1;iy=int(round((y%160)/1.25))+1
    assert -a[1,iy,ix]>=9*960/1672,p['name']
text=f'PASS {checks} shared-edge samples including east/west wrap; height, coast, forest and normal halos. Maximum shared-edge delta {maximum}. All {len(manifest["ports"])} departure berths safe.\n'
(HERE/'Review/atlas-checks.txt').write_text(text);print(text)
