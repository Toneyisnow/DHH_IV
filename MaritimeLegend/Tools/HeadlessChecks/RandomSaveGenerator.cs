using System;
using System.IO;
using System.Linq;
using MaritimeLegend;
using MaritimeLegend.Domain;

// Uses the production initializer and save checksum, with the headless JSON adapter.
static class RandomSaveGenerator {
 public static void Run(string[] args){
  if(args.Length!=5)throw new ArgumentException("--generate-saves output-directory slot slot slot (zero based)");
  var slots=args.Skip(2).Select(int.Parse).ToArray();
  if(slots.Distinct().Count()!=3||slots.Any(x=>x<0||x>=7))throw new ArgumentException("Three distinct slots from 0 to 6 required");
  SaveStore.TestDirectory=Path.GetFullPath(args[1]);
  if(slots.Any(x=>File.Exists(SaveStore.PathFor(x))||File.Exists(SaveStore.PathFor(x,true))))throw new IOException("Destination slots must be empty");
  var c=CatalogLoader.Load();var random=new Random();
  var homes=Enumerable.Range(0,c.ports.Length).OrderBy(_=>random.Next()).Take(3).ToArray();
  var bases=Enumerable.Range(0,10).OrderBy(_=>random.Next()).Take(3).ToArray();
  var events=c.events.Where(e=>e.trigger!="home"&&e.trigger!="final"&&e.prerequisites.Length==0).OrderBy(_=>random.Next()).Take(3).ToArray();
  string[] names={"林星澜","艾琳·维斯","萨米尔·纳吉"};
  for(int i=0;i<3;i++){
   var look=StateFactory.Appearance(c);
   look.baseId=bases[i];look.skinId=random.Next(6);look.hairId=random.Next(8);look.hairColor=random.Next(6);
   look.outfitId=random.Next(8);look.primaryColor=(i*3+random.Next(2))%8;look.secondaryColor=random.Next(8);
   look.accessoryIds=Enumerable.Range(0,5).Select(_=>random.Next(2)).ToArray();
   var state=GameService.New(c,slots[i],homes[i],random.Next(2)==0?"休闲":"经典",look,names[i],(uint)random.NextInt64(1,uint.MaxValue));
   new GameService(c,state).Emit(events[i].trigger);
   state.gold=99999999;state.screen="城市";
   SaveStore.Save(state);
   var loaded=SaveStore.Load(slots[i]);
   if(loaded.gold!=99999999||loaded.city!=homes[i]||loaded.appearance.baseId!=bases[i]||loaded.ships.Count!=1||!loaded.pendingEvents.Contains(events[i].id))throw new Exception("Save round trip failed");
   Console.WriteLine($"槽位 {slots[i]+1} | {loaded.name} | {c.ports[homes[i]].name} | {loaded.mode} | 外观模板 {bases[i]} | {events[i].title} | 金币 {loaded.gold}");
  }
 }
}
