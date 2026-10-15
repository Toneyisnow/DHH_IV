namespace MaritimeLegend.Domain {
public partial class GameService {
 public string ExpandWarehouse(){if(S.gold<200)return "金币不足";S.gold-=200;S.cities[S.city].warehouse.capacity+=100;return "仓库扩建完成";}
}
}
