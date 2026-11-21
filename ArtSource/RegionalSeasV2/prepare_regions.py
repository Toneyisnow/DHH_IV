"""Author seven regional maps with the unchanged Southeast Asia V2 heightfield recipe.
These illustration spaces are regional art, NOT yet a globally registered atlas.
"""
from pathlib import Path
import importlib.util, json, hashlib, struct
import numpy as np
from PIL import Image
from scipy import ndimage as nd
ROOT=Path(__file__).resolve().parents[2]
HERE=Path(__file__).resolve().parent
RES=ROOT/'MaritimeLegend/Assets/Resources'
REGIONS=['north_sea','mediterranean_sea','africa','indian_ocean','southeast_asia','east_asia','new_world']
spec=importlib.util.spec_from_file_location('sea_recipe',ROOT/'ArtSource/ManualSailingV2/SoutheastAsia/prepare_terrain.py')
recipe=importlib.util.module_from_spec(spec);spec.loader.exec_module(recipe)
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def main():
    reports=[]
    for region in REGIONS:
        out=RES/'Models/Environment/RegionalSeasV2'/region
        source=ROOT/'Assets/AutoSailing_Map_Pack/01_map'/f'{region}.png'
        report=recipe.generate(source,out)
        raw=(out/'terrain.bytes').read_bytes()
        w,h,ww,hh=struct.unpack_from('<IIff',raw)
        arrays=np.frombuffer(raw,dtype='<f4',offset=16).reshape(3,h,w)
        assert np.isfinite(arrays).all() and len(raw)==16+w*h*12
        # Collision in original CoastNavigation's pixel coordinates, derived from this relief.
        land=np.asarray(Image.fromarray((arrays[0]>=0).astype('uint8')*255).resize((1672,941),Image.Resampling.NEAREST))>0
        distance=(nd.distance_transform_edt(~land)-nd.distance_transform_edt(land)).astype('<f4')
        (out/'coast-distance.bytes').write_bytes(struct.pack('<II',1672,941)+distance.tobytes())
        # Reproducible review anchorage: open water near a visible shoreline, away from borders.
        coast=arrays[1];yy,xx=np.mgrid[:h,:w]
        candidates=(coast<-16)&(coast>-24)&(xx>100)&(xx<w-100)&(yy>100)&(yy<h-100)
        for dx,dy in [(-.01,.018),(.01,.036)]:
            fx=np.clip(np.rint(xx+dx*(w-1)).astype(int),0,w-1)
            fy=np.clip(np.rint(yy+dy*(h-1)).astype(int),0,h-1)
            candidates &= coast[fy,fx] < -10
        score=np.where(candidates,(xx-w*.5)**2+(yy-h*.5)**2,np.inf)
        iy,ix=np.unravel_index(np.argmin(score),score.shape)
        assert np.isfinite(score[iy,ix])
        uv=[float(ix/(w-1)),float(iy/(h-1))]
        # Review only. Actual departures remain the existing port catalog until geo-registration.
        (out/'review.json').write_text(json.dumps({'x':uv[0],'y':uv[1]}))
        report.update(id=region,terrain_sha256=sha(out/'terrain.bytes'),source_sha256=sha(source),review_uv=uv)
        if region=='southeast_asia':
            report['baseline_byte_identical']=sha(out/'terrain.bytes')==sha(RES/'Models/Environment/SoutheastAsiaV2/terrain.bytes')
            assert report['baseline_byte_identical'], 'SEA baseline drift: stop publication'
        # Diagnostic topdown image, explicitly separate from in-engine review renders.
        color=np.zeros((h,w,3),dtype=np.uint8);color[:]=[19,110,164]
        color[arrays[1]>-5]=[35,177,176];color[arrays[0]>0]=[207,181,112]
        color[arrays[0]>5]=[164,140,88];color[arrays[2]>.3]=[83,110,56]
        review=HERE/'Review';review.mkdir(exist_ok=True)
        Image.fromarray(color).save(review/f'{region}-mask.png')
        reports.append(report)
        print(region, report['land_fraction'], report['terrain_sha256'][:12])
    (HERE/'Review/generation.json').write_text(json.dumps(reports,indent=2))
if __name__=='__main__':main()
