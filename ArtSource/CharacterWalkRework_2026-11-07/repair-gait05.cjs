const fs=require('node:fs/promises'),path=require('node:path');
const sharp=require('C:/Users/charl/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const root=__dirname+'/Character05',base=root+'/GaitRepair-v2',size=168;
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
 await fs.mkdir(base+'/FinalFrames',{recursive:true});
 for(const d of ['SW','W'])for(let f=0;f<4;f++)await fs.writeFile(base+'/FinalFrames/'+d+'-0'+(f+1)+'.png',await normalize(d+'-equipment-edit.png',f,4,d+'-0'+(f+1)+'.png'));
 await fs.copyFile(base+'/Before/NE-01.png',base+'/FinalFrames/NE-01.png');
 for(const [f,file,col,cols]of [[2,'NE-attempt-rejected.png',1,4],[3,'NE-contact-opposite.png',0,1],[4,'NE-attempt-rejected.png',3,4]])await fs.writeFile(base+'/FinalFrames/NE-0'+f+'.png',await normalize(file,col,cols,'NE-0'+f+'.png'));
 for(const d of ['SW','W','NE']){
  const list=[];for(let f=1;f<=4;f++)list.push({input:await sharp(base+'/FinalFrames/'+d+'-0'+f+'.png').resize(336,336,{kernel:'nearest'}).png().toBuffer(),left:(f-1)*336,top:0});
  await sharp({create:{width:1344,height:336,channels:4,background:'#00000000'}}).composite(list).png().toFile(base+'/'+d+'-after.png');
 }
 const nw=[];for(let f=1;f<=4;f++)nw.push({input:await sharp(base+'/FinalFrames/NE-0'+f+'.png').flop().resize(336,336,{kernel:'nearest'}).png().toBuffer(),left:(f-1)*336,top:0});
 await sharp({create:{width:1344,height:336,channels:4,background:'#00000000'}}).composite(nw).png().toFile(base+'/NW-motion-guide.png');
 if(process.argv.includes('--nw'))for(let f=0;f<4;f++)await fs.writeFile(base+'/FinalFrames/NW-0'+(f+1)+'.png',await normalize('NW-equipment-edit.png',f,4,'NW-0'+(f+1)+'.png'));
 if(process.argv.includes('--nw'))await fs.writeFile(base+'/FinalFrames/W-idle.png',await normalize('W-idle-equipment.png',0,1,'W-idle.png'));
}
main().catch(e=>{console.error(e);process.exitCode=1});
