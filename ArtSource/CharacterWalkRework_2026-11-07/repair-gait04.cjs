const fs=require('node:fs/promises'),path=require('node:path');
const sharp=require('C:/Users/charl/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const root=__dirname+'/Character04',base=root+'/GaitRepair-v2',size=168;
async function raw(file){return sharp(file).ensureAlpha().raw().toBuffer({resolveWithObject:true});}
function bounds(data,w,h){let l=w,r=0,t=h,b=0;for(let y=0;y<h;y++)for(let x=0;x<w;x++)if(data[(y*w+x)*4+3]>127){l=Math.min(l,x);r=Math.max(r,x);t=Math.min(t,y);b=Math.max(b,y);}return {left:l,top:t,width:r-l+1,height:b-t+1};}
async function normalize(file,col,cols,target){
 let im=sharp(base+'/'+file),m=await im.metadata();const left=Math.round(col*m.width/cols),right=Math.round((col+1)*m.width/cols);
 let {data,info}=await im.extract({left,top:0,width:right-left,height:m.height}).ensureAlpha().raw().toBuffer({resolveWithObject:true});
 for(let i=0;i<data.length;i+=4)if(data[i+3]<128||(data[i]>200&&data[i+1]<65&&data[i+2]<65)||(data[i]>180&&data[i+1]>180&&data[i+2]<50))data[i+3]=0;
 // Remove tiny matte fragments before locating the character.
 const w=info.width,h=info.height,seen=new Uint8Array(w*h),cc=[];
 for(let n=0;n<w*h;n++)if(data[n*4+3]&&!seen[n]){const q=[n];seen[n]=1;for(let k=0;k<q.length;k++){const v=q[k],x=v%w,y=Math.floor(v/w);for(const z of [x>0?v-1:-1,x<w-1?v+1:-1,y>0?v-w:-1,y<h-1?v+w:-1])if(z>=0&&!seen[z]&&data[z*4+3]){seen[z]=1;q.push(z);}}cc.push(q);}
 const max=Math.max(...cc.map(c=>c.length));for(const c of cc)if(c.length<max*.01)for(const n of c)data[n*4+3]=0;
 const b=bounds(data,w,h),old=await raw(base+'/Before/'+target),ob=bounds(old.data,size,size);
 const scale=ob.height/b.height,nw=Math.round(b.width*scale),nh=ob.height;
 let hl=w,hr=0;for(let y=b.top;y<b.top+b.height*.30;y++)for(let x=b.left;x<b.left+b.width;x++)if(data[(y*w+x)*4+3]>127){hl=Math.min(hl,x);hr=Math.max(hr,x);}
 const x=Math.round(84-((hl+hr)/2-b.left)*scale),y=158-nh;
 if(x<0||x+nw>size||y<0)throw Error('Out of frame '+file);
 const input=await sharp(data,{raw:info}).extract(b).resize(nw,nh,{kernel:'nearest'}).png().toBuffer();
 return sharp({create:{width:size,height:size,channels:4,background:'#00000000'}}).composite([{input,left:x,top:y}]).png().toBuffer();
}

async function main(){
 const dirs=['N','NE','E','SE','S','SW','W','NW'];
 await fs.mkdir(base+'/FinalFrames',{recursive:true});
 const checks={directions:8,changedWalkFrames:0,headsUnchanged:true,idlesUnchanged:true,passingOverrides:['E','SE','W lower skirt/shoes','SW']};
 const frames={};
 for(const d of dirs){frames[d]=[];for(let f=0;f<4;f++){
  const file=d+'-0'+(f+1)+'.png';
  const input=await normalize(d+'-repair.png',f,4,file);
  frames[d][f]=await sharp(input).ensureAlpha().raw().toBuffer();
 }}
 for(const d of ['E','SE','SW'])for(const [col,f]of [[0,1],[1,3]]){
  frames[d][f]=await sharp(await normalize(d+'-passing.png',col,2,d+'-0'+(f+1)+'.png')).ensureAlpha().raw().toBuffer();
 }
 // Only the symmetrical lower skirt/feet are reflected. Upper-body gear is separately authored.
 for(const [src,dst]of [['E','W']])for(const f of [1,3]){
  const a=frames[src][f],b=frames[dst][f];
  for(let y=112;y<168;y++)for(let x=0;x<168;x++){const to=(y*168+x)*4,from=(y*168+167-x)*4;a.copy(b,to,from,from+4);}
 }
 for(const d of dirs)for(let f=0;f<4;f++){
  const name=d+'-0'+(f+1)+'.png',old=(await raw(base+'/Before/'+name)).data,out=frames[d][f];
  if(d==='N'||d==='S'){
   const collarY=a=>{for(let y=55;y<85;y++)for(let x=77;x<92;x++){const i=(y*168+x)*4;if(a[i+3]>127&&a[i]>a[i+1]*1.6&&a[i+2]<a[i]*.75&&a[i]>55)return y;}return 64;};
   const shift=Math.max(0,Math.min(9,collarY(out)-collarY(old))),source=Buffer.from(out);
   if(shift)for(let y=64;y<158;y++){const sy=Math.round(64+shift+(y-64)*(94-shift)/94);source.copy(out,y*168*4,sy*168*4,(sy+1)*168*4);}
   console.log({d,f,collarShift:shift});
  }
  old.copy(out,0,0,168*64*4);
  if(!out.equals(old))checks.changedWalkFrames++;
  if(!out.subarray(0,168*64*4).equals(old.subarray(0,168*64*4)))checks.headsUnchanged=false;
  await sharp(out,{raw:{width:168,height:168,channels:4}}).png().toFile(base+'/FinalFrames/'+name);
  await fs.copyFile(base+'/FinalFrames/'+name,root+'/Review168/'+name);
 }
 for(const d of dirs){const name=d+'-idle.png';await fs.copyFile(base+'/Before/'+name,root+'/Review168/'+name);}
 {
  const old=(await raw(base+'/Before/W-idle.png')).data,out=Buffer.from(old);
  const clean=await sharp(await normalize('idle-equipment-repair.png',0,2,'W-idle.png')).ensureAlpha().raw().toBuffer();
  for(let y=93;y<115;y++)for(let x=87;x<102;x++){const n=(y*168+x)*4;clean.copy(out,n,n,n+4);}
  await sharp(out,{raw:{width:168,height:168,channels:4}}).png().toFile(root+'/Review168/W-idle.png');
  checks.idlesUnchanged=out.equals(old);checks.idleLocalCorrection='W pouch region x87..101,y93..114 only; pose unchanged';
 }
 for(let row=0;row<2;row++){
  const layers=[];
  for(let i=0;i<4;i++)for(let f=0;f<4;f++){
   const d=dirs[row*4+i];layers.push({input:await sharp(frames[d][f],{raw:{width:168,height:168,channels:4}}).resize(336,336,{kernel:'nearest'}).png().toBuffer(),left:f*336,top:i*366+30});
   if(f===0)layers.push({input:Buffer.from('<svg width="1344" height="30"><text x="18" y="23" fill="#eee" font-family="sans-serif" font-size="22">'+d+' : contact / passing / opposite / passing</text></svg>'),left:0,top:i*366});
  }
  await sharp({create:{width:1344,height:1464,channels:4,background:'#29323b'}}).composite(layers).png().toFile(base+'/contact-review-'+(row+1)+'.png');
 }
 await fs.writeFile(base+'/checks.json',JSON.stringify(checks,null,2));console.log(checks);
}
main().catch(e=>{console.error(e);process.exitCode=1});


