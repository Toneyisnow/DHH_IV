using System;
using System.Collections.Generic;
namespace MaritimeLegend.Domain {
    [Serializable] public class SailingState {
        public int version; public string regionId,mode="normal",backgroundId="";
        // Additive regional presentation state; old saves start at a new safe Indian anchorage.
        public string manualMapId="",manualSpawnPort="";
        public float manualSpawnX,manualSpawnY,manualYaw,manualZoom=70;
        public ManualFollowerState[] manualFollowers;
        public double energy,foregroundSeconds,speed,lastDayDistance,spawnCountdown;
        public int nextEncounterId; public uint encounterSeed;
        public List<FleetDayState> fleets=new List<FleetDayState>();
        public List<SeaEncounterState> encounters=new List<SeaEncounterState>();
        public List<string> storyFlags=new List<string>(),seenChoices=new List<string>();
        public GameCalendarState calendar=new GameCalendarState();
    }
    [Serializable] public class ManualFollowerState { public double x,y,heading; }
    [Serializable] public class FleetDayState {
        public string fleetId; public long lastSettledDay; public int experience;
        public bool shortage; public double foodSpent,waterSpent,energySpent;
    }
    [Serializable] public class SeaEncounterState {
        public string instanceId,definitionId,regionId,status="Available",productId,resultKey;
        public uint seed; public double x,y,ignoreUntil,expiresAt;
        public int quality,quantity,energyCost; public bool claimed;
    }
    // Domain adapter required before automatic icon placement is enabled.
    // A real implementation must query the region's navigable-water mask/obstacle geometry.
    public interface ISeaPlacementPolicy { bool IsNavigable(string regionId,double x,double y); }
}
