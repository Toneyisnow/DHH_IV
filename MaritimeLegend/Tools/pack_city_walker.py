"""Pack Blender renders without resizing or changing their shared foot pivot."""
from pathlib import Path
import json
from PIL import Image

root = Path(__file__).resolve().parents[1]
out = root / 'Assets/Resources/CityDaylight'
atlas = Image.new('RGBA', (1024, 1280))
rects = []
for frame in range(8):
    for direction in range(8):
        source = root.parent / f'ArtSource/CityDaylight/walker_frames/{frame}_{direction}.png'
        with Image.open(source) as image:
            assert image.size == (128, 160), source
            atlas.paste(image, (direction * 128, frame * 160))
        rects.append(dict(x=direction / 8, y=1 - (frame + 1) / 8, width=1 / 8, height=1 / 8))
atlas.save(out / 'Walker.png')
(out / 'Walker.json').write_text(json.dumps(dict(columns=8, rows=8, rects=rects)), encoding='utf-8')
print('Packed 64 registered RGBA frames; columns S, SW, W, NW, N, NE, E, SE.')
