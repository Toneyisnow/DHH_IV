using System;
using System.IO;
using System.Linq;
using UnityEngine;
using MaritimeLegend.Domain;
using GUI=MaritimeLegend.LiveGui;
using GUIUtility=MaritimeLegend.LiveGuiUtility;
namespace MaritimeLegend {
public partial class MaritimeGame {
internal void Title(){if(globe!=null){GUIUtility.RotateAroundPivot(Time.realtimeSinceStartup*1.5f,new Vector2(600,515));GUI.DrawTexture(new Rect(150,65,900,900),globe,ScaleMode.ScaleToFit);GUIUtility.RotateAroundPivot(-Time.realtimeSinceStartup*1.5f,new Vector2(600,515));}Fill(new Rect(1020,0,900,1080),new Color(.015f,.04f,.07f,.8f));Label(1130,180,700,100,"航 海 世 家",new GUIStyle(heading){fontSize=76});Label(1135,290,660,60,"M A R I T I M E   L E G E N D",new GUIStyle(small){fontSize=26});Label(1140,390,570,100,"一张旧海图，八座远方的港口。\n让一个家族的承诺，成为世界的航路。");if(!titleReady){if(Btn(1180,570,480,70,"Press any key · 按任意键"))titleReady=true;}else{if(s!=null&&Btn(1240,500,420,66,"继续当前航程"))Change(s.atSea?"航海":"城市");if(Btn(1240,580,420,66,"新游戏")){OpenSlots("new");}if(Btn(1240,660,420,66,"读取航海日志")){OpenSlots("load");}if(Btn(1240,740,420,66,"设置"))settingsOpen=true;if(Btn(1240,820,420,66,"制作人员与素材"))Notify("代码、组件美术及音乐：本项目原创；参考重建图：用户提供。完整记录见 Documentation。");if(Btn(1240,900,420,66,"退出"))Application.Quit();}Label(50,995,950,40,"风会改变方向，承诺不会。",small);}
}}
