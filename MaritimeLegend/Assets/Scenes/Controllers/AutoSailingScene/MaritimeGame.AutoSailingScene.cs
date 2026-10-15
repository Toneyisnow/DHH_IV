using System;
using System.IO;
using System.Linq;
using UnityEngine;
using MaritimeLegend.Domain;
using GUI=MaritimeLegend.LiveGui;
using GUIUtility=MaritimeLegend.LiveGuiUtility;
namespace MaritimeLegend {
public partial class MaritimeGame {
internal void AutoConfig(){Panel(new Rect(120,145,1680,720));Label(165,175,1530,60,"自动运输 · "+c.ports[s.city].name+" → "+c.ports[selectedPort].name,heading);Label(170,270,1500,50,"仅允许已亲自发现的方向航线。每个航段60秒，仅在游戏运行时推进。",small);if(Btn(170,350,560,65,"商品："+c.products[autoProduct].name))autoProduct=(autoProduct+1)%c.products.Length;autoQuantity=IntSlider("数量上限",autoQuantity,450,1,80);autoBuy=IntSlider("最高单件买价",autoBuy,520,1,900);autoSell=IntSlider("最低单件卖价",autoSell,590,1,900);autoLoops=IntSlider("循环次数",autoLoops,660,1,10);autoReserve=IntSlider("港口最小保留",autoReserve,730,0,80);Label(1120,345,570,90,"补给策略：每次装货支付5金币；船体<20或补给<5暂停。无盈利不装货。",small);if(Btn(1120,480,560,65,"预算 "+autoBudget+" · 点击增加500"))autoBudget=autoBudget>=10000?500:autoBudget+500;if(Btn(1120,600,560,75,"提交运输任务")){int n=s.autoRoutes.Count;Notify(g.AddAuto(selectedPort,c.products[autoProduct].id,autoQuantity,autoBuy,autoSell));if(s.autoRoutes.Count>n){var a=s.autoRoutes.Last();a.loops=autoLoops;a.minReserve=autoReserve;a.budget=autoBudget;Change("自动航线");}}if(Btn(1120,715,560,65,"返回自动航线"))Change("自动航线");}
}}
