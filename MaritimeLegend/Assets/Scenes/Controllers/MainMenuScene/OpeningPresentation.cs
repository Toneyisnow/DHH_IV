using UnityEngine;
using GUI=MaritimeLegend.LiveGui;
namespace MaritimeLegend {
 // Scene-owned visual resources: released when the menu is left, recreated on return.
 public sealed class OpeningPresentation : MonoBehaviour {
  public float Elapsed => previewTime>=0?previewTime:Time.unscaledTime-started;
  public float previewTime=-1;
  public Texture2D Logo {get;private set;}
  public Font Serif {get;private set;}
  public RenderTexture Background {get;private set;}
  Texture2D worldMap,sparkle,ribbon,soft;
  Material globeMaterial,additive;
  float started;
  public const float PullbackSeconds=3.4f;
  public const float SweepSeconds=2.8f;
  public const float SweepPeriod=11.8f;
  void Awake(){
   started=Time.unscaledTime;
   worldMap=Resources.Load<Texture2D>("Opening/02_world_map_texture");
   Logo=Resources.Load<Texture2D>("Opening/03_title_maritime_legend");
   sparkle=Resources.Load<Texture2D>("Opening/07_sparkle");
   ribbon=Resources.Load<Texture2D>("Opening/06_light_ribbon");
   Serif=Font.CreateDynamicFontFromOSFont(new[]{"Georgia","Times New Roman"},36);
   globeMaterial=new Material(Resources.Load<Shader>("Opening/OpeningGlobe"));
   additive=new Material(Resources.Load<Shader>("Opening/OpeningAdditive"));
   Background=new RenderTexture(1920,1080,0,RenderTextureFormat.ARGB32){name="Opening spherical world",filterMode=FilterMode.Bilinear};
   Background.Create();
   soft=new Texture2D(64,64,TextureFormat.RGBA32,false){name="Opening soft light",wrapMode=TextureWrapMode.Clamp};
   var pixels=new Color[4096];
   for(int y=0;y<64;y++)for(int x=0;x<64;x++){
    float r=new Vector2((x-31.5f)/31.5f,(y-31.5f)/31.5f).magnitude;
    pixels[y*64+x]=new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-r),2.6f));
   }
   soft.SetPixels(pixels);soft.Apply(false,true);
  }
  public void DrawBackground(){
   float t=Mathf.Clamp01(Elapsed/PullbackSeconds);
   float ease=1-Mathf.Pow(1-t,3);
   globeMaterial.SetFloat("_Radius",Mathf.Lerp(1.12f,.9f,ease));
   globeMaterial.SetFloat("_Longitude",.055f+Elapsed/360f);
   Graphics.Blit(worldMap,Background,globeMaterial);
   GUI.DrawTexture(new Rect(0,0,1920,1080),Background);
  }
  public float LogoScale => Mathf.Lerp(1.17f,1,1-Mathf.Pow(1-Mathf.Clamp01(Elapsed/PullbackSeconds),3));
  static Vector2 Bezier(Vector2 a,Vector2 b,Vector2 c,Vector2 d,float t){float s=1-t;return s*s*s*a+3*s*s*t*b+3*s*t*t*c+t*t*t*d;}
  public static Vector2 SweepPoint(float t){
   // Preserve the left arc; match its time-scaled tangent at the join and glide offscreen.
   if(t<.62f)return Bezier(new Vector2(-130,430),new Vector2(560,860),new Vector2(800,280),new Vector2(1410,400),t/.62f);
   Vector2 join=new Vector2(1410,400);
   Vector2 tangent=(join-new Vector2(800,280))*(.38f/.62f);
   return Bezier(join,join+tangent,new Vector2(2100,510),new Vector2(2380,500),(t-.62f)/.38f);
  }
  void Light(Vector2 p,float w,float h,Texture texture,Color tint){GUI.color=tint;GUI.DrawTexture(new Rect(p.x-w/2,p.y-h/2,w,h),texture,ScaleMode.StretchToFill,additive);GUI.color=Color.white;}
  void Segment(Vector2 a,Vector2 b,float width,Color tint){
   var matrix=GUI.matrix;Vector2 delta=b-a;
   LiveGuiUtility.RotateAroundPivot(Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg,a);
   GUI.color=tint;GUI.DrawTexture(new Rect(a.x-width/2,a.y-width/2,delta.magnitude+width,width),soft,ScaleMode.StretchToFill,additive);
   GUI.color=Color.white;GUI.matrix=matrix;
  }
  public void DrawEffects(float strength){
   float time=Elapsed;
   // Sparse background motes, deterministic and independent of game RNG.
   for(int i=0;i<22;i++){
    float seed=i*2.399963f;
    float x=Mathf.Repeat(i*137.2f+Mathf.Sin(time*.14f+seed)*30,1920);
    float y=Mathf.Repeat(i*83.7f-time*(4+i%5),1080);
    float alpha=(.08f+.12f*Mathf.Pow(Mathf.Sin(time*.8f+seed),2))*strength;
    Light(new Vector2(x,y),i%4==0?19:8,i%4==0?19:8,i%4==0?sparkle:soft,new Color(1,.78f,.39f,alpha));
   }
   float sweep=Mathf.Repeat(time,SweepPeriod);
   if(sweep>SweepSeconds+1.35f)return;
   float head=sweep/SweepSeconds;
   // Finite luminous tail, not a translated full-screen ribbon.
   for(int j=0;j<64;j++){
    float age=j/63f*.38f;
    float u=head-age;
    if(u<0||u>1)continue;
    float a=(1-age/.38f)*strength*Mathf.Clamp01((SweepSeconds+1-sweep)*2);
    Vector2 p=SweepPoint(u),q=SweepPoint(Mathf.Max(0,u-.007f));
    Segment(q,p,88,new Color(1,.74f,.34f,a*.62f));
    Segment(q,p,18,new Color(1,.98f,.84f,a));
   }
   if(head<=1){
    var p=SweepPoint(head);
    Light(p,235,235,soft,new Color(1,.85f,.55f,strength));
    Light(p,105,105,sparkle,new Color(1,.97f,.82f,strength));
    // Reuse the supplied painted ribbon as a restrained halo at the moving head.
    Light(p,230,90,ribbon,new Color(1,.86f,.55f,.2f*strength));
   }
   for(int i=0;i<86;i++){
    float born=i/85f*SweepSeconds;
    float age=sweep-born;
    if(age<0||age>1.3f)continue;
    float seed=i*2.399963f;
    Vector2 p=SweepPoint(born/SweepSeconds)+new Vector2(Mathf.Sin(seed)*160*Mathf.Sqrt(age),Mathf.Cos(seed)*110*Mathf.Sqrt(age)+30*age*age);
    float size=i%5==0?48:20;
    Light(p,size,size,i%5==0?sparkle:soft,new Color(1,.9f,.6f,Mathf.Pow(1-age/1.3f,1.6f)*strength));
   }
  }
  void OnDestroy(){
   if(Background!=null){Background.Release();Destroy(Background);}
   if(globeMaterial!=null)Destroy(globeMaterial);
   if(additive!=null)Destroy(additive);
   if(soft!=null)Destroy(soft);
   if(Serif!=null)Destroy(Serif);
  }
 }
}
