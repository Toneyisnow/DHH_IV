const $=id=>document.getElementById(id);
const goods=[
 {name:'橄榄油',q:3,buy:560,sell:410,stock:16,kind:'oil',color:'#95ae6c'},
 {name:'葡萄酒',q:4,buy:840,sell:630,stock:8,kind:'wine',color:'#b45d67'},
 {name:'藏红花',q:5,buy:1680,sell:1250,stock:4,kind:'spice',color:'#e6a955'},
 {name:'羊毛',q:1,buy:240,sell:175,stock:24,kind:'wool',color:'#d7c9ab'},
 {name:'陶器',q:2,buy:380,sell:280,stock:12,kind:'pot',color:'#c98c6a'},
 {name:'小麦',q:0,buy:120,sell:80,stock:32,kind:'wheat',color:'#d0b369'}
];
const quality=['#b1b5a4','#9bbd93','#87bfc8','#aaa1d2','#d1ab73','#edcb7b'];
const names=['圣玛利亚号','海燕号','曙光号','产业园仓库'];
const shipTypes=['旗舰 · 大型帆船','商船 · 卡拉维尔','商船 · 卡拉克'];
const clone=x=>JSON.parse(JSON.stringify(x));
let base, draft, sales=[], lastHold=0, seq=0;
const flights=new Map();
function initial(){return {gold:18450,stock:goods.map(x=>x.stock),holds:[[1,0,null,4,null,null],[3,3,2,null,null,null],[5,null,4,null,null,null],[0,1,3,null,null,null,null,null,null,null,null,null]]};}
function art(id){const g=goods[id];let shapes='';
 if(g.kind==='oil')shapes='<path d="M49 27h22v13c0 8 16 12 18 29l-4 33c-17 8-35 8-50 0l-4-33c2-17 18-21 18-29z" fill="#8d9d55"/><path d="M48 25h24v8H48z" fill="#d5b47b"/><path d="M41 65q20-8 40 0v24q-20 7-40 0z" fill="#dfd0a0"/><path d="M58 81q-7-15 7-18m-5 13q13-8 13-3" fill="none" stroke="#687e46" stroke-width="3"/><path d="M43 51l-4 23" stroke="#e7dc9d" opacity=".6" stroke-width="4"/><ellipse cx="88" cy="101" rx="10" ry="5" fill="#6c773b"/><ellipse cx="98" cy="95" rx="8" ry="5" fill="#96a264"/>';
 if(g.kind==='wine')shapes='<path d="M32 24h15v29l9 15v37H22V68l10-15z" fill="#424e3c"/><path d="M31 23h17v15H31z" fill="#b67b60"/><path d="M25 73h28v23H25z" fill="#d9c49b"/><path d="M74 32h13v22c0 10 10 10 10 24v28H63V78c0-14 11-14 11-24z" fill="#70404a"/><path d="M73 27h15v13H73z" fill="#c6a46b"/><path d="M67 76h26v20H67z" fill="#d3b695"/><path d="M29 58v11m40-7v10" stroke="#e1d6a3" opacity=".4" stroke-width="3"/>';
 if(g.kind==='spice')shapes='<path d="M24 65l11 35q26 16 52 0l10-35z" fill="#a97143"/><ellipse cx="60" cy="65" rx="37" ry="16" fill="#d5aa6b"/><ellipse cx="60" cy="64" rx="29" ry="11" fill="#a64528"/><path d="M38 62l14-9m-2 14l18-14m-10 17l16-14m-29 3l18 4m3-3l15 5m-34-4l4 9m28-13l-7 13" stroke="#ed9c3f" stroke-width="2"/><path d="M88 39l13 4-7 35-14-4z" fill="#cfad77"/><path d="M88 39l13 4 2-8-13-4z" fill="#70533f"/>';
 if(g.kind==='wool')shapes='<path d="M27 62q-13-22 10-30 9-18 26-5 23-8 27 13 19 6 9 27 6 20-17 25-16 20-34 4-28 4-27-17z" fill="#d3ccb5"/><path d="M28 57q13-14 22 1t24-2 20 2M33 76q12-15 25-2t31-3M44 38q10-8 19 4" stroke="#a49a81" fill="none" stroke-width="3"/><path d="M39 30l14 66M78 30L64 99" stroke="#a67e4b" stroke-width="4"/><path d="M50 64l18 3" stroke="#7d5c39" stroke-width="3"/>';
 if(g.kind==='pot')shapes='<path d="M46 28h28l-4 17q30 18 19 53-25 15-51 0-10-35 13-53z" fill="#ba7956"/><ellipse cx="60" cy="28" rx="15" ry="5" fill="#6b4935"/><path d="M40 66q20 9 47 0M38 76q24 10 51 0" stroke="#debe8a" stroke-width="4" fill="none"/><path d="M76 43q31-2 22 28L85 82" fill="none" stroke="#ad704d" stroke-width="8"/><path d="M49 46q-9 13-7 19" stroke="#e2a77b" stroke-width="3" fill="none"/>';
 if(g.kind==='wheat')shapes='<path d="M32 48q28 10 56 0l12 48q-38 23-77 0z" fill="#ac9468"/><ellipse cx="60" cy="47" rx="28" ry="10" fill="#d9bd7e"/><path d="M38 53l-7 37M80 56l9 34" stroke="#d3bb8c" stroke-width="3"/><path d="M57 85L66 24m-9 46l-13-9m15-1l15-9m-12-1l-12-9m13-1l13-8m-11 0l-7-11" stroke="#e6c778" stroke-width="4" fill="none"/><path d="M25 99q33 15 71-1" stroke="#756644" stroke-width="3" fill="none"/>';
 return `<svg viewBox="0 0 120 120" aria-hidden="true"><ellipse cx="60" cy="108" rx="43" ry="5" fill="#000" opacity=".2"/>${shapes}</svg>`;
}
const stars=id=>goods[id].q?'★'.repeat(goods[id].q):'○';
const tint=id=>`--quality:${quality[goods[id].q]};--tint:${quality[goods[id].q]}27`;
const fmt=n=>n.toLocaleString('en-US');
function reset(){base=initial();draft=base.holds.map(h=>h.map(g=>g===null?null:{g,id:++seq,bought:false}));sales=[];lastHold=0;render();}
function totals(){const purchases=draft.flat().filter(x=>x?.bought);const cost=purchases.reduce((n,x)=>n+goods[x.g].buy,0),income=sales.reduce((n,x)=>n+goods[x.item.g].sell,0);return {purchases,cost,income,balance:base.gold-cost+income};}
function stock(g){return base.stock[g]-totals().purchases.filter(x=>x.g===g).length;}
function notify(message,error=false){$('status').textContent=message;$('status').classList.toggle('error',error);}
function holdSelector(h,i){return `[data-h="${h}"][data-slot="${i}"]`;}
async function fly(source,change,targetSelector){
 const flightId=Symbol(); flights.set(flightId,targetSelector);
 const rect=source.getBoundingClientRect(),ghost=source.cloneNode(true),width=source.offsetWidth,height=source.offsetHeight;
 ghost.removeAttribute('id');ghost.removeAttribute('aria-label');ghost.setAttribute('aria-hidden','true');ghost.tabIndex=-1;
 ghost.classList.add('flying-card');Object.assign(ghost.style,{position:'fixed',left:rect.left+'px',top:rect.top+'px',width:width+'px',height:height+'px',margin:'0',zIndex:'1000',pointerEvents:'none'});
 document.body.appendChild(ghost);let target;
 try{change();render();target=document.querySelector(targetSelector);if(!target)return;
 if(!targetSelector.startsWith('[data-buy='))target.scrollIntoView({block:'nearest',inline:'nearest'});
 const end=target.getBoundingClientRect();syncFlights();
 const animation=ghost.animate([{transform:`translate(0,0) scale(${rect.width/width},${rect.height/height})`},{transform:`translate(${end.left-rect.left}px,${end.top-rect.top}px) scale(${end.width/width},${end.height/height})`}],{duration:matchMedia('(prefers-reduced-motion: reduce)').matches?0:420/1.3,easing:'linear',fill:'forwards'});
 await animation.finished;
 }finally{ghost.remove();flights.delete(flightId);syncFlights();}
}
async function buy(g,source){if(totals().balance<goods[g].buy){notify("金币不足，无法买入该商品。",true);return;}if(stock(g)<1){notify('该商品已售罄。',true);return;}const slot=draft[lastHold].indexOf(null);if(slot<0){notify(`${names[lastHold]}没有空货仓，请先点击其他船只或仓库的卡片。`,true);return;}
 const h=lastHold;await fly(source,()=>{draft[h][slot]={g,id:++seq,bought:true};},holdSelector(h,slot));notify(`已送入${names[h]} ${slot+1}号位，等待成交。`);}
