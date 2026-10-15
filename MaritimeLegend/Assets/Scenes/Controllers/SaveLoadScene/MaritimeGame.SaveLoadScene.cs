using System;using System.IO;using UnityEngine;using MaritimeLegend.Domain;
namespace MaritimeLegend {public partial class MaritimeGame {
 string saveReturn="标题";int pendingSlot=-1;
 internal void OpenSlots(string mode){saveReturn=page;slotMode=mode;paused=false;Change("存档");}
 void WriteSlot(int index){s.slot=index;s.screen=saveReturn;SaveStore.Save(s,false);Notify("已保存至档案 "+(index+1));Change(saveReturn);}
 internal void Slots(){LiveGui.enabled=LiveGui.enabled&&pendingSlot<0;Panel(new Rect(40,35,1840,1000));Label(85,60,1600,65,slotMode=="save"?"保存家族档案 · 七个槽位":slotMode=="new"?"选择家族档案 · 七个独立航程":"读取家族档案",heading);
 for(int i=0;i<7;i++){float x=90+(i%4)*445,y=180+(i/4)*335;Panel(new Rect(x,y,420,295));Label(x+20,y+15,370,40,"档案 "+(i+1));GameState preview=null;string error="";bool exists=File.Exists(SaveStore.PathFor(i));if(exists){try{preview=SaveStore.Load(i);}catch(Exception ex){error=ex.Message;}}
 if(preview!=null){Avatar(new Rect(x+20,y+65,80,95),preview.appearance,0);Label(x+120,y+65,285,120,preview.name+" · "+preview.mode+"\n"+c.ports[preview.home].name+" / 第"+preview.chapter+"章\n金币 "+preview.gold,small);Label(x+20,y+188,385,32,preview.lastUtc,small);}else Label(x+25,y+75,365,100,error==""?"尚未启航":error,small);
 int index=i;if(Btn(x+20,y+232,180,45,slotMode=="new"?"建立档案":slotMode=="save"?"保存至此槽":"读取手动档",slotMode!="load"||preview!=null)){slot=index;if(slotMode=="save"){if(exists)pendingSlot=index;else WriteSlot(index);}else if(slotMode=="new"){if(exists)confirm="overwrite";else Change("模板");}else Load(index,false);}
 if(slotMode!="save"&&Btn(x+215,y+232,185,45,"读取自动档",File.Exists(SaveStore.PathFor(i,true))))Load(index,true);
 if(error!=""&&Btn(x+20,y+175,365,43,"尝试备份恢复"))Load(index,false,true);
 }
 if(Btn(85,940,330,60,"返回来源页面"))Change(saveReturn);
 if(pendingSlot>=0){LiveGui.enabled=true;Panel(new Rect(570,350,780,320));Label(620,400,680,90,"覆盖此槽位的手动档案？原手动档保留为备份。");if(Btn(620,560,300,65,"确认保存")){int index=pendingSlot;pendingSlot=-1;WriteSlot(index);}if(Btn(960,560,300,65,"取消覆盖"))pendingSlot=-1;}
 }
}}
