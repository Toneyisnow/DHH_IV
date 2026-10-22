using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace MaritimeLegend.CharacterArt {
 public sealed class PixelWalkDemo : MonoBehaviour {
  public PixelWalkActor actor;
  public Camera previewCamera;
  public string evidenceDirectory;
  public bool autoWalk;
  bool inspectFrames;
  bool loopInPlace;
  readonly string[] phases={"Right leg / Left arm", "Left follows", "Left leg / Right arm", "Right follows", "Idle"};
  float clock;
  UnityEngine.Rendering.RenderPipelineAsset previousQualityPipeline, previousDefaultPipeline;
  void Awake(){previousQualityPipeline=QualitySettings.renderPipeline;previousDefaultPipeline=UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline;QualitySettings.renderPipeline=null;UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline=null;}
  void OnDestroy(){QualitySettings.renderPipeline=previousQualityPipeline;UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline=previousDefaultPipeline;}
  readonly string[] outfits={"1  Bob / no cape","2  Long cape / sidearm"};
  readonly string[] palettes={"Madder","Teal","Indigo","Plum","Ochre","Sage","Ink","Rose"};
  IEnumerator Start(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-pixelWalkSmoke")>=0)yield return Smoke();}
  void Update(){
   if(actor==null)return;clock+=Time.deltaTime;
   if(Input.GetKeyDown(KeyCode.Alpha1))actor.SetAppearance(0,0);
   if(Input.GetKeyDown(KeyCode.Alpha2))actor.SetAppearance(1,2);
   
   if(Input.GetKeyDown(KeyCode.R))actor.transform.position=Vector3.zero;
   if(Input.GetAxisRaw("Horizontal")!=0||Input.GetAxisRaw("Vertical")!=0){inspectFrames=false;autoWalk=false;loopInPlace=false;}
   actor.keyboardControl=!autoWalk&&!inspectFrames&&!loopInPlace;
   if(autoWalk){int d=(int)(clock/1.15f)%8;actor.Step(PixelWalkActor.DirectionVectors[d],Time.deltaTime);}
   else if(loopInPlace){var pos=actor.transform.position;actor.Step(PixelWalkActor.DirectionVectors[actor.Facing],Time.deltaTime);actor.transform.position=pos;}
   var p=actor.transform.position;p.x=Mathf.Clamp(p.x,-5,5);p.y=Mathf.Clamp(p.y,-3.2f,2.5f);actor.transform.position=p;
  }
  void OnGUI(){
   GUI.skin.label.fontSize=15;GUI.skin.button.fontSize=14;
   GUILayout.BeginArea(new Rect(14,14,285,Screen.height-28),GUI.skin.box);
   GUILayout.Label("CHARACTER 04 / PIXEL WALK V14");GUILayout.Label("8 directions x (4 walk + 1 idle)");
   GUILayout.Space(10);GUILayout.Label("WASD / arrow keys: move\n1-2: outfit   R: reset position");
   int o=GUILayout.SelectionGrid(actor.outfit,outfits,1);if(o!=actor.outfit)actor.SetAppearance(o,o==0?0:2);
   GUILayout.Space(8);GUILayout.Label("Jacket palette (prototype mask)");int c=GUILayout.SelectionGrid(actor.palette,palettes,2);if(c!=actor.palette)actor.SetAppearance(actor.outfit,c);
   GUILayout.Space(8);autoWalk=GUILayout.Toggle(autoWalk,"Automatic 8-direction walk");
   bool loop=GUILayout.Toggle(loopInPlace,"Loop this direction in place");if(loop!=loopInPlace){loopInPlace=loop;if(loop)autoWalk=false;}
   GUILayout.Label("Facing: "+PixelWalkActor.DirectionNames[actor.Facing]+" / "+phases[actor.Frame]);
   for(int row=0;row<2;row++){GUILayout.BeginHorizontal();for(int col=0;col<4;col++){int d=row*4+col;if(GUILayout.Button(PixelWalkActor.DirectionNames[d])){autoWalk=false;inspectFrames=true;actor.keyboardControl=false;actor.SetFacing(d);}}GUILayout.EndHorizontal();}
   GUILayout.Label("Freeze a pose for inspection:");
   int f=GUILayout.SelectionGrid(actor.Frame,phases,2);if(f!=actor.Frame){autoWalk=false;loopInPlace=false;inspectFrames=true;actor.keyboardControl=false;actor.SetFacing(actor.Facing,f);}
   GUILayout.Label("Animation speed: "+actor.framesPerSecond.ToString("F1")+" fps");actor.framesPerSecond=GUILayout.HorizontalSlider(actor.framesPerSecond,2f,10f);
   GUILayout.Space(8);GUILayout.Label("Camera zoom");previewCamera.orthographicSize=GUILayout.HorizontalSlider(previewCamera.orthographicSize,2.7f,6f);
   GUILayout.Space(14);GUILayout.Label("Prototype: 2 baked equipment sets.\nPalette is configurable.\nEquipment layers are not yet independent.\nSource art needs final pixel cleanup.");
   GUILayout.EndArea();
  }
  IEnumerator Smoke(){
   autoWalk=false;actor.keyboardControl=false;enabled=false;
   string dir=string.IsNullOrEmpty(evidenceDirectory)?Path.Combine(Application.persistentDataPath,"PixelWalkReview"):evidenceDirectory;Directory.CreateDirectory(dir);
   yield return null;
   int combinations=0;
   for(int o=0;o<2;o++)for(int p=0;p<8;p++){actor.SetAppearance(o,p);for(int d=0;d<8;d++)for(int f=0;f<5;f++){actor.SetFacing(d,f);if(actor.GetComponent<SpriteRenderer>().sprite==null)throw new Exception("Missing sprite");}combinations++;}
   for(int d=0;d<8;d++){
    actor.transform.position=Vector3.zero;actor.Step(PixelWalkActor.DirectionVectors[d],.2f);if(actor.Facing!=d||Mathf.Abs(actor.transform.position.magnitude-actor.speed*.2f)>.001f)throw new Exception("Movement/direction mismatch "+d);
    actor.Step(Vector2.zero,.1f);if(actor.Moving||actor.Frame!=PixelWalkActor.IdleFrame||actor.Facing!=d)throw new Exception("Idle state failed "+d);
    actor.Step(PixelWalkActor.DirectionVectors[d],.01f);
    for(int f=0;f<4;f++){if(actor.Frame!=f)throw new Exception("Four phase sequence failed");actor.Step(PixelWalkActor.DirectionVectors[d],1f/actor.framesPerSecond);}
    if(actor.Frame!=0)throw new Exception("Walk cycle wrap failed");
   }
   foreach(var atlas in actor.outfitAtlases){
    var idleEyes=PixelWalkActor.FacePatches(0)[0];var idleOffset=PixelWalkActor.FaceOffset(atlas,0,4);
    for(int y=idleEyes.yMin;y<idleEyes.yMax;y++)for(int x=idleEyes.xMin;x<idleEyes.xMax;x++){
     var a=atlas.GetPixel(4*64+x+idleOffset.x,7*96+y+idleOffset.y);var b=atlas.GetPixel(x,7*96+y);
     if(a.a!=b.a||(a.a>.5f&&a!=b))throw new Exception("Downward idle eyes differ from walking eyes");
    }
    foreach(int d in new[]{0,1,7})foreach(var patch in PixelWalkActor.FacePatches(d))for(int f=1;f<4;f++)for(int y=patch.yMin;y<patch.yMax;y++)for(int x=patch.xMin;x<patch.xMax;x++){
     var offset=PixelWalkActor.FaceOffset(atlas,d,f);var a=atlas.GetPixel(f*64+x+offset.x,(7-d)*96+y+offset.y);var b=atlas.GetPixel(x,(7-d)*96+y);
     if(a.a!=b.a||(a.a>.5f&&a!=b))throw new Exception("Eye/mouth patch changed between walk frames");
    }
    for(int f=0;f<5;f++)for(int y=0;y<96;y++)for(int x=0;x<64;x++)
     {var a=atlas.GetPixel(f*64+x,2*96+y);var b=atlas.GetPixel((f<4?(f+2)%4:4)*64+63-x,4*96+y);if(a.a!=b.a||(a.a>.5f&&a!=b))throw new Exception("Exported NE does not match approved reflected NW gait");}
    for(int d=0;d<8;d++)for(int f=0;f<5;f++)for(int y=82;y<96;y++)for(int x=0;x<64;x++)if(atlas.GetPixel(f*64+x,(7-d)*96+y).a>.1f)throw new Exception("Nonempty top gutter");
    for(int d=0;d<8;d++)for(int a=0;a<5;a++)for(int b=a+1;b<5;b++){bool different=false;for(int y=8;y<38&&!different;y++)for(int x=0;x<64;x++)if(atlas.GetPixel(a*64+x,(7-d)*96+y)!=atlas.GetPixel(b*64+x,(7-d)*96+y)){different=true;break;}if(!different)throw new Exception("Duplicated lower-body frames");}
   }
   var anchorReport=new System.Text.StringBuilder("Side-view horizontal head center, measured relative to actual sprite pivot (pixels).\n");
   for(int o=0;o<2;o++)foreach(int d in new[]{2,6}){
    actor.SetAppearance(o,o==0?0:2);float min=float.MaxValue,max=float.MinValue;
    for(int f=0;f<5;f++){actor.SetFacing(d,f);var sprite=actor.GetComponent<SpriteRenderer>().sprite;float sum=0;int count=0;
     for(int y=61;y<=78;y++)for(int x=0;x<64;x++)if(sprite.texture.GetPixel((int)sprite.rect.x+x,(int)sprite.rect.y+y).a>160f/255f){sum+=x-sprite.pivot.x;count++;}
     float center=sum/count;min=Mathf.Min(min,center);max=Mathf.Max(max,center);anchorReport.AppendLine("Outfit "+o+" "+PixelWalkActor.DirectionNames[d]+" frame "+f+": "+center.ToString("F3"));
     if(f==4&&Mathf.Abs(sprite.pivot.x-32)>.001f)throw new Exception("Idle anchor changed");
    }
    anchorReport.AppendLine("Range: "+(max-min).ToString("F3"));if(max-min>1.001f)throw new Exception("Side-view head drift exceeds one pixel");
   }
   File.WriteAllText(Path.Combine(dir,"head-alignment-v14.txt"),anchorReport.ToString());
   var contact=new Texture2D(5*160,16*200,TextureFormat.RGB24,false);
   var sideContact=new Texture2D(5*160,4*200,TextureFormat.RGB24,false);
   var diagonalContact=new Texture2D(5*160,4*200,TextureFormat.RGB24,false);
   var rearContact=new Texture2D(5*160,4*200,TextureFormat.RGB24,false);
   previewCamera.rect=new Rect(0,0,1,1);previewCamera.orthographicSize=1.7f;previewCamera.transform.position=new Vector3(0,1.0f,-10);
   for(int o=0;o<2;o++)for(int d=0;d<8;d++)for(int f=0;f<5;f++){
    actor.transform.position=Vector3.zero;actor.SetAppearance(o,o==0?0:2);actor.SetFacing(d,f);yield return null;
    var rt=new RenderTexture(160,200,24);previewCamera.targetTexture=rt;previewCamera.Render();var old=RenderTexture.active;RenderTexture.active=rt;
    var shot=new Texture2D(160,200,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,160,200),0,0);shot.Apply();contact.SetPixels(f*160,(15-o*8-d)*200,160,200,shot.GetPixels());
    if(d==2||d==6)sideContact.SetPixels(f*160,(3-o*2-(d==6?1:0))*200,160,200,shot.GetPixels());
    if(d==1||d==7)diagonalContact.SetPixels(f*160,(3-o*2-(d==7?1:0))*200,160,200,shot.GetPixels());
    if(d==3||d==5)rearContact.SetPixels(f*160,(3-o*2-(d==5?1:0))*200,160,200,shot.GetPixels());
    previewCamera.targetTexture=null;RenderTexture.active=old;rt.Release();Destroy(rt);Destroy(shot);
   }
   contact.Apply();File.WriteAllBytes(Path.Combine(dir,"unity-v14-all-frames.png"),contact.EncodeToPNG());Destroy(contact);
   sideContact.Apply();File.WriteAllBytes(Path.Combine(dir,"unity-v14-side-cycles.png"),sideContact.EncodeToPNG());Destroy(sideContact);
   diagonalContact.Apply();File.WriteAllBytes(Path.Combine(dir,"unity-v14-diagonal-cycles.png"),diagonalContact.EncodeToPNG());Destroy(diagonalContact);
   rearContact.Apply();File.WriteAllBytes(Path.Combine(dir,"unity-v14-rear-cycles.png"),rearContact.EncodeToPNG());Destroy(rearContact);
   File.WriteAllText(Path.Combine(dir,"runtime-validation-v14.txt"),"PASS: 16 outfit/palette combinations; 640 sprite selections; 8 normalized movement directions; independent idle retains each facing; 0-1-2-3-0 animation cycle; transparent top gutters; distinct lower-body frame pixels; 80 rendered views.\nPixel differences do not replace visual gait review.\n"+DateTime.UtcNow.ToString("O"));
   Debug.Log("PIXEL_WALK_SMOKE_PASS");Application.Quit(0);
  }
 }
}







