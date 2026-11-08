const fs=require('node:fs/promises'),path=require('node:path');
const sharp=require('C:/Users/charl/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const root=__dirname,dirs=['N','NE','E','SE','S','SW','W','NW'],S=168,W=840,H=1344;
const headBottom=[65,74,73,69,64,73,74,69];
const specs=JSON.parse(require('node:fs').readFileSync(path.join(root,'variants.json'),'utf8'));
function hsv(r,g,b){r/=255;g/=255;b/=255;const max=Math.max(r,g,b),min=Math.min(r,g,b),d=max-min;let h=0;if(d)h=(max===r?(g-b)/d+(g<b?6:0):max===g?(b-r)/d+2:(r-g)/d+4)*60;return[h,max===0?0:d/max,max];}
function inside(x,y,p){let c=false;for(let i=0,j=p.length-1;i<p.length;j=i++)if(((p[i][1]>y)!==(p[j][1]>y))&&(x<(p[j][0]-p[i][0])*(y-p[i][1])/(p[j][1]-p[i][1])+p[i][0]))c=!c;return c;}
// Explicit garment envelopes: all masks are exported for inspection/editing.
const topPolys=[[[76,67],[98,70],[100,85],[78,84]],[[86,77],[102,80],[103,91],[86,91]],[[91,80],[106,81],[106,96],[90,94]],[[80,73],[98,71],[103,88],[79,89]],[[75,72],[94,68],[95,84],[72,85]],[[69,78],[89,74],[93,92],[70,91]],[[65,78],[85,75],[87,95],[64,96]],[[75,70],[96,73],[96,89],[76,88]]];
const facePolys=[[],[[111,44],[116,46],[116,64],[109,66],[104,60]],[[99,35],[109,38],[112,49],[117,58],[112,67],[100,67],[96,57],[88,55],[87,48],[92,45],[98,48]],[[85,37],[97,34],[106,39],[106,49],[102,60],[96,65],[85,63],[79,56],[79,43]],[[76,33],[86,31],[95,35],[98,43],[97,52],[93,58],[85,61],[76,58],[70,51],[70,41]],[[70,38],[79,35],[91,40],[96,50],[92,61],[84,68],[72,66],[66,57],[65,46]],[[63,37],[73,37],[79,45],[87,47],[88,55],[82,58],[78,68],[66,69],[59,61],[58,50]],[[60,45],[65,42],[69,49],[69,60],[62,63],[58,56]]];
const image=async p=>{const {data}=await sharp(p).ensureAlpha().raw().toBuffer({resolveWithObject:true});return data;};
const png=(data,w=S,h=S)=>sharp(data,{raw:{width:w,height:h,channels:4}}).png().toBuffer();
function bounds(data,top=0,bottom=75){let l=S,r=0,t=S,b=0;for(let y=top;y<bottom;y++)for(let x=0;x<S;x++)if(data[(y*S+x)*4+3]>127){l=Math.min(l,x);r=Math.max(r,x);t=Math.min(t,y);b=Math.max(b,y);}return {l,r,t,b,cx:(l+r)/2};}
function put(dst,src,dx,dy){for(let y=0;y<S;y++)for(let x=0;x<S;x++){const xx=x+dx,yy=y+dy;if(xx<0||xx>=S||yy<0||yy>=S)continue;const i=(y*S+x)*4,j=(yy*S+xx)*4;if(src[i+3])src.copy(dst,j,i,i+4);}}
async function main(){
 await fs.mkdir(path.join(root,'Layers'),{recursive:true});await fs.mkdir(path.join(root,'Composites'),{recursive:true});
 const overrides={};for(const key of ['top','skirt','cape','beads']){const file=path.join(root,'MaskOverrides','mask-'+key+'.png');if(require('node:fs').existsSync(file)){const m=await sharp(file).metadata();if(m.width!==W||m.height!==H)throw Error('Mask override must be 840x1344: '+file);overrides[key]=await image(file);}}
 const heads={};
 for(const h of ['H1','H7']){
  const normalized=await sharp(path.join(root,h+'-generated.png')).resize(S*4,S*2,{kernel:'nearest'}).png().toBuffer();
  heads[h]=[];for(let d=0;d<8;d++)heads[h].push(await image(await sharp(normalized).extract({left:d%4*S,top:Math.floor(d/4)*S,width:S,height:S}).png().toBuffer()));
 }
 const body=Buffer.alloc(W*H*4);
 const masks={top:Buffer.alloc(W*H*4),skirt:Buffer.alloc(W*H*4),cape:Buffer.alloc(W*H*4),beads:Buffer.alloc(W*H*4),head:Buffer.alloc(W*H*4)};
 const atlases=Object.fromEntries(specs.map(s=>[s.id,Buffer.alloc(W*H*4)]));
 const overlayAtlases={};for(const s of specs)for(const layer of ['cloth','head','beads'])overlayAtlases[s.id+'-'+layer]=Buffer.alloc(W*H*4);
 const anchors=[],qa={unchangedBodyPixels:true,protectedFacesUnchanged:true,feetUnchanged:true,frames:0,variants:3,maskPixelCounts:{top:0,skirt:0,cape:0,beads:0},notes:['H1/H7 are rigid head modules with per-frame translation, not independent hair physics.','C2/C5 change top/skirt colors, not garment cut; original cape retained.','Accessory prototype changes existing bead colors and C2 hoop earrings to silver; no new A catalog identity assigned.']};
 for(let d=0;d<8;d++){
  const idle=await image(path.join(root,'../Character06/Review168',dirs[d]+'-idle.png'));const ib=bounds(idle,0,headBottom[d]-10);
  for(let f=0;f<5;f++){
   const base=await image(path.join(root,'../Character06/Review168',dirs[d]+'-'+(f===4?'idle':String(f+1).padStart(2,'0'))+'.png'));
   const hb=bounds(base,0,headBottom[d]-10),dx=Math.round(hb.cx-ib.cx),dy=hb.t-ib.t,cut=headBottom[d]+dy;
   anchors.push({direction:dirs[d],frame:f,headOffset:[dx,dy],neckY:cut});
   const labels=new Uint8Array(S*S);
   for(let y=0;y<S;y++)for(let x=0;x<S;x++){
    const i=(y*S+x)*4;if(base[i+3]<128)continue;const [h,s,v]=hsv(base[i],base[i+1],base[i+2]);const rx=x-dx,ry=y-dy;
    let id=0;
    if(y>=cut&&y<145&&h>195&&h<260&&s>.18)id=2;
    // Cape and skin share orange/brown colors. Keep cape original in this minimal prototype.
    if(inside(rx,ry,topPolys[d])&&h>=28&&h<56&&s>.3)id=1;
    // Bead color only within a narrow neck region; never recolor earrings or skin.
    if(y>=cut-1&&y<cut+12&&x>66+dx&&x<108+dx&&h>180&&h<260&&s>.20)id=4;
    const maskIndex=((d*S+y)*W+f*S+x)*4;
    if(id&&overrides[['','top','skirt','cape','beads'][id]])id=0;
    for(const [key,mask]of Object.entries(overrides))if(mask[maskIndex]>127&&mask[maskIndex+3]>127)id=['','top','skirt','cape','beads'].indexOf(key);
    labels[y*S+x]=id;
    const gi=((d*S+y)*W+f*S+x)*4;
    if(y>=cut)base.copy(body,gi,i,i+4);
    if(id){const key=['','top','skirt','cape','beads'][id];masks[key][gi]=255;masks[key][gi+1]=255;masks[key][gi+2]=255;masks[key][gi+3]=255;qa.maskPixelCounts[key]++;}
    if(y<cut){masks.head[gi]=masks.head[gi+1]=masks.head[gi+2]=255;masks.head[gi+3]=base[i+3];}
   }
   for(const spec of specs){
    const result=Buffer.from(base),cloth=Buffer.alloc(S*S*4),beads=Buffer.alloc(S*S*4),head=Buffer.alloc(S*S*4);
    if(spec.head!=='H5'){
     const src=heads[spec.head][d];
     // Head-only extraction; body pixels from generated images are never used.
     const sb=bounds(src,0,headBottom[d]);
     const sx=Math.round(ib.cx-sb.cx),sy=0;
     const tailRects=[[76,61,90,81],[62,67,80,86],[68,66,82,85],[70,64,82,79],null,[96,67,110,83],[91,66,105,86],[94,62,108,82]];
     const module=Buffer.alloc(S*S*4);for(let y=0;y<S;y++)for(let x=0;x<S;x++){
      const i=(y*S+x)*4,t=tailRects[d];const [h,s,v]=hsv(src[i],src[i+1],src[i+2]);
      const tail=spec.head==='H7'&&t&&x>=t[0]&&x<t[2]&&y>=t[1]&&y<t[3]&&((src[i]<105&&src[i+1]<90&&src[i+2]<85)||(h>150&&h<200&&v<.65));
      if(y<headBottom[d]||tail)src.copy(module,i,i,i+4);
     }
     put(head,module,sx+dx,sy+dy);
     // Preserve original visible face pixels. Hair/cap artwork can overlap only outside this protected face.
     for(let y=0;y<cut;y++)for(let x=0;x<S;x++){
      const i=(y*S+x)*4;
      const [fh,fsat]=hsv(base[i],base[i+1],base[i+2]);const jewelry=fh>30&&fh<65&&fsat>.45;
      if(facePolys[d].length&&inside(x-dx,y-dy,facePolys[d])&&base[i+3]>127&&!jewelry)base.copy(head,i,i,i+4);
      head.copy(result,i,i,i+4);
     }
     for(let y=cut;y<S;y++)for(let x=0;x<S;x++){const i=(y*S+x)*4;if(head[i+3]>127)head.copy(result,i,i,i+4);}
    }
    for(let y=0;y<S;y++)for(let x=0;x<S;x++){
     const i=(y*S+x)*4,id=labels[y*S+x];
     if(id&&spec[['','top','skirt','cape','beads'][id]]){const key=['','top','skirt','cape','beads'][id],target=spec[key];const lum= id===1?Math.max(base[i],base[i+1],base[i+2])/210:id===2?Math.max(base[i],base[i+1],base[i+2])/120:id===3?base[i]/190:Math.max(base[i],base[i+1],base[i+2])/120;for(let c=0;c<3;c++)result[i+c]=Math.min(255,Math.round(target[c]*Math.max(.25,lum)));const layer=id===4?beads:cloth;result.copy(layer,i,i,i+4);}
     if(y>=cut&&head[i+3]>127)head.copy(result,i,i,i+4);
     if(spec.id==='C2-H1'&&y>=43+dy&&y<cut+2&&!inside(x-dx,y-dy,facePolys[d])){
      const [h,s,v]=hsv(result[i],result[i+1],result[i+2]);if(h>32&&h<62&&s>.50&&v>.45&&result[i+3]>127){const silver=[195,210,220];for(let c=0;c<3;c++)result[i+c]=Math.round(silver[c]*v);result.copy(beads,i,i,i+4);}
     }
     const gi=((d*S+y)*W+f*S+x)*4;result.copy(atlases[spec.id],gi,i,i+4);cloth.copy(overlayAtlases[spec.id+'-cloth'],gi,i,i+4);beads.copy(overlayAtlases[spec.id+'-beads'],gi,i,i+4);head.copy(overlayAtlases[spec.id+'-head'],gi,i,i+4);
     if(y>=cut&&!id&&!head[i+3]&&!beads[i+3]&&!base.subarray(i,i+4).equals(result.subarray(i,i+4)))qa.unchangedBodyPixels=false;
     const [fh,fsat]=hsv(base[i],base[i+1],base[i+2]);const jewelry=fh>30&&fh<65&&fsat>.45;
     if(y<cut&&facePolys[d].length&&inside(x-dx,y-dy,facePolys[d])&&base[i+3]>127&&!jewelry&&!base.subarray(i,i+4).equals(result.subarray(i,i+4)))qa.protectedFacesUnchanged=false;
     if(y>=146&&!base.subarray(i,i+4).equals(result.subarray(i,i+4)))qa.feetUnchanged=false;
    }
   }qa.frames++;
  }
 }
 await fs.writeFile(path.join(root,'Layers','body-no-head.png'),await png(body,W,H));
 for(const [k,v]of Object.entries(masks))await fs.writeFile(path.join(root,'Layers','mask-'+k+'.png'),await png(v,W,H));
 for(const [k,v]of Object.entries(overlayAtlases))await fs.writeFile(path.join(root,'Layers',k+'.png'),await png(v,W,H));
 for(const [k,v]of Object.entries(atlases))await fs.writeFile(path.join(root,'Composites',k+'.png'),await png(v,W,H));
 await fs.writeFile(path.join(root,'anchors.json'),JSON.stringify(anchors,null,2));await fs.writeFile(path.join(root,'recipes.json'),JSON.stringify({character:'V2-06',cellSize:168,directions:dirs,walkFrames:4,idleFrame:4,colorScope:'top and skirt only; cape unchanged',accessories:'existing beads recolored; C2 silver hoop earrings',variants:specs},null,2));await fs.writeFile(path.join(root,'checks.json'),JSON.stringify(qa,null,2));
 const tiles=[];for(let v=0;v<3;v++)for(let d=0;d<8;d++){const input=await sharp(await png(atlases[specs[v].id],W,H)).extract({left:4*S,top:d*S,width:S,height:S}).resize(252,252,{kernel:'nearest'}).png().toBuffer();tiles.push({input,left:d*252,top:v*280+28});}
 await sharp({create:{width:2016,height:840,channels:4,background:'#333b43'}}).composite(tiles).png().toFile(path.join(root,'comparison.png'));
 console.log(JSON.stringify(qa));
}
main().catch(e=>{console.error(e);process.exitCode=1});
