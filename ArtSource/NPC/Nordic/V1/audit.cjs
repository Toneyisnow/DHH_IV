// Review contact and passing poses separately; pixel inequality is not gait validation.
const fs=require('node:fs/promises'),path=require('node:path');
const sharp=require('C:/Users/charl/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
async function main(){const root=__dirname,out=path.join(root,'Audit');await fs.mkdir(out,{recursive:true});
 for(const id of ['01-Male','02-Female','03-Elder','04-Child'])for(const [kind,cols] of [['contact',[0,2]],['passing',[1,3]]]){
 const layers=[];let row=0;
 for(const h of [1,2])for(const m of [1,2]){let col=0;for(const d of ['W','E','N','S'])for(const f of cols){layers.push({input:await sharp(path.join(root,'Review168',`${id}-H${h}-M${m}`,`${d}-${f+1}.png`)).png().toBuffer(),left:col++*168,top:row*168});}row++;}
 await sharp({create:{width:1344,height:672,channels:4,background:'#c9c5bb'}}).composite(layers).png().toFile(path.join(out,id+'-'+kind+'.png'));}
 console.log('8 comparison boards written. Rows H1M1,H1M2,H2M1,H2M2; column pairs W,E,N,S.');}
main().catch(e=>{console.error(e);process.exit(1)});
