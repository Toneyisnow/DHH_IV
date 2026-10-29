using UnityEngine;
namespace MaritimeLegend {
[ExecuteAlways]
public sealed class ManualSeaV2Ship : MonoBehaviour {
 public Camera view;
 public ManualSeaV2Environment environment;
 public Texture2D[] directions;
 public Transform billboard;
 public Renderer art;
 public Vector3 heading=new Vector3(1,0,-1);
 public float phase;
 public float sailTrim,sailOpening=1,windFill=1;
 MaterialPropertyBlock properties;
 void LateUpdate(){Refresh();}
 public void Refresh(){
  if(!view||!art||!billboard||directions==null||directions.Length!=8)return;
  Vector3 projected=view.WorldToScreenPoint(transform.position+heading)-view.WorldToScreenPoint(transform.position);
  int index=((int)Mathf.Round(Mathf.Atan2(-projected.y,projected.x)*Mathf.Rad2Deg/45)+8)%8;
  billboard.rotation=view.transform.rotation;
  float t=Application.isPlaying?Time.time:12;
  billboard.localPosition=Vector3.up*(.08f+Mathf.Sin(t*.9f+phase)*.10f);
  if(properties==null)properties=new MaterialPropertyBlock();properties.SetTexture("_MainTex",directions[index]);
  var mode=environment?environment.mode:ManualSeaV2Environment.SkyMode.Sunny;
  properties.SetColor("_Tint",mode==ManualSeaV2Environment.SkyMode.Sunny?Color.white:mode==ManualSeaV2Environment.SkyMode.Sunset?new Color(1,.73f,.48f):new Color(.36f,.48f,.68f));properties.SetFloat("_SailTrim",sailTrim*Mathf.Deg2Rad);properties.SetFloat("_SailOpening",sailOpening);properties.SetFloat("_WindFill",windFill);art.SetPropertyBlock(properties);
 }
}
}
