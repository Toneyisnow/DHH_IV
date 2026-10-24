using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
namespace MaritimeLegend {
public sealed class SeaReliefRenderer {
 readonly Texture2D seaData;readonly Texture2DArray mapTiles;readonly GameObject root;readonly Camera camera;readonly Mesh terrain;readonly Material groundMaterial,oceanMaterial;readonly GameObject ocean;
 public RenderTexture Image {get;private set;}
 public int VertexCount=>terrain.vertexCount;
 public SeaReliefRenderer(Transform owner,Texture2D map){
  root=new GameObject("Manual sea 3D relief");root.transform.SetParent(owner,false);root.transform.position=new Vector3(0,-5000,0);
  groundMaterial=new Material(Resources.Load<Shader>("Shaders/SeaReliefGround"));groundMaterial.mainTexture=map;
  oceanMaterial=new Material(Resources.Load<Shader>("Shaders/SeaReliefWater"));oceanMaterial.mainTexture=map;
  mapTiles=SeaMapTiles.Load();groundMaterial.SetTexture("_MapTiles",mapTiles);oceanMaterial.SetTexture("_MapTiles",mapTiles);
  using(var coast=new BinaryReader(new MemoryStream(Resources.Load<TextAsset>("Models/ManualSailing/coast-distance").bytes)))
  using(var depth=new BinaryReader(new MemoryStream(Resources.Load<TextAsset>("Models/ManualSailing/bathymetry").bytes))){
   int w=coast.ReadInt32(),h=coast.ReadInt32();if(depth.ReadInt32()!=w||depth.ReadInt32()!=h)throw new System.InvalidOperationException("Sea data dimensions differ");
   var colors=new Color[w*h];for(int y=0;y<h;y++)for(int x=0;x<w;x++)colors[(h-1-y)*w+x]=new Color(coast.ReadSingle(),depth.ReadSingle(),0,1);
   seaData=new Texture2D(w,h,TextureFormat.RGFloat,false,true){name="Coast SDF and bathymetry",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};seaData.SetPixels(colors);seaData.Apply(false,true);
   groundMaterial.SetTexture("_SeaData",seaData);oceanMaterial.SetTexture("_SeaData",seaData);
  }
  using(var reader=new BinaryReader(new MemoryStream(Resources.Load<TextAsset>("Models/ManualSailing/relief").bytes))){
   int w=reader.ReadInt32(),h=reader.ReadInt32();var vertices=new Vector3[w*h];var uv=new Vector2[w*h];var indices=new int[(w-1)*(h-1)*6];int k=0;
   for(int y=0;y<h;y++)for(int x=0;x<w;x++){int i=y*w+x;float u=x/(float)(w-1),v=y/(float)(h-1);vertices[i]=new Vector3(u*1672,reader.ReadSingle(),-v*941);uv[i]=new Vector2(u,1-v);if(x<w-1&&y<h-1){indices[k++]=i;indices[k++]=i+1;indices[k++]=i+w;indices[k++]=i+1;indices[k++]=i+w+1;indices[k++]=i+w;}}
   terrain=new Mesh{name="SEA relief - 393k real height vertices",indexFormat=IndexFormat.UInt32};terrain.vertices=vertices;terrain.uv=uv;terrain.triangles=indices;terrain.RecalculateNormals();terrain.RecalculateBounds();
  }
  var land=new GameObject("Raised islands and mountain ranges",typeof(MeshFilter),typeof(MeshRenderer));land.layer=28;land.transform.SetParent(root.transform,false);land.GetComponent<MeshFilter>().sharedMesh=terrain;land.GetComponent<MeshRenderer>().sharedMaterial=groundMaterial;
  ocean=GameObject.CreatePrimitive(PrimitiveType.Quad);Object.Destroy(ocean.GetComponent<Collider>());ocean.name="Sea level";ocean.layer=28;ocean.transform.SetParent(root.transform,false);ocean.transform.localPosition=new Vector3(836,0,-470.5f);ocean.transform.localRotation=Quaternion.Euler(90,0,0);ocean.transform.localScale=new Vector3(1672,941,1);ocean.GetComponent<MeshRenderer>().sharedMaterial=oceanMaterial;
  // A sea-level land apron carries the exact coastline between height-grid vertices.
  // Without it, cells spanning negative seabed heights trim the shore into square steps.
  var shore=Object.Instantiate(ocean,root.transform);shore.name="Sea-level coastline apron";shore.GetComponent<MeshRenderer>().sharedMaterial=groundMaterial;

  camera=new GameObject("Relief camera 45 degrees",typeof(Camera)).GetComponent<Camera>();camera.transform.SetParent(root.transform,false);camera.enabled=false;camera.orthographic=true;camera.orthographicSize=SeaProjection.CameraHalfHeight;camera.aspect=1920f/1080;camera.transform.localRotation=Quaternion.Euler(45,0,0);camera.nearClipPlane=.1f;camera.farClipPlane=2500;camera.cullingMask=1<<28;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.035f,.29f,.4f);
  Image=new RenderTexture(1920,1080,24,RenderTextureFormat.ARGB32){name="Manual sea relief view",antiAliasing=2};Image.Create();camera.targetTexture=Image;
 }
 public void Render(Vector2 mapCenter){var center=new Vector3(mapCenter.x*1672,0,-mapCenter.y*941);camera.transform.localPosition=center+new Vector3(0,100,-100);camera.Render();}
 public void Dispose(){if(Image!=null){Image.Release();Object.Destroy(Image);}Object.Destroy(mapTiles);Object.Destroy(seaData);Object.Destroy(terrain);Object.Destroy(groundMaterial);Object.Destroy(oceanMaterial);Object.Destroy(root);}
}
}
