using System;
namespace MaritimeLegend.Domain {
[Serializable] public class BalanceConfig { public InterfaceParameters ui;
public SailingParameters sailing;
public WorldParameters world;
public EconomyParameters economy;
public ProductionParameters production;
public SkillsParameters skills;
public QuestsParameters quests;
public UpgradesParameters upgrades;
public CombatParameters combat;
public FleetParameters fleet;
public QualityParameters quality;
public NavigationParameters navigation;
public EventsParameters events;
public PartnerRolesParameters partnerRoles;
}
[Serializable] public class WorldParameters {
public int secondsPerHour;
public double visionRadius;
public double visionPerLevel;
public double scoutVisionBonus;
public double navigatorVisionBonus;
public double foodPerSecond;
public double cookFoodFactor;
public double fedHullDecay;
public double starvingHullDecay;
public int windIntervalMin;
public int windIntervalRange;
public int hoursPerDay;
public int maxCommandsPerTick;
public int discoveryExperience;
}
[Serializable] public class EconomyParameters {
public int minimumRawSupplyPorts;

public double localPriceFactor;
public double foreignPriceFactor;
public int specialtyBands;
public double specialtyBandStep;
public int maxTradeQuantity;
public int unitsPerReputation;
public int maxTradeExperience;
public int shortageThreshold;
public int rawHourlySupply;
public int foreignRawHourlyConsumption;
public int processedStockTarget;
public int processedSupplyQuality;
public int processedHourlySupply;
public int contractProductStep;
public int contractHours;
public double contractPriceFactor;
public double contractPartnerFactor;
public int contractReputation;
public int initialSecondaryQuality;
}
[Serializable] public class ProductionParameters {
public double partnerFeeFactor;
public int offlineMinimumSeconds;
public int localMinimumSeconds;
public int awayMinimumSeconds;
public double speedPerLevel;
public double partnerDurationFactor;
public int completionExperience;
public double cancelRefundFactor;
public int offlineMaxSeconds;
public int maxEffectiveLevel;
public int partnerTrustDivisor;
public int chemistIndustry;
public int minimumEffectiveLevel;
}
[Serializable] public class SkillsParameters {
public int productionSkill;
public int gunnerySkill;
public int experiencePerLevel;
public int maxTrust;
public int legendaryPreviousTrust;
public int legendaryChapter;
public int trainingPricePerTrust;
public int navigationSkill;
public int reputationSkill;
public int visionSkill;
public int tradeSkill;
}
[Serializable] public class QuestsParameters {
public int firstChapter;
public int secondChapter;
public int finalChapter;
public int startingShipType;
public int materialQuantity;

public int instrumentQuality;
public int chapterTwoReward;
public int chapterThreeReward;
public int licensedCitiesRequired;
public int productionOrdersRequired;
public int materialQuality;
public int finalRoutesRequired;
public int finalPartnerTrust;
public int finalPort;
public int completedStage;
public int endingReward;
}
[Serializable] public class UpgradesParameters {
public int warehousePrice;
public int warehouseCapacityStep;
public int shipMaxLevel;
public int shipPrice;
public int licenseBasePrice;
public int licensePricePerCity;
public double diplomatLicenseFactor;
public int licenseMaxReputation;
public int licenseReputationPerCity;
}
[Serializable] public class CombatParameters {
public int finalReward;
public int practiceReward;
public int victoryExperience;
public float enemyFinalHull;
public float enemyPracticeHull;
public float enemyInitialSails;
public float enemyInitialCooldown;
public float turnSpeed;
public float playerSpeed;
public float brakeFactor;
public float enemyTurnRange;
public float enemyTurnSpeed;
public float enemyDamagedSailsThreshold;
public float enemySpeed;
public float enemyDamagedSpeed;
public float enemyRange;
public float guardDamage;
public float enemyDamage;
public float enemyCooldown;
public float broadsideDotLimit;
public float playerRange;
public float gunnerCooldown;
public float playerCooldown;
public int baseDamage;
public int damagePerGun;
public int damagePerShipLevel;
}
[Serializable] public class FleetParameters {
public double hullRepairPrice;
public double repairPartnerFactor;
public int autoMinimumHull;
public int autoMinimumFood;
public int autoSupplyFee;
public int autoLegSeconds;
public double autoFoodPerSecond;
}
[Serializable] public class QualityParameters {
public double minimumLambda;
public double baseLambda;
public double lambdaPerLevel;
public int levelOrigin;
}
[Serializable] public class NavigationParameters {
public int minimumDepartureHull;
public int minimumDepartureFood;
public double minimumWindFactor;
public double baseWindFactor;
public double windDotWeight;
public double trimPenalty;
public double optimalTrim;
public double headwindThreshold;
public double navigatorHeadwindFactor;
public double baseSpeed;
public double minSailSpeed,maxSailSpeed,windSpeed;
public double minimumHullSpeedFactor;
public double hullSpeedWeight;
public double cargoSpeedPenalty;
public double mapMinX;
public double mapMaxX;
public double mapMinY;
public double mapMaxY;
public double arrivalRadius;
public int routeExperience;
public int partnerArrivalExperience;
public int rescueFee;
public int rescueHull;
public int rescueFood;
}
[Serializable] public class EventsParameters {
public int journalLimit;
public int supportPrice;
public int supportReputation;
public int supportExperience;
}
[Serializable] public class PartnerRolesParameters {
public int merchant;
public int carpenter;
public int headwind;
public int diplomat;
public int craftsman;
public int chemist;
public int scout;
public int navigator;
public int cook;
public int gunner; public int guard;
}
}

namespace MaritimeLegend.Domain { [System.Serializable] public class InterfaceParameters {
public float autosaveSeconds;
public float characterSpeed;
public float walkArrivalDistance;
public double steeringRate;
public float minimumThrottle;
public double trimRate;
public double maximumFrameStep;
public float notificationSeconds;
public float dialogueCharactersPerSecond;
public float brightnessMinimum;
public float textSpeedMinimum;
public float textSpeedMaximum;
public float movementMinimum;
public float movementMaximum;
public float battleMinimum;
public float battleMaximum;
public int marketQuantityLimit;
public int nameLength;
public int autoBudgetStep;
public int autoBudgetLimit;
public int autoLoopLimit;
public int autoPriceLimit;
}}
