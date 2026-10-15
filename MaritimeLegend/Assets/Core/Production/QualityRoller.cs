using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    public class QualityRoller : IQualityRoller {
        public static double Random(ref uint seed){seed^=seed<<13;seed^=seed>>17;seed^=seed<<5;if(seed==0)seed=71527;return seed/(double)uint.MaxValue;}
        public int Roll(int worst,int level,double difficulty,ref uint seed){double lambda=Math.Max(.55,2.3-.065*(level-1)+difficulty),total=0;for(int q=0;q<=worst;q++)total+=Math.Exp(-lambda*(worst-q));double v=Random(ref seed)*total;for(int q=0;q<=worst;q++){v-=Math.Exp(-lambda*(worst-q));if(v<=0)return q;}return worst;}
    }
}
