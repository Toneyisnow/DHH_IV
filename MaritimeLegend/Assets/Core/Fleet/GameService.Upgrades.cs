namespace MaritimeLegend.Domain {
public partial class GameService {
 public string UpgradeShip(int index){if(index<0||index>=S.ships.Count)return "船只不存在";var ship=S.ships[index];if(ship.busy)return "运输中的船只无法升级";if(ship.level>=5)return "船只已经达到等级上限";if(S.gold<300)return "金币不足";S.gold-=300;ship.level++;return "船只等级提升，炮击伤害增加";}
 public void RewardBattleVictory(bool final){S.gold+=final?750:120;Emit("battle");Gain(9,25);if(final)Finish();}
}
}
