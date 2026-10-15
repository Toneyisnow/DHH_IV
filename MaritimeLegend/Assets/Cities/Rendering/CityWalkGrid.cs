using System;using System.Collections.Generic;using UnityEngine;
namespace MaritimeLegend {
// Pathfinding samples actual raised geometry. No teleport across water or terrace walls.
public sealed class CityWalkGrid {
 const int W=45,H=45;const float Step=.5f;readonly Vector3?[] nodes=new Vector3?[W*H];readonly List<Rect> blocked;
 public CityWalkGrid(List<Rect> blocked){this.blocked=blocked;for(int z=0;z<H;z++)for(int x=0;x<W;x++){Vector3 p=new Vector3(-11+x*Step,25,-9+z*Step);bool stop=false;foreach(var r in blocked)if(r.Contains(new Vector2(p.x,p.z))){stop=true;break;}if(!stop&&Physics.Raycast(p,Vector3.down,out var hit,40,1<<21))nodes[z*W+x]=hit.point;}}
 int Nearest(Vector3 p){int best=-1;float d=float.MaxValue;for(int i=0;i<nodes.Length;i++)if(nodes[i].HasValue){float q=(nodes[i].Value-p).sqrMagnitude;if(q<d){best=i;d=q;}}return best;}
 public Vector3 Snap(Vector3 p){int n=Nearest(p);return n<0?p:nodes[n].Value;}
 public List<Vector3> Path(Vector3 from,Vector3 to){int start=Nearest(from),goal=Nearest(to);var result=new List<Vector3>();if(start<0||goal<0)return result;var queue=new Queue<int>();var parents=new int[nodes.Length];for(int i=0;i<parents.Length;i++)parents[i]=-1;parents[start]=start;queue.Enqueue(start);
 while(queue.Count>0){int a=queue.Dequeue();if(a==goal)break;int x=a%W,z=a/W;for(int dz=-1;dz<=1;dz++)for(int dx=-1;dx<=1;dx++){if(dx==0&&dz==0)continue;int nx=x+dx,nz=z+dz;if(nx<0||nz<0||nx>=W||nz>=H)continue;int b=nz*W+nx;if(parents[b]>=0||!nodes[b].HasValue||Mathf.Abs(nodes[b].Value.y-nodes[a].Value.y)>.42f)continue;if(dx!=0&&dz!=0&&(!nodes[z*W+nx].HasValue||!nodes[nz*W+x].HasValue))continue;parents[b]=a;queue.Enqueue(b);}}
 if(parents[goal]<0)return result;for(int n=goal;n!=start;n=parents[n])result.Add(nodes[n].Value);result.Reverse();return result;}
}
}
