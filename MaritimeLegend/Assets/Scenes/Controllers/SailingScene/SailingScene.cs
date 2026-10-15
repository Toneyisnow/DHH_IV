namespace MaritimeLegend { public sealed class SailingScene : FunctionalScene {public override void Render(MaritimeGame game){if(game.page=="战斗")game.BattleUI();else game.SailingView();}}}
