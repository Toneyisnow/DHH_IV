"""Bake a small terrain index from the actual streamed sailing tiles (south-up rows)."""
from pathlib import Path
import json, struct, uuid
ROOT = Path(__file__).resolve().parents[2]
DATA = ROOT / 'MaritimeLegend/Assets/Resources/Data/StreamedSeasV2'
def main():
    atlas = json.loads((DATA / 'atlas.json').read_text(encoding='utf-8'))
    w, h = 1152, 512
    pixels = bytearray(w*h)
    for key, ocean in enumerate(atlas['ocean']):
        if ocean: continue
        cx, cy = key%72, key//72
        raw = (DATA / 'Chunks' / f'{cx:02d}_{cy:02d}.bytes').read_bytes()
        assert struct.unpack_from('<4i',raw) == (0x53563232,cx,cy,131)
        for y in range(16):
            for x in range(16):
                k=(y*8+5)*131+x*8+5
                height=struct.unpack_from('<f',raw,16+k*4)[0]
                coast=struct.unpack_from('<f',raw,16+(131*131+k)*4)[0]
                pixels[(cy*16+y)*w+cx*16+x] = 0 if coast<=0 else 3 if height>15 else 2 if height>5 else 1
    out=DATA/'overview.bytes'
    out.write_bytes(pixels)
    meta=Path(str(out)+'.meta')
    if not meta.exists(): meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nTextScriptImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n')
    print(f'Overview baked: {w}x{h}, terrain categories: {sorted(set(pixels))}')
if __name__=='__main__': main()
