const sharp=require('C:/Users/charl/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const path=require('node:path'),fs=require('node:fs/promises');
async function main(){
 const base=path.join(__dirname,'Character06'),dest=path.join(base,'Loop');await fs.mkdir(dest,{recursive:true});
 const dirs=['N','NE','E','SE','S','SW','W','NW'];
 for(let frame=1;frame<=4;frame++){
  const layers=[];
  for(let i=0;i<8;i++){
   const input=await sharp(path.join(base,'Review168',dirs[i]+'-'+String(frame).padStart(2,'0')+'.png')).resize(336,336,{kernel:'nearest'}).png().toBuffer();
   layers.push({input,left:(i%4)*336,top:Math.floor(i/4)*366+30});
   const label=Buffer.from('<svg width="336" height="30"><text x="168" y="23" fill="#e9e0c9" font-family="sans-serif" text-anchor="middle" font-size="22">'+dirs[i]+'</text></svg>');
   layers.push({input:label,left:i%4*336,top:Math.floor(i/4)*366});
  }
  await sharp({create:{width:1344,height:732,channels:4,background:'#29323b'}}).composite(layers).png().toFile(path.join(dest,'frame-'+String(frame).padStart(2,'0')+'.png'));
 }
}
main().catch(e=>{console.error(e);process.exitCode=1});
