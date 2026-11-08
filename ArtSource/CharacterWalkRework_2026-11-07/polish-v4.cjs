const fs=require('node:fs/promises'),path=require('node:path');
const sharp=require('C:/Users/charl/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const root=__dirname,base=root+'/MotionPolish-v4',size=168;
async function raw(p){return sharp(p).ensureAlpha().raw().toBuffer({resolveWithObject:true});}
function bounds(data,w,h){let l=w,r=0,t=h,b=0;for(let y=0;y<h;y++)for(let x=0;x<w;x++)if(data[(y*w+x)*4+3]>127){l=Math.min(l,x);r=Math.max(r,x);t=Math.min(t,y);b=Math.max(b,y);}return {left:l,top:t,width:r-l+1,height:b-t+1};}
(async()=>{
 for(let f=1;f<=4;f++){
  const name='E-0'+f+'.png',old=(await raw(base+'/Before04/'+name)).data,ob=bounds(old,168,168),im=sharp(base+'/04-arms-generated.png'),m=await im.metadata(),left=Math.round((f-1)*m.width/4),right=Math.round(f*m.width/4);
  const {data,info}=await im.extract({left,top:0,width:right-left,height:m.height}).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  for(let i=0;i<data.length;i+=4)if(data[i+3]<128||(data[i]>180&&data[i+1]<70&&data[i+2]<70)||(data[i]>180&&data[i+1]>180&&data[i+2]<65))data[i+3]=0;
  const b=bounds(data,info.width,info.height),scale=ob.height/b.height,w=Math.round(b.width*scale),h=ob.height;
  let l=info.width,r=0,ol=168,orr=0;for(let y=b.top;y<b.top+b.height*.30;y++)for(let x=b.left;x<b.left+b.width;x++)if(data[(y*info.width+x)*4+3]>127){l=Math.min(l,x);r=Math.max(r,x);}for(let y=ob.top;y<64;y++)for(let x=0;x<168;x++)if(old[(y*168+x)*4+3]>127){ol=Math.min(ol,x);orr=Math.max(orr,x);}
  const x=Math.round((ol+orr)/2-((l+r)/2-b.left)*scale);
  const input=await sharp(data,{raw:info}).extract(b).resize(w,h,{kernel:'nearest'}).png().toBuffer();
  const patch=await sharp({create:{width:168,height:168,channels:4,background:'#00000000'}}).composite([{input,left:x,top:ob.top}]).raw().toBuffer();
  const out=Buffer.from(old);for(let y=65;y<112;y++)for(let x=0;x<168;x++){const n=(y*168+x)*4;patch.copy(out,n,n,n+4);}
  await sharp(out,{raw:{width:168,height:168,channels:4}}).png().toFile(root+'/Character04/Review168/'+name);
 }
 const changes=[];
 for(const d of ['N','NW'])for(const f of ['01','02','03','04','idle']){
  const name=d+'-'+f+'.png',{data,info}=await raw(base+'/Before05/'+name),b=bounds(data,168,168),scale=d==='N'?1.045:1.04,w=Math.round(b.width*scale),h=Math.round(b.height*scale),x=Math.round(84+(b.left-84)*scale),y=b.top+b.height-h;
  const input=await sharp(data,{raw:info}).extract(b).resize(w,h,{kernel:'nearest'}).png().toBuffer();
  await sharp({create:{width:168,height:168,channels:4,background:'#00000000'}}).composite([{input,left:x,top:y}]).png().toFile(root+'/Character05/Review168/'+name);
  changes.push({name,uniformScale:scale,oldHeight:b.height,newHeight:h,bottom:b.top+b.height-1});
 }
 await fs.writeFile(base+'/05-height-adjustments.json',JSON.stringify(changes,null,2));
 for(const id of ['04','05']){let list=[];const ds=id==='04'?['E','W']:['N','NW','NE'];for(let row=0;row<ds.length;row++)for(let f=1;f<=4;f++)list.push({input:await sharp(root+'/Character'+id+'/Review168/'+ds[row]+'-0'+f+'.png').resize(336,336,{kernel:'nearest'}).png().toBuffer(),left:(f-1)*336,top:row*336});await sharp({create:{width:1344,height:ds.length*336,channels:4,background:'#29323b'}}).composite(list).png().toFile(base+'/'+id+'-after.png');}
})().catch(e=>{console.error(e);process.exitCode=1});
