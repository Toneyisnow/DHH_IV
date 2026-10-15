namespace MaritimeLegend.Domain {
public partial class GameService {
 public string UpgradeShip(int index){if(index<0||index>=S.ships.Count)return "船只不存在";var ship=S.ships[index];if(ship.busy)return "运输中的船只无法升级";if(ship.level>=C.balance.upgrades.shipMaxLevel)return "船只已经达到等级上限";if(S.gold<C.balance.upgrades.shipPrice)return "金币不足";S.gold-=C.balance.upgrades.shipPrice;ship.level++;return "船只等级提升，炮击伤害增加";}
 public void RewardBattleVictory(bool final){S.gold+=final?C.balance.combat.finalReward:C.balance.combat.practiceReward;Emit("battle");Gain(C.balance.skills.gunnerySkill,C.balance.combat.victoryExperience);if(final)Finish();}
}
}
