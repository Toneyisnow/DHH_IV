using System;
using UnityEngine;
namespace MaritimeLegend {
 // Independent slots share one 8-direction gait; no outfit-combination atlases are loaded.
 public sealed class CityModularWardrobe:MonoBehaviour {
  static readonly int[] selected=new int[9];
  static readonly string[] Names={"hat-back","cape-back","accessory-back","body-hat","cape-front","accessory-front","hat-front"};
  static readonly int[] Bits={1,2,4,0,2,4,1};
  readonly SpriteRenderer[] renderers=new SpriteRenderer[7];
  readonly Sprite[][] frames=new Sprite[7][];
  SpriteRenderer body;Sprite[] original;int character,facing,pose=4;
  public int Mask {get;private set;}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void Reset(){Array.Clear(selected,0,selected.Length);}
  public void Initialize(SpriteRenderer source,Sprite[] sprites,Material material,int id){
   body=source;original=sprites;character=id;
   for(int n=0;n<7;n++)if(n!=3){var go=new GameObject(Names[n]);go.layer=20;go.transform.SetParent(transform,false);go.transform.localPosition=Vector3.back*((n-3)*.001f);var r=go.AddComponent<SpriteRenderer>();r.sharedMaterial=material;r.sortingOrder=n-3;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.TwoSided;r.receiveShadows=true;renderers[n]=r;}
   SetMask(selected[id]);
  }
  Sprite[] Load(string name){
   var t=Resources.Load<Texture2D>("Sprites/Characters/Character"+character.ToString("00")+"/ModularWalk/"+name);
   if(t==null||t.width!=840||t.height!=1344)throw new InvalidOperationException("Missing modular walk layer "+character+"/"+name);
   var a=new Sprite[40];for(int d=0;d<8;d++)for(int f=0;f<5;f++)a[d*5+f]=Sprite.Create(t,new Rect(f*168,(7-(d+4)%8)*168,168,168),new Vector2(.5f,10f/168f),64,0,SpriteMeshType.FullRect);return a;
  }
  public void SetMask(int mask){Mask=mask&7;selected[character]=Mask;if(Mask!=0)for(int n=0;n<7;n++)if(frames[n]==null)frames[n]=Load(Names[n]);Apply(facing,pose);}
  public void Apply(int direction,int frame){facing=direction;pose=frame;int i=direction*5+frame;body.sprite=(Mask&1)!=0?frames[3][i]:original[i];for(int n=0;n<7;n++)if(n!=3){renderers[n].enabled=(Mask&Bits[n])!=0;renderers[n].sprite=renderers[n].enabled?frames[n][i]:null;}}
  void OnDestroy(){foreach(var a in frames)if(a!=null)foreach(var s in a)if(s!=null)Destroy(s);}
 }
}
