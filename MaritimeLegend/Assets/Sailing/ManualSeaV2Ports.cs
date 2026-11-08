using System;
using System.IO;
using System.Linq;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
public sealed class ManualSeaV2Ports : IDisposable {
 public static string[] Names=>ManualSeaPortCatalog.All.Select(p=>p.name).ToArray();
 public static Vector3[] Positions=>ManualSeaPortCatalog.All.Select(p=>p.Position).ToArray();
 readonly ManualSeaPort[] ports=ManualSeaPortCatalog.All;
 readonly GameObject[] objects;readonly Material material;readonly Mesh[] meshes;readonly MeshCollider[] colliders;
 readonly Camera camera;SeaExploration knowledge;
 readonly float[] heights;readonly int terrainWidth,terrainHeight;
 public ManualSeaV2Ports(Transform parent,Camera view,bool useBaked=true){
  camera=view;objects=new GameObject[ports.Length];meshes=new Mesh[ports.Length];colliders=new MeshCollider[ports.Length];
  using(var reader=new BinaryReader(new MemoryStream(Resources.Load<TextAsset>("Models/Environment/SoutheastAsiaV2/terrain").bytes))){terrainWidth=reader.ReadInt32();terrainHeight=reader.ReadInt32();reader.ReadSingle();reader.ReadSingle();heights=new float[terrainWidth*terrainHeight];for(int i=0;i<heights.Length;i++)heights[i]=reader.ReadSingle();}
  material=new Material(Resources.Load<Shader>("Shaders/CoastalCity3D"));
  for(int i=0;i<ports.Length;i++){
   var port=ports[i];var go=new GameObject("Coastal port - "+port.name);objects[i]=go;go.layer=28;go.transform.SetParent(parent,false);
   go.transform.position=port.Position+Vector3.up*port.height*.5f;
   var baked=useBaked?Resources.Load<Mesh>("Models/ManualSailing/Ports3D/"+port.id):null;
   meshes[i]=baked?UnityEngine.Object.Instantiate(baked):CoastalCityMesh.Build(port,Ground);
   go.AddComponent<MeshFilter>().sharedMesh=meshes[i];var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;
   renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;renderer.receiveShadows=true;
   colliders[i]=go.AddComponent<MeshCollider>();colliders[i].sharedMesh=meshes[i];
  }
 }
 public float Ground(float wx,float wz){
  var uv=ManualSeaV2Runtime.UV(new Vector3(wx,0,wz));float x=Mathf.Clamp01(uv.x)*(terrainWidth-1),y=Mathf.Clamp01(uv.y)*(terrainHeight-1);
  int ix=Mathf.Min((int)x,terrainWidth-2),iy=Mathf.Min((int)y,terrainHeight-2);
  return Mathf.Lerp(Mathf.Lerp(heights[iy*terrainWidth+ix],heights[iy*terrainWidth+ix+1],x-ix),Mathf.Lerp(heights[(iy+1)*terrainWidth+ix],heights[(iy+1)*terrainWidth+ix+1],x-ix),y-iy);
 }
 public void Render(SeaExploration explored,ManualSeaV2Environment.SkyMode mode){
  knowledge=explored;
  for(int i=0;i<ports.Length;i++){var port=ports[i];SeaExploration.Hex(port.x,port.z,out int q,out int r);objects[i].SetActive(explored!=null&&explored.At(q,r)>0);}
  material.color=mode==ManualSeaV2Environment.SkyMode.Night?new Color(.32f,.43f,.59f):mode==ManualSeaV2Environment.SkyMode.Sunset?new Color(1,.75f,.51f):new Color(.96f,.98f,.92f);
 }
 public string Hover(Vector2 pointer){
  if(knowledge==null)return null;
  var ray=camera.ViewportPointToRay(new Vector3(pointer.x/1920,1-pointer.y/1080,0));float nearest=float.MaxValue;string name=null;
  for(int i=0;i<ports.Length;i++){
   var port=ports[i];SeaExploration.Hex(port.x,port.z,out int q,out int r);if(knowledge.At(q,r)!=2)continue;
   if(colliders[i].Raycast(ray,out var hit,2000)&&hit.distance<nearest&&Visible(hit.point)){nearest=hit.distance;name=port.name;}
  }return name;
 }
 bool Visible(Vector3 target){var start=camera.transform.position;int steps=Mathf.CeilToInt(Vector3.Distance(start,target)*2);for(int i=1;i<steps;i++){var p=Vector3.Lerp(start,target,(float)i/steps);if(Ground(p.x,p.z)>p.y+.3f)return false;}return true;}
 public void Dispose(){if(material)UnityEngine.Object.Destroy(material);foreach(var go in objects)if(go)UnityEngine.Object.Destroy(go);foreach(var mesh in meshes)if(mesh)UnityEngine.Object.Destroy(mesh);}
}
}
