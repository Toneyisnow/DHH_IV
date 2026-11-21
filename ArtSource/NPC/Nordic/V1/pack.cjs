// Deterministic crop, alpha cleanup and nearest-neighbour packing. Does not invent poses.
const fs=require('node:fs/promises'),path=require('node:path');
const sharp=require('C:/Users/charl/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const root=__dirname,dirs=['N','S','W','E'],size=168;
async function main(){
 const files=(await fs.readdir(path.join(root,'Sources'))).filter(f=>f.endsWith('.png')&&!f.includes('alpha-v2'));
 const report=[],overview=[];
 for(const file of files){
  const key=file.slice(0,-4),source=file==='02-Female-H2-M1.png'?'02-Female-H2-M1-alpha-v2.png':file;
  const im=sharp(path.join(root,'Sources',source)),meta=await im.metadata(),frames=[];
  for(let row=0;row<4;row++)for(let col=0;col<5;col++){
   // Three generated sheets place their first character right of the nominal cell centre.
   const cuts=['02-Female-H1-M2','02-Female-H2-M1','03-Elder-H2-M1'].includes(key)?[0,.246,.4,.6,.8,1]:[0,.2,.4,.6,.8,1];
   const x=Math.round(cuts[col]*meta.width),y=Math.round(row*meta.height/4),w=Math.round(cuts[col+1]*meta.width)-x,h=Math.round((row+1)*meta.height/4)-y;
   const {data,info}=await im.clone().extract({left:x,top:y,width:w,height:h}).ensureAlpha().raw().toBuffer({resolveWithObject:true});
   for(let i=0;i<data.length;i+=4){const r=data[i],g=data[i+1],b=data[i+2];if(data[i+3]<150||(r>190&&g<75&&b<75)||(r>170&&g>170&&b<65))data[i+3]=0;}
   const seen=new Uint8Array(w*h),parts=[];
   for(let n=0;n<w*h;n++)if(data[n*4+3]&&!seen[n]){const q=[n];seen[n]=1;for(let k=0;k<q.length;k++){const v=q[k],px=v%w,py=Math.floor(v/w);for(const z of [px>0?v-1:-1,px<w-1?v+1:-1,py>0?v-w:-1,py<h-1?v+w:-1])if(z>=0&&!seen[z]&&data[z*4+3]){seen[z]=1;q.push(z);}}parts.push(q);}
   const largest=Math.max(...parts.map(p=>p.length));for(const p of parts)if(p.length<largest*.012)for(const n of p)data[n*4+3]=0;
   let l=w,t=h,r=0,b=0,count=0;for(let py=0;py<h;py++)for(let px=0;px<w;px++)if(data[(py*w+px)*4+3]){l=Math.min(l,px);r=Math.max(r,px);t=Math.min(t,py);b=Math.max(b,py);count++;}
   if(!count)throw Error('Empty '+key+row+col);
   let hl=w,hr=0;for(let py=t;py<t+(b-t)*.28;py++)for(let px=l;px<=r;px++)if(data[(py*w+px)*4+3]){hl=Math.min(hl,px);hr=Math.max(hr,px);}
   const bounds={left:l,top:t,width:r-l+1,height:b-t+1};
   frames.push({row,col,raw:await sharp(data,{raw:info}).extract(bounds).png().toBuffer(),w:bounds.width,h:bounds.height,head:(hl+hr)/2-l});
   report.push({key,direction:dirs[row],frame:col,source,x:x+l,y:y+t,width:bounds.width,height:bounds.height,touchesEdge:l===0||t===0||r===w-1||b===h-1});
  }
  const dest=path.join(root,'Review168',key);await fs.mkdir(dest,{recursive:true});
  const target=key.startsWith('04')?112:key.startsWith('03')?138:key.startsWith('02')?143:148;
  const scale=target/Math.max(...frames.map(f=>f.h)),layers=[];
  for(const f of frames){const w=Math.round(f.w*scale),h=Math.round(f.h*scale),left=Math.round(84-f.head*scale),top=158-h;if(left<0||left+w>168||top<0)throw Error('Overflow '+key);
   const input=await sharp(f.raw).resize(w,h,{kernel:'nearest'}).png().toBuffer();
   const png=await sharp({create:{width:size,height:size,channels:4,background:'#00000000'}}).composite([{input,left,top}]).png().toBuffer();
   await fs.writeFile(path.join(dest,dirs[f.row]+'-'+(f.col===4?'idle':f.col+1)+'.png'),png);layers.push({input:png,left:f.col*size,top:f.row*size});
   if(f.row===1&&f.col===4)overview.push({input:png,left:(Number(key[0]+key[1])-1)*size,top:((Number(key.match(/H(\d)/)[1])-1)*2+Number(key.match(/M(\d)/)[1])-1)*size});
  }
  await sharp({create:{width:840,height:672,channels:4,background:'#00000000'}}).composite(layers).png().toFile(path.join(dest,'walk-sheet.png'));
 }
 await sharp({create:{width:672,height:672,channels:4,background:'#e6dfcf'}}).composite(overview).png().toFile(path.join(root,'lineup.png'));
 await fs.writeFile(path.join(root,'checks.json'),JSON.stringify({sheetCount:files.length,frameCount:report.length,cellSize:168,rows:dirs,walkColumns:[0,1,2,3],idleColumn:4,pivotBottomLeft:[84,10],filter:'Point',status:'art-review-prototype',layered:false,unityVerified:false,gaitApproved:false,edgeWarnings:report.filter(r=>r.touchesEdge),frames:report},null,2));
 const checksPath=path.join(root,'checks.json');const checks=JSON.parse(await fs.readFile(checksPath,'utf8'));checks.status='rejected-gait-and-art-direction';checks.artDirectionApproved=false;checks.gaitReview='Audit/REVIEW.md';checks.structuralChecksOnly=true;await fs.writeFile(checksPath,JSON.stringify(checks,null,2));
 console.log(JSON.stringify({sheets:files.length,frames:report.length,edgeWarnings:report.filter(r=>r.touchesEdge).length,gaitApproved:false,status:checks.status}));
}
main().catch(e=>{console.error(e);process.exit(1)});
