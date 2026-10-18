"""Author the initial 1016 mock configuration and its schemas; not a runtime dependency."""
from pathlib import Path
import json

ROOT = Path(__file__).resolve().parents[1]
DATA = ROOT / 'Assets/Resources/Data'
def read(path): return json.loads(path.read_text(encoding='utf-8-sig'))
def write(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
def schema(value):
    if isinstance(value, dict):
        return dict(type='object', required=list(value), additionalProperties=False, properties={k:schema(v) for k,v in value.items()})
    if isinstance(value, list): return dict(type='array', items=schema(value[0]) if value else {'type':'string'})
    if isinstance(value, bool): return {'type':'boolean'}
    if isinstance(value, str): return {'type':'string'}
    return {'type':'integer' if isinstance(value,int) else 'number'}

def main():
    manifest=read(DATA/'manifest.json'); index=read(DATA/'Schemas/index.json')
    params=dict(realSecondsPerGameDay=180.0,foodPerCrewDay=.20,waterPerCrewDay=.30,logisticsFactor=1.0,
        supplyReductionPerLevel=.01,minimumSupplyFactor=.5,energyPerDay=6.0,minimumEnergyPerDay=1.0,
        navigationEnergyReduction=.1,supplyEnergyReduction=.1,initialWater=100.0,maximumEnergy=100.0,
        baseDailyXp=5,xpPerNavigationLevel=0,xpPerVisionLevel=0,minimumDailyDistance=.01,
        shortageSpeedFactor=.25,turnDegreesPerSecond=90.0,sailDegreesPerSecond=45.0,
        defaultSailDegrees=0.0,declineSeconds=10.0,iconLifetimeSeconds=360.0,contactRadius=.025,
        waterUnitPrice=1.0,energyUnitPrice=.1)
    rules=[]; fields={}
    for key,value in params.items():
        lo,hi=0,10000
        if key in ('realSecondsPerGameDay','initialWater','maximumEnergy','iconLifetimeSeconds'): lo=.01
        if key in ('minimumSupplyFactor','shortageSpeedFactor','logisticsFactor'): lo=.01
        if key in ('minimumSupplyFactor','shortageSpeedFactor','supplyReductionPerLevel','contactRadius'): hi=1
        if key=='defaultSailDegrees': lo,hi=-90,90
        rules.append(dict(name=key,integer=isinstance(value,int),minimum=lo,maximum=hi))
        fields[key]=dict(type='integer' if isinstance(value,int) else 'number',minimum=lo,maximum=hi)
    write(DATA/'Balance/sailing.json',{'sailing':params})
    write(DATA/'Schemas/sailing.constraints.json',dict(group='sailing',fields=rules))
    write(DATA/'Schemas/sailing.schema.json',dict(type='object',required=['sailing'],additionalProperties=False,properties={'sailing':dict(type='object',required=list(params),additionalProperties=False,properties=fields)}))
    if 'Balance/sailing' not in manifest['balance']: manifest['balance'].append('Balance/sailing')
    if 'Schemas/sailing.constraints' not in manifest['schemas']: manifest['schemas'].append('Schemas/sailing.constraints')
    index['documents']['Balance/sailing']='Schemas/sailing.schema'
    regions=[]
    for id,rate,weights,low,high,pool in [
        ('north_sea',.35,[45,30,5,20],1,6,['p19','p8']),
        ('mediterranean_sea',.40,[35,40,5,20],2,8,['p19','p13']),
        ('africa',.30,[20,40,10,30],2,10,['p19','p43']),
        ('indian_ocean',.45,[20,35,20,25],2,12,['p19','p14']),
        ('southeast_asia',.55,[20,45,20,15],3,12,['p19','p20']),
        ('east_asia',.35,[40,25,20,15],1,7,['p19','p13']),
        ('new_world',.50,[25,35,10,30],2,12,['p19','p43'])]:
        regions.append(dict(id=id,nameKey='sea.'+id+'.name',mapAssetKey='Sailing/SeaMaps/'+id,mock=True,eventsPerGameDay=rate,maxIcons=4,quantityMin=low,quantityMax=high,
            weights=[dict(encounterId=e,weight=w) for e,w in zip(['bottle','crate','mermaid','wreck'],weights)],rewardProducts=pool))
    encounters=[dict(id=id,promptKey='sailing.event.'+id+'.prompt',resultKey='sailing.event.'+id+'.result',rewardKind=kind,
        exclusiveGroup='',followupId='',mock=True,energyCost=cost,quality=5,saveVersion=1,prerequisiteFlags=[]) for id,kind,cost in [('bottle','clue',4),('crate','goods',8),('mermaid','clue',10),('wreck','goods',12)]]
    backgrounds=[dict(id=id,nameKey='background.'+id+'.name',prologueKey='background.'+id+'.prologue',materialProductId='p19' if id=='artisan' else '',mock=True,
        goldMultiplier=gold,supplyMultiplier=supply,hullMultiplier=hull,materialQuantity=8 if id=='artisan' else 0)
        for id,gold,supply,hull in [('merchant',1.2,1.,1.),('explorer',.9,1.2,1.),('artisan',1.,1.,1.),('escort',1.,1.,1.1)]]
    docs={'Sailing/regions':{'seaRegions':regions},'Sailing/encounters':{'seaEncounters':encounters},
        'Sailing/defaults':{'sailingDefaults':dict(mock=True,supplySkillId='补给',crewByShipType=[8,12,20,30],portRegions=['mediterranean_sea']*5+['indian_ocean','indian_ocean','east_asia'])},
        'Characters/backgrounds':{'backgrounds':backgrounds},
        'Presentation/localization-plan':{'localizationPlan':dict(sourceLocale='zh-CN',fallbackLocale='zh-CN',locales=['zh-CN','en-US','zh-TW','ja-JP','ko-KR'],translationsComplete=False)}}
    for path,value in docs.items():
        write(DATA/(path+'.json'),value)
        sp='Schemas/Documents/'+path+'.schema'
        write(DATA/(sp+'.json'),schema(value));index['documents'][path]=sp
        if path not in manifest['content']:manifest['content'].append(path)
    write(DATA/'manifest.json',manifest);write(DATA/'Schemas/index.json',index)

if __name__=='__main__':main()
