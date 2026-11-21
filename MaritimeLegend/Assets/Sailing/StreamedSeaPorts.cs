using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
// The existing coastal-city meshes/layouts are conformed to the streamed heightfield.
public sealed class StreamedSeaPorts:IDisposable {
 sealed class Port {public GameObject go;public Mesh mesh;public MeshCollider collider;public WorldPort data;}
 readonly Dictionary<int,Port> live=new Dictionary<int,Port>();readonly Transform parent;readonly Camera camera;readonly StreamedSeaTerrain terrain;readonly Material material;
 public StreamedSeaPorts(Transform parent,Camera camera,StreamedSeaTerrain terrain){this.parent=parent;this.camera=camera;this.terrain=terrain;material=new Material(Resources.Load<Shader>("Shaders/CoastalCity3D"));}
 float Ground(float x,float z){terrain.TryHeight(x,z,out float h);return h;}
 public void Update(WorldPort[] ports,double focusX,double focusY,float sight,ManualSeaV2Environment.SkyMode sky){
  foreach(var pair in live.ToArray())if(WorldMap.Distance(focusX,focusY,pair.Value.data.x,pair.Value.data.y)>sight+100){UnityEngine.Object.Destroy(pair.Value.go);UnityEngine.Object.Destroy(pair.Value.mesh);live.Remove(pair.Key);}
  material.color=sky==ManualSeaV2Environment.SkyMode.Night?new Color(.32f,.43f,.59f):sky==ManualSeaV2Environment.SkyMode.Sunset?new Color(1,.75f,.51f):new Color(.96f,.98f,.92f);
  foreach(var port in ports){if(live.ContainsKey(port.id)||WorldMap.Distance(focusX,focusY,port.x,port.y)>sight)continue;
   var template=ManualSeaPortCatalog.All.FirstOrDefault(p=>p.gamePortId==port.id);if(template==null)continue;
   float px=(float)(focusX+WorldMap.DeltaX(WorldMap.Wrap(focusX),port.x)),pz=(float)port.y;float best=float.MaxValue;Vector3 land=Vector3.zero;
   for(int z=-60;z<=60;z+=2)for(int x=-60;x<=60;x+=2){float score=x*x+z*z;if(score>=best||!terrain.TryHeight(px+x,pz+z,out float h)||h<.2f||h>7)continue;best=score;land=new Vector3(px+x,h,pz+z);}
   if(best==float.MaxValue||!terrain.Ready(WorldMap.Key(land.x-30,land.z-30))||!terrain.Ready(WorldMap.Key(land.x+30,land.z+30)))continue;
   var art=new ManualSeaPort{id=template.id,name=port.name,gamePortId=port.id,x=land.x,y=land.y,z=land.z,height=template.height,aspect=template.aspect,spawnX=px/960+.5f,spawnY=.5f-pz/540};
   var mesh=CoastalCityMesh.Build(art,Ground);var go=new GameObject("Coastal port - "+port.name);go.layer=28;go.transform.SetParent(parent,false);go.transform.position=art.Position+Vector3.up*art.height*.5f;
   go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;var collider=go.AddComponent<MeshCollider>();collider.sharedMesh=mesh;live.Add(port.id,new Port{go=go,mesh=mesh,collider=collider,data=port});break;
  }
 }
 public string Hover(Vector2 pointer){var ray=camera.ViewportPointToRay(new Vector3(pointer.x/1920,1-pointer.y/1080,0));float nearest=float.MaxValue;string name=null;foreach(var p in live.Values)if(p.collider.Raycast(ray,out var hit,1000)&&hit.distance<nearest){bool visible=true;int steps=Mathf.CeilToInt(hit.distance*2);for(int i=1;i<steps;i++){var sample=Vector3.Lerp(ray.origin,hit.point,(float)i/steps);if(Ground(sample.x,sample.z)>sample.y+.3f){visible=false;break;}}if(visible){nearest=hit.distance;name=p.data.name;}}return name;}
 public void SetActive(bool active){foreach(var p in live.Values)p.go.SetActive(active);}
 public void Dispose(){foreach(var p in live.Values){UnityEngine.Object.Destroy(p.go);UnityEngine.Object.Destroy(p.mesh);}live.Clear();UnityEngine.Object.Destroy(material);}
}
}
