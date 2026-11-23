"""Publish editable coastal-city layouts without changing existing city identities."""
from pathlib import Path
import json, uuid
HERE=Path(__file__).resolve().parent
RES=HERE.parents[1]/'MaritimeLegend/Assets/Resources'
def publish(source,target):
    target.parent.mkdir(parents=True,exist_ok=True)
    target.write_bytes(source.read_bytes())
    meta=Path(str(target)+'.meta')
    if not meta.exists(): meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n',encoding='utf-8')
def main():
    catalog=json.loads((HERE/'catalog.json').read_text(encoding='utf-8'))
    game=json.loads((RES/'Data/Cities/ports.json').read_text(encoding='utf-8'))['ports']
    world=json.loads((RES/'Data/StreamedSeasV2/atlas.json').read_text(encoding='utf-8'))['ports']
    ids=[p['gamePortId'] for p in catalog['ports']]
    assert len(ids)==len(set(ids)) and set(ids)=={p['id'] for p in game}=={p['id'] for p in world}
    for city in catalog['ports']:
        source=HERE/(city['id']+'.json');target=RES/'Models/ManualSailing/Ports3D'/(city['id']+'.json')
        if source.exists(): publish(source,target)
        assert target.exists(),city['name']
        layout=json.loads(target.read_text(encoding='utf-8'))
        assert layout['buildings'] and len(layout['wall'])==len(layout['roof'])==3
    publish(HERE/'catalog.json',RES/'Data/StreamedSeasV2/cities.json')
    print(f'PASS: {len(ids)} city layouts published and matched to game/world city IDs.')
if __name__=='__main__': main()
