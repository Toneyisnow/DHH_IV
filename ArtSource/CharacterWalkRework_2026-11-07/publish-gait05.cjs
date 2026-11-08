const fs=require('node:fs/promises'),sharp=require('C:/Users/charl/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const root=__dirname+'/Character05',base=root+'/GaitRepair-v2',dirs=['N','NE','E','SE','S','SW','W','NW'];
async function main(){
 const checks={changedDirections:['NE','SW','W','NW'],approvedSEUnchanged:true,approvedEUnchanged:true,idlesUnchanged:true,headsUnchanged:true,changedWalkFrames:0};
 for(const d of dirs)for(let f=1;f<=5;f++){
  const name=d+'-'+(f===5?'idle':'0'+f)+'.png',old=await sharp(base+'/Before/'+name).ensureAlpha().raw().toBuffer();let out=Buffer.from(old);
  if(f<5&&checks.changedDirections.includes(d)){
   out=await sharp(base+'/FinalFrames/'+name).ensureAlpha().raw().toBuffer();
   // Preserve original face/hair/headband region, including its authored bob.
   old.copy(out,0,0,168*70*4);
   if(!out.equals(old))checks.changedWalkFrames++;
  }
  if(d==='W'&&f===5){
   const clean=await sharp(base+'/FinalFrames/W-idle.png').ensureAlpha().raw().toBuffer();
   for(let y=108;y<130;y++)for(let x=79;x<101;x++)if(x>=86||y>=118){const n=(y*168+x)*4;clean.copy(out,n,n,n+4);}
   checks.idleEquipmentCorrection='W hip pouch region only; original pose retained';
  }
  if(d==='SE'&&!out.equals(old))checks.approvedSEUnchanged=false;
  if(d==='E'&&!out.equals(old))checks.approvedEUnchanged=false;
  if(f===5&&!out.equals(old))checks.idlesUnchanged=false;
  if(!out.subarray(0,168*70*4).equals(old.subarray(0,168*70*4)))checks.headsUnchanged=false;
  await sharp(out,{raw:{width:168,height:168,channels:4}}).png().toFile(root+'/Review168/'+name);
 }
 await fs.writeFile(base+'/checks.json',JSON.stringify(checks,null,2));console.log(checks);
 const display=['SE','SW','NE','NW','E','W'];
 for(let f=1;f<=4;f++){
  const layers=[];
  for(let i=0;i<display.length;i++){
   const d=display[i],x=i%3*336,y=Math.floor(i/3)*366;
   layers.push({input:await sharp(root+'/Review168/'+d+'-0'+f+'.png').resize(336,336,{kernel:'nearest'}).png().toBuffer(),left:x,top:y+30});
   layers.push({input:Buffer.from('<svg width="336" height="30"><text x="168" y="23" fill="#eee" font-family="sans-serif" text-anchor="middle" font-size="20">'+d+' · '+f+'</text></svg>'),left:x,top:y});
  }
  await sharp({create:{width:1008,height:732,channels:4,background:'#29323b'}}).composite(layers).png().toFile(base+'/preview-0'+f+'.png');
 }
 const strip=[];for(let i=0;i<4;i++)for(let f=1;f<=4;f++){const d=checks.changedDirections[i];strip.push({input:await sharp(root+'/Review168/'+d+'-0'+f+'.png').resize(336,336,{kernel:'nearest'}).png().toBuffer(),left:(f-1)*336,top:i*366+30});if(f===1)strip.push({input:Buffer.from('<svg width="1344" height="30"><text x="20" y="23" fill="#eee" font-family="sans-serif" font-size="22">'+d+' : 1 contact / 2 passing / 3 opposite / 4 passing</text></svg>'),left:0,top:i*366});}
 await sharp({create:{width:1344,height:1464,channels:4,background:'#29323b'}}).composite(strip).png().toFile(base+'/contact-review.png');
}
main().catch(e=>{console.error(e);process.exitCode=1});
