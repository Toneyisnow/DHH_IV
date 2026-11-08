using System;
using UnityEngine;

namespace MaritimeLegend {
 // Small, authored wardrobe experiment: shared body animation + aligned replacement layers.
 public sealed class CityCartoonWardrobe : MonoBehaviour {
  public static readonly string[] Names={"原搭配","旅行搭配","绿色搭配"};
  static readonly string[] Recipes={"C1-H5","C2-H1","C5-H7"};
  static int selectedLook;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  static void ResetSelection(){selectedLook=0;}
  SpriteRenderer body;
  Sprite[] original,headless;
  readonly SpriteRenderer[] layers=new SpriteRenderer[3];
  readonly Sprite[][][] options=new Sprite[3][][];
  int facing,frame=4;
  public int AppearanceIndex {get;private set;}
  public string AppearanceName=>Names[AppearanceIndex];
  public void Initialize(SpriteRenderer renderer,Sprite[] baseFrames,Material material){
   body=renderer;original=baseFrames;
   string[] labels={"Clothing color layer","Head replacement layer","Jewelry layer"};
   for(int i=0;i<3;i++){
    var go=new GameObject(labels[i]);go.transform.SetParent(transform,false);go.transform.localPosition=Vector3.back*((i+1)*.001f);go.layer=20;
    layers[i]=go.AddComponent<SpriteRenderer>();layers[i].sharedMaterial=material;layers[i].sortingOrder=i+1;
    layers[i].shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.TwoSided;layers[i].receiveShadows=true;
   }
   SetAppearance(selectedLook);
  }
  Sprite[] Load(string name){
   var atlas=Resources.Load<Texture2D>("Sprites/Characters/Character06/WardrobeTest/"+name);
   if(atlas==null||atlas.width!=840||atlas.height!=1344)throw new InvalidOperationException("Missing 168px wardrobe atlas: "+name);
   var sprites=new Sprite[40];for(int d=0;d<8;d++)for(int f=0;f<5;f++){
    sprites[d*5+f]=Sprite.Create(atlas,new Rect(f*168,(7-(d+4)%8)*168,168,168),new Vector2(.5f,10f/168f),64,0,SpriteMeshType.FullRect);
    sprites[d*5+f].name=name+"-"+d+"-"+f;
   }return sprites;
  }
  public void SetAppearance(int index){
   index=Mathf.Clamp(index,0,2);
   if(index>0&&options[index]==null){
    if(headless==null)headless=Load("body-no-head");
    options[index]=new[]{Load(Recipes[index]+"-cloth"),Load(Recipes[index]+"-head"),Load(Recipes[index]+"-beads")};
   }
   selectedLook=AppearanceIndex=index;Apply(facing,frame);
  }
  public void Apply(int direction,int pose){
   facing=(direction%8+8)%8;frame=Mathf.Clamp(pose,0,4);int i=facing*5+frame;
   body.sprite=(AppearanceIndex==0?original:headless)[i];
   for(int n=0;n<layers.Length;n++){layers[n].enabled=AppearanceIndex>0;layers[n].sprite=AppearanceIndex>0?options[AppearanceIndex][n][i]:null;}
  }
  void OnDestroy(){
   if(headless!=null)foreach(var sprite in headless)Destroy(sprite);
   foreach(var option in options)if(option!=null)foreach(var layer in option)foreach(var sprite in layer)Destroy(sprite);
  }
 }
}
