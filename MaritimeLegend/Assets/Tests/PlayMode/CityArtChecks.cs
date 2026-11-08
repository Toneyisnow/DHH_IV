using System;using System.IO;using System.Linq;using System.Collections;using UnityEngine;
namespace MaritimeLegend.Tests {
public sealed class CityArtChecks:MonoBehaviour {
 string dir;WorldSession world;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Bootstrap(){if(!Environment.GetCommandLineArgs().Contains("-cityArtSmoke"))return;var go=new GameObject("CityArtChecks");DontDestroyOnLoad(go);go.AddComponent<CityArtChecks>();}
 void Start(){dir=Path.GetFullPath("../ArtSource/CharacterWalkRework_2026-11-07/WardrobeTest/UnityReview");Directory.CreateDirectory(dir);StartCoroutine(Guard(Run()));}
 IEnumerator Guard(IEnumerator run){while(true){object next;try{if(!run.MoveNext())break;next=run.Current;}catch(Exception e){Debug.LogException(e);File.WriteAllText(Path.Combine(dir,"FAILED.txt"),e.ToString());Application.Quit(1);yield break;}yield return next;}Debug.Log("CITY_ART_PLAYMODE_PASS");File.WriteAllText(Path.Combine(dir,"PASS.txt"),"Lisbon stairs traversed, Venice bridge crossed, 8-direction atlas frames, depth buffer captured, weather cycled, perspective follow and dolly verified, large district reachable, authored 8 x (4 walk + 1 idle) frames and shadow lights verified, eight-scene structure retained. These are runtime captures, not static concept mockups.");Application.Quit(0);}
 void Check(bool ok,string message){if(!ok)throw new Exception("CITY ART FAILED: "+message);Debug.Log("CITY ART PASS "+message);}
 void Capture(CityDiorama view,string name){view.View.Render();var prior=RenderTexture.active;RenderTexture.active=view.Image;var tex=new Texture2D(view.Image.width,view.Image.height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,tex.width,tex.height),0,0);tex.Apply();var pixels=tex.GetPixels();for(int i=0;i<pixels.Length;i++)pixels[i]=pixels[i].gamma;tex.SetPixels(pixels);tex.Apply();File.WriteAllBytes(Path.Combine(dir,name+".png"),tex.EncodeToPNG());Destroy(tex);RenderTexture.active=prior;LiveGui.Capture(Path.Combine(dir,name+"-UI.png"));}
 IEnumerator Run(){yield return new WaitForSecondsRealtime(4);world=FindAnyObjectByType<WorldSession>();var city=FindAnyObjectByType<CityScene>();Check(city!=null&&city.Diorama!=null&&city.Diorama.Player!=null,"Lisbon live diorama loaded");var view=city.Diorama;Check(view.HasPath(view.Player.position,new Vector3(0,1.83f,7)),"Lisbon stair path exists");foreach(var marker in view.Landmarks)Check(view.HasPath(view.Player.position,marker.Value),"Lisbon landmark reachable: "+marker.Key);Check(!view.View.orthographic,"perspective camera");
 var painted=view.GetComponentsInChildren<Renderer>().SelectMany(r=>r.sharedMaterials).Where(m=>m!=null&&m.HasProperty("_Painted")&&m.GetFloat("_Painted")>.5f).ToArray();
 Check(painted.Select(m=>(int)m.GetFloat("_Tile")).Distinct().Count()==6,"six painted pixel surfaces bound to real scene geometry");
 Check(painted.All(m=>m.mainTexture!=null&&m.mainTexture.filterMode==FilterMode.Point),"painted atlas uses point sampling");Check(CityDiorama.MapWidth>=100&&CityDiorama.MapDepth==72,"three-wide two-deep district dimensions");Check(view.HasPath(view.Player.position,new Vector3(40,0,44)),"far district reachable");
 Check(view.PixelWalker!=null&&view.PixelWalker.CharacterId==6,"new cartoon Character06 is the controlled city player");
 Check(CityPixelWalker.ResolveCharacter(new MaritimeLegend.Domain.GameState{slot=0,appearance=new MaritimeLegend.Domain.Appearance{baseId=5}})==4,"approved slot 1 legacy preview");
 Check(CityPixelWalker.ResolveCharacter(new MaritimeLegend.Domain.GameState{slot=1,appearance=new MaritimeLegend.Domain.Appearance{baseId=5}})==0,"unmapped legacy identity preserved");
 Check(CityPixelWalker.ResolveCharacter(new MaritimeLegend.Domain.GameState{appearance=new MaritimeLegend.Domain.Appearance{portraitV2="06-C1"}})==0,"unsupported V2 identity preserved");
 Check(world.State.appearance.baseId==2&&string.IsNullOrEmpty(world.State.appearance.portraitV2),"unmapped legacy player gets new visual without save identity mutation");
 Check(view.Player.Find("Walker")==null,"controlled player has no old walker quad");
 Check(CityPixelWalker.ResolvePlayerVisual(new MaritimeLegend.Domain.GameState{appearance=new MaritimeLegend.Domain.Appearance{portraitV2="05-C1"}})==6,"city visual override also applies to explicit saved identities");
 Check(CityPixelWalker.ResolvePlayerVisual(new MaritimeLegend.Domain.GameState{appearance=new MaritimeLegend.Domain.Appearance{portraitV2="06-C1"}})==6,"requested Character06 player visual");
 Vector3 keyboardStart=view.Player.position;
 // Start off-grid: this reproduced the old snap-induced diagonal drift.
 Vector3 offGrid=keyboardStart+new Vector3(.13f,0,.17f);
 Vector2[] inputs={Vector2.down,new Vector2(-1,-1),Vector2.left,new Vector2(-1,1),Vector2.up,new Vector2(1,1),Vector2.right,new Vector2(1,-1)};
 float expected=4.2f*.05f*world.State.settings.moveSpeed;
 for(int d=0;d<8;d++){
  view.Player.position=offGrid;view.GoTo(new Vector3(0,1.83f,7));
  Check(view.MoveKeyboard(inputs[d],.05f),"keyboard moves direction "+d);
  Vector3 displacement=view.Player.position-offGrid;Vector2 actual=new Vector2(displacement.x,displacement.z);
  Check((actual-inputs[d].normalized*expected).magnitude<.0001f,"exact map direction and normalized speed "+d);
  Check(view.Facing==d&&view.PathLength==0,"keyboard pose and mouse path cancellation "+d);
  Vector3 stopped=view.Player.position;Check(!view.MoveKeyboard(Vector2.zero,.05f)&&view.Player.position==stopped,"release stops without drift "+d);
 }
 view.Player.position=new Vector3(8,0,4.95f);
 Check(!view.MoveKeyboard(Vector2.up,.05f),"keyboard cannot climb terrace wall");
 view.Player.position=keyboardStart;
 foreach(int character in new[]{4,5,6}){
  var actor=CityPixelWalker.Create(view.Player,character,Shader.Find("Maritime/CitySprite"),character==4?"04-C1":"05-H1");
  actor.transform.rotation=view.View.transform.rotation;Vector3 original=actor.transform.position;
  for(int d=0;d<8;d++)for(int f=0;f<5;f++){
   actor.Pose(d,f);Check(actor.Facing==d&&actor.Frame==f,"pixel pose "+character+"/"+d+"/"+f);
   foreach(var r in actor.GetComponentsInChildren<SpriteRenderer>())if(r.enabled){int cw=character==6?168:64,ch=character==6?168:96,row=character==6?(d+4)%8:d;Check(r.sprite!=null&&r.sprite.rect.width==cw&&r.sprite.rect.height==ch,"sprite cell size");Check(r.sprite.rect.x==f*cw&&r.sprite.rect.y==(7-row)*ch,"atlas direction and frame");if(character==6)Check(Mathf.Abs(r.sprite.pivot.y-10)<.001f,"cartoon foot anchor");Check(r.gameObject.layer==20&&r.sprite.texture.filterMode==FilterMode.Point,"city visible point sampled sprite");}
  }
  actor.Animate(7,true,.2f);Check(actor.Frame<4,"walk excludes idle frame");actor.Animate(7,false,.2f);Check(actor.Frame==4&&actor.Facing==7,"stop retains direction with dedicated idle");Check(actor.transform.position==original,"animation does not move city actor");
  view.PixelWalker.gameObject.SetActive(false);
  var oldPosition=view.View.transform.position;view.View.transform.position=view.Player.position+Vector3.up*(character==6?1.8f:.9f)-view.View.transform.forward*(character==6?8:5);
  for(int d=0;d<8;d++){actor.Pose(d,4);Capture(view,"Character"+character+"-direction"+d);}
  if(character==6){
   for(int look=0;look<3;look++){
    actor.Pose(5,2);var before=actor.transform.position;actor.SetAppearance(look);
    Check(actor.AppearanceIndex==look&&actor.Facing==5&&actor.Frame==2&&actor.transform.position==before,"wardrobe preserves movement and pose "+look);
    for(int d=0;d<8;d++)for(int f=0;f<5;f++){
     actor.Pose(d,f);var active=actor.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.enabled).ToArray();
     Check(active.Length==(look==0?1:4),"wardrobe active layer count "+look);
     foreach(var r in active)Check(r.sprite!=null&&r.sprite.rect.x==f*168&&r.sprite.rect.y==(7-(d+4)%8)*168&&r.sprite.pivot==new Vector2(84,10),"wardrobe synchronized frame and anchor "+look+"/"+d+"/"+f);
    }
    for(int d=0;d<8;d++){actor.Pose(d,0);Capture(view,"Wardrobe"+look+"-direction"+d);}
   }
   actor.SetAppearance(0);
  }
  view.View.transform.position=oldPosition;view.PixelWalker.gameObject.SetActive(true);Destroy(actor.gameObject);yield return null;
 }

