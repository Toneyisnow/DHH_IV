using System;
namespace MaritimeLegend.Domain {
    [Serializable] public class MarketEntry {
        public string product;
        public int stock;
        public double pressure;
        public int bought,sold;
        public int[] qualityStock;
        // Legacy saves supplied every unit as quality 3. Preserve that stock on first use.
        public void EnsureQualityStock(){if(qualityStock!=null&&qualityStock.Length==SaveContract.QualityCount)return;qualityStock=new int[SaveContract.QualityCount];qualityStock[SaveContract.LegacyMarketQuality]=stock;}
        public int Stock(int quality){return qualityStock!=null&&qualityStock.Length==SaveContract.QualityCount?qualityStock[quality]:(quality==SaveContract.LegacyMarketQuality?stock:0);}
        public void ChangeStock(int quality,int delta){EnsureQualityStock();qualityStock[quality]+=delta;stock+=delta;}
    }
}
