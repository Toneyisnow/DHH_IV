const fs=require('fs'),path=require('path');
const root=path.resolve(__dirname,'..');
const h=fs.readFileSync(path.join(root,'preview.html'),'utf8');
const refs=[...h.matchAll(/(?:src|href)="([^"]+)"/g)].map(x=>x[1]);
const missing=refs.filter(r=>!fs.existsSync(path.join(root,r)));
if(missing.length)throw Error(JSON.stringify(missing));
new Function(h.match(/<script>([\s\S]*)<\/script>/)[1]);
for(const file of ['manifest.json','atlas_regions.json','prompts.json','verification.json'])JSON.parse(fs.readFileSync(path.join(root,file),'utf8').replace(/^\uFEFF/,''));
console.log('All '+refs.length+' preview links exist; preview JavaScript and metadata parse successfully.');
