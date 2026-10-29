using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
// Random seeded positions; shared mesh and a baked, matching shadow mask drift together.
public sealed class ManualSeaV2CloudLayer : MonoBehaviour {
 public Camera view;public Material material;
 public int CloudCount=>centers.Count;
 Mesh mesh;readonly List<Vector3> centers=new List<Vector3>();readonly List<float> sizes=new List<float>();
 readonly Matrix4x4[] visible=new Matrix4x4[1023];readonly Plane[] planes=new Plane[6];
 const float FieldWidth=1458,FieldHeight=1026;
 const int MaskWidth=2048,MaskHeight=1536,Population=438;
 Texture2D shadowMask;bool shadowDirty=true;Vector3 shadowLight;float shadowHeight;int seed;SailingState weather;
 public Vector3 Drift=>weather==null?Vector3.zero:new Vector3((float)weather.cloudOffsetX,0,(float)weather.cloudOffsetZ);
 public void BindWeather(SailingState state){
  if(state.cloudSeed==0)state.cloudSeed=Random.Range(1,int.MaxValue);
  weather=state;if(seed==state.cloudSeed&&centers.Count>0)return;seed=state.cloudSeed;
  centers.Clear();sizes.Clear();var random=new System.Random(seed);
  for(int i=0;i<Population;i++){
   Vector3 position=Vector3.zero;
   for(int attempt=0;attempt<80;attempt++){
    position=new Vector3(((float)random.NextDouble()-.5f)*FieldWidth,0,((float)random.NextDouble()-.5f)*FieldHeight);
    bool clear=true;foreach(var other in centers){float dx=Mathf.Repeat(position.x-other.x+FieldWidth*.5f,FieldWidth)-FieldWidth*.5f,dz=Mathf.Repeat(position.z-other.z+FieldHeight*.5f,FieldHeight)-FieldHeight*.5f;if(dx*dx+dz*dz<18*18){clear=false;break;}}
    if(clear)break;
   }
   centers.Add(position);sizes.Add(.9f+.2f*(float)random.NextDouble());
  }
  shadowDirty=true;
 }
 // Project the actual shared mesh onto sea level. Elevated receivers shift this
 // footprint along the same light ray in the shader. Rebuild only on weather/height changes.
 void BakeShadowMask(Vector3 light){
  shadowDirty=false;shadowLight=light;shadowHeight=transform.position.y;
  if(!shadowMask)shadowMask=new Texture2D(MaskWidth,MaskHeight,TextureFormat.RGBA32,false,true){name="Cloud mesh silhouettes",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Bilinear};
  var pixels=new Color32[MaskWidth*MaskHeight];var vertices=mesh.vertices;var triangles=mesh.triangles;
  var projected=new Vector2[vertices.Length];var slope=new Vector2(light.x,light.z)/Mathf.Max(.08f,light.y);
  for(int i=0;i<centers.Count;i++){
   for(int j=0;j<vertices.Length;j++){
    var v=transform.position+centers[i]+vertices[j]*sizes[i];
    var point=new Vector2(v.x,v.z)-slope*v.y;
    projected[j]=new Vector2((point.x/FieldWidth+.5f)*MaskWidth,(point.y/FieldHeight+.5f)*MaskHeight);
   }
   for(int j=0;j<triangles.Length;j+=3){
    var a=projected[triangles[j]];var b=projected[triangles[j+1]];var c=projected[triangles[j+2]];
    float area=Cross(b-a,c-a);if(Mathf.Abs(area)<.0001f)continue;
    int x0=Mathf.FloorToInt(Mathf.Min(a.x,Mathf.Min(b.x,c.x))),x1=Mathf.CeilToInt(Mathf.Max(a.x,Mathf.Max(b.x,c.x)));
    int y0=Mathf.FloorToInt(Mathf.Min(a.y,Mathf.Min(b.y,c.y))),y1=Mathf.CeilToInt(Mathf.Max(a.y,Mathf.Max(b.y,c.y)));
    for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++){
     var point=new Vector2(x+.5f,y+.5f);
     if(Cross(b-a,point-a)*area<0||Cross(c-b,point-b)*area<0||Cross(a-c,point-c)*area<0)continue;
     int index=((y%MaskHeight+MaskHeight)%MaskHeight)*MaskWidth+(x%MaskWidth+MaskWidth)%MaskWidth;
     pixels[index]=new Color32(255,255,255,255);
    }
   }
  }
  shadowMask.SetPixels32(pixels);shadowMask.Apply(false);
 }
 static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
 static double Wrap(double value,double size)=>value-System.Math.Floor((value+size*.5)/size)*size;
 public void AdvanceWeather(SailingState state,double wind,double seconds,double minimumShipSpeed){
  BindWeather(state);if(seconds<=0)return;
  // Same UV-to-world conversion as actual ship movement; wind is its destination direction.
  state.cloudOffsetX=Wrap(state.cloudOffsetX+System.Math.Cos(wind)*960*minimumShipSpeed*seconds,FieldWidth);
  state.cloudOffsetZ=Wrap(state.cloudOffsetZ-System.Math.Sin(wind)*540*minimumShipSpeed*seconds,FieldHeight);
 }

 public void Initialize(Camera camera,Material shared){view=camera;material=shared;mesh=new Mesh{name="Shared five-puff cartoon cloud"};
  var vertices=new List<Vector3>();var normals=new List<Vector3>();var indices=new List<int>();
  Vector3[] offsets={new Vector3(-5,2.5f,0),new Vector3(0,4,0),new Vector3(5,3,0),new Vector3(-2,3,3),new Vector3(3,2,-3)};
  Vector3[] radii={new Vector3(5,2.5f,3.5f),new Vector3(6,4,4.5f),new Vector3(5,3,3.5f),new Vector3(4,3,4),new Vector3(4,2,3)};
  const int rings=6,sides=10;
  for(int puff=0;puff<offsets.Length;puff++){int start=vertices.Count;var scale=radii[puff];
   for(int y=0;y<=rings;y++)for(int x=0;x<=sides;x++){float a=y*Mathf.PI/rings,b=x*Mathf.PI*2/sides;var n=new Vector3(Mathf.Sin(a)*Mathf.Cos(b),Mathf.Cos(a),Mathf.Sin(a)*Mathf.Sin(b));vertices.Add(offsets[puff]+Vector3.Scale(n,scale));normals.Add(new Vector3(n.x/scale.x,n.y/scale.y,n.z/scale.z).normalized);}
   for(int y=0;y<rings;y++)for(int x=0;x<sides;x++){int a=start+y*(sides+1)+x,b=a+sides+1;indices.Add(a);indices.Add(a+1);indices.Add(b);indices.Add(a+1);indices.Add(b+1);indices.Add(b);}
  }
  mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetTriangles(indices,0);mesh.RecalculateBounds();
  material.enableInstancing=true;
 }
 public void Render(Vector3 light){if(!view||!mesh)return;
  if(shadowDirty||(light-shadowLight).sqrMagnitude>.00001f||Mathf.Abs(transform.position.y-shadowHeight)>.001f)BakeShadowMask(light);
  Shader.SetGlobalFloat("_SeaCloudTop",transform.position.y+mesh.bounds.max.y*1.1f);GeometryUtility.CalculateFrustumPlanes(view,planes);int count=0;var drift=Drift;
  Shader.SetGlobalTexture("_SeaCloudMask",shadowMask);Shader.SetGlobalVector("_SeaCloudOffset",new Vector4(drift.x,drift.z,FieldWidth,FieldHeight));
  for(int i=0;i<centers.Count;i++){var p=transform.position+centers[i]+drift;
   p.x=view.transform.position.x+Mathf.Repeat(p.x-view.transform.position.x+FieldWidth*.5f,FieldWidth)-FieldWidth*.5f;
   p.z=view.transform.position.z+Mathf.Repeat(p.z-view.transform.position.z+FieldHeight*.5f,FieldHeight)-FieldHeight*.5f;
   if((p-view.transform.position).sqrMagnitude>350*350||!GeometryUtility.TestPlanesAABB(planes,new Bounds(p+Vector3.up*4,Vector3.one*28)))continue;visible[count++]=Matrix4x4.TRS(p,Quaternion.identity,Vector3.one*sizes[i]);}
  if(count==0)return;
  if(SystemInfo.supportsInstancing)Graphics.DrawMeshInstanced(mesh,0,material,visible,count,null,ShadowCastingMode.Off,false,28,view,LightProbeUsage.Off);
  else for(int i=0;i<count;i++)Graphics.DrawMesh(mesh,visible[i],material,28,view,0,null,ShadowCastingMode.Off,false);
 }
 void OnDestroy(){if(mesh)Destroy(mesh);if(shadowMask)Destroy(shadowMask);}
}
}
