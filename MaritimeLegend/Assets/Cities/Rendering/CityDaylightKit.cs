using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MaritimeLegend {
// Blender-baked, material-batched meshes. Kept in Resources for player builds.
public sealed class CityDaylightKit : IDisposable {
 [Serializable] public class Part { public Vector3[] vertices; public int[] triangles; public Color color; }
 [Serializable] public class Model { public Part[] parts; }
 readonly Dictionary<string, GameObject> templates = new Dictionary<string, GameObject>();
 readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
 readonly Transform parent; readonly Shader shader;
 public CityDaylightKit(Transform parent, Shader shader) { this.parent=parent; this.shader=shader; }
 public GameObject Place(string model, string name, Vector3 position, float rotation=0, float scale=1, bool walkable=false) {
  if(!templates.TryGetValue(model,out var template)) {
   var source=Resources.Load<TextAsset>("CityDaylight/"+model);
   if(source==null)throw new InvalidOperationException("Missing Blender city model: "+model);
   var data=JsonUtility.FromJson<Model>(source.text);
   template=new GameObject(model+" template");template.transform.SetParent(parent,false);template.SetActive(false);
   foreach(var part in data.parts) {
    var mesh=new Mesh {name=model,indexFormat=IndexFormat.UInt32,vertices=part.vertices,triangles=part.triangles};mesh.RecalculateNormals();mesh.RecalculateBounds();owned.Add(mesh);
    var mat=new Material(shader) {color=part.color};owned.Add(mat);
    var child=new GameObject("Baked mesh",typeof(MeshFilter),typeof(MeshRenderer));child.transform.SetParent(template.transform,false);child.layer=20;
    child.GetComponent<MeshFilter>().sharedMesh=mesh;var renderer=child.GetComponent<MeshRenderer>();renderer.sharedMaterial=mat;renderer.shadowCastingMode=ShadowCastingMode.On;renderer.receiveShadows=true;
   }
   templates.Add(model,template);
  }
  var go=UnityEngine.Object.Instantiate(template,parent);go.name=name;go.transform.position=position;go.transform.rotation=Quaternion.Euler(0,rotation,0);go.transform.localScale=Vector3.one*scale;go.SetActive(true);
  // Inland scenery remains visible to the main camera and sunlight, but does not
  // crowd the deliberately restrained waterfront reflection.
  if((model=="House"&&position.z>0)||model=="TownHall"||(model=="Tree"&&position.z>15))
   foreach(var renderer in go.GetComponentsInChildren<Renderer>())renderer.gameObject.layer=22;
  if(walkable)foreach(var filter in go.GetComponentsInChildren<MeshFilter>()){filter.gameObject.layer=21;filter.gameObject.AddComponent<MeshCollider>().sharedMesh=filter.sharedMesh;}
  return go;
 }
 public void Dispose(){foreach(var o in owned)if(o!=null)UnityEngine.Object.Destroy(o);}
}
}
