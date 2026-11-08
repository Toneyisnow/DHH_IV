const fs=require('node:fs');
for(const id of ['04','05']){
const p=__dirname+'/pack-character'+id+'.cjs';let s=fs.readFileSync(p,'utf8');
s=s.replace('let l=w,t=h,r=0,b=0,count=0;',`// Strip transparent noise and saturated matte artifacts; preserve shaded costume reds.
      for(let i=0;i<data.length;i+=4)if(data[i+3]<128 || (data[i]>200&&data[i+1]<65&&data[i+2]<65)||(data[i]>180&&data[i+1]>180&&data[i+2]<50)){data[i+3]=0;}
      // Connected components prevent neighboring-cell fragments shifting the head anchor.
      const seen=new Uint8Array(w*h), components=[];
      for(let n=0;n<w*h;n++)if(data[n*4+3]&&!seen[n]){
        const q=[n];seen[n]=1;
        for(let k=0;k<q.length;k++){const v=q[k],x=v%w,y=Math.floor(v/w);for(const z of [x>0?v-1:-1,x<w-1?v+1:-1,y>0?v-w:-1,y<h-1?v+w:-1])if(z>=0&&!seen[z]&&data[z*4+3]){seen[z]=1;q.push(z);}}
        components.push(q);
      }
      const largest=Math.max(...components.map(c=>c.length));
      for(const c of components)if(c.length<largest*.015)for(const n of c)data[n*4+3]=0;
      let l=w,t=h,r=0,b=0,count=0;`);
fs.writeFileSync(p,s);
}
