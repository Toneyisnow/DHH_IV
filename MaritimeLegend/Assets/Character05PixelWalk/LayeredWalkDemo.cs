using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace MaritimeLegend.CharacterArt {
 public sealed class LayeredWalkDemo : MonoBehaviour {
  public LayeredPixelWalker actor; public Camera previewCamera; public string evidenceDirectory;
  bool autoWalk,loop,inspect; float clock; Vector2 scroll;
  UnityEngine.Rendering.RenderPipelineAsset quality, pipeline;
  readonly string[] colors={"C1 Ivory / sand","C2 Indigo / gold","C3 Sand / ink","C4 Sage / brown","C5 Rust / brown","C6 Grey / charcoal","C7 Ochre / brown","C8 Slate / rust"};
  readonly string[] capes={"M0 None","M1 Short cotton","M2 Long cotton","M3 Striped shoulder","M4 Trade silk","M5 Night cloak","M6 Crossed wrap"};
  readonly string[] hats={"H0 Hair","H1 White turban","H3 Cotton cap","H5 Palm hat"};
  readonly string[] phases={"Right step / left arm","Left follows","Left step / right arm","Right follows","Planted idle"};
  void Awake(){quality=QualitySettings.renderPipeline;pipeline=UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline;QualitySettings.renderPipeline=null;UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline=null;}
  void OnDestroy(){QualitySettings.renderPipeline=quality;UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline=pipeline;}
  IEnumerator Start(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-layeredWalkSmoke")>=0)yield return Smoke();}
  void Update(){clock+=Time.deltaTime;if(Input.GetAxisRaw("Horizontal")!=0||Input.GetAxisRaw("Vertical")!=0){autoWalk=loop=inspect=false;}actor.keyboardControl=!autoWalk&&!loop&&!inspect;
   if(autoWalk)actor.Step(LayeredPixelWalker.Vectors[(int)(clock/1.3f)%8],Time.deltaTime);
   else if(loop){var p=actor.transform.position;actor.Step(LayeredPixelWalker.Vectors[actor.Facing],Time.deltaTime);actor.transform.position=p;}
   if(Input.GetKeyDown(KeyCode.R))actor.transform.position=Vector3.zero;
   var pos=actor.transform.position;pos.x=Mathf.Clamp(pos.x,-4,5);pos.y=Mathf.Clamp(pos.y,-2.8f,2.5f);actor.transform.position=pos;
  }
  void OnGUI(){GUI.skin.label.fontSize=14;GUI.skin.button.fontSize=13;GUILayout.BeginArea(new Rect(12,12,320,Screen.height-24),GUI.skin.box);scroll=GUILayout.BeginScrollView(scroll);
   GUILayout.Label("CHARACTER 05 / LAYERED WALK TEST");GUILayout.Label("Swahili coast sailor / 8 directions");GUILayout.Label("WASD / arrows: move    R: center");
   GUILayout.Space(5);GUILayout.Label("Clothing palettes (3 material channels)");actor.palette=GUILayout.SelectionGrid(actor.palette,colors,2);
   GUILayout.Label("Independent cape");actor.cape=GUILayout.SelectionGrid(actor.cape,capes,2);
   GUILayout.Label("Independent headwear");actor.hat=GUILayout.SelectionGrid(actor.hat,hats,2);
   GUILayout.Label("Weapon on character RIGHT");actor.weapon=GUILayout.SelectionGrid(actor.weapon,new[]{"None","A1 Knife","A2 Sword"},3);
   actor.parrot=GUILayout.Toggle(actor.parrot,"A3 Parrot on LEFT shoulder");
   GUILayout.Label("Utility kit on LEFT waist");actor.kit=GUILayout.SelectionGrid(actor.kit,new[]{"None","A4 Navigator","A5 Trader","A6 Deck kit"},2);
   GUILayout.Space(6);autoWalk=GUILayout.Toggle(autoWalk,"Automatic 8-direction movement");bool next=GUILayout.Toggle(loop,"Loop current direction in place");if(next!=loop){loop=next;if(loop)autoWalk=false;}
   GUILayout.Label("Facing: "+LayeredPixelWalker.Directions[actor.Facing]+" / "+phases[actor.Frame]);
   int d=GUILayout.SelectionGrid(actor.Facing,LayeredPixelWalker.Directions,4);if(d!=actor.Facing){autoWalk=false;inspect=true;actor.keyboardControl=false;actor.SetFacing(d);}
   int f=GUILayout.SelectionGrid(actor.Frame,phases,2);if(f!=actor.Frame){autoWalk=loop=false;inspect=true;actor.keyboardControl=false;actor.SetFacing(actor.Facing,f);}
   GUILayout.Label("Speed "+actor.framesPerSecond.ToString("F1")+" fps");actor.framesPerSecond=GUILayout.HorizontalSlider(actor.framesPerSecond,2,10);
   GUILayout.Label("Zoom");previewCamera.orthographicSize=GUILayout.HorizontalSlider(previewCamera.orthographicSize,1.5f,5);
   if(GUILayout.Button("Clear equipment")){actor.cape=actor.hat=actor.weapon=actor.kit=0;actor.parrot=false;}
   if(GUILayout.Button("Example: navigator with parrot")){actor.palette=1;actor.cape=2;actor.hat=1;actor.weapon=2;actor.parrot=true;actor.kit=1;}
   GUILayout.Label("8 colors / 6 capes / 3 hats / 6 accessories\n5376 selectable combinations incl. None\nLayered prototype; 4 walk + separate idle.\nCape/gear motion uses anchor and pixel sway.");GUILayout.EndScrollView();GUILayout.EndArea();actor.Refresh();
  }
  void Configure(int p,int c,int h,int w,bool pet,int k){actor.palette=p;actor.cape=c;actor.hat=h;actor.weapon=w;actor.parrot=pet;actor.kit=k;actor.Refresh();}
  Texture2D Capture(){var rt=new RenderTexture(160,220,24);previewCamera.targetTexture=rt;previewCamera.Render();var old=RenderTexture.active;RenderTexture.active=rt;var shot=new Texture2D(160,220,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,160,220),0,0);shot.Apply();RenderTexture.active=old;previewCamera.targetTexture=null;rt.Release();Destroy(rt);return shot;}
  void Save(Texture2D t,string name){t.Apply();File.WriteAllBytes(Path.Combine(evidenceDirectory,name),t.EncodeToPNG());Destroy(t);}
  IEnumerator Smoke(){enabled=false;actor.keyboardControl=false;autoWalk=loop=false;Directory.CreateDirectory(evidenceDirectory);yield return null;
   int combinations=0,selections=0;for(int p=0;p<8;p++)for(int c=0;c<7;c++)for(int h=0;h<4;h++)for(int w=0;w<3;w++)for(int pet=0;pet<2;pet++)for(int k=0;k<4;k++){
    Configure(p,c,h,w,pet==1,k);for(int d=0;d<8;d++)foreach(int f in new[]{0,2,4}){actor.SetFacing(d,f);if(!actor.LayersAgree())throw new Exception("Layer frame mismatch");selections++;}combinations++;
   }
   for(int d=0;d<8;d++){actor.transform.position=Vector3.zero;actor.Step(LayeredPixelWalker.Vectors[d],.2f);if(actor.Facing!=d||Mathf.Abs(actor.transform.position.magnitude-actor.speed*.2f)>.001f)throw new Exception("Movement mismatch");actor.Step(Vector2.zero,0);if(actor.Frame!=4||actor.Facing!=d)throw new Exception("Idle mismatch");}
   Configure(0,0,0,0,false,0);if(actor.VisibleLayers()!=1)throw new Exception("Removed equipment remains visible");
   // Clothes must never recolor the visible head/skin in the upper band.
   for(int p=1;p<8;p++)for(int d=0;d<8;d++)for(int f=0;f<5;f++)for(int y=63;y<96;y++)for(int x=0;x<64;x++){var a=actor.bodies[p].GetPixel(f*64+x,(7-d)*96+y);var b=actor.bodies[0].GetPixel(f*64+x,(7-d)*96+y);if(a.a!=b.a||(a.a>.5f&&a!=b))throw new Exception("Palette leaked into head");}
   actor.transform.position=Vector3.zero;previewCamera.rect=new Rect(0,0,1,1);previewCamera.orthographicSize=1.75f;previewCamera.transform.position=new Vector3(0,1.2f,-10);
   var baseContact=new Texture2D(800,1760,TextureFormat.RGB24,false);
   for(int d=0;d<8;d++)for(int f=0;f<5;f++){Configure(1,0,0,0,false,0);actor.SetFacing(d,f);yield return null;var shot=Capture();baseContact.SetPixels(f*160,(7-d)*220,160,220,shot.GetPixels());Destroy(shot);}Save(baseContact,"base-eight-direction-frames.png");
   var wardrobes=new Texture2D(1280,440,TextureFormat.RGB24,false);
   for(int p=0;p<8;p++){Configure(p,0,0,0,false,0);actor.SetFacing(1,4);yield return null;var shot=Capture();wardrobes.SetPixels(p*160,220,160,220,shot.GetPixels());Destroy(shot);Configure(p,p%6+1,p%3+1,p%3,p%2==0,p%4);actor.SetFacing(1,4);yield return null;shot=Capture();wardrobes.SetPixels(p*160,0,160,220,shot.GetPixels());Destroy(shot);}Save(wardrobes,"eight-palettes-and-equipped-looks.png");
   var options=new Texture2D(960,660,TextureFormat.RGB24,false);
   for(int i=0;i<18;i++){Configure(0,i<6?i+1:0,i>=6&&i<9?i-5:0,i==9?1:i==10?2:0,i==11,i>=12&&i<15?i-11:0);if(i>=15)Configure(i-14, i-13, i-14,2,true,i-14);actor.SetFacing(i==9||i==10?7:1,4);yield return null;var shot=Capture();options.SetPixels(i%6*160,(2-i/6)*220,160,220,shot.GetPixels());Destroy(shot);}Save(options,"equipment-options.png");
   var cloakViews=new Texture2D(960,440,TextureFormat.RGB24,false);for(int c=1;c<=6;c++)for(int v=0;v<2;v++){Configure(0,c,0,0,false,0);actor.SetFacing(v==0?0:4,4);yield return null;var shot=Capture();cloakViews.SetPixels((c-1)*160,(1-v)*220,160,220,shot.GetPixels());Destroy(shot);}Save(cloakViews,"capes-front-back.png");
   for(int p=0;p<8;p++)for(int f=0;f<4;f++)for(int y=64;y<74;y++)for(int x=27;x<37;x++){var a=actor.bodies[p].GetPixel(f*64+x,7*96+y);var b=actor.bodies[p].GetPixel(4*64+x,7*96+y);if(a.a!=b.a||(a.a>.5f&&a!=b))throw new Exception("Front facial master drift");}
   var hatsReview=new Texture2D(1280,880,TextureFormat.RGB24,false);
   for(int h=0;h<4;h++)for(int d=0;d<8;d++){Configure(0,0,h,0,false,0);actor.SetFacing(d,4);yield return null;var shot=Capture();hatsReview.SetPixels(d*160,(3-h)*220,160,220,shot.GetPixels());Destroy(shot);}Save(hatsReview,"headwear-fit-eight-directions.png");
   var backReview=new Texture2D(1440,1320,TextureFormat.RGB24,false);
   for(int c=1;c<=6;c++)for(int d=3;d<=5;d++)for(int f=0;f<3;f++){Configure(0,c,0,0,false,0);actor.SetFacing(d,f==2?4:f*2);yield return null;var shot=Capture();backReview.SetPixels(((d-3)*3+f)*160,(6-c)*220,160,220,shot.GetPixels());Destroy(shot);}Save(backReview,"cape-rear-three-directions.png");
   var faceReview=new Texture2D(800,220,TextureFormat.RGB24,false);for(int f=0;f<5;f++){Configure(0,0,0,0,false,0);actor.SetFacing(0,f);yield return null;var shot=Capture();faceReview.SetPixels(f*160,0,160,220,shot.GetPixels());Destroy(shot);}Save(faceReview,"front-face-five-frames.png");
   var equipped=new Texture2D(800,1760,TextureFormat.RGB24,false);
   Configure(1,2,3,2,true,1);for(int d=0;d<8;d++)for(int f=0;f<5;f++){actor.SetFacing(d,f);yield return null;var shot=Capture();equipped.SetPixels(f*160,(7-d)*220,160,220,shot.GetPixels());Destroy(shot);}Save(equipped,"equipped-eight-direction-frames.png");
   File.WriteAllText(Path.Combine(evidenceDirectory,"runtime-validation.txt"),"PASS: "+combinations+" combinations, "+selections+" direction/pose layer selections, normalized movement, planted idle index, equipment removal, palette/head isolation, front eye/nose/mouth master pixel equality across all five frames and eight palettes. 217 rendered views including isolated hat fit and rear cape contact poses.\nAutomated checks do not establish anatomical correctness or absence of every clipping combination.\n"+DateTime.UtcNow.ToString("O"));Debug.Log("LAYERED_WALK_SMOKE_PASS");Application.Quit(0);
  }
 }
}

