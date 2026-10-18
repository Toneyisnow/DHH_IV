using System;
using System.Linq;
namespace MaritimeLegend.Domain {
    public partial class GameService {
        public ISeaPlacementPolicy SeaPlacement {get;set;}
        public SeaEncounterState PromptedEncounter=>S.sailing.encounters.FirstOrDefault(x=>x.status=="Prompting");
        public bool DialogueOpen=>S.pendingEvents.Count>0||PromptedEncounter!=null;
        public bool NormalSailing=>S.atSea&&S.sailing.mode=="normal";
        public FleetDayState ManualFleet=>S.sailing.fleets.First(x=>x.fleetId=="player");

        // Additive v1 extension: older saves get the new calendar at day zero, never retroactive debt.
        void EnsureSailingState() {
            if(S.sailing==null||S.sailing.version==0) {
                S.sailing=new SailingState {version=1,energy=C.balance.sailing.maximumEnergy,
                    encounterSeed=S.seed==0?C.initial.game.seed:S.seed,
                    regionId=C.sailingDefaults.portRegions[S.city],mode="normal"};
                S.sailing.fleets.Add(new FleetDayState {fleetId="player"});
                ScheduleEncounter();
            }
            if(S.sailing.version!=1)throw new InvalidOperationException("Unsupported sailing save version");
            if(S.sailing.calendar==null||S.sailing.fleets==null||S.sailing.encounters==null||S.sailing.fleets.Count(x=>x.fleetId=="player")!=1)
                throw new InvalidOperationException("Invalid sailing save state");
            C.SeaRegion(S.sailing.regionId);
            SynchronizeDayDuration();
            foreach(var ship in S.ships)EnsureCrew(ship);
        }
        void EnsureCrew(Ship ship) {
            if(ship.sailingVersion!=0)return;
            ship.crew=C.sailingDefaults.crewByShipType[ship.type];
            ship.water=C.balance.sailing.initialWater;ship.sailingVersion=1;
        }
        public bool SetSailingMode(string mode) {
            if(mode!="normal"&&mode!="battle")throw new ArgumentException("Unknown sailing mode");
            if(mode=="battle"&&(S.mode!="经典"||DialogueOpen))return false;
            S.sailing.mode=mode;return true; // Pose and sail angle intentionally survive transitions.
        }
        public bool SetSeaRegion(string id) {
            C.SeaRegion(id);
            if(PromptedEncounter!=null)return false;
            if(S.sailing.regionId!=id){S.sailing.regionId=id;ScheduleEncounter();}
            return true;
        }
        public void SteerToward(double x,double y,double seconds) {
            if(!NormalSailing||DialogueOpen)return;
            if(double.IsNaN(x)||double.IsNaN(y)||double.IsInfinity(x)||double.IsInfinity(y))throw new ArgumentException("Invalid target");
            if(Math.Abs(x-S.seaX)+Math.Abs(y-S.seaY)<1e-9)return;
            S.heading=SailingFormulas.Turn(S.heading,Math.Atan2(y-S.seaY,x-S.seaX),C.balance.sailing.turnDegreesPerSecond,seconds);
        }
        public void AdjustSail(double axis,double seconds,bool reset=false) {
            GameCalendar.ValidateSeconds(seconds);
            if(!NormalSailing||DialogueOpen)return;
            double degrees=reset?C.balance.sailing.defaultSailDegrees:SailingFormulas.SailDegrees(S.trim)+Clamp(axis,-1,1)*C.balance.sailing.sailDegreesPerSecond*seconds;
            S.trim=(Clamp(degrees,-90,90)+90)/180;
        }
        internal void AdvanceCalendar(double realSeconds) {
            GameCalendar.ValidateSeconds(realSeconds);
            SynchronizeDayDuration();
            var clock=S.sailing.calendar;var p=C.balance.sailing;
            // Segment at every day boundary; long frames cannot skip a settlement.
            while(realSeconds>0) {
                double step=Math.Min(realSeconds,p.realSecondsPerGameDay-clock.secondsIntoDay);
                clock.secondsIntoDay+=step;realSeconds-=step;
                if(NormalSailing)AdvanceSeaEncounters(step);
                if(clock.secondsIntoDay+1e-9<p.realSecondsPerGameDay)break;
                clock.secondsIntoDay=0;clock.elapsedGameDays++;
                SettleManualFleet(clock.elapsedGameDays);
                clock.lastSettledDay=clock.elapsedGameDays;
            }
        }
        void SynchronizeDayDuration() {
            var clock=S.sailing.calendar;double duration=C.balance.sailing.realSecondsPerGameDay;
            GameCalendar.ValidateSeconds(clock.secondsIntoDay);
            if(clock.elapsedGameDays<0||clock.lastSettledDay>clock.elapsedGameDays)throw new InvalidOperationException("Invalid saved calendar");
            if(clock.dayDurationSeconds<=0)clock.dayDurationSeconds=duration;
            if(clock.secondsIntoDay>=clock.dayDurationSeconds)throw new InvalidOperationException("Invalid saved day fraction");
            // Changing difficulty/balance preserves the fraction of the current day, not debt.
            clock.secondsIntoDay=clock.secondsIntoDay/clock.dayDurationSeconds*duration;
            clock.dayDurationSeconds=duration;
        }
        void SettleManualFleet(long day) {
            var fleet=ManualFleet;if(day<=fleet.lastSettledDay)return;
            fleet.foodSpent=fleet.waterSpent=fleet.energySpent=0;
            // Dock and battle days do not consume provisions in this mock policy.
            if(NormalSailing) {
                var ship=S.ships[0];EnsureCrew(ship);
                int nav=S.skillLevels[C.balance.skills.navigationSkill],vision=S.skillLevels[C.balance.skills.visionSkill];
                int supply=S.skillLevels[Array.FindIndex(C.skills,x=>x.id==C.sailingDefaults.supplySkillId)];
                var cost=SailingFormulas.Daily(C.balance.sailing,ship.crew,nav,supply,vision);
                bool enough=ship.food>=cost.food&&ship.water>=cost.water&&S.sailing.energy>=cost.energy;
                fleet.foodSpent=Math.Min(ship.food,cost.food);fleet.waterSpent=Math.Min(ship.water,cost.water);fleet.energySpent=Math.Min(S.sailing.energy,cost.energy);
                ship.food-=fleet.foodSpent;ship.water-=fleet.waterSpent;S.sailing.energy-=fleet.energySpent;
                fleet.shortage=!enough||ship.food<=0||ship.water<=0||S.sailing.energy<=0;
                if(!fleet.shortage&&S.sailing.lastDayDistance>=C.balance.sailing.minimumDailyDistance) {
                    fleet.experience+=cost.experience;
                    Gain(C.balance.skills.navigationSkill,cost.experience);
                }
            }
            S.sailing.lastDayDistance=0;fleet.lastSettledDay=day;
        }
        double ShortageSpeedFactor() {
            var ship=S.ships[0];
            return ship.food<=0||ship.water<=0||S.sailing.energy<=0?C.balance.sailing.shortageSpeedFactor:1;
        }
        int ExtraSupplyCost()=> (int)Math.Ceiling(Math.Max(0,C.balance.sailing.initialWater-S.ships[0].water)*C.balance.sailing.waterUnitPrice+
            Math.Max(0,C.balance.sailing.maximumEnergy-S.sailing.energy)*C.balance.sailing.energyUnitPrice);
        void RestoreSailingSupplies() {
            S.ships[0].water=C.balance.sailing.initialWater;S.sailing.energy=C.balance.sailing.maximumEnergy;ManualFleet.shortage=false;
        }
    }
}
