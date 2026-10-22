using System;
using UnityEngine;

namespace MaritimeLegend.CharacterArt {
 [RequireComponent(typeof(SpriteRenderer))]
 public sealed class PixelWalkActor : MonoBehaviour {
  public Texture2D[] outfitAtlases;
  public int outfit, palette;
  public float speed=2.4f, framesPerSecond=7f;
  public bool keyboardControl=true;
  public int Facing { get; private set; }
  public int Frame { get; private set; }
  public bool Moving { get; private set; }
  public const int CellWidth=64, CellHeight=96, Frames=5, WalkFrames=4, IdleFrame=4, Directions=8;
  public static RectInt[] FacePatches(int direction){
   if(direction==0)return new[]{new RectInt(24,65,15,4),new RectInt(29,60,6,4)};
   if(direction==1)return new[]{new RectInt(22,65,14,4),new RectInt(23,60,7,4)};
   if(direction==7)return new[]{new RectInt(29,65,14,4),new RectInt(34,60,7,4)};
   return Array.Empty<RectInt>();
  }
  public static Vector2Int FaceOffset(Texture2D atlas,int direction,int frame){
   Vector2 Anchor(int f){float sum=0;int count=0,top=0;for(int y=70;y<82;y++)for(int x=0;x<64;x++)if(atlas.GetPixel(f*64+x,(7-direction)*96+y).a>.5f){sum+=x;count++;top=Mathf.Max(top,y);}return new Vector2(sum/Mathf.Max(1,count),top);}
   var delta=Anchor(frame)-Anchor(0);return new Vector2Int(Mathf.RoundToInt(delta.x),Mathf.RoundToInt(delta.y));
  }
  public static readonly string[] DirectionNames={"S","SW","W","NW","N","NE","E","SE"};
  public static readonly Vector2[] DirectionVectors={Vector2.down,new Vector2(-1,-1).normalized,Vector2.left,new Vector2(-1,1).normalized,Vector2.up,new Vector2(1,1).normalized,Vector2.right,new Vector2(1,-1).normalized};
  // Palette is baked once per selected outfit, not tinting skin and hair every frame.
  public static readonly Color[] ClothColors={new Color(.60f,.15f,.13f),new Color(.10f,.40f,.36f),new Color(.16f,.28f,.49f),new Color(.43f,.25f,.44f),new Color(.68f,.48f,.20f),new Color(.34f,.43f,.30f),new Color(.28f,.29f,.33f),new Color(.63f,.39f,.40f)};
  SpriteRenderer view;
  Sprite[][] cache;
  Texture2D[] generated;
  float phase;
  int activeOutfit=-1,activePalette=-1;
  void Awake(){Frame=IdleFrame;view=GetComponent<SpriteRenderer>();EnsureCache();SetAppearance(outfit,palette);}
  void EnsureCache(){if(cache!=null)return;int n=(outfitAtlases==null?0:outfitAtlases.Length)*8;cache=new Sprite[n][];generated=new Texture2D[n];}
  public static int DirectionFor(Vector2 d){return (Mathf.RoundToInt(Mathf.Atan2(-d.x,-d.y)*Mathf.Rad2Deg/45f)+8)%8;}
  public void SetAppearance(int selectedOutfit,int selectedPalette){
   EnsureCache();if(outfitAtlases==null||outfitAtlases.Length==0)return;
   outfit=Mathf.Clamp(selectedOutfit,0,outfitAtlases.Length-1);palette=Mathf.Clamp(selectedPalette,0,7);
   if(activeOutfit==outfit&&activePalette==palette)return;
   int key=outfit*8+palette;
   if(cache[key]==null){
    Texture2D source=outfitAtlases[outfit];if(source==null)throw new InvalidOperationException("PixelWalk atlas missing");
    var pixels=source.GetPixels32();Color target=ClothColors[palette];
    // Source jackets: red/base, blue/voyage. Restrict to jacket y-band.
    for(int y=0;y<source.height;y++)for(int x=0;x<source.width;x++){
     int cy=y%CellHeight;int i=y*source.width+x;Color c=pixels[i];
     if(c.a<.5f||cy<40||cy>66)continue;
     Color.RGBToHSV(c,out float h,out float s,out float v);
     bool cloth=outfit==0?(s>.42f&&(h<.045f||h>.95f)&&c.r>c.g*1.6f):(h>.55f&&h<.72f&&s>.30f);
     if(cloth){float value=Mathf.Clamp(v*1.55f,.25f,1.2f);Color painted=target*value;painted.a=c.a;pixels[i]=painted;}
    }
    var tex=new Texture2D(source.width,source.height,TextureFormat.RGBA32,false);tex.name="Cached outfit "+outfit+" palette "+palette;tex.filterMode=FilterMode.Point;tex.wrapMode=TextureWrapMode.Clamp;tex.SetPixels32(pixels);tex.Apply(false,false);generated[key]=tex;
    cache[key]=new Sprite[Directions*Frames];
    for(int d=0;d<Directions;d++)for(int f=0;f<Frames;f++){
     // Body bounds change with stride/cape width. Side-view heads must not inherit
     // that horizontal recentering. Integer-pixel pivots preserve crisp sampling.
     float shift=(d==2||d==6)?Mathf.Round(HeadCenter(pixels,source.width,d,f)-HeadCenter(pixels,source.width,d,IdleFrame)):0;
     cache[key][d*Frames+f]=Sprite.Create(tex,new Rect(f*CellWidth,(7-d)*CellHeight,CellWidth,CellHeight),new Vector2((CellWidth*.5f+shift)/CellWidth,8f/CellHeight),32f,0,SpriteMeshType.FullRect);
    }
   }
   activeOutfit=outfit;activePalette=palette;Refresh();
  }
  public void SetFacing(int facing,int frame=IdleFrame){Facing=(facing%8+8)%8;Frame=Mathf.Clamp(frame,0,Frames-1);Refresh();}
  static float HeadCenter(Color32[] pixels,int width,int direction,int frame){
   float sum=0;int count=0;
   // Fixed band through the hair/face, excluding sleeves and upper torso.
   for(int y=61;y<=78;y++)for(int x=0;x<CellWidth;x++)if(pixels[((7-direction)*CellHeight+y)*width+frame*CellWidth+x].a>160){sum+=x;count++;}
   if(count==0)throw new InvalidOperationException("Side-view head anchor is empty");
   return sum/count;
  }
  public void Step(Vector2 input,float dt){
   Moving=input.sqrMagnitude>.0001f;
   if(Moving){input=Vector2.ClampMagnitude(input,1);Facing=DirectionFor(input);transform.position+=(Vector3)(input*speed*dt);phase+=dt*framesPerSecond;Frame=(int)phase%WalkFrames;}
   else {phase=0;Frame=IdleFrame;}
   Refresh();
  }
  void Update(){if(keyboardControl)Step(new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical")),Time.deltaTime);}
  void Refresh(){
   if(view==null)view=GetComponent<SpriteRenderer>();if(activeOutfit<0)return;
   view.flipX=false;
   view.sprite=cache[activeOutfit*8+activePalette][Facing*Frames+Frame];
  }
  void OnDestroy(){if(cache!=null)foreach(var set in cache)if(set!=null)foreach(var sprite in set)if(sprite!=null)Destroy(sprite);if(generated!=null)foreach(var tex in generated)if(tex!=null)Destroy(tex);}
 }
}


