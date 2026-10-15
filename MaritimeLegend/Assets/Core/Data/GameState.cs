using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    [Serializable] public class GameState {
        public int version,slot,city,home,gold,chapter,produced,trades,contracts,finalStage; public string ownerId="",name="",mode="",screen="",lastUtc=""; public uint seed;
        public Appearance appearance=new Appearance(); public Settings settings=new Settings(); public int[] skillLevels=new int[0]; public int[] skillXp=new int[0]; public List<int> startingIndustries=new List<int>();
        public Inventory cargo=new Inventory(); public List<CityState> cities=new List<CityState>(); public List<Ship> ships=new List<Ship>(); public List<PartnerState> partners=new List<PartnerState>(); public List<Order> orders=new List<Order>(); public List<AutoRoute> autoRoutes=new List<AutoRoute>(); public List<string> routes=new List<string>(),eventFlags=new List<string>(),journal=new List<string>(); public List<int> visited=new List<int>(),discovered=new List<int>();
        public bool ended,atSea,finalVoyage; public int seaFrom,seaTo; public double seaX,seaY,heading,trim,wind,windTimer,worldHours,playSeconds,npcHours; public bool materialsSubmitted,instrumentMade,sailsMade; public int contractProduct,contractQuantity; public double contractDeadline; public List<string> pendingEvents=new List<string>();
    }
}
