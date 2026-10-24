using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace MaritimeLegend {
public sealed class IndianOceanRenderer:IDisposable {
 [Serializable]public class Placement{public string asset;public float x,y,z,scale,rotation;public bool snowy;}
 [Serializable]public class Port{public string id,name;public float x,y,landX,landZ;}
 [Serializable]public class Layout{public int width,height;public Placement[] placements;public Port[] ports;}
 public readonly Layout Data;
 public RenderTexture Image{get;private set;}
 public float Yaw{get;private set;} public float Zoom{get;private set;}=70;
 public float Pitch{get;private set;}=25;
 public const float Base=-5000;
 float minimumZoom=8,maximumZoom=90;
 readonly GameObject root;readonly Camera camera;readonly Texture2D terrainData;
 readonly Material ground,trees,mountains,dunes,water;readonly List<Mesh> ownedMeshes=new List<Mesh>();
 readonly int width,height;readonly float[] heights,coast;readonly Color32[] colors;
 Vector2 focus;
 public IndianOceanRenderer(Transform owner){
  Data=JsonUtility.FromJson<Layout>(Resources.Load<TextAsset>("Environment/IndianOcean/layout").text);
  root=new GameObject("Indian Ocean - tiled terrain and reusable scenery");root.transform.SetParent(owner,false);root.transform.position=new Vector3(0,Base,0);
  using(var r=new BinaryReader(new MemoryStream(Resources.Load<TextAsset>("Environment/IndianOcean/terrain").bytes))){
   width=r.ReadInt32();height=r.ReadInt32();int n=width*height;heights=new float[n];coast=new float[n];var pixels=new Color[n];colors=new Color32[n];
   for(int i=0;i<n;i++)heights[i]=r.ReadSingle();for(int i=0;i<n;i++)coast[i]=r.ReadSingle();for(int i=0;i<n;i++)pixels[i]=new Color(heights[i],coast[i],r.ReadSingle(),1);
   for(int i=0;i<n;i++)colors[i]=new Color32(r.ReadByte(),r.ReadByte(),r.ReadByte(),r.ReadByte());
   terrainData=new Texture2D(width,height,TextureFormat.RGBAFloat,false,true){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};terrainData.SetPixels(pixels);terrainData.Apply(false,true);
  }
  ground=new Material(Resources.Load<Shader>("Shaders/CoastalStudyGround"));ground.SetTexture("_GrassTex",Resources.Load<Texture2D>("Textures/Environment/CoastalStudy/GrassPainted"));ground.SetFloat("_GroundDetail",1);
  trees=new Material(Resources.Load<Shader>("Shaders/CoastalStudyGround"));mountains=new Material(Resources.Load<Shader>("Shaders/CoastalStudyMountain"));
  mountains.SetTexture("_TerrainData",terrainData);mountains.SetVector("_TerrainRect",new Vector4(-.5f,.5f,width,-height));mountains.SetFloat("_TerrainBase",Base);mountains.SetFloat("_UseTerrainMap",1);
  dunes=new Material(mountains);dunes.SetFloat("_FadeHeight",.32f);dunes.SetFloat("_FootDarkenHeight",.55f);dunes.SetColor("_FootColor",new Color(.66f,.50f,.25f));
  water=new Material(Resources.Load<Shader>("Shaders/IndianOceanWater"));water.SetTexture("_TerrainData",terrainData);
  for(int y=0;y<height-1;y+=128)for(int x=0;x<width-1;x+=128)BuildLand(x,y);
  BuildScenery();
  var sea=GameObject.CreatePrimitive(PrimitiveType.Quad);UnityEngine.Object.Destroy(sea.GetComponent<Collider>());sea.name="Depth-colored Indian Ocean";sea.layer=28;sea.transform.SetParent(root.transform,false);sea.transform.localPosition=new Vector3(width*.5f,-.025f,-height*.5f);sea.transform.localRotation=Quaternion.Euler(90,0,0);sea.transform.localScale=new Vector3(width,height,1);sea.GetComponent<Renderer>().sharedMaterial=water;
  var sun=new GameObject("Indian Ocean sun").AddComponent<Light>();sun.transform.SetParent(root.transform,false);sun.type=LightType.Directional;sun.transform.rotation=Quaternion.Euler(50,155,0);sun.cullingMask=1<<28;sun.intensity=1.25f;sun.shadows=LightShadows.Soft;sun.shadowStrength=.85f;sun.shadowBias=.035f;sun.shadowNormalBias=.15f;
  QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowResolution=ShadowResolution.VeryHigh;QualitySettings.shadowDistance=2200;QualitySettings.shadowCascades=4;
  camera=new GameObject("Indian Ocean camera - scene controlled pitch").AddComponent<Camera>();camera.transform.SetParent(root.transform,false);camera.enabled=false;camera.orthographic=true;camera.aspect=1920f/1080;camera.nearClipPlane=.1f;camera.farClipPlane=6000;camera.cullingMask=1<<28;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.018f,.12f,.22f);camera.allowHDR=false;camera.allowMSAA=true;camera.gameObject.AddComponent<CoastalPixelInk>();
  Image=new RenderTexture(1920,1080,24,RenderTextureFormat.ARGB32){antiAliasing=4,name="Indian Ocean view"};Image.Create();camera.targetTexture=Image;
 }
 struct V{public Vector3 p;public Color c;public float d;public V(Vector3 p,Color c,float d){this.p=p;this.c=c;this.d=d;}}
 V Vertex(int x,int y){int k=y*width+x;return new V(new Vector3(x,heights[k],-y),colors[k],coast[k]);}
 void BuildLand(int startX,int startY){
  var v=new List<Vector3>();var c=new List<Color>();var indices=new List<int>();
  for(int y=startY;y<Mathf.Min(height-1,startY+128);y++)for(int x=startX;x<Mathf.Min(width-1,startX+128);x++){int x1=Mathf.Min(x+1,width-1),y1=Mathf.Min(y+1,height-1);var a=Vertex(x,y);var b=Vertex(x1,y);var d=Vertex(x,y1);var e=Vertex(x1,y1);Triangle(a,b,d,v,c,indices);Triangle(b,e,d,v,c,indices);}
  if(v.Count==0)return;var mesh=new Mesh{name="Indian terrain "+startX+","+startY,indexFormat=IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetColors(c);mesh.SetTriangles(indices,0);var normals=new Vector3[v.Count];for(int i=0;i<v.Count;i++){float px=v[i].x,py=-v[i].z;normals[i]=new Vector3(HeightAt(px-1,py)-HeightAt(px+1,py),2,HeightAt(px,py+1)-HeightAt(px,py-1)).normalized;}mesh.normals=normals;mesh.RecalculateBounds();ownedMeshes.Add(mesh);MeshObject(mesh,ground,mesh.name);
 }
 void Triangle(V a,V b,V c,List<Vector3> vertices,List<Color> colors,List<int> indices){
  var input=new[]{a,b,c};var polygon=new List<V>(4);
  for(int i=0;i<3;i++){var p=input[i];var q=input[(i+1)%3];if(p.d<=0)polygon.Add(p);if((p.d<=0)!=(q.d<=0)){float t=p.d/(p.d-q.d);var pos=Vector3.Lerp(p.p,q.p,t);pos.y=0;polygon.Add(new V(pos,Color.Lerp(p.c,q.c,t),0));}}
  for(int i=1;i<polygon.Count-1;i++)foreach(var p in new[]{polygon[0],polygon[i],polygon[i+1]}){indices.Add(vertices.Count);vertices.Add(p.p);colors.Add(p.c);}
 }
 void MeshObject(Mesh mesh,Material material,string name){var obj=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));obj.layer=28;obj.transform.SetParent(root.transform,false);obj.GetComponent<MeshFilter>().sharedMesh=mesh;obj.GetComponent<MeshRenderer>().sharedMaterial=material;mesh.UploadMeshData(true);}
 float HeightAt(float x,float y){x=Mathf.Clamp(x,0,width-1.001f);y=Mathf.Clamp(y,0,height-1.001f);int ix=(int)x,iy=(int)y;return Mathf.Lerp(Mathf.Lerp(heights[iy*width+ix],heights[iy*width+ix+1],x-ix),Mathf.Lerp(heights[(iy+1)*width+ix],heights[(iy+1)*width+ix+1],x-ix),y-iy);}
 void BuildScenery(){
  var buckets=new Dictionary<string,List<CombineInstance>>();var templates=new Dictionary<string,Mesh>();
  foreach(var p in Data.placements){bool isTree=p.asset.StartsWith("Pine");string key=(isTree?"tree":p.asset.StartsWith("Dune")?"dune":"mountain")+"_"+(int)(p.x/96)+"_"+(int)(-p.z/96);if(!buckets.ContainsKey(key))buckets[key]=new List<CombineInstance>();
   string modelKey=p.asset+(p.snowy?"_Snow":"");if(!templates.TryGetValue(modelKey,out var mesh)){
    var prefab=Resources.Load<GameObject>("Prefabs/Environment/"+(isTree?"CoastalStudy/":"IndianOcean/")+p.asset);if(!prefab)throw new Exception("Missing Indian scenery "+p.asset);mesh=prefab.GetComponentInChildren<MeshFilter>().sharedMesh;
    if(p.snowy){mesh=UnityEngine.Object.Instantiate(mesh);var paint=mesh.colors;var verts=mesh.vertices;for(int i=0;i<paint.Length;i++){float t=Mathf.InverseLerp(mesh.bounds.max.y*.42f,mesh.bounds.max.y*.74f,verts[i].y);paint[i]=Color.Lerp(paint[i],new Color(.89f,.94f,.98f),t);}mesh.colors=paint;ownedMeshes.Add(mesh);}templates[modelKey]=mesh;
   }
   var transform=Matrix4x4.TRS(new Vector3(p.x,p.y,p.z),Quaternion.Euler(0,p.rotation+180,0),Vector3.one*p.scale);
   if(p.asset.StartsWith("Dune")){
    // Conform the complete dune apron to local terrain; no floating rectangular base.
    mesh=UnityEngine.Object.Instantiate(mesh);var vertices=mesh.vertices;
    for(int i=0;i<vertices.Length;i++){var world=transform.MultiplyPoint3x4(vertices[i]);vertices[i].y+=(HeightAt(world.x,-world.z)-p.y)/p.scale;}
    mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();ownedMeshes.Add(mesh);
   }
   buckets[key].Add(new CombineInstance{mesh=mesh,transform=transform});
  }
  foreach(var entry in buckets){var mesh=new Mesh{name=entry.Key,indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(entry.Value.ToArray(),true,true);ownedMeshes.Add(mesh);MeshObject(mesh,entry.Key.StartsWith("tree")?trees:entry.Key.StartsWith("dune")?dunes:mountains,entry.Key);}
 }
 public void ConfigureCamera(float nearest,float farthest,float pitch){minimumZoom=Mathf.Max(1,nearest);maximumZoom=Mathf.Max(minimumZoom,farthest);Pitch=Mathf.Clamp(pitch,10,85);Zoom=Mathf.Clamp(Zoom,minimumZoom,maximumZoom);PositionCamera();}
 public void SetView(float yaw,float zoom){Yaw=yaw;Zoom=Mathf.Clamp(zoom,minimumZoom,maximumZoom);PositionCamera();}
 public void Orbit(float horizontalDrag,float scroll){SetView(Yaw+horizontalDrag*2,Zoom*Mathf.Exp(-scroll*.12f));}
 void PositionCamera(){if(!camera)return;camera.orthographicSize=Zoom;camera.transform.localRotation=Quaternion.Euler(Pitch,Yaw,0);var target=new Vector3(focus.x*width,0,-focus.y*height);float distance=Mathf.Max(350,Zoom/Mathf.Tan(Pitch*Mathf.Deg2Rad)+180);camera.farClipPlane=Mathf.Max(6000,distance+Zoom/Mathf.Sin(Pitch*Mathf.Deg2Rad)+300);camera.transform.localPosition=target-camera.transform.forward*distance;}
 public void Center(Vector2 center){focus=center;PositionCamera();}
 public void Follow(Vector2 ship){var p=Project(ship);var bounded=new Vector2(Mathf.Clamp(p.x,768,1152),Mathf.Clamp(p.y,432,648));if((p-bounded).sqrMagnitude>.1f){var world=Unproject(bounded);focus+=ship-world;PositionCamera();}}
 public Vector2 Project(Vector2 point){var p=camera.WorldToViewportPoint(root.transform.TransformPoint(new Vector3(point.x*width,0,-point.y*height)));return new Vector2(p.x*1920,(1-p.y)*1080);}
 public Vector2 Unproject(Vector2 pointer){var ray=camera.ViewportPointToRay(new Vector3(pointer.x/1920,1-pointer.y/1080,0));var plane=new Plane(Vector3.up,new Vector3(0,Base,0));if(!plane.Raycast(ray,out var distance))return focus;var p=ray.GetPoint(distance);return new Vector2(p.x/width,-p.z/height);}
 public void Render(Vector2 ship){Follow(ship);camera.Render();}
 public void Capture(string path){camera.Render();var previous=RenderTexture.active;RenderTexture.active=Image;var texture=new Texture2D(Image.width,Image.height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,Image.width,Image.height),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.Destroy(texture);RenderTexture.active=previous;}
 public void Dispose(){if(Image){Image.Release();UnityEngine.Object.Destroy(Image);}foreach(var mesh in ownedMeshes)UnityEngine.Object.Destroy(mesh);foreach(var material in new[]{ground,trees,mountains,dunes,water})UnityEngine.Object.Destroy(material);UnityEngine.Object.Destroy(terrainData);UnityEngine.Object.Destroy(root);}
}
}
