using UnityEngine;
namespace MaritimeLegend {
public partial class MaritimeGame {
 internal void HarborUI(){
  if(selectedPort<0||selectedPort>=c.ports.Length||selectedPort==s.city)selectedPort=(s.city+1)%c.ports.Length;
  if(currentCity.Current.background!=null)LiveGui.DrawTexture(new Rect(30,130,1860,755),currentCity.Current.background,ScaleMode.ScaleAndCrop);
  Fill(new Rect(30,130,1860,755),new Color(.015f,.06f,.09f,.6f));
  Panel(new Rect(440,230,1040,560));
  Label(500,265,920,65,c.ports[s.city].name+" · 港口",heading);
  Label(500,350,920,50,"请选择航行方式");
  if(Btn(500,422,100,52,"上一港"))CycleHarborPort(-1);
  Label(630,432,680,45,"目的港："+c.ports[selectedPort].name);
  if(Btn(1320,422,100,52,"下一港"))CycleHarborPort(1);
  if(Btn(500,520,430,80,"自动航行"))Change("自动航线");
  if(Btn(990,520,430,80,"手动航行")){
   Notify(g.Depart(selectedPort));
   if(s.atSea){seaOwner=null;Change("航海");}
  }
  Label(500,620,430,60,"配置自动航线与舰队运输。",small);
  Label(990,620,430,60,"印度随机港外出发，驾驶三船纵队。",small);
  if(Btn(740,710,440,54,"返回城市"))Change("城市");
 }
 void CycleHarborPort(int step){
  do{selectedPort=(selectedPort+step+c.ports.Length)%c.ports.Length;}while(selectedPort==s.city&&c.ports.Length>1);
 }
}
}
