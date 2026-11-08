using System;
using System.Collections.Generic;
using UnityEngine;
namespace MaritimeLegend.Editor {
public static class CoastalPortChecks {
 public static void Run(){
  var ports=ManualSeaPortCatalog.All;var coast=new CoastNavigation(Resources.Load<TextAsset>("Environment/SoutheastAsiaV2/coast-distance").bytes,9);
  int w=(int)CoastNavigation.Width,h=(int)CoastNavigation.Height;var safe=new bool[w*h];var visited=new bool[w*h];var queue=new int[w*h];
  for(int y=2;y<h-2;y++)for(int x=2;x<w-2;x++)safe[y*w+x]=coast.Distance(new Vector2(x,y))>=10;
  var origin=coast.Pixels(ports[0].Spawn);int start=Mathf.RoundToInt(origin.y)*w+Mathf.RoundToInt(origin.x),head=0,tail=0;
  if(!safe[start])throw new Exception("Port root is not navigable");queue[tail++]=start;visited[start]=true;
  while(head<tail){int p=queue[head++];Visit(p-1);Visit(p+1);Visit(p-w);Visit(p+w);}
  void Visit(int p){if(p<0||p>=safe.Length||visited[p]||!safe[p])return;visited[p]=true;queue[tail++]=p;}
  var ids=new HashSet<string>();var names=new HashSet<string>();
  foreach(var port in ports){
   if(!ids.Add(port.id)||!names.Add(port.name))throw new Exception("Duplicate coastal city "+port.id);
   var tex=Resources.Load<Texture2D>(port.art);if(!tex||port.cropW<=0||port.cropH<=0||port.aspect<=0)throw new Exception("Invalid coastal art "+port.id);
   var p=coast.Pixels(port.Spawn);if(coast.Distance(p)<17.9f||!visited[Mathf.RoundToInt(p.y)*w+Mathf.RoundToInt(p.x)])throw new Exception("Port not connected to open ocean: "+port.id+" clearance="+coast.Distance(p)+" reachable="+visited[Mathf.RoundToInt(p.y)*w+Mathf.RoundToInt(p.x)]);
   if(Vector3.Distance(ManualSeaV2Runtime.World(port.Spawn),port.Position)>22)throw new Exception("Departure too far from city "+port.id);
   Debug.Log("COASTAL_PORT_PASS "+port.id+" ocean reachable, coast clearance="+coast.Distance(p));
  }
  if(ManualSeaPortCatalog.Departure(6,"马六甲")==null||ManualSeaPortCatalog.Departure(0,"里斯本")!=null)throw new Exception("Incorrect regional city mapping");
  Debug.Log("COASTAL_PORT_CATALOG_PASS ports="+ports.Length+" connected navigable cells="+tail);
 }
}
}
