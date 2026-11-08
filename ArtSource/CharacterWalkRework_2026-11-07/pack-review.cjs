// Deterministic slicing/nearest-neighbour packing only; no invented animation frames.
const fs = require('node:fs/promises');
const path = require('node:path');
const sharp = require('C:/Users/charl/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const root = __dirname, dir = path.join(root, 'Character06');
const order = ['N','NE','E','SE','S','SW','W','NW'];
async function main() {
  const frames = [], report = [];
  for (const [file, dirs] of [['cartoon-walk-A-v1.png',['S','N','SE','NW']],['cartoon-walk-B-repair-v2.png',['E','W','SW','NE']],['cartoon-S-repair-v2.png',['S']]]) {
    const image = sharp(path.join(dir,file));
    const m = await image.metadata();
    for (let row=0; row<dirs.length; row++) for(let col=0;col<5;col++) {
      if(file==='cartoon-walk-A-v1.png'&&row===0)continue;
      const x=Math.round(col*m.width/5), y=Math.round(row*m.height/dirs.length);
      const w=Math.round((col+1)*m.width/5)-x, h=Math.round((row+1)*m.height/dirs.length)-y;
      const {data,info}=await image.clone().extract({left:x,top:y,width:w,height:h}).ensureAlpha().raw().toBuffer({resolveWithObject:true});
      let l=w,t=h,r=0,b=0,count=0;
      for(let py=0;py<h;py++)for(let px=0;px<w;px++)if(data[(py*w+px)*4+3]>127){l=Math.min(l,px);r=Math.max(r,px);t=Math.min(t,py);b=Math.max(b,py);count++;}
      if(!count)throw Error('Empty cell '+dirs[row]+col);
      // Head bounding midpoint avoids hand/cape movement shifting the whole sprite.
      let hl=w,hr=0;
      for(let py=t;py<t+(b-t)*.30;py++)for(let px=l;px<=r;px++)if(data[(py*w+px)*4+3]>127){hl=Math.min(hl,px);hr=Math.max(hr,px);}
      const bounds={left:Math.max(0,l-1),top:Math.max(0,t-1),width:Math.min(w-1,r+1)-Math.max(0,l-1)+1,height:Math.min(h-1,b+1)-Math.max(0,t-1)+1};
      const raw=await sharp(data,{raw:info}).extract(bounds).png().toBuffer();
      frames.push({direction:dirs[row],frame:col,raw,headX:(hl+hr)/2-bounds.left,height:bounds.height,width:bounds.width,source:file});
      report.push({file,direction:dirs[row],frame:col,sourceRect:{x:x+bounds.left,y:y+bounds.top,w:bounds.width,h:bounds.height},visiblePixels:count,touchesCellEdge:l===0||t===0||r===w-1||b===h-1});
    }
  }
  const maxHeight=Math.max(...frames.map(f=>f.height));
  for(const size of [128,168]) {
    const dest=path.join(dir,'Review'+size);await fs.mkdir(dest,{recursive:true});
    const baseline=size-10,layers=[];
    for(const f of frames) {
      const scale=(size-20)/Math.max(...frames.filter(other=>other.source===f.source).map(other=>other.height));
      const w=Math.round(f.width*scale),h=Math.round(f.height*scale);
      const left=Math.round(size/2-f.headX*scale),top=baseline-h;
      if(left<0||left+w>size||top<0)throw Error('Frame does not fit');
      const input=await sharp(f.raw).resize(w,h,{kernel:'nearest'}).png().toBuffer();
      const png=await sharp({create:{width:size,height:size,channels:4,background:'#00000000'}}).composite([{input,left,top}]).png().toBuffer();
      const filename=f.direction+'-'+(f.frame===4?'idle':String(f.frame+1).padStart(2,'0'))+'.png';
      await fs.writeFile(path.join(dest,filename),png);
      layers.push({input:png,left:f.frame*size,top:order.indexOf(f.direction)*size});
    }
    await sharp({create:{width:size*5,height:size*8,channels:4,background:'#00000000'}}).composite(layers).png().toFile(path.join(dest,'walk-sheet.png'));
    await fs.writeFile(path.join(dest,'manifest.json'),JSON.stringify({status:'review-only',character:'V2-06',cellSize:size,columns:5,rows:order,walkFrames:[0,1,2,3],idleFrame:4,fps:8,pivotPixels:[size/2,10],pivotOrigin:'bottom-left',filter:'Point',layered:false,unityVerified:false},null,2));
  }
  await fs.writeFile(path.join(dir,'slice-review.json'),JSON.stringify(report,null,2));
  console.log(JSON.stringify({frames:frames.length,sizes:[128,168],edgeWarnings:report.filter(r=>r.touchesCellEdge),maxSourceHeight:maxHeight}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});
