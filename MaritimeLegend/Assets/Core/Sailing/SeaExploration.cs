using System;
namespace MaritimeLegend.Domain {
 // Axial coordinates are stable world coordinates, independent of camera and render resolution.
 [Serializable] public sealed class SeaExploration {
  public const int Width=96, Height=64, MinQ=-48, MinR=-32;
  public const float Radius=12;
  public string mapId="southeast_asia_v2";
  public int[] tiles=new int[Width*Height]; // 0 unknown, 1 discovered, 2 explored; never demote.
  public static void Hex(double x,double z,out int q,out int r){
   double a=(Math.Sqrt(3)/3*x-z/3)/Radius,b=2*z/(3*Radius),c=-a-b;
   int ia=(int)Math.Round(a),ib=(int)Math.Round(b),ic=(int)Math.Round(c);
   double da=Math.Abs(ia-a),db=Math.Abs(ib-b),dc=Math.Abs(ic-c);
   if(da>db&&da>dc)ia=-ib-ic;else if(db>dc)ib=-ia-ic;q=ia;r=ib;
  }
  public int At(int q,int r){int x=q-MinQ,y=r-MinR;return x<0||y<0||x>=Width||y>=Height?0:tiles[y*Width+x];}
  public const double TileSpacing=20.78460969082653;
  public bool Reveal(double x,double z,double sight=72){
   if(tiles==null||tiles.Length!=Width*Height)throw new InvalidOperationException("Unsupported exploration grid; preserve the save for migration.");
   if(double.IsNaN(sight)||double.IsInfinity(sight)||sight<=0||sight>1000)throw new ArgumentOutOfRangeException(nameof(sight));
   Hex(x,z,out int q,out int r);bool changed=false;
   double outer=sight+TileSpacing;int extent=(int)Math.Ceiling(outer/(Radius*1.5))+2;
   // World-space circle around the actual ship, not hex Manhattan distance or the camera.
   // Hexes remain the persistent units; the partially revealed band is one tile spacing wide.
   for(int row=Math.Max(MinR,r-extent);row<Math.Min(MinR+Height,r+extent+1);row++)
   for(int col=Math.Max(MinQ,q-extent);col<Math.Min(MinQ+Width,q+extent+1);col++){
    double dx=TileSpacing*(col+row*.5)-x,dz=Radius*1.5*row-z,d2=dx*dx+dz*dz;
    if(d2>outer*outer)continue;
    int index=(row-MinR)*Width+col-MinQ,target=d2<=sight*sight?2:1;
    while(tiles[index]<target){tiles[index]++;changed=true;}
   }return changed;
  }
 }
}
