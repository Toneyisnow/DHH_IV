using System;
namespace MaritimeLegend.Domain {
[Serializable] public class InitialData {
 public GameState game; public Settings settings; public Ship ship; public AutoRoute route;
 public PartnerState partner; public Appearance appearance; public Inventory warehouse; public CreationDefaults creation;
}
[Serializable] public class CreationDefaults {
 public int startingIndustryCount,startingSkillLevel,startingFactoryLevel,starterRecipeLevel,starterRecipeCount,starterMaterialBatches,starterMaterialQuality;
 public int[] startingSkillIndices;
}
public static class StateFactory {
 public static GameState Game(Catalog c){var s=CatalogLoader.Copy(c.initial.game);s.settings=Settings(c);s.appearance=Appearance(c);s.skillLevels=new int[c.skills.Length];s.skillXp=new int[c.skills.Length];
 s.cargo=Warehouse(c);s.cargo.capacity=c.initial.game.cargo.capacity;
 s.startingIndustries=new System.Collections.Generic.List<int>();s.cities=new System.Collections.Generic.List<CityState>();s.ships=new System.Collections.Generic.List<Ship>();s.partners=new System.Collections.Generic.List<PartnerState>();s.orders=new System.Collections.Generic.List<Order>();s.autoRoutes=new System.Collections.Generic.List<AutoRoute>();
 s.routes=new System.Collections.Generic.List<string>();s.eventFlags=new System.Collections.Generic.List<string>();s.journal=new System.Collections.Generic.List<string>();s.visited=new System.Collections.Generic.List<int>();s.discovered=new System.Collections.Generic.List<int>();s.pendingEvents=new System.Collections.Generic.List<string>();return s;}
 public static Settings Settings(Catalog c)=>CatalogLoader.Copy(c.initial.settings);
 public static Appearance Appearance(Catalog c)=>CatalogLoader.Copy(c.initial.appearance);
 public static Inventory Warehouse(Catalog c)=>new Inventory{capacity=c.initial.warehouse.capacity};
 public static PartnerState Partner(Catalog c)=>CatalogLoader.Copy(c.initial.partner);
 public static AutoRoute Route(Catalog c){var route=CatalogLoader.Copy(c.initial.route);route.cargo=new Inventory();return route;}
 public static Ship Ship(Catalog c,string owner){var ship=CatalogLoader.Copy(c.initial.ship);ship.id=Guid.NewGuid().ToString("N");ship.ownerId=owner;return ship;}
}
}

namespace MaritimeLegend.Domain {[System.Serializable] public class ObjectiveProducts {public string materialOne,materialTwo,instrument,sails;}}
