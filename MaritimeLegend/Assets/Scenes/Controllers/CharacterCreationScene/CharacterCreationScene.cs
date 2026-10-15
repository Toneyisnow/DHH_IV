namespace MaritimeLegend { public sealed class CharacterCreationScene : FunctionalScene {public override void Render(MaritimeGame game){if(game.page=="模板")game.Templates();else game.Customize();}}}
