const fs=require('node:fs/promises'),path=require('node:path');
const sharp=require('C:/Users/charl/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const root=__dirname+'/Character04',base=root+'/SideStep-v3',size=168;
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
 const changed=[];
 for(const [col,f]of [[0,2],[1,4]]){
  const input=await sharp(await normalize('E-passing-low.png',col,2,'E-0'+f+'.png')).ensureAlpha().raw().toBuffer();
  for(const d of ['E','W']){
   const name=d+'-0'+f+'.png',old=(await raw(base+'/Before/'+name)).data,out=Buffer.from(old);
   for(let y=112;y<168;y++)for(let x=0;x<168;x++){const to=(y*168+x)*4,from=(y*168+(d==='E'?x:167-x))*4;input.copy(out,to,from,from+4);}
   // Remove the obsolete upturned skirt tips above the main splice, outside hands/gear.
   if(d==='E')for(let y=96;y<112;y++)for(let x=0;x<168;x++){
    const n=(y*168+x)*4;
    if((f===4&&x>=101)||(f===2&&y>=102&&x<66&&old[n]<150))input.copy(out,n,n,n+4);
   }
   if(!out.subarray(0,96*168*4).equals(old.subarray(0,96*168*4)))throw Error('Upper-body changed');
   await sharp(out,{raw:{width:168,height:168,channels:4}}).png().toFile(root+'/Review168/'+name);changed.push(name);
  }
 }
 const dirs=['N','NE','E','SE','S','SW','W','NW'];let untouched=true;
 for(const d of dirs)for(const suffix of ['01','02','03','04','idle']){const n=d+'-'+suffix+'.png';if(!changed.includes(n)){const a=(await raw(base+'/Before/'+n)).data,b=(await raw(root+'/Review168/'+n)).data;if(!a.equals(b))untouched=false;}}
 const checks={changedFrames:changed,changedRegion:'y112..167 lower skirt/shoes plus obsolete E skirt tips y96..111 outside hands/gear',upperBodiesUnchanged:true,other36FramesUnchanged:untouched};
 await fs.writeFile(base+'/checks.json',JSON.stringify(checks,null,2));console.log(checks);
 for(let f=1;f<=4;f++){
  const list=[];for(const [row,d]of ['E','W'].entries()){list.push({input:await sharp(root+'/Review168/'+d+'-0'+f+'.png').resize(504,504,{kernel:'nearest'}).png().toBuffer(),left:row*504,top:30});list.push({input:Buffer.from('<svg width="504" height="30"><text x="252" y="23" fill="white" font-family="sans-serif" text-anchor="middle" font-size="22">'+d+' / '+f+'</text></svg>'),left:row*504,top:0});}
  await sharp({create:{width:1008,height:534,channels:4,background:'#29323b'}}).composite(list).png().toFile(base+'/frame-0'+f+'.png');
 }
}
main().catch(e=>{console.error(e);process.exitCode=1});



