using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    public class InventoryService : IInventoryService {
        readonly Catalog c; public InventoryService(Catalog catalog){c=catalog;}
        public int Count(Inventory i,string p,int q=-1){return i.batches.Where(x=>x.product==p&&(q<0||x.quality==q)).Sum(x=>x.quantity);}
        public double Weight(Inventory i){return i.batches.Sum(x=>c.Product(x.product).weight*x.quantity);}
        public void Add(Inventory i,string p,int q,int n,int origin){if(n<=0)return;var b=i.batches.FirstOrDefault(x=>x.product==p&&x.quality==q&&x.origin==origin);if(b==null)i.batches.Add(new Batch(p,q,n,origin));else b.quantity+=n;}
        public List<Batch> Take(Inventory i,string p,int n,int q=-1){if(n<=0||Count(i,p,q)<n)throw new Exception("库存不足");var result=new List<Batch>();foreach(var b in i.batches.Where(x=>x.product==p&&(q<0||x.quality==q)).OrderByDescending(x=>x.quality).ToList()){int take=Math.Min(n,b.quantity);result.Add(new Batch(p,b.quality,take,b.origin));b.quantity-=take;n-=take;if(n==0)break;}i.batches.RemoveAll(x=>x.quantity<=0);return result;}
        public string Transfer(Inventory from,Inventory to,string p,int q,int n){if(n<1||Count(from,p,q)<n)return "库存不足";if(Weight(to)+c.Product(p).weight*n>to.capacity)return "目标仓库容量不足";foreach(var b in Take(from,p,n,q))Add(to,b.product,b.quality,b.quantity,b.origin);return "已转移 "+n+" 件";}
    }
}
