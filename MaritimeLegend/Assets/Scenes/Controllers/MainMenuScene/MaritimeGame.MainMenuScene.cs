using UnityEngine;
using GUI=MaritimeLegend.LiveGui;
namespace MaritimeLegend {
 public partial class MaritimeGame {
  float titleMenuStarted=-1;
  int titleInputFrame=-1;
  bool titleInputReleased;
  GUIStyle openingPrompt,openingSubtitle,openingButton,openingHitTarget;
  void BeginTitleMenu(){
   if(titleReady||settingsOpen||confirm!="")return;
   titleReady=true;titleMenuStarted=Time.unscaledTime;titleInputFrame=Time.frameCount;titleInputReleased=false;
  }
  bool TitleInputReady(float progress,bool pointerHeld){
   // Latch only the release of the opening gesture. Disabling a uGUI Button
   // during later pointer-down frames prevents its pointer-up from clicking.
   if(Time.frameCount>titleInputFrame+1&&!pointerHeld)titleInputReleased=true;
   return progress>.98f&&Time.frameCount>titleInputFrame+1&&titleInputReleased;
  }
  internal void Title(OpeningPresentation art){
   art.DrawBackground();
   float menu=titleReady?(titleMenuStarted<0?1:Mathf.SmoothStep(0,1,(Time.unscaledTime-titleMenuStarted)/.7f)):0;
   float width=Mathf.Lerp(1672*art.LogoScale,1000,menu);
   float height=width*941/1672;
   float cy=Mathf.Lerp(445,287,menu);
   GUI.DrawTexture(new Rect(960-width/2,cy-height/2,width,height),art.Logo,ScaleMode.ScaleToFit);
   if(openingPrompt==null||openingPrompt.font!=art.Serif){
    openingPrompt=new GUIStyle(small){font=art.Serif,fontSize=38,alignment=TextAnchor.MiddleCenter,wordWrap=false};
    openingSubtitle=new GUIStyle(openingPrompt){fontSize=29};
    openingButton=new GUIStyle(button){fontSize=25};
    openingHitTarget=new GUIStyle(openingPrompt){fontSize=1};
   }
   if(!titleReady){
    openingSubtitle.normal.textColor=new Color(.94f,.87f,.7f);

    float pulse=.48f+.52f*(.5f+.5f*Mathf.Sin(art.Elapsed*Mathf.PI*2/1.8f));
    openingPrompt.normal.textColor=new Color(1,.94f,.78f,pulse);
    Label(460,947,1000,55,"Press Any Key to Start",openingPrompt);
    // Keep an accessible uGUI button for mouse/keyboard and existing automation.
    GUI.backgroundColor=Color.clear;
    if(GUI.Button(new Rect(550,940,820,72),"Press any key",openingHitTarget))BeginTitleMenu();
    GUI.backgroundColor=Color.white;
   }else{
    Fill(new Rect(610,550,700,440),new Color(.025f,.018f,.012f,.67f*menu));
    Fill(new Rect(650,550,620,1),new Color(.85f,.65f,.3f,.55f*menu));
    bool enabled=GUI.enabled;
    // Require release + completion of the transition; the wake-up click cannot select a menu item.
    GUI.enabled=enabled&&TitleInputReady(menu,Input.GetMouseButton(0));
    float y=575;
    if(s!=null){if(OpeningMenuButton(y,"继续当前航程"))Change(s.atSea?"航海":"城市");y+=62;}
    if(OpeningMenuButton(y,"新游戏"))OpenSlots("new");y+=62;
    if(OpeningMenuButton(y,"读取航海日志"))OpenSlots("load");y+=62;
    if(OpeningMenuButton(y,"设置"))settingsOpen=true;y+=62;
    if(OpeningMenuButton(y,"制作人员与素材"))Notify("代码、组件美术及音乐：本项目原创；参考重建图：用户提供。完整记录见 Documentation。");y+=62;
    if(OpeningMenuButton(y,"退出"))Application.Quit();
    GUI.enabled=enabled;
   }
   art.DrawEffects(Mathf.Lerp(1,.28f,menu));
   GUI.color=Color.white;
  }
  bool OpeningMenuButton(float y,string caption){
   GUI.backgroundColor=new Color(.18f,.12f,.065f,.62f);
   bool clicked=GUI.Button(new Rect(700,y,520,51),caption,openingButton);
   GUI.backgroundColor=Color.white;if(clicked)sound.Cue(1);return clicked;
  }
 }
}
