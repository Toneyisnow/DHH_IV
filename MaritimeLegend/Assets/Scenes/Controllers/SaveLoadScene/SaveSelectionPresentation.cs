using UnityEngine;
namespace MaritimeLegend {
 public sealed class SaveSelectionPresentation : MonoBehaviour {
  readonly Texture2D[] cards=new Texture2D[7];
  readonly float[] hover=new float[7];
  const float CardGroupOffsetY=-110f;
  readonly Vector2[] centers={new Vector2(610,355),new Vector2(960,355),new Vector2(1310,355),new Vector2(410,790),new Vector2(770,790),new Vector2(1130,790),new Vector2(1490,790)};
  Texture2D table,glow,candle,flame,instrument,logo,shade,candleSoftLight;
  Material candleLightMaterial;
  static readonly Vector2 CandleWick=new Vector2(1860,115);
  float entered;Vector2 lastMouse;bool keyboard;
  public int Focus {get;private set;}=0;
  public int Selected {get;private set;}=-1;
  public int PreviewHover=-2;
  public float HoverAmount(int index)=>hover[index];
  static Texture2D Load(string name)=>Resources.Load<Texture2D>("SaveSelection/"+name);
  void Awake(){
   for(int i=0;i<7;i++)cards[i]=Load("card"+(i+1));
   table=Load("01_clean_table_background");glow=Load("frame_glow");candle=Load("03_candle_holder");flame=Load("flame");instrument=Load("06_foreground_instrument");logo=Resources.Load<Texture2D>("Opening/03_title_maritime_legend");
   entered=Time.unscaledTime;lastMouse=LiveGui.Mouse;
   shade=new Texture2D(128,72,TextureFormat.RGBA32,false);var pixels=new Color[128*72];
   for(int y=0;y<72;y++)for(int x=0;x<128;x++){float d=new Vector2((x-63.5f)/64,(y-35.5f)/36).magnitude;pixels[y*128+x]=new Color(.025f,.012f,.007f,Mathf.SmoothStep(0,.64f,Mathf.Clamp01((d-.45f)/.8f)));}
   shade.SetPixels(pixels);shade.Apply();
   candleLightMaterial=new Material(Resources.Load<Shader>("Opening/OpeningAdditive")){name="Selection candle light"};
   candleSoftLight=new Texture2D(128,128,TextureFormat.RGBA32,false){name="Selection candle falloff",wrapMode=TextureWrapMode.Clamp};
   var lightPixels=new Color[128*128];
   for(int y=0;y<128;y++)for(int x=0;x<128;x++){
    float radius=new Vector2((x-63.5f)/63.5f,(y-63.5f)/63.5f).magnitude;
    lightPixels[y*128+x]=new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-radius),2.5f));
   }
   candleSoftLight.SetPixels(lightPixels);candleSoftLight.Apply(false,true);
  }
  void CandleLight(Vector2 center,float width,float height,Color tint){
   LiveGui.color=tint;
   LiveGui.DrawTexture(new Rect(center.x-width/2,center.y-height/2,width,height),candleSoftLight,ScaleMode.StretchToFill,candleLightMaterial);
   LiveGui.color=Color.white;
  }
  // One table projection for every card. Near edges widen; far edges converge.
  public static Vector2 Project(Vector2 p){float depth=1+(p.y-540)*.00024f;return new Vector2(960+(p.x-960)*depth,540+(p.y-540)*.82f*depth);}
  public Vector2[] Corners(int index,float lift=0,float expand=1){
   Vector2 c=centers[index];float w=340*expand,h=430*expand;
   var q=new[]{new Vector2(-w/2,-h/2),new Vector2(w/2,-h/2),new Vector2(w/2,h/2),new Vector2(-w/2,h/2)};
   float angle=(index==0?-2:index==2?2:index==3?-3:index==6?3:0)*Mathf.Deg2Rad;
   for(int j=0;j<4;j++){var p=q[j];q[j]=Project(c+new Vector2(p.x*Mathf.Cos(angle)-p.y*Mathf.Sin(angle),p.x*Mathf.Sin(angle)+p.y*Mathf.Cos(angle)))+new Vector2(0,CardGroupOffsetY-lift);}
   return q;
  }
  public int Hit(Vector2 mouse){
   for(int i=6;i>=0;i--){var q=Corners(i);bool inside=true;for(int j=0;j<4;j++){var a=q[j];var b=q[(j+1)%4];if((b.x-a.x)*(mouse.y-a.y)-(b.y-a.y)*(mouse.x-a.x)<0){inside=false;break;}}if(inside)return i;}return -1;
  }
  public void Select(int index){Selected=index;Focus=index;}
  public int Draw(bool interactive){
   LiveGui.FitReferenceViewport();
   Vector2 mouse=LiveGui.ReferenceMouse;if((mouse-lastMouse).sqrMagnitude>2)keyboard=false;lastMouse=mouse;
   int hit=Hit(mouse);if(PreviewHover>=-1)hit=PreviewHover;
   if(interactive){
    Vector2 direction=Vector2.zero;
    if(Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.A))direction=Vector2.left;
    if(Input.GetKeyDown(KeyCode.RightArrow)||Input.GetKeyDown(KeyCode.D))direction=Vector2.right;
    if(Input.GetKeyDown(KeyCode.UpArrow)||Input.GetKeyDown(KeyCode.W))direction=Vector2.up;
    if(Input.GetKeyDown(KeyCode.DownArrow)||Input.GetKeyDown(KeyCode.S))direction=Vector2.down;
    if(direction!=Vector2.zero){keyboard=true;float best=float.MaxValue;int next=Focus;direction.y=-direction.y;for(int i=0;i<7;i++){Vector2 delta=centers[i]-centers[Focus];float forward=Vector2.Dot(delta,direction);if(forward<=1)continue;float side=Mathf.Abs(delta.x*direction.y-delta.y*direction.x);float score=delta.magnitude+side*2;if(score<best){best=score;next=i;}}Focus=next;}
    if(hit>=0&&!keyboard)Focus=hit;
   }
   LiveGui.color=Color.white;LiveGui.DrawTexture(new Rect(0,0,1920,1080),table);
   float time=Time.unscaledTime;
   float flicker=1+.07f*(Mathf.PerlinNoise(time*2.6f,1.7f)*2-1)+.025f*(Mathf.PerlinNoise(time*7.1f,8.3f)*2-1);
   // The UI uses unlit textures: additive falloff supplies local candle illumination.
   CandleLight(CandleWick+new Vector2(-65,100),680,480,new Color(1,.62f,.24f,.48f*flicker));
   LiveGui.DrawTexture(new Rect(1710,70,300,260),candle,ScaleMode.ScaleToFit);
   for(int i=0;i<7;i++){
    float target=interactive&&(keyboard?i==Focus:i==hit)?1:0;
    hover[i]=Mathf.MoveTowards(hover[i],target,Time.unscaledDeltaTime/(target>hover[i]?.15f:.2f));
    float lift=Mathf.SmoothStep(0,17,hover[i]);float size=1+.018f*hover[i];
    var shadow=Corners(i,-12-lift*.3f,size);for(int j=0;j<4;j++)shadow[j].x+=8+lift*.2f;
    LiveGui.DrawTableQuad(shadow,cards[i],new Color(0,0,0,.55f));
    LiveGui.DrawTableQuad(Corners(i,lift,size),cards[i],Color.white);
    float light=Mathf.Max(hover[i],i==Selected?.5f:0);
    if(light>0)LiveGui.DrawTableQuad(Corners(i,lift,size*1.113f),glow,new Color(1,1,1,light));
   }
   LiveGui.color=new Color(.8f,.72f,.55f,.85f);LiveGui.DrawTexture(new Rect(1175,815,425,350),instrument,ScaleMode.ScaleToFit);
   LiveGui.color=Color.white;LiveGui.DrawTexture(new Rect(0,0,1920,1080),shade);
   CandleLight(CandleWick+new Vector2(0,-27),330,310,new Color(1,.6f,.18f,.62f*flicker));
   CandleLight(CandleWick+new Vector2(0,-14),96,110,new Color(1,.84f,.48f,.55f*flicker));
   // Keep the flame base attached to the wick as its height and lean change.
   var flameMatrix=LiveGui.matrix;
   LiveGuiUtility.RotateAroundPivot((Mathf.PerlinNoise(time*1.8f,3.1f)*2-1)*2.5f,CandleWick);
   float flameHeight=120*(1+(flicker-1)*.65f);
   LiveGui.color=new Color(1,1,1,.92f*flicker);
   LiveGui.DrawTexture(new Rect(CandleWick.x-32,CandleWick.y-flameHeight*.86f,62,flameHeight),flame);
   LiveGui.color=Color.white;LiveGui.matrix=flameMatrix;
   LiveGui.DrawTexture(new Rect(6,12,396,223.2f),logo,ScaleMode.ScaleToFit);
   if(!interactive||Time.unscaledTime-entered<.25f)return -1;
   if(hit>=0&&Input.GetMouseButtonUp(0)){Select(hit);return hit;}
   if(keyboard&&(Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.Space))){Select(Focus);return Focus;}
   return -1;
  }
  void OnDestroy(){if(shade!=null)Destroy(shade);if(candleSoftLight!=null)Destroy(candleSoftLight);if(candleLightMaterial!=null)Destroy(candleLightMaterial);}
 }
}
