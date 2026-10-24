"""Verify exported tiles reconstruct the SR image and share exact border pixels."""
from pathlib import Path
from PIL import Image
import numpy as np
root=Path(__file__).resolve().parents[3]
folder=root/'MaritimeLegend/Assets/Resources/Textures/ManualSailing/Tiles'
tiles=[np.asarray(Image.open(folder/f'map_{i:02}.png')) for i in range(28)]
assert all(t.shape==(1088,1088,4) for t in tiles)
edges=0
for y in range(4):
    for x in range(7):
        tile=tiles[y*7+x]
        if x<6:
            assert np.array_equal(tile[:,-64:],tiles[y*7+x+1][:,:64]),('horizontal',x,y)
            edges+=1
        if y<3:
            assert np.array_equal(tile[-64:],tiles[(y+1)*7+x][:64]),('vertical',x,y)
            edges+=1
restored=np.concatenate([np.concatenate([tiles[y*7+x][32:-32,32:-32,:3] for x in range(7)],axis=1) for y in range(4)],axis=0)
source=np.asarray(Image.open(Path(__file__).parent/'Upscale/southeast_asia_4x.png'))
assert np.array_equal(restored[:3764,:6688],source)
print(f'TILE_EXPORT_PASS: {edges} shared borders identical; 6688 x 3764 reconstruction exact')