 foreach(int character in new[]{4,5}){
  var actor=CityPixelWalker.CreateCartoon(view.Player,Shader.Find("Maritime/CitySprite"),character);
  actor.transform.rotation=view.View.transform.rotation;
  view.PixelWalker.gameObject.SetActive(false);
  var cameraPositionBefore=view.View.transform.position;
  view.View.transform.position=view.Player.position+Vector3.up*1.8f-view.View.transform.forward*8;
  for(int d=0;d<8;d++){
   for(int f=0;f<5;f++){
    actor.Pose(d,f);var r=actor.GetComponent<SpriteRenderer>();
    Check(actor.CharacterId==character&&actor.Facing==d&&actor.Frame==f,"new cartoon pose "+character+"/"+d+"/"+f);
    Check(r.sprite.rect==new Rect(f*168,(7-(d+4)%8)*168,168,168)&&r.sprite.pivot==new Vector2(84,10),"new cartoon atlas and anchor");
    Check(r.sprite.texture.filterMode==FilterMode.Point&&r.gameObject.layer==20,"new cartoon sampling and city layer");
    if(character==5&&f<4&&(d==1||d==2||d==3||d==4||d==5))Capture(view,"GaitRepair05-direction"+d+"-frame"+f);
    if(character==4&&f<4)Capture(view,"GaitRepair04-direction"+d+"-frame"+f);
   }
   actor.Animate(d,true,.2f);Check(actor.Frame<4,"new cartoon walking uses walk frame");Capture(view,"Cartoon"+character+"-walk-"+d);
   actor.Animate(d,false,.2f);Check(actor.Frame==4&&actor.Facing==d,"new cartoon idle keeps facing");Capture(view,"Cartoon"+character+"-idle-"+d);
  }
  view.View.transform.position=cameraPositionBefore;view.PixelWalker.gameObject.SetActive(true);Destroy(actor.gameObject);yield return null;
 }
 // New five: real layered rendering, independent slot masks, shared walking/idle frames.
 foreach(int character in new[]{1,2,3,7,8}){
  var actor=CityPixelWalker.CreateCartoon(view.Player,Shader.Find("Maritime/CitySprite"),character);
  actor.transform.rotation=view.View.transform.rotation;view.PixelWalker.gameObject.SetActive(false);
  var cameraBeforeNew=view.View.transform.position;view.View.transform.position=view.Player.position+Vector3.up*1.8f-view.View.transform.forward*8;
  Check(actor.HasModularWardrobe,"new character has independent equipment slots "+character);
  foreach(int mask in new[]{0,1,2,4,7}){
   actor.SetEquipmentMask(mask);
   Check(actor.EquipmentMask==mask,"independent equipment mask "+character+"/"+mask);
   for(int d=0;d<8;d++)for(int f=0;f<5;f++){
    actor.Pose(d,f);
    foreach(var r in actor.GetComponentsInChildren<SpriteRenderer>())if(r.enabled)Check(r.sprite!=null&&r.sprite.rect==new Rect(f*168,(7-(d+4)%8)*168,168,168)&&r.sprite.pivot==new Vector2(84,10)&&r.sprite.texture.filterMode==FilterMode.Point,"new layered synchronized pose");
    if(f==0&&(mask==0||mask==7))Capture(view,"NewFive-"+character+"-mask"+mask+"-direction"+d);
    if(f==4&&d==0&&(mask==1||mask==2||mask==4))Capture(view,"NewFive-"+character+"-single"+mask);
   }
  }
  actor.Animate(6,true,.2f);Check(actor.Frame<4,"new walker animates "+character);actor.Animate(6,false,.2f);Check(actor.Frame==4,"new walker stops "+character);
  actor.SetEquipmentMask(0);view.View.transform.position=cameraBeforeNew;view.PixelWalker.gameObject.SetActive(true);Destroy(actor.gameObject);yield return null;
 }
 Check(view.Landmarks.ContainsKey("市场A")&&view.Landmarks.ContainsKey("市场B")&&view.Landmarks.ContainsKey("酒馆"),"required building destinations");
 Check(view.GetComponentsInChildren<Light>().Any(l=>l.type==LightType.Point&&l.shadows!=LightShadows.None),"local shadow-casting lights");
 Vector3 cameraBefore=view.View.transform.position;float distanceBefore=view.CameraDistance;Capture(view,"Lisbon-before");
 var reflection=view.View.GetComponent<CityWaterReflection>();Check(reflection.ReflectionTexture.wrapMode==TextureWrapMode.Clamp,"reflection cannot tile beyond its projection");Check((reflection.ReflectionMask&(1<<22))==0&&(view.View.cullingMask&(1<<22))!=0,"inland houses visible but excluded from waterfront reflections");
 foreach(var asset in new[]{"town_frame","sidebar_background","label_bar","data_bar_black","product_row","function_button"})Check(Resources.Load<Texture2D>("TownUI/"+asset)!=null,"town artwork loaded: "+asset);
 Check(view.ScreenViewport.xMax<1580&&view.ScreenViewport.yMax<1015,"scene fits inside ornate frame and sidebar");view.GoTo(new Vector3(0,1.83f,7));float deadline=Time.realtimeSinceStartup+15;while(view.PathLength>0&&Time.realtimeSinceStartup<deadline)yield return null;Check(view.PixelWalker.Frame==4,"arrival uses independent idle frame");Check(view.Player.position.y>1.5f&&view.WalkedDistance>6,"player climbed real terrace geometry");Check(Vector3.Distance(cameraBefore,view.View.transform.position)>5,"camera follows walker");Check(view.CameraDistance<distanceBefore,"depth motion dollies inward");Capture(view,"Lisbon-after-walk");
 var cameraPosition=view.View.transform.position;view.View.transform.position=new Vector3(0,75,-55);view.View.transform.LookAt(new Vector3(0,0,27));Capture(view,"Daylight-district-overview");view.View.transform.position=cameraPosition;view.View.transform.rotation=Quaternion.Euler(32,0,0);var lens=view.View.GetComponent<CityLens>();lens.debugDepth=true;Capture(view,"Lisbon-depth");lens.debugDepth=false;view.CycleWeather();yield return new WaitForSecondsRealtime(1);Check(view.WeatherIndex==1,"fog weather control");Capture(view,"Lisbon-fog");
 world.State.city=2;yield return new WaitForSecondsRealtime(2);view=FindAnyObjectByType<CityScene>().Diorama;Check(view!=null,"Venice live diorama loaded");Check(view.HasPath(view.Player.position,new Vector3(4,0,1)),"bridge path exists");foreach(var marker in view.Landmarks)Check(view.HasPath(view.Player.position,marker.Value),"Venice landmark reachable: "+marker.Key);Capture(view,"Venice-before");view.GoTo(new Vector3(4,0,1));deadline=Time.realtimeSinceStartup+15;while(view.PathLength>0&&Time.realtimeSinceStartup<deadline)yield return null;Check(view.Player.position.x>3.5f&&view.WalkedDistance>5,"player crossed real bridge without walking on water");Capture(view,"Venice-after-walk");lens=view.View.GetComponent<CityLens>();lens.debugDepth=true;Capture(view,"Venice-depth");lens.debugDepth=false;view.CycleWeather();view.CycleWeather();yield return new WaitForSecondsRealtime(1);Check(view.WeatherIndex==2,"rain weather control");Capture(view,"Venice-rain");Check(world.Workflow.Active,"workflow continues during city art runtime");
 foreach(int id in new[]{1,3,4,5,6,7}){world.State.city=id;yield return new WaitForSecondsRealtime(.6f);var loaded=FindAnyObjectByType<CityScene>();Check(loaded.Current.cityId==id&&loaded.Diorama!=null,"daylight city loaded: "+id);foreach(var marker in loaded.Diorama.Landmarks)Check(loaded.Diorama.HasPath(loaded.Diorama.Player.position,marker.Value),"city "+id+" reachable: "+marker.Key);}

