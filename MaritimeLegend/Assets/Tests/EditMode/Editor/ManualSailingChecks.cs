#if UNITY_EDITOR || MANUAL_SAILING_CHECKS
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend.Editor {
 public static class ManualSailingChecks {
  static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
  static void Near(double a,double b,string message)=>Check(Math.Abs(a-b)<1e-7,message);
  [MenuItem("Maritime Legend/Run manual sailing checks")]
  public static void Run(){
   Near(SkillResolver.Linear(0,20,2,8),2,"zero skill");
   Near(SkillResolver.Linear(10,20,2,8),5,"mid skill");
   Near(SkillResolver.Linear(20,20,2,8),8,"max skill");
   Near(SkillResolver.Linear(200,20,2,8),8,"clamp skill");
   Near(SkillResolver.Linear(10,20,8,2),5,"decreasing effect");
   var catalog=CatalogLoader.Load();var state=GameService.New(catalog,0,0,"休闲",StateFactory.Appearance(catalog),"Manual checks",42);
   state.pendingEvents.Clear();var game=new GameService(catalog,state);game.Depart(1);
   var nav=catalog.balance.navigation;int skill=catalog.balance.skills.navigationSkill;
   foreach(int points in new[]{0,10,20})foreach(double opening in new[]{0.0,1.0,2.0,4.0}){
    state.skillLevels[skill]=points;state.seaX=.5;state.seaY=.5;
    double expected=nav.windSpeed*opening*SkillResolver.Linear(points,catalog.skills[skill].maxLevel,nav.minSailSpeed,nav.maxSailSpeed);
    Near(game.Sail(.01,opening/4),expected,"sailing formula");
   }
   var coast=new CoastNavigation(Resources.Load<TextAsset>("Environment/IndianOcean/coast-distance").bytes);
   int shallow=0;bool testedBoundary=false;
   using(var reader=new BinaryReader(new MemoryStream(Resources.Load<TextAsset>("Environment/IndianOcean/terrain").bytes))){
    int width=reader.ReadInt32(),height=reader.ReadInt32();reader.BaseStream.Position+=width*height*4;
    for(int y=0;y<height;y++)for(int x=0;x<width;x++){
     float d=reader.ReadSingle();
     if(x<2||y<2||x>=width-2||y>=height-2)continue;
     if(d>0&&d<20){Near(coast.Distance(new Vector2(x,y)),Math.Min(d,Math.Min(Math.Min(x,y),Math.Min(width-1-x,height-1-y))),"same terrain shoreline");shallow++;}
     if(!testedBoundary&&d>1&&d<3){
      var start=new Vector2(x,y);var moved=coast.MovePixels(start,new Vector2(.05f,0));
      Check(Vector2.Distance(start,moved)>.04f,"shallow water is navigable");
      for(int i=0;i<32;i++){float a=i*Mathf.PI/16;var end=coast.MovePixels(start,new Vector2(Mathf.Cos(a),Mathf.Sin(a))*80);Check(coast.Distance(end)>=-.001f,"swept move stays outside land");}
      testedBoundary=true;
     }
    }
   }
   Check(shallow>100&&testedBoundary,"real shallow coast fixtures");
   Vector2 origin=new Vector2(.48f,.72f);var followers=SeaFleetFollowing.Spawn(origin,Math.PI);
   var target=origin+new Vector2(.02f,0);var before=new Vector2((float)followers[0].x,(float)followers[0].y);
   SeaFleetFollowing.Step(followers,target,3,1.0/60,90,coast);
   Check(Vector2.Distance(before,new Vector2((float)followers[0].x,(float)followers[0].y))<1e-6f,"opposite heading turns in place");
   Check(Math.Abs(followers[0].heading-Math.PI)<=Math.PI/120+1e-8,"bounded heading");
   for(int i=0;i<600;i++){
    before=coast.Pixels(new Vector2((float)followers[0].x,(float)followers[0].y));
    SeaFleetFollowing.Step(followers,target,3,1.0/60,90,coast);
    var after=coast.Pixels(new Vector2((float)followers[0].x,(float)followers[0].y));
    Check(Vector2.Distance(before,after)<.077f,"no teleport");
   }
   Check(followers[0].x>origin.x+.005,"follower catches up");
   for(int i=0;i<600;i++)SeaFleetFollowing.Step(followers,target,0,1.0/60,90,coast);
   Check(Vector2.Distance(coast.Pixels(target),coast.Pixels(new Vector2((float)followers[1].x,(float)followers[1].y)))<.1f,"stopped fleet gathers");
   var primary=coast.Pixels(target);
   for(int i=0;i<600;i++){
    // Restart from the gathered fleet and sail around a bend.
    float angle=i<300?0:(i-300)*Mathf.PI/600;
    primary+=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*.05f;
    before=coast.Pixels(new Vector2((float)followers[1].x,(float)followers[1].y));
    SeaFleetFollowing.Step(followers,coast.UV(primary),3,1.0/60,90,coast);
    var after=coast.Pixels(new Vector2((float)followers[1].x,(float)followers[1].y));
    Check(Vector2.Distance(before,after)<.077f,"continuous turn after restart");
    Check(coast.Distance(after)>=0,"followers remain in water");
   }
   Check(Vector2.Distance(primary,coast.Pixels(new Vector2((float)followers[1].x,(float)followers[1].y)))>3,"restart stretches fleet line");
   Check(Vector2.Distance(primary,coast.Pixels(new Vector2((float)followers[1].x,(float)followers[1].y)))<6,"compact fleet after bend");
   state.sailing.manualFollowers=followers;var restored=CatalogLoader.Copy(state);
   Near(restored.sailing.manualFollowers[0].heading,followers[0].heading,"saved follower heading");
   Near(restored.sailing.manualFollowers[1].x,followers[1].x,"saved follower position");
   SailingArchitectureChecks.Run();
   Debug.Log("MANUAL_SAILING_CHECKS_PASS: skill endpoints and midpoint, speed formula, shallow water, terrain coastline agreement, swept collisions, turn in place, continuous following, slack gathering, save roundtrip, sailing architecture.");
  }
 }
}
#endif
