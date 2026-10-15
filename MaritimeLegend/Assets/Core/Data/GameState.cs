using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    [Serializable] public class GameState {
        public int version=1,slot,city,home,gold=99999999,chapter=1,produced,trades,contracts,finalStage; public string ownerId="",name="海岚",mode="休闲",screen="城市",lastUtc=""; public uint seed=71527;
        public Appearance appearance=new Appearance(); public Settings settings=new Settings(); public int[] skillLevels=new int[12]; public int[] skillXp=new int[12]; public List<int> startingIndustries=new List<int>();
        public Inventory cargo=new Inventory{capacity=80}; public List<CityState> cities=new List<CityState>(); public List<Ship> ships=new List<Ship>(); public List<PartnerState> partners=new List<PartnerState>(); public List<Order> orders=new List<Order>(); public List<AutoRoute> autoRoutes=new List<AutoRoute>(); public List<string> routes=new List<string>(),eventFlags=new List<string>(),journal=new List<string>(); public List<int> visited=new List<int>(),discovered=new List<int>();
        public bool ended,atSea,finalVoyage; public int seaFrom,seaTo; public double seaX,seaY,heading,trim=.5,wind,windTimer=90,worldHours,playSeconds,npcHours; public bool materialsSubmitted,instrumentMade,sailsMade; public int contractProduct,contractQuantity=3; public double contractDeadline=24; public List<string> pendingEvents=new List<string>();
    }
}
