namespace MaritimeLegend.Domain {
 // Optional runtime policy. Saves and automatic trade routes remain independent of scene geometry.
 public interface ISeaNavigationPolicy {void ResolveMove(double fromX,double fromY,double toX,double toY,out double x,out double y);}
 public partial class GameService {public ISeaNavigationPolicy SeaNavigation {get;set;}}
}
