using UnityEngine;
using UnityEngine.Rendering;
namespace MaritimeLegend {
[ExecuteAlways]
public sealed class ManualSeaV2Instances : MonoBehaviour {
 [System.Serializable] public class Cell {public Bounds bounds;public Matrix4x4[] transforms;public int variant;}
 public Mesh[] nearMeshes,farMeshes;
 public Material material;
 public Cell[] cells;
 public Camera view;
 public float lodDistance=180;
 public int VisibleInstances {get;private set;}
 public int VisibleBatches {get;private set;}
 readonly Plane[] planes=new Plane[6];
 void Update(){Render();}
 public void Render(){
  if(!view||material==null||cells==null)return;
  GeometryUtility.CalculateFrustumPlanes(view,planes);VisibleInstances=0;VisibleBatches=0;
  foreach(var cell in cells){
   if(!GeometryUtility.TestPlanesAABB(planes,cell.bounds))continue;
   var mesh=(Vector3.Distance(view.transform.position,cell.bounds.center)>lodDistance?farMeshes:nearMeshes)[cell.variant];
   if(!mesh||cell.transforms.Length==0)continue;
   // Cells are bounded to <= 1023 instances by the authoring tool. Culling is per cell, not whole-world.
   if(SystemInfo.supportsInstancing)Graphics.DrawMeshInstanced(mesh,0,material,cell.transforms,cell.transforms.Length,null,ShadowCastingMode.On,true,0,view,LightProbeUsage.Off);
   else foreach(var matrix in cell.transforms)Graphics.DrawMesh(mesh,matrix,material,0,view,0,null,ShadowCastingMode.On,true,null,LightProbeUsage.Off);
   VisibleInstances+=cell.transforms.Length;VisibleBatches++;
  }
 }
}
}
