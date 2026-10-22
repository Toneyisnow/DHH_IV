using System;
namespace MaritimeLegend.Domain {
 public static class V2PortraitPreset {
  // Explicit legacy mapping; West Asian female preview approved as Asian female by user.
  static readonly int[] LegacyCharacters={3,4,1,2,0,4,5,6,0,0};
  public static string Resolve(Appearance appearance){
   if(appearance==null)return "";
   if(!string.IsNullOrEmpty(appearance.portraitV2))return appearance.portraitV2;
   int old=appearance.baseId;
   if(old<0||old>=LegacyCharacters.Length||LegacyCharacters[old]==0)return "";
   int character=LegacyCharacters[old];
   bool Has(int index)=>appearance.accessoryIds!=null&&index<appearance.accessoryIds.Length&&appearance.accessoryIds[index]>0;
   // Legacy flags describe only broad styles, not authored V2 outfit combinations.
   // Select a complete visible preset rather than paste incompatible drawings together.
   string variant=Has(2)?"M"+(Math.Abs(appearance.outfitId%6)+1):Has(0)?"H"+(Math.Abs(appearance.hairId%5)+1):"C"+(Math.Abs(appearance.primaryColor%8)+1);
   return character.ToString("00")+"-"+variant;
  }
 }
}
