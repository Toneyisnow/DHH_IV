using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    [Serializable] public class Catalog : IRecipeCatalog {
        public int version; public string[] industries; public Product[] products; public Recipe[] recipes; public Port[] ports; public PartnerDef[] partners; public ShipDef[] ships; public SkillDef[] skills; public EventDef[] events; public QuestDef[] quests; public FactoryDef[] factories; public MarketConfig market;
        public Product Product(string id) { return products.First(x=>x.id==id); }
        public Product Named(string name) { return products.First(x=>x.name==name); }
        public Recipe Recipe(string id) { return recipes.First(x=>x.id==id); }
        public void Validate() {
            if(products.Select(x=>x.id).Distinct().Count()!=products.Length || recipes.Select(x=>x.id).Distinct().Count()!=recipes.Length) throw new Exception("重复ID");
            foreach(var p in products) { if(p.basePrice<=0||p.weight<=0||p.quality<0||p.quality>5)throw new Exception("商品数值错误"); if(p.category=="R"&&ports.Count(c=>c.raw.Contains(p.id))<2)throw new Exception("原料缺少双港供应："+p.name); }
            foreach(var r in recipes) { Product(r.output); if(r.inputs.Length==0||r.quantity<1)throw new Exception("空配方"); foreach(var i in r.inputs){Product(i.product);if(i.quantity<1)throw new Exception("空原料");} Visit(r.output,new HashSet<string>()); }
            if(skills.Length!=12 || ports.Length<8||partners.Length<12||events.Length<12)throw new Exception("内容不足");
        }
        void Visit(string id,HashSet<string> path) { if(!path.Add(id))throw new Exception("配方循环:"+id); foreach(var r in recipes.Where(x=>x.output==id))foreach(var i in r.inputs)Visit(i.product,new HashSet<string>(path)); }
    }
}
