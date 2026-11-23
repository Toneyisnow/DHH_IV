using System;
using System.Collections.Generic;
using UnityEngine;
using MaritimeLegend.Domain;
using GUI=MaritimeLegend.LiveGui;
namespace MaritimeLegend {
// Small baked terrain index; never loads the world's full-resolution streaming tiles.
internal sealed class SailingWorldOverview : IDisposable {
 const int Width=1152,Height=512;
 // Fields below are populated by Unity JsonUtility.
#pragma warning disable 0649
 [Serializable] sealed class Region {public string id;public double[] bounds;}
 [Serializable] sealed class Atlas {public WorldPort[] ports;public Region[] regions;}
#pragma warning restore 0649
 readonly Region[] regions;
 readonly byte[] terrain;
 readonly Color32[] pixels=new Color32[Width*Height];
 readonly Dictionary<long,int> explored=new Dictionary<long,int>();
 readonly Color32[] palette={new Color32(36,100,135,255),new Color32(137,164,89,255),new Color32(92,124,66,255),new Color32(145,122,89,255)};
 readonly double west,south,east,north;
 readonly Texture2D image;
 GameState owner;float nextRefresh;bool lastWorld;Rect lastBounds;
 public WorldPort[] Ports {get;}
 public SailingWorldOverview(){
  var asset=Resources.Load<TextAsset>("Data/StreamedSeasV2/overview");
  if(!asset)throw new InvalidOperationException("Missing sailing overview; run build_overview.py");
  terrain=asset.bytes;Resources.UnloadAsset(asset);
  if(terrain.Length!=Width*Height)throw new InvalidOperationException("Invalid sailing overview size");
  asset=Resources.Load<TextAsset>("Data/StreamedSeasV2/atlas");var atlas=JsonUtility.FromJson<Atlas>(asset.text);Resources.UnloadAsset(asset);Ports=atlas.ports;regions=atlas.regions;
  var bounds=Array.Find(atlas.regions,r=>r.id=="southeast_asia").bounds;
  west=(bounds[0]+180)*32;south=(bounds[1]+80)*32;east=(bounds[2]+180)*32;north=(bounds[3]+80)*32;
  image=new Texture2D(Width,Height,TextureFormat.RGBA32,false){name="World sailing overview",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
 }
 static long Key(int q,int r)=>((long)q<<32)|(uint)r;
 public Vector2 Position(GameState state,bool world)=>world?new Vector2((float)WorldMap.Wrap(state.seaX),(float)state.seaY):new Vector2((float)(west+state.seaX*(east-west)),(float)(north-state.seaY*(north-south)));
 public static readonly Rect WorldBounds=new Rect(0,0,(float)WorldMap.Width,(float)WorldMap.Height);
 static Rect Bounds(Region region){var b=region.bounds;return Rect.MinMaxRect((float)((b[0]+180)*32),(float)((b[1]+80)*32),(float)((b[2]+180)*32),(float)((b[3]+80)*32));}
 public Rect RegionalBounds(GameState state,bool world,out string regionId){
  var position=Position(state,world);
  var region=Array.Find(regions,r=>r.id==(world?state.sailing.regionId:"southeast_asia"));
  // Atlas regions overlap. Prefer the current gameplay region when it contains the ship.
  if(region==null||!Bounds(region).Contains(position)){
   Region containing=null;float area=float.MaxValue;
   foreach(var candidate in regions){var b=Bounds(candidate);if(b.Contains(position)&&b.width*b.height<area){containing=candidate;area=b.width*b.height;}}
   if(containing!=null)region=containing;
  }
  if(region==null)region=Array.Find(regions,r=>r.id==WorldMap.Region(position.x,position.y));
  regionId=region.id;var bounds=Bounds(region);
  // The authored rectangles do not cover every open-ocean location. Keep the main ship visible.
  if(!bounds.Contains(position))bounds=Rect.MinMaxRect(Mathf.Max(0,Mathf.Min(bounds.xMin,position.x-32)),Mathf.Max(0,Mathf.Min(bounds.yMin,position.y-32)),Mathf.Min((float)WorldMap.Width,Mathf.Max(bounds.xMax,position.x+32)),Mathf.Min((float)WorldMap.Height,Mathf.Max(bounds.yMax,position.y+32)));
  return bounds;
 }
 public Texture2D Image(GameState state,bool world,Rect bounds){
  if(owner==state&&lastWorld==world&&lastBounds==bounds&&Time.unscaledTime<nextRefresh)return image;
  owner=state;lastWorld=world;lastBounds=bounds;nextRefresh=Time.unscaledTime+.5f;
  explored.Clear();
  if(world&&state.sailing.streamedExploration!=null)foreach(var h in state.sailing.streamedExploration)explored[Key(h.x,h.y)]=h.value;
  for(int y=0;y<Height;y++)for(int x=0;x<Width;x++){
   double wx=bounds.xMin+(x+.5)*bounds.width/Width,wy=bounds.yMin+(y+.5)*bounds.height/Height;SeaExploration.Hex(wx,wy,out int q,out int r);
   // Match StreamedSeaRuntime.FogKey, including the wrapped east/west seam.
   SeaExploration.Hex(WorldMap.Wrap(SeaExploration.TileSpacing*(q+r*.5)),18*r,out q,out r);
   int value=explored.TryGetValue(Key(q,r),out int v)?v:0;
   if(!world&&wx>=west&&wx<=east&&wy>=south&&wy<=north&&state.sailing.southeastAsiaExploration!=null){
    double u=(wx-west)/(east-west),vv=(north-wy)/(north-south);
    SeaExploration.Hex((u-.5)*960,(.5-vv)*540,out q,out r);
    value=state.sailing.southeastAsiaExploration.At(q,r);
   }
   int i=y*Width+x;int terrainX=Mathf.Clamp((int)(wx/10),0,Width-1),terrainY=Mathf.Clamp((int)(wy/10),0,Height-1);var color=palette[terrain[terrainY*Width+terrainX]];
   float visible=1-SeaExploration.FogOpacity(value);
   pixels[i]=new Color32((byte)(color.r*visible),(byte)(color.g*visible),(byte)(color.b*visible),255);
  }
  image.SetPixels32(pixels);image.Apply(false);return image;
 }
 public void Dispose(){UnityEngine.Object.Destroy(image);}
}
public partial class MaritimeGame {
 enum SailingOverviewMode { Closed, Regional, World }
 SailingOverviewMode sailingOverviewMode;
 bool sailingOverviewOpen=>sailingOverviewMode!=SailingOverviewMode.Closed;
 SailingWorldOverview sailingOverview;
 void SailingOverviewUI(){
  if(!sailingOverviewOpen)return;
  if(sailingOverview==null)sailingOverview=new SailingWorldOverview();
  // Reference viewport is 1920x1080; the centred panel is exactly half on each axis.
  Fill(new Rect(480,270,960,540),new Color(.025f,.06f,.085f,.98f));
  bool regional=sailingOverviewMode==SailingOverviewMode.Regional;
  string regionId=null;
  var bounds=regional?sailingOverview.RegionalBounds(s,IsWorldSea,out regionId):SailingWorldOverview.WorldBounds;
  Label(500,278,850,38,regional?"区域海域地图 · "+SeaText(c.SeaRegion(regionId).nameKey):"世界缩略图",text);
  if(Btn(1386,278,42,36,"×")){sailingOverviewMode=SailingOverviewMode.Closed;return;}
  var map=new Rect(492,322,936,416);
  GUI.DrawTexture(map,sailingOverview.Image(s,IsWorldSea,bounds));
  foreach(var port in sailingOverview.Ports)if((s.discovered.Contains(port.id)||s.visited.Contains(port.id))&&bounds.Contains(new Vector2((float)port.x,(float)port.y)))OverviewDot(OverviewPoint(map,bounds,port.x,port.y),4,new Color(1,.88f,.52f));
  var ship=sailingOverview.Position(s,IsWorldSea);OverviewStar(OverviewPoint(map,bounds,ship.x,ship.y),10);
  OverviewDot(new Vector2(510,772),4,new Color(1,.88f,.52f));Label(524,756,170,32,"已发现城市",small);
  OverviewStar(new Vector2(716,772),9);Label(733,756,125,32,"主船",small);
  Fill(new Rect(860,765,16,16),Color.black);Label(885,756,210,32,"未探索区域",small);
  Label(1115,756,310,32,regional?"Tab 世界地图 / × 关闭":"Tab / × 关闭",small);
 }
 static Vector2 OverviewPoint(Rect map,Rect bounds,double x,double y)=>new Vector2(map.x+(float)((WorldMap.Wrap(x)-bounds.xMin)/bounds.width)*map.width,map.yMax-(float)((WorldMap.ClampY(y)-bounds.yMin)/bounds.height)*map.height);
 void OverviewDot(Vector2 p,float radius,Color color){for(int y=-(int)radius;y<=radius;y++){float half=Mathf.Sqrt(radius*radius-y*y);Fill(new Rect(p.x-half,p.y+y,Mathf.Max(1,half*2),1),color);}}
 void OverviewStar(Vector2 p,float radius){
  for(int i=0;i<10;i++){float a=-Mathf.PI/2+i*Mathf.PI/5,b=a+Mathf.PI/5;float ra=i%2==0?radius:radius*.42f,rb=i%2==0?radius*.42f:radius;
   var from=p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*ra;var to=p+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*rb;
   Line(from,to,Color.black,5);Line(from,to,new Color(1,.92f,.28f),2);
  }
 }
}}