 world.State.cities[0].weatherPreset=0;world.State.city=0;yield return new WaitForSecondsRealtime(.7f);
 view=FindAnyObjectByType<CityScene>().Diorama;var game=FindAnyObjectByType<MaritimeGame>();
 Capture(view,"Town-layout");
 var playerBeforeSwitch=view.Player;var positionBeforeSwitch=view.Player.position;
 view.PixelWalker.SetAppearance(1);
 foreach(int expectedCharacter in new[]{4,1,2,3,7,8,5,6}){
  Check(LiveGui.ClickText("切换角色 · "+view.PixelWalker.CharacterName),"top character button clickable");yield return null;yield return null;
  Check(view.PixelWalker.CharacterId==expectedCharacter,"button selects current cartoon "+expectedCharacter);
  Check(view.Player==playerBeforeSwitch&&view.Player.position==positionBeforeSwitch,"switch preserves controlled player position");
  Capture(view,"Player-switch-"+expectedCharacter);
 }
 Check(view.PixelWalker.AppearanceIndex==1,"African female outfit retained after switching away and back");view.PixelWalker.SetAppearance(0);
 view.GoTo(new Vector3(0,1.83f,7));
 var pendingPath=view.PathLength;var cameraAtSwitch=view.View.transform.position;
 view.PixelWalker.Animate(6,true,.23f);int frameAtSwitch=view.PixelWalker.Frame;
 view.CyclePlayerCharacter();
 Check(view.PathLength==pendingPath&&pendingPath>0&&view.View.transform.position==cameraAtSwitch,"switch preserves pending route and camera");
 Check(view.PixelWalker.Facing==6&&view.PixelWalker.Frame==frameAtSwitch,"switch preserves walk phase and facing");
 Check(view.MoveKeyboard(Vector2.right,.05f),"switched character responds to movement");
 world.State.city=2;yield return new WaitForSecondsRealtime(.7f);
 view=FindAnyObjectByType<CityScene>().Diorama;
 Check(view.PixelWalker.CharacterId==4,"selection retained across city changes");
 world.State.city=0;yield return new WaitForSecondsRealtime(.7f);
 view=FindAnyObjectByType<CityScene>().Diorama;while(view.PixelWalker.CharacterId!=6)view.CyclePlayerCharacter();
 Check(world.State.appearance.baseId==2&&string.IsNullOrEmpty(world.State.appearance.portraitV2),"character switching preserves saved identity");
 yield return null;yield return null;
 Check(LiveGui.ClickText("功能"),"wardrobe function menu opens");yield return null;yield return null;
 Check(LiveGui.ClickText("搭配 · 原搭配"),"wardrobe switch button clickable");yield return null;yield return null;
 Check(view.PixelWalker.AppearanceIndex==1,"city player changed to travel layers");Capture(view,"Wardrobe-city-travel");view.PixelWalker.SetAppearance(0);
 Check(LiveGui.ClickText("功能"),"town function button clickable");yield return null;yield return null;
 Check(LiveGui.ClickText("切换天气"),"weather reachable in function menu");yield return null;yield return null;
 Check(view.WeatherIndex==1,"skinned weather button updates scene");
 Check(LiveGui.ClickText("功能"),"function menu reopens");yield return null;yield return null;
 Check(LiveGui.ClickText("仓库"),"warehouse remains reachable");yield return new WaitForSecondsRealtime(.6f);
 Check(game.page=="仓库","warehouse navigation works");game.Change("城市");yield return new WaitForSecondsRealtime(.6f);
 Check(LiveGui.ClickText("市场",true),"skinned market button clickable");yield return new WaitForSecondsRealtime(.6f);Check(game.page=="市场","market navigation works");
 game.Change("城市");yield return new WaitForSecondsRealtime(.6f);Check(FindAnyObjectByType<CityScene>().Diorama!=null,"return to live city works");


 }
}
}

