using System;
namespace MaritimeLegend.Domain {
    public struct DailySailingCost { public double food,water,energy; public int experience; }
    public static class SailingFormulas {
        // Match the existing sail instrument's direction in regional map space.
        public static double WindFill(double heading,double trim,double wind) {
            double forward=Math.Atan2(Math.Sin(heading)*941,Math.Cos(heading)*1672);
            double windDirection=Math.Atan2(Math.Sin(wind)*941,Math.Cos(wind)*1672);
            return Math.Max(0,Math.Min(1,Math.Cos(windDirection-forward-SailDegrees(trim)*Math.PI/180)));
        }
        public static double SailPower(double opening,double heading,double trim,double wind) =>
            Math.Max(0,Math.Min(1,opening))*(.02+1.98*WindFill(heading,trim,wind));
        public static DailySailingCost Daily(SailingParameters p,int crew,int navigation,int supply,int vision,double weatherRisk=0,double exploration=0) {
            if(crew<0||navigation<0||supply<0||vision<0)throw new ArgumentOutOfRangeException("skills/crew");
            double factor=p.logisticsFactor*Math.Max(p.minimumSupplyFactor,1-supply*p.supplyReductionPerLevel);
            return new DailySailingCost {
                food=crew*p.foodPerCrewDay*factor,water=crew*p.waterPerCrewDay*factor,
                energy=Math.Max(p.minimumEnergyPerDay,p.energyPerDay-navigation*p.navigationEnergyReduction-supply*p.supplyEnergyReduction+weatherRisk),
                experience=(int)Math.Floor(p.baseDailyXp+navigation*p.xpPerNavigationLevel+vision*p.xpPerVisionLevel+exploration)
            };
        }
        public static double Turn(double currentRadians,double targetRadians,double degreesPerSecond,double seconds) {
            GameCalendar.ValidateSeconds(seconds);
            double delta=Math.Atan2(Math.Sin(targetRadians-currentRadians),Math.Cos(targetRadians-currentRadians));
            double step=degreesPerSecond*Math.PI/180*seconds;
            return currentRadians+Math.Max(-step,Math.Min(step,delta));
        }
        public static double HeadingDegrees(double radians)=>((radians*180/Math.PI+90)%360+360)%360;
        public static double SailDegrees(double legacyTrim)=>legacyTrim*180-90;
        public static double NextEncounterSeconds(double ratePerDay,double secondsPerDay,ref uint seed) {
            return ratePerDay<=0?double.MaxValue:-Math.Log(Math.Max(1e-9,1-QualityRoller.Random(ref seed)))*secondsPerDay/ratePerDay;
        }
    }
}
