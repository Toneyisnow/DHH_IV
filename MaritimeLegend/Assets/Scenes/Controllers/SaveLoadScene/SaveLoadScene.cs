namespace MaritimeLegend {
 public sealed class SaveLoadScene : FunctionalScene {
  SaveSelectionPresentation presentation;
  public override void Render(MaritimeGame game){if(presentation==null)presentation=gameObject.AddComponent<SaveSelectionPresentation>();game.Slots(presentation);}
 }
}
