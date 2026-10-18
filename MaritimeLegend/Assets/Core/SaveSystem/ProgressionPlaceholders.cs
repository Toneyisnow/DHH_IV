using System;
using System.Collections.Generic;
namespace MaritimeLegend.Domain {
    // Reserved extension contracts. No fabricated measurements and no automatic unlocks.
    [Serializable] public class ProgressMetrics {
        public double activePlaySeconds,offlineSeconds,manualSailSeconds;
        public int productionQueueCount,tradeCount;
        public string blockingReason;
        public List<ChapterTiming> chapters=new List<ChapterTiming>();
    }
    [Serializable] public class ChapterTiming {
        public int chapter; public double chapterStart,chapterEnd; public bool completed;
    }
    [Serializable] public class EventChainProgress {
        public string eventId,choiceId,exclusiveGroup,followupId;
        public int saveVersion; public uint seed; public double cooldownUntil;
        public bool resolved;
    }
    // Separate account/profile file contract, deliberately not embedded in GameState.
    // Future profile storage can use UserDataPaths + AtomicJsonFile; never copy economic state.
    [Serializable] public class ReplayProfile {
        public int version=1;
        public List<string> seenBranches=new List<string>(),appearanceUnlocks=new List<string>(),achievements=new List<string>();
    }
}
