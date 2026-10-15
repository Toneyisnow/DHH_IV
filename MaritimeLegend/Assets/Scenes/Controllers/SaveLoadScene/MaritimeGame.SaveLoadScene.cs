using System;
using System.IO;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {public partial class MaritimeGame {
 string saveReturn="标题";int pendingSlot=-1;
 readonly GameState[] slotPreviews=new GameState[7];
 readonly string[] slotErrors=new string[7];
 readonly bool[] slotExists=new bool[7],slotAutos=new bool[7];
 bool slotsCached;
 internal void OpenSlots(string mode){saveReturn=page;slotMode=mode;paused=false;slotsCached=false;pendingSlot=-1;Change("存档");}
 void WriteSlot(int index){s.slot=index;s.screen=saveReturn;SaveStore.Save(s,false);slotsCached=false;Notify("已保存至档案 "+(index+1));Change(saveReturn);}
 void CacheSlots(){
  for(int i=0;i<7;i++){
   slotPreviews[i]=null;slotErrors[i]="";slotExists[i]=File.Exists(SaveStore.PathFor(i));slotAutos[i]=File.Exists(SaveStore.PathFor(i,true));
   if(slotExists[i])try{slotPreviews[i]=SaveStore.Load(i);}catch(Exception ex){slotErrors[i]=ex.Message;}
  }
  slotsCached=true;
 }
 void ActivateSlot(int index){
  slot=index;
  if(slotMode=="save"){if(slotExists[index])pendingSlot=index;else WriteSlot(index);}
  else if(slotMode=="new"){if(slotExists[index])confirm="overwrite";else Change("模板");}
  else if(slotPreviews[index]!=null)Load(index,false);
 }
 bool SlotButton(float x,float y,float w,float h,string value,bool enabled=true){bool old=LiveGui.enabled;LiveGui.enabled=old&&enabled;LiveGui.backgroundColor=new Color(.22f,.145f,.075f,.9f);bool clicked=LiveGui.Button(new Rect(x,y,w,h),value,button);LiveGui.enabled=old;LiveGui.backgroundColor=Color.white;if(clicked)sound.Cue(1);return clicked;}
 internal void Slots(SaveSelectionPresentation art){
  if(!slotsCached)CacheSlots();
  bool enabled=LiveGui.enabled&&pendingSlot<0;
  int clicked=art.Draw(enabled);
  LiveGui.enabled=enabled;
  var caption=new GUIStyle(small){fontSize=22,alignment=TextAnchor.MiddleCenter};caption.normal.textColor=new Color(.95f,.86f,.65f);
  Label(620,35,680,48,slotMode=="save"?"保存航海档案":slotMode=="new"?"选择档案 · 开启新的航程":"读取航海档案",caption);
  for(int i=0;i<7;i++){
   var q=art.Corners(i);float x=(q[2].x+q[3].x)/2,y=Mathf.Max(q[2].y,q[3].y)-18;
   Label(x-105,y,210,28,"档案 "+(i+1)+(art.Selected==i?" · 已选择":""),caption);
  }
  if(clicked>=0)ActivateSlot(clicked);
  int focus=art.Focus;var preview=slotPreviews[focus];
  Fill(new Rect(0,997,1920,83),new Color(.025f,.018f,.009f,.88f));
  string info=preview!=null?preview.name+" · "+preview.mode+" · 第"+preview.chapter+"章 · 金币 "+preview.gold:slotErrors[focus]!=""?"档案损坏，可尝试备份恢复":"尚未启航 · 选择卡片建立独立航程";
  if(slotMode=="load"&&preview==null&&slotErrors[focus]=="")info="此槽暂无手动档案";
  Label(270,1009,920,31,"档案 "+(focus+1)+"  /  "+info,small);
  Label(270,1043,920,28,preview!=null?preview.lastUtc:"鼠标选择 / 方向键移动 / Enter 确认",small);
  if(SlotButton(35,1012,195,48,"返回")){slotsCached=false;Change(saveReturn);}
  if(SlotButton(1210,1012,225,48,slotMode=="new"?"建立档案":slotMode=="save"?"保存至此槽":"读取手动档",slotMode!="load"||preview!=null)){art.Select(focus);ActivateSlot(focus);}
  if(slotMode!="save"&&SlotButton(1455,1012,220,48,"读取自动档",slotAutos[focus]))Load(focus,true);
  if(slotErrors[focus]!=""&&SlotButton(1690,1012,205,48,"尝试备份恢复"))Load(focus,false,true);
  LiveGui.MaskReferenceViewport();
  if(pendingSlot>=0){LiveGui.enabled=true;Fill(new Rect(0,0,1920,1080),new Color(0,0,0,.65f));Panel(new Rect(570,350,780,320));Label(620,400,680,90,"覆盖此槽位的手动档案？原手动档保留为备份。");if(SlotButton(620,560,300,65,"确认保存")){int index=pendingSlot;pendingSlot=-1;WriteSlot(index);}if(SlotButton(960,560,300,65,"取消覆盖"))pendingSlot=-1;}
 }
}}
