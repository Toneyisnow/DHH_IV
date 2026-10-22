using System;
using UnityEngine;
namespace MaritimeLegend.CharacterArt {
 [RequireComponent(typeof(SpriteRenderer))]
 public sealed class LayeredPixelWalker : MonoBehaviour {
  public Texture2D[] bodies, hattedBodies, capeBacks, capeFronts, hats, hatRears, accessories;
  public Texture2D foregroundHands;
  public int palette, cape, hat, weapon, kit; public bool parrot;
  public float speed=2.4f, framesPerSecond=6f; public bool keyboardControl=true;
  public int Facing{get;private set;} public int Frame{get;private set;}=4;
  public bool Moving{get;private set;}
  public static readonly string[] Directions={"S","SW","W","NW","N","NE","E","SE"};
  public static readonly Vector2[] Vectors={Vector2.down,new Vector2(-1,-1).normalized,Vector2.left,new Vector2(-1,1).normalized,Vector2.up,new Vector2(1,1).normalized,Vector2.right,new Vector2(1,-1).normalized};
  SpriteRenderer body,back,front,head,headRear,blade,pet,utility,hands;
  readonly System.Collections.Generic.Dictionary<Texture2D,Sprite[]> cache=new();
  void Awake(){body=GetComponent<SpriteRenderer>();back=Layer("Cape back",0);front=Layer("Cape front",20);head=Layer("Headwear",60);headRear=Layer("Rear head tie",8);blade=Layer("Weapon",25);pet=Layer("Parrot",50);utility=Layer("Utility kit",26);hands=Layer("Foreground hands",40);body.sortingOrder=10;Refresh();}
  SpriteRenderer Layer(string name,int order){var go=new GameObject(name);go.transform.SetParent(transform,false);var r=go.AddComponent<SpriteRenderer>();r.sharedMaterial=body.sharedMaterial;r.sortingOrder=order;return r;}
  Sprite Get(Texture2D tex){if(tex==null)return null;if(!cache.TryGetValue(tex,out var frames)){frames=new Sprite[40];for(int d=0;d<8;d++)for(int f=0;f<5;f++)frames[d*5+f]=Sprite.Create(tex,new Rect(f*64,(7-d)*96,64,96),new Vector2(.5f,8f/96),32,0,SpriteMeshType.FullRect);cache.Add(tex,frames);}return frames[Facing*5+Frame];}
  void Set(SpriteRenderer r,Texture2D t){if(r==null)return;r.sprite=Get(t);r.enabled=t!=null;}
  Texture2D Option(Texture2D[] a,int index){return a!=null&&index>=0&&index<a.Length?a[index]:null;}
  public void Refresh(){if(body==null)return;palette=Mathf.Clamp(palette,0,7);cape=Mathf.Clamp(cape,0,6);hat=Mathf.Clamp(hat,0,3);weapon=Mathf.Clamp(weapon,0,2);kit=Mathf.Clamp(kit,0,3);
   Set(body,Option(hat>0?hattedBodies:bodies,palette));Set(back,Option(capeBacks,cape-1));Set(front,Option(capeFronts,cape-1));Set(head,Option(hats,hat-1));
   Set(headRear,hat==1?Option(hatRears,0):null);headRear.sortingOrder=Facing>=3&&Facing<=5?55:8;
   Set(blade,Option(accessories,weapon-1));Set(pet,parrot?Option(accessories,2):null);Set(utility,Option(accessories,kit>0?kit+2:-1));Set(hands,(weapon>0||kit>0||cape>0)?foregroundHands:null);
   // Physical right stays physical right: equipment positioning is not inherited from body mirroring.
   blade.sortingOrder=(Facing==1||Facing==2||Facing==3)?5:25;utility.sortingOrder=(Facing==5||Facing==6||Facing==7)?6:26;pet.sortingOrder=(Facing==5||Facing==6||Facing==7)?7:50;
  }
  float phase;
  public void SetFacing(int d,int f=4){Facing=(d%8+8)%8;Frame=Mathf.Clamp(f,0,4);Refresh();}
  public void Step(Vector2 input,float dt){Moving=input.sqrMagnitude>.0001f;if(Moving){var v=Vector2.ClampMagnitude(input,1);Facing=(Mathf.RoundToInt(Mathf.Atan2(-v.x,-v.y)*Mathf.Rad2Deg/45)+8)%8;transform.position+=(Vector3)v*speed*dt;phase+=dt*framesPerSecond;Frame=(int)phase%4;}else{phase=0;Frame=4;}Refresh();}
  void Update(){if(keyboardControl)Step(new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical")),Time.deltaTime);}
  public bool LayersAgree(){foreach(var r in GetComponentsInChildren<SpriteRenderer>())if(r.enabled&&r.sprite!=null){var rect=r.sprite.rect;if(rect.x!=Frame*64||rect.y!=(7-Facing)*96)return false;}return true;}
  public int VisibleLayers(){int n=0;foreach(var r in GetComponentsInChildren<SpriteRenderer>())if(r.enabled&&r.sprite!=null)n++;return n;}
  void OnDestroy(){foreach(var set in cache.Values)foreach(var sprite in set)if(sprite)Destroy(sprite);}
 }
}

