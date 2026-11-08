const fs=require('node:fs/promises'),path=require('node:path');
const sharp=require('C:/Users/charl/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const dirs=['N','NE','E','SE','S','SW','W','NW'],size=168;
async function main(){for(const id of (process.argv[2]?[process.argv[2]]:['04','05'])){
 const base=path.join(__dirname,'Character'+id),review=path.join(base,'Review168');
 const frames=[];for(const d of dirs)for(let f=0;f<5;f++)frames.push({d,f,data:await sharp(path.join(review,d+'-'+(f===4?'idle':String(f+1).padStart(2,'0'))+'.png')).ensureAlpha().raw().toBuffer()});
 const idle=frames[24].data,face=id==='04'?{cx:84,cy:45,rx:15,ry:12}:{cx:94,cy:50,rx:18,ry:14};
 const mask=[];for(let y=0;y<size;y++)for(let x=0;x<size;x++)if(((x-face.cx)/face.rx)**2+((y-face.cy)/face.ry)**2<=1)mask.push([x,y]);
 const faceOffsets=[];
 for(let f=0;f<4;f++){
  const data=frames[20+f].data;let best=Infinity,offset=[0,0];
  for(let dy=-5;dy<=5;dy++)for(let dx=-5;dx<=5;dx++){let error=0;for(const [x,y]of mask){const a=(y*size+x)*4,b=((y+dy)*size+x+dx)*4;for(let c=0;c<3;c++)error+=(idle[a+c]-data[b+c])**2;}if(error<best){best=error;offset=[dx,dy];}}
  for(const [x,y]of mask){const a=(y*size+x)*4,b=((y+offset[1])*size+x+offset[0])*4;idle.copy(data,b,a,a+4);}
  faceOffsets.push(offset);
 }
 const layers={head:[],body:[],clothing:[]},all=[],anchors=[];let mismatch=0;
 await fs.mkdir(path.join(base,'Layers'),{recursive:true});await fs.mkdir(path.join(base,'Loop'),{recursive:true});
 for(const {d,f,data}of frames){
  const buffers={head:Buffer.alloc(data.length),body:Buffer.alloc(data.length),clothing:Buffer.alloc(data.length)};
  const cutoff=id==='04'?66:74;
  for(let y=0;y<size;y++)for(let x=0;x<size;x++){
   const i=(y*size+x)*4,r=data[i]/255,g=data[i+1]/255,b=data[i+2]/255,max=Math.max(r,g,b),min=Math.min(r,g,b),delta=max-min;
   let hue=delta===0?0:max===r?60*((g-b)/delta%6):max===g?60*((b-r)/delta+2):60*((r-g)/delta+4);if(hue<0)hue+=360;
   const sat=max?delta/max:0;
   const cloth=id==='04'?y<108&&hue<16&&sat>.55:y<148&&hue>195&&hue<265&&sat>.2;
   const part=y<cutoff?'head':cloth?'clothing':'body';data.copy(buffers[part],i,i,i+4);
   for(let c=0;c<4;c++)if(buffers.head[i+c]+buffers.body[i+c]+buffers.clothing[i+c]!==data[i+c])mismatch++;
  }
  const png=await sharp(data,{raw:{width:size,height:size,channels:4}}).png().toBuffer();
  await fs.writeFile(path.join(review,d+'-'+(f===4?'idle':String(f+1).padStart(2,'0'))+'.png'),png);
  all.push({input:png,left:f*size,top:dirs.indexOf(d)*size});
  for(const key of Object.keys(layers))layers[key].push({input:await sharp(buffers[key],{raw:{width:size,height:size,channels:4}}).png().toBuffer(),left:f*size,top:dirs.indexOf(d)*size});
  anchors.push({direction:d,frame:f,feet:[84,158],headRegionBottom:cutoff,faceOffset:d==='S'&&f<4?faceOffsets[f]:null});
 }
 const canvas=()=>sharp({create:{width:840,height:1344,channels:4,background:'#00000000'}});
 await canvas().composite(all).png().toFile(path.join(review,'walk-sheet.png'));
 for(const key of Object.keys(layers))await canvas().composite(layers[key]).png().toFile(path.join(base,'Layers',key+'.png'));
 await fs.copyFile(path.join(review,'S-idle.png'),path.join(base,'mother-S-cartoon.png'));
 await fs.writeFile(path.join(base,'anchors.json'),JSON.stringify({face,faceOffsets,anchors},null,2));
 await fs.writeFile(path.join(base,'checks.json'),JSON.stringify({frames:40,directions:8,walkFrames:4,idleFrame:4,cell:168,losslessLayerPartition:mismatch===0,faceSharedMaster:true,unityVerified:false},null,2));
 const manifest=JSON.parse(await fs.readFile(path.join(review,'manifest.json'),'utf8'));manifest.layered=true;manifest.layers=['../Layers/body.png','../Layers/clothing.png','../Layers/head.png'];manifest.note='Head includes face and current hair; clothing mask covers selected main-color pixels only. Not an arbitrary wardrobe.';await fs.writeFile(path.join(review,'manifest.json'),JSON.stringify(manifest,null,2));
 for(let f=0;f<4;f++){
  const list=[];for(let d=0;d<8;d++){
   list.push({input:await sharp(all[d*5+f].input).resize(336,336,{kernel:'nearest'}).png().toBuffer(),left:d%4*336,top:Math.floor(d/4)*366+30});
   list.push({input:Buffer.from('<svg width="336" height="30"><text x="168" y="23" fill="#eee" font-family="sans-serif" text-anchor="middle" font-size="22">'+dirs[d]+'</text></svg>'),left:d%4*336,top:Math.floor(d/4)*366});
  }
  await sharp({create:{width:1344,height:732,channels:4,background:'#29323b'}}).composite(list).png().toFile(path.join(base,'Loop','frame-'+String(f+1).padStart(2,'0')+'.png'));
 }
 console.log({id,frames:frames.length,faceOffsets,layerMismatch:mismatch});
}}
main().catch(e=>{console.error(e);process.exitCode=1});


