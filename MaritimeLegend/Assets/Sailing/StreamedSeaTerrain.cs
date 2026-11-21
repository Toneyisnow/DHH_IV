using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
[Serializable] public sealed class StreamedSeaAtlas {
 public int version;public string worldId;public bool[] ocean;public WorldPort[] ports;
 public void Validate(){if(version!=2||worldId!="regional_world_v2"||ocean==null||ocean.Length!=2304||ports==null||ports.Length==0)throw new InvalidDataException("Unsupported regional world atlas");}
}
// No Unity object is created off-thread. Resources IO -> worker arrays -> bounded main-thread upload.
public sealed class StreamedSeaTerrain:IDisposable,IWorldTerrain {
 public const int Cells=128,Side=131,Capacity=196;
 public const float Step=1.25f;
 public sealed class Tile {
  public int key;public float touched,retry;public ResourceRequest request;public Task<Payload> worker;
  public Payload data;public Mesh mesh;public Texture2D depth;public string error;
  public float displayX=float.NaN;public ManualSeaV2Instances.Cell[] displayProps;
  public bool Ready=>data!=null&&depth;
 }
 public sealed class Payload {
  public float[] h,c,f;public Vector3[] vertices,normals;public Color[] colors;public int[] triangles;
  public ManualSeaV2Instances.Cell[] props;
  public float Sample(float[] values,float x,float y){float px=Mathf.Clamp(x/Step+1,0,Side-1.001f),py=Mathf.Clamp(y/Step+1,0,Side-1.001f);int ix=(int)px,iy=(int)py;return Mathf.Lerp(Mathf.Lerp(values[iy*Side+ix],values[iy*Side+ix+1],px-ix),Mathf.Lerp(values[(iy+1)*Side+ix],values[(iy+1)*Side+ix+1],px-ix),py-iy);}
 }
 readonly Dictionary<int,Tile> tiles=new Dictionary<int,Tile>();readonly StreamedSeaAtlas atlas;
 public int Count=>tiles.Count;public int Pending=>tiles.Values.Count(t=>!t.Ready&&t.error==null);public int Failures=>tiles.Values.Count(t=>t.error!=null);
 public int Loads,Evictions;bool disposed;
 public Func<int,string> ResourceKey;
 public long Bytes=>tiles.Values.Sum(t=>t.data==null?800000L:t.data.h.Length*16L+t.mesh.vertexCount*40L+(long)t.mesh.GetIndexCount(0)*4L+t.data.props.Sum(c=>c.transforms.Length*64L));
 public StreamedSeaTerrain(StreamedSeaAtlas atlas){this.atlas=atlas;atlas.Validate();}
 public Tile Find(int key)=>tiles.TryGetValue(key,out var t)?t:null;
 public bool Ready(int key)=>atlas.ocean[key]||(Find(key)?.Ready??false);
 public bool TryHeight(double x,double y,out float value){int key=WorldMap.Key(x,y);if(atlas.ocean[key]){value=-32;return true;}var t=Find(key);if(t?.data==null){value=-32;return false;}value=t.data.Sample(t.data.h,(float)(WorldMap.Wrap(x)-key%72*160),(float)(WorldMap.ClampY(y)-key/72*160));return true;}
 public bool TryDistance(double x,double y,out double value){int key=WorldMap.Key(x,y);if(atlas.ocean[key]){value=32;return true;}var t=Find(key);if(t?.data==null){value=0;return false;}value=-t.data.Sample(t.data.c,(float)(WorldMap.Wrap(x)-key%72*160),(float)(WorldMap.ClampY(y)-key/72*160));return true;}
 public void Update(List<int> priority,HashSet<int> retained,float now){
  if(disposed)return;
  foreach(var t in tiles.Values.ToArray()){
   if(retained.Contains(t.key))t.touched=now;
   if(!retained.Contains(t.key)&&now-t.touched>2&&t.request==null&&t.worker==null){Release(t);tiles.Remove(t.key);Evictions++;}
  }
  var clock=System.Diagnostics.Stopwatch.StartNew();int uploads=0;
  foreach(var t in tiles.Values.ToArray()){
   if(t.request!=null&&t.request.isDone){var asset=t.request.asset as TextAsset;t.request=null;if(!asset){t.error="Missing tile "+t.key;t.retry=now+3;continue;}
    var bytes=asset.bytes;Resources.UnloadAsset(asset);int key=t.key;t.worker=Task.Run(()=>Build(bytes,key));}
   if(t.worker==null||!t.worker.IsCompleted||uploads>=1||clock.Elapsed.TotalMilliseconds>3)continue;
   var task=t.worker;t.worker=null;
   if(task.IsFaulted){t.error=task.Exception.GetBaseException().Message;t.retry=now+3;continue;}
   if(!retained.Contains(t.key)){tiles.Remove(t.key);Evictions++;continue;}
   try{var p=task.Result;t.mesh=new Mesh{name="Streamed SEA terrain "+t.key,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};t.mesh.vertices=p.vertices;t.mesh.normals=p.normals;t.mesh.colors=p.colors;t.mesh.triangles=p.triangles;t.mesh.RecalculateBounds();
    t.depth=new Texture2D(Side,Side,TextureFormat.RFloat,false,true){wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};var d=new float[p.h.Length];for(int i=0;i<d.Length;i++)d[i]=(p.h[i]+32)/64;t.depth.SetPixelData(d,0);t.depth.Apply(false,true);t.mesh.UploadMeshData(true);p.vertices=null;p.normals=null;p.colors=null;p.triangles=null;t.data=p;uploads++;Loads++;
   }catch(Exception ex){Release(t);t.error=ex.Message;t.retry=now+3;}
  }
  int active=tiles.Values.Count(t=>t.request!=null||t.worker!=null);
  foreach(int key in priority){if(atlas.ocean[key])continue;var t=Find(key);if(t!=null&&(t.Ready||t.request!=null||t.worker!=null||now<t.retry))continue;if(active>=4)break;
   if(t==null){if(tiles.Count>=Capacity)continue;t=new Tile{key=key,touched=now};tiles.Add(key,t);}t.error=null;t.request=Resources.LoadAsync<TextAsset>(ResourceKey?.Invoke(key)??("Data/StreamedSeasV2/Chunks/"+(key%72).ToString("00")+"_"+(key/72).ToString("00")));active++;
  }
 }
 static Payload Build(byte[] bytes,int key){
  var p=new Payload();using(var r=new BinaryReader(new MemoryStream(bytes))){if(r.ReadInt32()!=0x53563232||r.ReadInt32()!=key%72||r.ReadInt32()!=key/72||r.ReadInt32()!=Side||bytes.Length!=16+Side*Side*12)throw new InvalidDataException("Invalid streamed terrain tile "+key);
   p.h=new float[Side*Side];p.c=new float[Side*Side];p.f=new float[Side*Side];foreach(var a in new[]{p.h,p.c,p.f})for(int i=0;i<a.Length;i++){a[i]=r.ReadSingle();if(float.IsNaN(a[i])||float.IsInfinity(a[i]))throw new InvalidDataException("Non-finite terrain");}}
  var v=new List<Vector3>();var n=new List<Vector3>();var colors=new List<Color>();var indices=new List<int>();var lookup=new Dictionary<int,int>();
  Func<int,int,int> vertex=(ix,iy)=>{int k=iy*Side+ix;if(lookup.TryGetValue(k,out int result))return result;result=v.Count;lookup[k]=result;float x=(ix-1)*Step,z=(iy-1)*Step,y=p.h[k];
   float dx=(p.h[k+1]-p.h[k-1])/(2*Step),dz=(p.h[k+Side]-p.h[k-Side])/(2*Step),slope=Mathf.Sqrt(dx*dx+dz*dz),inland=p.c[k];
   Color grass=Color.Lerp(new Color(.57f,.59f,.25f),new Color(.26f,.39f,.15f),p.f[k]);Color color=Color.Lerp(new Color(.86f,.74f,.47f),grass,Mathf.SmoothStep(0,1,(inland-.6f)/2.4f));
   float rock=Mathf.Max(Mathf.SmoothStep(0,1,(slope-.3f)/.9f),Mathf.SmoothStep(0,1,(y-4)/6));color=Color.Lerp(color,new Color(.68f,.59f,.37f),rock);
   color=Color.Lerp(color,new Color(.48f,.36f,.18f),Mathf.SmoothStep(0,1,(y-15)/9)*Mathf.SmoothStep(0,1,(inland-18)/18));
   color*=Mathf.Lerp(.94f,1.06f,Mathf.PerlinNoise((float)WorldMap.Wrap(key%72*160+x)*.9f+400,(key/72*160+z)*.9f+400));
   v.Add(new Vector3(x,y,z));n.Add(new Vector3(-dx,1,-dz).normalized);colors.Add(color);return result;};
  for(int iy=1;iy<=Cells;iy++)for(int ix=1;ix<=Cells;ix++){int k=iy*Side+ix;if(Mathf.Max(Mathf.Max(p.h[k],p.h[k+1]),Mathf.Max(p.h[k+Side],p.h[k+Side+1]))<-.8f)continue;int a=vertex(ix,iy),b=vertex(ix+1,iy),c=vertex(ix,iy+1),d=vertex(ix+1,iy+1);indices.AddRange(new[]{a,c,b,b,c,d});}
  p.vertices=v.ToArray();p.normals=n.ToArray();p.colors=colors.ToArray();p.triangles=indices.ToArray();
  var buckets=new Dictionary<Vector3Int,List<Matrix4x4>>();var random=new System.Random(261028+key);
  for(float z=1.4f;z<160;z+=2.8f)for(float x=1.4f;x<160;x+=2.8f){float px=x+(float)random.NextDouble()*1.8f,pz=z+(float)random.NextDouble()*1.8f;if(px>=160||pz>=160)continue;float inside=p.Sample(p.c,px,pz),y=p.Sample(p.h,px,pz),tree=p.Sample(p.f,px,pz);if(inside<1||y>12)continue;bool rock=inside<3.5f;if(random.NextDouble()>(rock?.27:.15+tree*.75))continue;
   int variant=rock?2:random.Next(2);float size=rock?.35f+(float)random.NextDouble()*.6f:.60f+(float)random.NextDouble()*.65f;var cell=new Vector3Int((int)(px/40),(int)(pz/40),variant);if(!buckets.ContainsKey(cell))buckets[cell]=new List<Matrix4x4>();buckets[cell].Add(Matrix4x4.TRS(new Vector3(px,y-.05f,pz),Quaternion.Euler(0,(float)random.NextDouble()*360,0),Vector3.one*size));}
  p.props=buckets.Select(b=>new ManualSeaV2Instances.Cell{variant=b.Key.z,bounds=new Bounds(new Vector3(b.Key.x*40+20,12,b.Key.y*40+20),new Vector3(46,32,46)),transforms=b.Value.ToArray()}).ToArray();return p;
 }
 static void Release(Tile t){if(t.mesh)UnityEngine.Object.Destroy(t.mesh);if(t.depth)UnityEngine.Object.Destroy(t.depth);t.mesh=null;t.depth=null;t.data=null;}
 public void Dispose(){if(disposed)return;disposed=true;foreach(var t in tiles.Values){if(t.request!=null){var request=t.request;if(request.isDone){if(request.asset)Resources.UnloadAsset(request.asset);}else request.completed+=_=>{if(request.asset)Resources.UnloadAsset(request.asset);};}if(t.worker!=null)t.worker.ContinueWith(task=>{if(task.IsFaulted){var observed=task.Exception;}},TaskScheduler.Default);Release(t);}tiles.Clear();}
}
}
