using UnityEngine;
namespace MaritimeLegend {
 public sealed class MainMenuScene : FunctionalScene {
  OpeningPresentation presentation;
  public override void Render(MaritimeGame game){
   if(presentation==null)presentation=gameObject.AddComponent<OpeningPresentation>();
   game.Title(presentation);
  }
 }
}
