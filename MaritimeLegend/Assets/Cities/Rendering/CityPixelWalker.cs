using UnityEngine;
using MaritimeLegend.Domain;
using MaritimeLegend.CharacterArt;

namespace MaritimeLegend {
 // Reuse the authored animation and pivots; the city alone owns movement and pathfinding.
 public sealed class CityPixelWalker : MonoBehaviour {
  PixelWalkActor baked;
  LayeredPixelWalker layered;
  Material material;
  SpriteRenderer cartoonView;
  Sprite[] cartoonFrames;
  CityCartoonWardrobe wardrobe;
  CityModularWardrobe modular;
  public bool HasModularWardrobe=>modular!=null;
  public int EquipmentMask=>modular!=null?modular.Mask:0;
  public void SetEquipmentMask(int mask){if(modular!=null)modular.SetMask(mask);}
  public void ToggleEquipment(int bit){SetEquipmentMask(EquipmentMask^bit);}
  public int AppearanceIndex=>wardrobe!=null?wardrobe.AppearanceIndex:modular!=null&&modular.Mask!=0?1:0;
  public string AppearanceName=>wardrobe!=null?wardrobe.AppearanceName:modular!=null&&modular.Mask!=0?"分层变种":"原搭配";
  public void SetAppearance(int index){if(wardrobe!=null)wardrobe.SetAppearance(index);else if(modular!=null)modular.SetMask(index==0?0:7);}
  public void CycleAppearance(){SetAppearance((AppearanceIndex+1)%(modular!=null?2:3));}
  int cartoonFrame=4,cartoonFacing;
  float phase;
  public int CharacterId { get; private set; }
  public int Frame => cartoonView != null ? cartoonFrame : baked != null ? baked.Frame : layered.Frame;
  public int Facing => cartoonView != null ? cartoonFacing : baked != null ? baked.Facing : layered.Facing;
  public static int ResolveCharacter(GameState state) {
   var appearance=state?.appearance;
   if(appearance==null)return 0;
   string preset=appearance.portraitV2;
   if(!string.IsNullOrEmpty(preset)) {
    if(preset.StartsWith("04-",System.StringComparison.Ordinal))return 4;
    if(preset.StartsWith("05-",System.StringComparison.Ordinal))return 5;
    return 0;
   }
   // Slot 1 is zero-based slot 0. Only this legacy preview was explicitly approved.
   return state.slot==0&&appearance.baseId==5?4:0;
  }
  static int selectedPlayer;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  static void ResetPlayerSelection(){selectedPlayer=0;}
  public string CharacterName=>new[]{"","欧洲男","欧洲女","亚裔男","亚裔女","非裔男","非裔女","拉美男","拉美女"}[CharacterId];
  public static void SelectPlayerVisual(int character){
   if(character<1||character>8)throw new System.ArgumentOutOfRangeException(nameof(character));
   selectedPlayer=character;
  }
  public void CopyMotionFrom(CityPixelWalker other){phase=other.phase;Pose(other.Facing,other.Frame);}
  // User-selected city player visual (2026-11-07). Never rewrite saved identity.
  public static int ResolvePlayerVisual(GameState state) {
   if(selectedPlayer!=0)return selectedPlayer;
   foreach(var arg in System.Environment.GetCommandLineArgs()) {
    if(arg=="-cityCartoon04")return selectedPlayer=4;
    if(arg=="-cityCartoon05")return selectedPlayer=5;
   }
   return selectedPlayer=6;
  }
  public static CityPixelWalker Create(Transform parent,int character,Shader shader,string preset="") {
   if(character==6)return CreateCartoon(parent,shader);
   string path=character==4?"Prefabs/Characters/Character04/PixelWalker":"Prefabs/Characters/Character05/LayeredPixelWalker";
   var prefab=Resources.Load<GameObject>(path);
   if(prefab==null)throw new System.InvalidOperationException("Missing city pixel walker: "+path);
   var go=Instantiate(prefab,parent,false);
   go.name="City pixel walker Character"+character.ToString("00");
   go.transform.localScale=Vector3.one*1.6f;
   var actor=go.AddComponent<CityPixelWalker>();actor.CharacterId=character;
   actor.baked=go.GetComponent<PixelWalkActor>();actor.layered=go.GetComponent<LayeredPixelWalker>();
   actor.material=new Material(shader);actor.material.SetColor("_Tint",Color.white);
   int option=0;char kind=' ';
   if(!string.IsNullOrEmpty(preset)&&preset.Length>4){kind=preset[3];int.TryParse(preset.Substring(4),out option);}
   if(actor.baked!=null){actor.baked.keyboardControl=false;actor.baked.SetAppearance(0,kind=='C'?Mathf.Clamp(option-1,0,7):0);}
   if(actor.layered!=null){actor.layered.keyboardControl=false;actor.layered.palette=kind=='C'?Mathf.Clamp(option-1,0,7):0;actor.layered.cape=kind=='M'?Mathf.Clamp(option,0,6):0;actor.layered.hat=kind=='H'?(option==1?1:option==3?2:option==5?3:0):0;}
   foreach(var renderer in go.GetComponentsInChildren<SpriteRenderer>()) {
    renderer.gameObject.layer=20;renderer.sharedMaterial=actor.material;
    renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.TwoSided;
    renderer.receiveShadows=true;
   }
   actor.Pose(0,4);return actor;
  }
  public static CityPixelWalker CreateCartoon(Transform parent,Shader shader,int character=6) {
   if(character<1||character>8)throw new System.ArgumentOutOfRangeException(nameof(character));
   string identity="Character"+character.ToString("00");
   string folder=character==6?"PixelWalk":"CartoonWalk";
   var atlas=Resources.Load<Texture2D>("Sprites/Characters/"+identity+"/"+folder+"/walk-sheet");
   if(atlas==null||atlas.width!=840||atlas.height!=1344)throw new System.InvalidOperationException(identity+" requires the 168px 5x8 walk atlas");
   var go=new GameObject("City player "+identity+" cartoon");go.transform.SetParent(parent,false);go.transform.localScale=Vector3.one*1.6f;go.layer=20;
   var actor=go.AddComponent<CityPixelWalker>();actor.CharacterId=character;
   actor.cartoonView=go.AddComponent<SpriteRenderer>();actor.cartoonFrames=new Sprite[40];
   // City direction order S,SW,W,NW,N,NE,E,SE; source rows N,NE,E,SE,S,SW,W,NW.
   for(int d=0;d<8;d++)for(int f=0;f<5;f++) {
    int row=(d+4)%8;
    actor.cartoonFrames[d*5+f]=Sprite.Create(atlas,new Rect(f*168,(7-row)*168,168,168),new Vector2(.5f,10f/168f),64f,0,SpriteMeshType.FullRect);
    actor.cartoonFrames[d*5+f].name=identity+"-"+PixelWalkActor.DirectionNames[d]+"-"+f;
   }
   actor.material=new Material(shader);actor.material.SetColor("_Tint",Color.white);
   actor.cartoonView.sharedMaterial=actor.material;actor.cartoonView.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.TwoSided;actor.cartoonView.receiveShadows=true;
   if(character==6){actor.wardrobe=go.AddComponent<CityCartoonWardrobe>();actor.wardrobe.Initialize(actor.cartoonView,actor.cartoonFrames,actor.material);}
   if(character==1||character==2||character==3||character==7||character==8){actor.modular=go.AddComponent<CityModularWardrobe>();actor.modular.Initialize(actor.cartoonView,actor.cartoonFrames,actor.material,character);}
   actor.Pose(0,4);return actor;
  }
  public void Pose(int direction,int frame) {
   if(cartoonView!=null){cartoonFacing=(direction%8+8)%8;cartoonFrame=Mathf.Clamp(frame,0,4);if(wardrobe!=null)wardrobe.Apply(cartoonFacing,cartoonFrame);else if(modular!=null)modular.Apply(cartoonFacing,cartoonFrame);else cartoonView.sprite=cartoonFrames[cartoonFacing*5+cartoonFrame];return;}
   if(baked!=null)baked.SetFacing(direction,frame);else layered.SetFacing(direction,frame);
   // Alpha-tested layers write depth. Offset toward the camera in authored sort order.
   if(layered!=null)foreach(var renderer in GetComponentsInChildren<SpriteRenderer>())
    if(renderer.transform!=transform)renderer.transform.localPosition=Vector3.back*(renderer.sortingOrder-10)*.001f;
  }
  public void Animate(int direction,bool moving,float dt) {
   phase=moving?phase+dt*7:0;Pose(direction,moving?(int)phase%4:4);
  }
  void OnDestroy(){if(material!=null)Destroy(material);if(cartoonFrames!=null)foreach(var sprite in cartoonFrames)if(sprite!=null)Destroy(sprite);}
 }
}
