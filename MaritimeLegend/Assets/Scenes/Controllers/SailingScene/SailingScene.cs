using UnityEngine;
namespace MaritimeLegend {
 public sealed class SailingScene : FunctionalScene {
  public static SailingScene Active {get;private set;}
  [Header("手动航海镜头（运行时可调整）")]
  [Min(1),Tooltip("最近距离：透视镜头距离，数值越小越近。")]
  public float minimumZoom=24;
  [Min(1),Tooltip("最远距离：透视镜头距离，必须不小于最近距离。")]
  public float maximumZoom=120;
  [Range(20,55),Tooltip("相对水平面的俯角。右键旋转只改变方位，不改变此角度。")]
  public float pitch=35;
  void OnEnable(){Active=this;}
  void OnDisable(){if(Active==this)Active=null;}
  void OnValidate(){minimumZoom=Mathf.Max(1,minimumZoom);maximumZoom=Mathf.Max(minimumZoom,maximumZoom);pitch=Mathf.Clamp(pitch,20,55);}
  public override void Render(MaritimeGame game){if(game.page=="战斗")game.BattleUI();else game.SailingView();}
 }
}
