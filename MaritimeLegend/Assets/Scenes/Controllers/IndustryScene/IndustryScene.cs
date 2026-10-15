namespace MaritimeLegend { public sealed class IndustryScene : FunctionalScene {public override void Render(MaritimeGame game){if(game.page=="仓库")game.WarehouseUI();else game.IndustryUI();}}}
