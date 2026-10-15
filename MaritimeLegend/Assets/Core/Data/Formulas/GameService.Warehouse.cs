namespace MaritimeLegend.Domain {
public partial class GameService {
 public string ExpandWarehouse(){if(S.gold<C.balance.upgrades.warehousePrice)return "金币不足";S.gold-=C.balance.upgrades.warehousePrice;S.cities[S.city].warehouse.capacity+=C.balance.upgrades.warehouseCapacityStep;return "仓库扩建完成";}
}
}
