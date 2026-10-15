using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    public class QualityRoller : IQualityRoller {
        readonly Catalog C; public QualityRoller(Catalog catalog){C=catalog;}
        public static double Random(ref uint seed){seed^=seed<<13;seed^=seed>>17;seed^=seed<<5;if(seed==0)throw new ArgumentException("PRNG seed must be nonzero");return seed/(double)uint.MaxValue;}
        public int Roll(int worst,int level,double difficulty,ref uint seed){double lambda=Math.Max(C.balance.quality.minimumLambda,C.balance.quality.baseLambda-C.balance.quality.lambdaPerLevel*(level-C.balance.quality.levelOrigin)+difficulty),total=0;for(int q=0;q<=worst;q++)total+=Math.Exp(-lambda*(worst-q));double v=Random(ref seed)*total;for(int q=0;q<=worst;q++){v-=Math.Exp(-lambda*(worst-q));if(v<=0)return q;}return worst;}
    }
}