async function slotClick(h,i,source){lastHold=h;const item=draft[h][i];if(!item){render();notify(`已选定${names[h]}，购买商品将进入其第一个空位。`);return;}
 if(item.bought){await fly(source,()=>{draft[h][i]=null;},`[data-buy="${item.g}"]`);notify('已撤回买入。');return;}
 await fly(source,()=>{draft[h][i]=null;sales.push({item,from:h,slot:i});},`[data-sale="${item.id}"]`);notify(`${goods[item.g].name}已移至待售区，原货仓可接收新商品。`);}
async function returnSale(id,source){const sale=sales.find(x=>x.item.id===id);if(!sale)return;
 const h=sale.from;let slot=-1;for(let i=sale.slot;i<draft[h].length;i++){if(draft[h][i]===null){slot=i;break;}}
 if(slot<0){notify(`${names[h]}原位及右侧均无空位，无法放回；请先腾出货仓。`,true);return;}
 await fly(source,()=>{draft[h][slot]=sale.item;sales=sales.filter(x=>x!==sale);},holdSelector(h,slot));notify(`已放回${names[h]} ${slot+1}号位。`);}
function emptyArt(){return '<svg viewBox="0 0 80 48" aria-hidden="true"><path d="M12 15L40 5l28 10v23L40 45 12 38z" fill="#233a40" stroke="#617873"/><path d="M12 15l28 9 28-9M40 24v21M22 19v21M58 19v21" fill="none" stroke="#617873"/><path d="M22 18l18-6 18 6-18 6z" fill="#102128"/></svg>';}
function itemMarkup(item,tag=''){return `<span class="stars">${stars(item.g)}</span><span class="art">${art(item.g)}</span><span class="slot-name">${goods[item.g].name}</span><span class="sell-price">${fmt(item.bought?goods[item.g].buy:goods[item.g].sell)}</span>${tag?`<span class="tag">${tag}</span>`:''}`;}
function slotMarkup(h,i){const item=draft[h][i];return `<button class="slot ${item?(item.bought?'buy':''):'empty'}" data-h="${h}" data-slot="${i}" ${item?`style="${tint(item.g)}"`:''} aria-label="${names[h]} ${i+1}号货仓 ${item?goods[item.g].name+(item.bought?' 待入':''):'空货仓'}">${item?itemMarkup(item,item.bought?'待入':''):`<span class="empty-art">${emptyArt()}</span><span>空货仓</span>`}</button>`;}
function syncFlights(){
 document.querySelectorAll('.landing').forEach(el=>el.classList.remove('landing'));
 for(const selector of flights.values())if(!selector.startsWith('[data-buy='))document.querySelector(selector)?.classList.add('landing');
 const t=totals();
 $('confirm').disabled=flights.size>0||(!t.purchases.length&&!sales.length)||t.balance<0;
 $('reset').disabled=flights.size>0;
}
function render(){const t=totals();$('gold').textContent='◈ '+fmt(base.gold);$('balance').textContent=fmt(t.balance);$('balance').classList.toggle('error',t.balance<0);
 $('marketCards').innerHTML=goods.map((g,i)=>`<button class="product ${t.balance<g.buy?'unaffordable':''}" data-buy="${i}" style="${tint(i)}" ${stock(i)<1||t.balance<g.buy?'disabled':''} aria-label="买入${g.name}，品质${g.q}，${g.buy}金币，剩余${stock(i)}份${t.balance<g.buy?'，金币不足':''}"><span class="stars">${stars(i)}</span><div class="art">${art(i)}</div><div class="product-info"><div class="product-name">${g.name}<span>＋</span></div><div class="price">${fmt(g.buy)}<small>金币 / 份</small></div></div><div class="stock"><span>城市库存</span><span><b>${stock(i)}</b> 份</span></div></button>`).join('');
 $('ships').innerHTML=draft.slice(0,3).map((hold,h)=>`<div class="ship ${lastHold===h?'active-hold':''}"><div class="ship-head"><h3>⚓ ${names[h]}<small>${shipTypes[h]}</small></h3></div><div class="slots">${hold.map((_,i)=>slotMarkup(h,i)).join('')}</div></div>`).join('');
 $('warehouse').classList.toggle('active-hold',lastHold===3);$('warehouseSlots').innerHTML=draft[3].map((_,i)=>slotMarkup(3,i)).join('');
 $('saleCards').innerHTML=sales.map(s=>`<button class="slot sale-card" data-sale="${s.item.id}" style="${tint(s.item.g)}" aria-label="放回${goods[s.item.g].name}至${names[s.from]}，原${s.slot+1}号位">${itemMarkup(s.item,'待售')}</button>`).join('');
 $('buyTotal').textContent=fmt(t.cost);$('sellTotal').textContent=fmt(t.income);$('confirm').disabled=(!t.purchases.length&&!sales.length)||t.balance<0;$('confirm').textContent=t.balance<0?'金币不足':t.purchases.length||sales.length?`确认成交 · ${t.cost>=t.income?'支付':'收入'} ${fmt(Math.abs(t.cost-t.income))}`:'确认成交';
 syncFlights();
}
document.addEventListener('click',async e=>{const b=e.target.closest('button');if(!b||b.disabled)return;if(b.dataset.buy!==undefined)await buy(+b.dataset.buy,b);else if(b.dataset.h!==undefined)await slotClick(+b.dataset.h,+b.dataset.slot,b);else if(b.dataset.sale!==undefined)await returnSale(+b.dataset.sale,b);});
$('confirm').onclick=()=>{if(flights.size)return;const t=totals();if(t.balance<0||(!sales.length&&!t.purchases.length))return;for(const item of t.purchases)base.stock[item.g]--;for(const sale of sales)base.stock[sale.item.g]++;base.gold=t.balance;for(const item of draft.flat())if(item)item.bought=false;base.holds=draft.map(h=>h.map(x=>x?x.g:null));sales=[];render();notify('交易完成，货物与金币已更新。');};
$('reset').onclick=()=>{if(flights.size)return;reset();notify('演示已重置，默认选定圣玛利亚号。');};
reset();




function fitDialog(){document.documentElement.style.setProperty("--ui-scale",Math.min(innerWidth/1920,innerHeight/1080));}
addEventListener("resize",fitDialog);fitDialog();
