const fs=require('node:fs/promises'),sharp=require('C:/Users/charl/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');const root=__dirname,dirs=['N','NE','E','SE','S','SW','W','NW'],S=168;const keys=['hat-back','cape-back','accessory-back','body-hat','cape-front','accessory-front','hat-front'];
const raw=async p=>(await sharp(p).ensureAlpha().raw().toBuffer());const png=b=>sharp(b,{raw:{width:S,height:S,channels:4}}).png().toBuffer();
function box(data,w,h){let l=w,r=0,t=h,b=0;for(let y=0;y<h;y++)for(let x=0;x<w;x++)if(data[(y*w+x)*4+3]>127){l=Math.min(l,x);r=Math.max(r,x);t=Math.min(t,y);b=Math.max(b,y);}return {left:l,top:t,width:r-l+1,height:b-t+1};}
async function piece(file,c,r){const im=sharp(file),m=await im.metadata(),left=Math.round(c*m.width/8),top=Math.round(r*m.height/3),w=Math.round((c+1)*m.width/8)-left,h=Math.round((r+1)*m.height/3)-top;const {data,info}=await im.extract({left,top,width:w,height:h}).ensureAlpha().raw().toBuffer({resolveWithObject:true});for(let n=0;n<data.length;n+=4)if(data[n+3]<170||(data[n]>185&&data[n+1]<60&&data[n+2]<60)||(data[n]>165&&data[n+1]>165&&data[n+2]<60))data[n+3]=0;let b=box(data,w,h);let input=await sharp(data,{raw:info}).extract(b).png().toBuffer();return {input,b};}
function paste(dst,src,w,h,x0,y0,warp=0){for(let y=0;y<h;y++)for(let x=0;x<w;x++){let xx=x+x0+Math.round(warp*y/h),yy=y+y0;if(xx<0||xx>=168||yy<0||yy>=168)continue;let n=(y*w+x)*4,j=(yy*168+xx)*4;if(src[n+3]>127)src.copy(dst,j,n,n+4);}}
function over(base,layer){for(let n=0;n<base.length;n+=4)if(layer[n+3]>127)layer.copy(base,n,n,n+4);}
(async()=>{for(const id of ['01','02','03','07','08']){const base=root+'/Character'+id,ward=base+'/Wardrobe';await fs.mkdir(ward+'/Layers',{recursive:true});await fs.mkdir(ward+'/Loop',{recursive:true});const all=Object.fromEntries(keys.map(k=>[k,[]])),variants=[],anchors=[],single={hat:[],cape:[],accessory:[]};
for(let d=0;d<8;d++){
 const hat=await piece(ward+'/equipment-generated.png',d,0),cape=await piece(ward+'/equipment-generated.png',d,1),acc=await piece(ward+'/equipment-generated.png',d,2);
 const hw=({ '01':68,'02':65,'03':57,'07':77,'08':80})[id],hh=id==='08'?45:id==='03'?32:30,hatData=await sharp(hat.input).resize(hw,hh,{kernel:'nearest'}).raw().toBuffer();
 const ch=id==='07'?60:46,cw=Math.round(cape.b.width*ch/cape.b.height),capeData=await sharp(cape.input).resize(cw,ch,{kernel:'nearest'}).raw().toBuffer();
 // Locate the actual collar in the upper 12% rather than centering the entire cape bounds.
 let cl=cw,cr=0;for(let y=0;y<Math.max(2,ch*.12);y++)for(let x=0;x<cw;x++)if(capeData[(y*cw+x)*4+3]>127){cl=Math.min(cl,x);cr=Math.max(cr,x);}let collar=(cl+cr)/2;
 const ah=['03','07'].includes(id)?32:25,aw=Math.round(acc.b.width*ah/acc.b.height),accData=await sharp(acc.input).resize(aw,ah,{kernel:'nearest'}).raw().toBuffer();
 for(let f=0;f<5;f++){
  let body=await raw(base+'/Review168/'+dirs[d]+'-'+(f===4?'idle':'0'+(f+1))+'.png'),layers=Object.fromEntries(keys.map(k=>[k,Buffer.alloc(S*S*4)]));layers['body-hat']=Buffer.from(body);
  let bob=(f===1||f===3)?2:0,headX=84,brimY=34+bob;
  const crownH=id==='08'?Math.round(hh*.64):hh,hatX=Math.round(headX-hw/2),hatY=brimY-crownH;
  // Remove only the crown/topknot under a worn cap. Original body restores it when unequipped.
  for(let y=0;y<Math.max(0,brimY-8);y++)for(let x=0;x<S;x++)layers['body-hat'][(y*S+x)*4+3]=0;
  let fullHat=Buffer.alloc(S*S*4);paste(fullHat,hatData,hw,hh,hatX,hatY);
  for(let y=0;y<S;y++)for(let x=0;x<S;x++){let n=(y*S+x)*4;const rearCloth=id==='08'&&y>brimY;const key=rearCloth&&!([0,1,7].includes(d))?'hat-back':'hat-front';fullHat.copy(layers[key],n,n,n+4);}
  let neckX=[84,89,89,87,84,81,79,79][d],neckY=69+bob,cp=Buffer.alloc(S*S*4);paste(cp,capeData,cw,ch,Math.round(neckX-collar),neckY,f===4?0:([0,2,0,-2][f]));
  for(let y=0;y<S;y++)for(let x=0;x<S;x++){let n=(y*S+x)*4,back=[0,1,7].includes(d);let front=back||y<neckY+7;
   if([2,3].includes(d))front=x<neckX-7;
   if([5,6].includes(d))front=x>neckX+7;
   // Keep hands, lower sleeves and long hair in front of cloak, using body silhouette outside shoulder core.
   if(y>neckY+14&&body[n+3]&&(x<70||x>98)&&body[n]>body[n+1]*1.13&&body[n+1]>body[n+2]*1.08)front=false;
   cp.copy(layers[front?'cape-front':'cape-back'],n,n,n+4);
  }
  let waistX=[66,66,68,97,103,104,99,71][d],waistY=103+bob;
  let accLayer=([1,2].includes(d))?'accessory-back':'accessory-front';paste(layers[accLayer],accData,aw,ah,Math.round(waistX-aw/2),waistY,f===4?0:[0,1,0,-1][f]);
  // Near hand always covers waist equipment; preserve forearm skin and cuffs where overlaps occur.
  if(accLayer==='accessory-front')for(let y=80;y<122;y++)for(let x=0;x<S;x++){let n=(y*S+x)*4,r=body[n],g=body[n+1],b=body[n+2];if(body[n+3]&&r>g*1.13&&g>b*1.08&&r>70)layers[accLayer][n+3]=0;}
  let combined=Buffer.alloc(S*S*4);for(const k of keys)over(combined,layers[k]);const input=await png(combined);variants.push({input,left:f*S,top:d*S});
  for(const k of keys)all[k].push({input:await png(layers[k]),left:f*S,top:d*S});
  for(const kind of Object.keys(single)){let comp=Buffer.alloc(S*S*4);if(kind==='hat')over(comp,layers['hat-back']);if(kind==='cape')over(comp,layers['cape-back']);if(kind==='accessory')over(comp,layers['accessory-back']);over(comp,kind==='hat'?layers['body-hat']:body);over(comp,layers[kind+'-front']);single[kind].push({input:await png(comp),left:f*S,top:d*S});}
  anchors.push({direction:dirs[d],frame:f,head:[headX,brimY],neck:[neckX,neckY],waist:[waistX,waistY],capeCollarLocal:[collar,0],hatHairCut:brimY-8});
 }
}
const canvas=()=>sharp({create:{width:840,height:1344,channels:4,background:'#00000000'}});for(const k of keys)await canvas().composite(all[k]).png().toFile(ward+'/Layers/'+k+'.png');await canvas().composite(variants).png().toFile(ward+'/variant-review.png');for(const k of Object.keys(single))await canvas().composite(single[k]).png().toFile(ward+'/'+k+'-only-review.png');await fs.writeFile(ward+'/anchors.json',JSON.stringify(anchors,null,2));
for(let f=0;f<4;f++){let comp=[];for(let d=0;d<8;d++)comp.push({input:await sharp(variants[d*5+f].input).resize(336,336,{kernel:'nearest'}).png().toBuffer(),left:d%4*336,top:Math.floor(d/4)*336});await sharp({create:{width:1344,height:672,channels:4,background:'#29323b'}}).composite(comp).png().toFile(ward+'/Loop/frame-0'+(f+1)+'.png');}console.log('wardrobe '+id);
}})().catch(e=>{console.error(e);process.exitCode=1});
