using System;
using UnityEngine;
namespace MaritimeLegend {
// Viewport paths make the complete offscreen-to-offscreen crossing independent of camera size.
public sealed class RegionalBirdFlock:MonoBehaviour {
 public Camera targetCamera;
 public GameObject largeBird,smallBird;
 public int seed=7411;
 public float interval=6,flightDepth=10;
 public int BirdCount {get;private set;}
 public float Duration {get;private set;}
 public Vector2 StartPoint {get;private set;}
 public Vector2 EndPoint {get;private set;}
 readonly GameObject[] birds=new GameObject[3];readonly Vector2[] offsets=new Vector2[3];
 int cycle=int.MinValue;float startTime;
 void OnEnable(){startTime=Time.time;cycle=int.MinValue;}
 static float Range(System.Random rng,float min,float max){return min+(float)rng.NextDouble()*(max-min);}
 void Plan(int index){
  cycle=index;var rng=new System.Random(unchecked(seed+index*7919));BirdCount=rng.Next(2,4);Duration=Range(rng,1,2);
  float angle=Range(rng,0,Mathf.PI*2);Vector2 direction=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
  Vector2 mid=new Vector2(.5f,.5f)+new Vector2(-direction.y,direction.x)*Range(rng,-.20f,.20f);
  float reach=1.5f/Mathf.Max(Mathf.Abs(direction.x),Mathf.Abs(direction.y));
  StartPoint=mid-direction*reach;EndPoint=mid+direction*reach;
  // Clip the line to an expanded viewport. Formation offsets cannot leave a bird onscreen at either endpoint.
  float lo=float.NegativeInfinity,hi=float.PositiveInfinity;
  for(int axis=0;axis<2;axis++)if(Mathf.Abs(direction[axis])>.00001f){float a=(-.24f-mid[axis])/direction[axis],b=(1.24f-mid[axis])/direction[axis];lo=Mathf.Max(lo,Mathf.Min(a,b));hi=Mathf.Min(hi,Mathf.Max(a,b));}
  StartPoint=mid+direction*lo;EndPoint=mid+direction*hi;
  for(int i=0;i<3;i++){
   offsets[i]=new Vector2(Range(rng,-.065f,.065f),Range(rng,-.065f,.065f));
   if(birds[i]!=null){if(Application.isPlaying)Destroy(birds[i]);else DestroyImmediate(birds[i]);}
   // Every group includes both sizes of the same species.
   var prefab=i==0?largeBird:i==1?smallBird:rng.Next(2)==0?largeBird:smallBird;
   if(prefab==null)continue;birds[i]=Instantiate(prefab,transform);birds[i].name="Flock bird "+i;
   foreach(var renderer in birds[i].GetComponentsInChildren<Renderer>()){var block=new MaterialPropertyBlock();block.SetFloat("_Phase",Range(rng,0,6.28f));renderer.SetPropertyBlock(block);}
  }
 }
 public void Sample(float time,Camera view=null){
  view=view!=null?view:targetCamera!=null?targetCamera:Camera.main;if(view==null||largeBird==null||smallBird==null)return;
  float gap=Mathf.Max(interval,2.5f);int index=Mathf.FloorToInt(Mathf.Max(0,time)/gap);if(index!=cycle)Plan(index);
  float elapsed=Mathf.Max(0,time)-index*gap;bool visible=elapsed<=Duration;
  Vector2 p=Vector2.LerpUnclamped(StartPoint,EndPoint,elapsed/Duration);Vector2 d=EndPoint-StartPoint;
  // Use screen aspect in the angle so the bird's head follows its actual image-space path.
  float angle=Mathf.Atan2(d.y,d.x*view.aspect)*Mathf.Rad2Deg;
  float depth=Mathf.Max(flightDepth,view.nearClipPlane+1);
  float screenHeight=view.orthographic?view.orthographicSize*2:2*depth*Mathf.Tan(view.fieldOfView*Mathf.Deg2Rad*.5f);
  for(int i=0;i<3;i++)if(birds[i]!=null){birds[i].SetActive(visible&&i<BirdCount);var uv=p+offsets[i];birds[i].transform.position=view.ViewportToWorldPoint(new Vector3(uv.x,uv.y,depth));birds[i].transform.rotation=view.transform.rotation*Quaternion.Euler(0,0,angle);birds[i].transform.localScale=Vector3.one*(screenHeight/24);}
 }
 void Update(){Sample(Time.time-startTime);}
}
}
