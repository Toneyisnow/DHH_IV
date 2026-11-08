const fs=require('node:fs/promises'),path=require('node:path');
const sharp=require('C:/Users/charl/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
async function main(){const root=__dirname,variants=JSON.parse(await fs.readFile(path.join(root,'variants.json'),'utf8'));await fs.mkdir(path.join(root,'Loop'),{recursive:true});
 for(let f=0;f<5;f++){const layers=[];for(let v=0;v<3;v++){
 const input=await sharp(path.join(root,'Composites',variants[v].id+'.png')).extract({left:f*168,top:4*168,width:168,height:168}).resize(336,336,{kernel:'nearest'}).png().toBuffer();layers.push({input,left:v*336,top:34});
 const label=Buffer.from(`<svg width="336" height="34"><text x="168" y="26" fill="#f5e2b5" text-anchor="middle" font-family="Microsoft YaHei, sans-serif" font-size="20">${v+1} · ${variants[v].name}</text></svg>`);layers.push({input:label,left:v*336,top:0});
 }await sharp({create:{width:1008,height:370,channels:4,background:'#29323b'}}).composite(layers).png().toFile(path.join(root,f===4?'front-comparison.png':'Loop/frame-'+String(f+1).padStart(2,'0')+'.png'));}
}main().catch(e=>{console.error(e);process.exitCode=1});
