using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
public sealed class WorldChunkRepository : IWorldTerrain,IDisposable {
 public sealed class Entry {
  public int key;public float wanted;public ResourceRequest request;
  public Task<Payload> worker;public WorldChunkData data;public Mesh near,far;public Texture2D distance;
  public string error;public float retry;public bool ready;
 }
 public sealed class Payload {public WorldChunkData data;public WorldChunkGeometry near,far;}
 readonly Dictionary<int,Entry> entries=new Dictionary<int,Entry>();
 readonly WorldMapManifest map;bool disposed;
 public const int Capacity=180,ConcurrentLoads=4;
 public int Count=>entries.Count;
 public int Pending=>entries.Values.Count(e=>!e.ready&&e.error==null);
 public int Failures=>entries.Values.Count(e=>e.error!=null);
 public long EstimatedBytes=>entries.Values.Sum(e=>e.ready?(e.near?e.near.vertexCount*44L+e.far.vertexCount*44L:0)+WorldChunkData.Side*WorldChunkData.Side*8L:400000L);
 public WorldChunkRepository(WorldMapManifest map){this.map=map;}
 public Entry Find(int key)=>entries.TryGetValue(key,out var e)?e:null;
 public bool TryDistance(double x,double y,out double d){
  int key=WorldMap.Key(x,y);if(map.chunks[key].ocean){d=80;return true;}
  var e=Find(key);if(e!=null&&e.data!=null){d=e.data.Distance(x,y);return true;}d=0;return false;
 }
 public bool Ready(int key)=>map.chunks[key].ocean||(Find(key)?.ready??false);
 public void Update(IList<int> priority,HashSet<int> retained,float now){
  if(disposed)return;
  foreach(int key in retained){var e=Find(key);if(e!=null)e.wanted=now;}
  foreach(var e in entries.Values.ToArray())if(!retained.Contains(e.key)&&now-e.wanted>5&&e.request==null&&e.worker==null){Release(e);entries.Remove(e.key);}
  var clock=System.Diagnostics.Stopwatch.StartNew();int uploaded=0;
  foreach(var e in entries.Values.ToArray()){
   if(e.request!=null&&e.request.isDone){
    var asset=e.request.asset as TextAsset;e.request=null;
    if(!asset){e.error="Missing world chunk "+e.key;e.retry=now+5;continue;}
    byte[] bytes=asset.bytes;Resources.UnloadAsset(asset);int key=e.key;
    e.worker=Task.Run(()=>{var data=WorldChunkData.Parse(bytes,key%WorldMap.Columns,key/WorldMap.Columns);return new Payload{data=data,near=WorldChunkGeometry.Build(data,1),far=WorldChunkGeometry.Build(data,2)};});
   }
   if(e.worker==null||!e.worker.IsCompleted||uploaded>=2||clock.Elapsed.TotalMilliseconds>4)continue;
   if(e.worker.IsFaulted){e.error=e.worker.Exception.GetBaseException().Message;e.retry=now+5;e.worker=null;continue;}
   var result=e.worker.Result;e.worker=null;e.data=result.data;
   try{
    e.near=result.near.Upload("World near "+e.key);e.far=result.far.Upload("World far "+e.key);
    e.distance=new Texture2D(WorldChunkData.Side,WorldChunkData.Side,TextureFormat.RFloat,false,true){name="World coast "+e.key,wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};
    e.distance.SetPixelData(e.data.Samples,0);e.distance.Apply(false,true);e.ready=true;uploaded++;
   }catch(Exception ex){Release(e);e.error=ex.Message;e.retry=now+5;}
  }
  int loading=entries.Values.Count(e=>e.request!=null||e.worker!=null);
  foreach(int key in priority){
   if(map.chunks[key].ocean)continue;
   var e=Find(key);if(e!=null&&(e.ready||e.request!=null||e.worker!=null||now<e.retry))continue;
   if(loading>=ConcurrentLoads)break;
   if(e==null){if(entries.Count>=Capacity)continue;e=new Entry{key=key,wanted=now};entries.Add(key,e);}
   e.error=null;e.request=Resources.LoadAsync<TextAsset>(map.chunks[key].key);loading++;
  }
 }
 static void Release(Entry e){foreach(var obj in new UnityEngine.Object[]{e.near,e.far,e.distance})if(obj)WorldSeaAssets.Destroy(obj);e.near=e.far=null;e.distance=null;e.data=null;e.ready=false;}
 public void Dispose(){if(disposed)return;disposed=true;foreach(var e in entries.Values){
   if(e.request!=null){var request=e.request;if(request.isDone){if(request.asset)Resources.UnloadAsset(request.asset);}else request.completed+=_=>{if(request.asset)Resources.UnloadAsset(request.asset);};}
   if(e.worker!=null)e.worker.ContinueWith(t=>{if(t.IsFaulted){var observed=t.Exception;}},TaskScheduler.Default);
   Release(e);
  }entries.Clear();}
}
}
