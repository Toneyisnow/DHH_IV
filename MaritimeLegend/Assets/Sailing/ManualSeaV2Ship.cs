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
 // Visible keel anchors in E, SE, S, SW, W, NW, N, NE order. Measured at
 // the ship shader's alpha cutoff; source images have different transparent margins.
 static readonly Vector2[][] KeelAnchors={
  new[]{new Vector2(.529f,.0818f),new Vector2(.6191f,.0727f),new Vector2(.5f,.0783f),new Vector2(.3524f,.0783f),new Vector2(.497f,.1275f),new Vector2(.6568f,.0783f),new Vector2(.5057f,.0455f),new Vector2(.4171f,.0682f)},
  new[]{new Vector2(.475f,.2012f),new Vector2(.6041f,.168f),new Vector2(.45f,.2188f),new Vector2(.4234f,.2363f),new Vector2(.5391f,.248f),new Vector2(.6232f,.1914f),new Vector2(.4514f,.0996f),new Vector2(.3597f,.1523f)},
  new[]{new Vector2(.406f,.2695f),new Vector2(.5506f,.1445f),new Vector2(.4883f,.1934f),new Vector2(.4091f,.2051f),new Vector2(.5167f,.1641f),new Vector2(.6116f,.1953f),new Vector2(.5065f,.0898f),new Vector2(.3826f,.1113f)}
 };
 Texture2D anchorSource;Vector2[] keelAnchors;
 Vector2 KeelAnchor(int index){
  if(anchorSource!=directions[0]){
   anchorSource=directions[0];keelAnchors=null;
   string[] types={"junk","square","lateen"};
   for(int i=0;i<types.Length;i++)if(anchorSource==Resources.Load<Texture2D>("Sprites/ManualSailing/"+types[i]+"/E")){keelAnchors=KeelAnchors[i];break;}
  }
  return keelAnchors==null?new Vector2(.5f,.06f):keelAnchors[index];
 }
 void LateUpdate(){Refresh();}
 public void Refresh(){
  if(!view||!art||!billboard||directions==null||directions.Length!=8)return;
  Vector3 projected=view.WorldToScreenPoint(transform.position+heading)-view.WorldToScreenPoint(transform.position);
  int index=((int)Mathf.Round(Mathf.Atan2(-projected.y,projected.x)*Mathf.Rad2Deg/45)+8)%8;
  billboard.rotation=view.transform.rotation;
  float t=Application.isPlaying?Time.time:12;
  billboard.localPosition=Vector3.up*(.08f+Mathf.Sin(t*.9f+phase)*.10f);
  // The 10x10 billboard mesh spans (-5,-.6) to (5,9.4). Place the visible
  // keel at the sea anchor, rather than placing the transparent image edge there.
  Vector2 keel=KeelAnchor(index);
  billboard.position-=billboard.TransformVector(new Vector3(keel.x*10-5,keel.y*10-.6f,0));
  if(properties==null)properties=new MaterialPropertyBlock();properties.SetTexture("_MainTex",directions[index]);
  var mode=environment?environment.mode:ManualSeaV2Environment.SkyMode.Sunny;
  properties.SetColor("_Tint",mode==ManualSeaV2Environment.SkyMode.Sunny?Color.white:mode==ManualSeaV2Environment.SkyMode.Sunset?new Color(1,.73f,.48f):new Color(.36f,.48f,.68f));properties.SetFloat("_SailTrim",sailTrim*Mathf.Deg2Rad);properties.SetFloat("_SailOpening",sailOpening);properties.SetFloat("_WindFill",windFill);art.SetPropertyBlock(properties);
 }
}
}
