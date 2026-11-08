const fs=require('node:fs/promises'),path=require('node:path');
async function main(){
 const root=__dirname;
 let source=await fs.readFile(path.join(root,'pack-review.cjs'),'utf8');
 for(const id of ['04','05']){
  let s=source.replace("'Character06'","'Character'+process.argv[2]").replace("'cartoon-walk-B-repair-v2.png'","'cartoon-walk-B-v1.png'");
  s=s.replace(",['cartoon-S-repair-v2.png',['S']]",id==='04'?",['cartoon-NW-repair-v2.png',['NW']]":'');
  s=s.replace("if(file==='cartoon-walk-A-v1.png'&&row===0)continue;",id==='04'?"if(file==='cartoon-walk-A-v1.png'&&row===3)continue;":'');
  s=s.replace("character:'V2-06'","character:'V2-'+process.argv[2]");
  await fs.writeFile(path.join(root,'pack-character'+id+'.cjs'),s);
 }
}
main();
