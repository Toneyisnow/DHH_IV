using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace MaritimeLegend.Editor {
public static class ShipContactReview {
 public static void Capture(){
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var shader=Resources.Load<Shader>("Shaders/ManualSeaV2ShipContact");
  if(ShaderUtil.ShaderHasError(shader))throw new System.Exception("Contact shader failed");
  var camera=new GameObject("Review camera").AddComponent<Camera>();camera.transform.position=new Vector3(0,22,-32);camera.transform.LookAt(new Vector3(0,1,0));camera.fieldOfView=43;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.04f,.36f,.57f);
  var sea=GameObject.CreatePrimitive(PrimitiveType.Plane);sea.transform.localScale=Vector3.one*100;var seaMat=new Material(Shader.Find("Unlit/Color"));seaMat.color=camera.backgroundColor;sea.GetComponent<Renderer>().sharedMaterial=seaMat;
  var material=new Material(shader);var ships=new ManualSeaV2Ship[3];string[] types={"junk","square","lateen"},names={"E","SE","S","SW","W","NW","N","NE"};
  for(int i=0;i<3;i++){
   var root=new GameObject(types[i]);root.transform.position=new Vector3((i-1)*13,0,i*2);
   var art=new GameObject("Art",typeof(MeshFilter),typeof(MeshRenderer));art.transform.SetParent(root.transform,false);art.GetComponent<MeshFilter>().sharedMesh=Resources.Load<Mesh>("Models/Environment/SoutheastAsiaV2/ShipBillboard");art.GetComponent<Renderer>().sharedMaterial=Resources.Load<Material>("Materials/SoutheastAsiaV2/Ships");
   var ship=root.AddComponent<ManualSeaV2Ship>();ship.view=camera;ship.billboard=art.transform;ship.art=art.GetComponent<Renderer>();ship.directions=new Texture2D[8];for(int d=0;d<8;d++)ship.directions[d]=Resources.Load<Texture2D>("Sprites/ManualSailing/"+types[i]+"/"+names[d]);ships[i]=ship;
  }
  var rt=new RenderTexture(1440,800,24);camera.targetTexture=rt;camera.aspect=1.8f;
  for(int direction=0;direction<8;direction++){
   var heading=new Vector3(Mathf.Cos(direction*Mathf.PI/4),0,-Mathf.Sin(direction*Mathf.PI/4));
   var contacts=new GameObject[3];
   for(int i=0;i<3;i++){
    ships[i].heading=heading;ships[i].Refresh();var contact=GameObject.CreatePrimitive(PrimitiveType.Quad);contact.transform.position=ships[i].transform.position+Vector3.up*.025f;contact.transform.rotation=Quaternion.LookRotation(Vector3.up,heading);contact.transform.localScale=new Vector3(7,12,1);contact.GetComponent<Renderer>().sharedMaterial=material;contacts[i]=contact;
   }
   camera.Render();RenderTexture.active=rt;var image=new Texture2D(1440,800,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1440,800),0,0);image.Apply();File.WriteAllBytes("../ArtSource/ManualSailing/ShipContactReview/direction-"+direction+".png",image.EncodeToPNG());Object.DestroyImmediate(image);foreach(var contact in contacts)Object.DestroyImmediate(contact);
  }
  Debug.Log("SHIP_CONTACT_REVIEW_PASS");
 }
}
