using System;
using System.IO;
namespace MaritimeLegend.Domain {
public sealed class WorldChunkData {
 public readonly int X,Y; public readonly float[] Samples;
 public const int Side=WorldMap.Cells+3;
 public WorldChunkData(int x,int y,float[] samples){X=x;Y=y;Samples=samples;}
 public static WorldChunkData Parse(byte[] bytes,int x,int y){
  if(bytes==null||bytes.Length!=16+Side*Side*4)throw new InvalidDataException("World chunk length mismatch");
  using(var r=new BinaryReader(new MemoryStream(bytes))){
   if(r.ReadInt32()!=0x574D5031||r.ReadInt32()!=x||r.ReadInt32()!=y||r.ReadInt32()!=Side)throw new InvalidDataException("World chunk header mismatch");
   var a=new float[Side*Side];for(int i=0;i<a.Length;i++){a[i]=r.ReadSingle();if(!WorldMap.Finite(a[i]))throw new InvalidDataException("Invalid coast sample");}
   return new WorldChunkData(x,y,a);
  }
 }
 public float At(int x,int y)=>Samples[(y+1)*Side+x+1];
 public double Distance(double x,double y){
  double fx=WorldMap.Wrap(x)/WorldMap.SampleStep-X*WorldMap.Cells,fy=y/WorldMap.SampleStep-Y*WorldMap.Cells;
  fx=Math.Max(0,Math.Min(WorldMap.Cells,fx));fy=Math.Max(0,Math.Min(WorldMap.Cells,fy));
  int ix=Math.Min(WorldMap.Cells-1,(int)fx),iy=Math.Min(WorldMap.Cells-1,(int)fy);double tx=fx-ix,ty=fy-iy;
  double a=At(ix,iy),b=At(ix+1,iy),c=At(ix,iy+1),d=At(ix+1,iy+1);
  return tx+ty<=1?a+(b-a)*tx+(c-a)*ty:d+(c-d)*(1-tx)+(b-d)*(1-ty);
 }
}
}
