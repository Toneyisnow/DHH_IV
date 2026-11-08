using System;
using System.IO;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace MaritimeLegend.Editor {
 public static class CoastalCityModelExport {
  public static void PrepareCityDefinitions(){
   var catalog=MaritimeLegend.Domain.CatalogLoader.Load();
   var template=AssetDatabase.LoadAssetAtPath<CityDefinition>("Assets/Cities/MalaccaCity/MalaccaCity.asset");
   foreach(var port in catalog.ports){
    string folder="Assets/Cities/"+port.cityKey,path=folder+"/"+port.cityKey+".asset";
    if(AssetDatabase.LoadAssetAtPath<CityDefinition>(path))continue;
    Directory.CreateDirectory(folder);var definition=UnityEngine.Object.Instantiate(template);
    definition.cityId=port.id;definition.cityKey=port.cityKey;definition.displayName=port.name;
    AssetDatabase.CreateAsset(definition,path);
   }
   AssetDatabase.SaveAssets();
  }
  public static void Run(){
   var root=new GameObject("Coastal model export");var cam=root.AddComponent<Camera>();cam.enabled=false;
   var cities=new ManualSeaV2Ports(root.transform,cam,false);
   var project=Application.dataPath.Replace('\\','/');int marker=project.IndexOf("/MaritimeLegend/",StringComparison.OrdinalIgnoreCase);
   string source=project.Substring(0,marker)+"/ArtSource/ManualSailingV2/CoastalPorts/Models3D";
   string ready="Assets/Resources/Models/ManualSailing/Ports3D";Directory.CreateDirectory(source);Directory.CreateDirectory(ready);
   foreach(var port in ManualSeaPortCatalog.All){
    var filter=root.transform.Find("Coastal port - "+port.name).GetComponent<MeshFilter>();var mesh=filter.sharedMesh;
    if(mesh.vertexCount<100||mesh.bounds.size.z<2||mesh.bounds.size.x<2)throw new Exception("City is not a solid model: "+port.id);
    string path=ready+"/"+port.id+".asset";var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
    if(saved)EditorUtility.CopySerialized(mesh,saved);else AssetDatabase.CreateAsset(UnityEngine.Object.Instantiate(mesh),path);
    var obj=new StringBuilder("# Terrain-fitted solid city, local pivot = shore anchor + half height\nmtllib "+port.id+".mtl\no "+port.id+"\n");
    foreach(var p in mesh.vertices)obj.AppendFormat(CultureInfo.InvariantCulture,"v {0} {1} {2}\n",p.x,p.y,p.z);
    var colors=mesh.colors;var faces=mesh.triangles;var palette=new System.Collections.Generic.Dictionary<Color,int>();var mtl=new StringBuilder();
    for(int i=0;i<faces.Length;i+=3){Color c=colors[faces[i]];if(!palette.TryGetValue(c,out int id)){id=palette.Count;palette[c]=id;mtl.AppendFormat(CultureInfo.InvariantCulture,"newmtl color{0}\nKd {1} {2} {3}\n\n",id,c.r,c.g,c.b);}obj.Append("usemtl color"+id+"\nf "+(faces[i]+1)+" "+(faces[i+1]+1)+" "+(faces[i+2]+1)+"\n");}
    File.WriteAllText(source+"/"+port.id+".obj",obj.ToString());File.WriteAllText(source+"/"+port.id+".mtl",mtl.ToString());
    Debug.Log("COASTAL_SOLID_MODEL_PASS "+port.id+" vertices="+mesh.vertexCount+" bounds="+mesh.bounds.size);
   }
   AssetDatabase.SaveAssets();UnityEngine.Object.DestroyImmediate(root);
  }
 }
}
