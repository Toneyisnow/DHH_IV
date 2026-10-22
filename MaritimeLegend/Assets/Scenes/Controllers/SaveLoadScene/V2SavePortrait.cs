using System;
using UnityEngine;
using MaritimeLegend.Domain;

namespace MaritimeLegend {
 // Complete illustrated presets from the approved V2 boards, never block avatars.
 public static class V2SavePortrait {
  static readonly float[] ColorTop={43,35,38,43,41,35,45,33};
  static readonly float[] ColorBelt={198,174,192,198,204,194,203,191};
  static readonly float[] ColorCenter={116,116,131,136,125,128,127,120};
  static readonly float[] ColorStep={188,188,187,184,186,186,186,187};
  static readonly float[] MantleTop={628,614,610,638,625,648,642,648};
  static readonly float[] MantleBelt={780,754,785,790,791,790,791,790};
  public static Texture2D Create(GameState state){
   return CreatePreset(V2PortraitPreset.Resolve(state.appearance));
  }
  public static Texture2D CreatePreset(string key){
   if(string.IsNullOrEmpty(key))return null;
   var parts=key.Split('-');
   if(parts.Length!=2||!int.TryParse(parts[0],out int character)||character<1||character>8||parts[1].Length<2||!int.TryParse(parts[1].Substring(1),out int option))return null;
   char group=parts[1][0];int limit=(group=='C'||group=='H')?8:(group=='M'||group=='A')?6:0;
   if(option<1||option>limit)return null;
   var source=Resources.Load<Texture2D>("Sprites/Characters/Character"+character.ToString("00")+"/SavePortraitV2/"+(group=='A'?"Accessories":"Wardrobe"));
   if(source==null){Debug.LogError("Missing V2 portrait board: "+key);return null;}
   int index=character-1;Rect crop;
   var waist=group=='H'?Resources.Load<Texture2D>("Sprites/Characters/Character"+character.ToString("00")+"/SavePortraitV2/Waist-H"+option):null;
   if(waist!=null){
    source=waist;
    // Waist landmarks for the corrected V2 identity-preserving costume edits.
    float top=character==1?.03f:0f,bottom=character==1?.91f:.98f;
    float spanY=(bottom-top)*source.height,spanX=spanY*376f/348f;
    crop=new Rect((source.width-spanX)/2,top*source.height,spanX,spanY);
   }else{
    // H-only design rows have no torso. Until a matching waist asset is authored,
    // use that character's complete C1 figure rather than mix shoulder/waist crops.
    if(group=='H'){group='C';option=1;}
    float center,top,bottom;
    if(group=='C'){
     center=ColorCenter[index]+(option-1)*ColorStep[index];top=ColorTop[index];bottom=ColorBelt[index];
    }else if(group=='M'){
     center=144+(option-1)*250;top=MantleTop[index];bottom=MantleBelt[index];
    }else{
     center=370+((option-1)%3)*397;top=option<=3?38:495;bottom=top+184;
    }
    float cropHeight=bottom-top,cropWidth=cropHeight*376f/348f;
    crop=new Rect(center-cropWidth/2,top,cropWidth,cropHeight);
    // Board coordinates are authored at 1536 x 1024.
    crop=new Rect(crop.x*source.width/1536f,crop.y*source.height/1024f,crop.width*source.width/1536f,crop.height*source.height/1024f);
   }
   const int width=376,height=348;
   var pixels=new Color[width*height];
   for(int y=0;y<height;y++)for(int x=0;x<width;x++){
    float sx=crop.x+(x+.5f)*crop.width/width,sy=crop.y+(y+.5f)*crop.height/height;
    Color pixel=source.GetPixelBilinear(sx/source.width,1-sy/source.height);
    // M3 is a board annotation in empty paper, left of the character's silhouette.
    // Sample the adjacent clean paper only here; preserve the figure and crop exactly.
    if(character==5&&group=='M'&&option==3){
     float boardX=sx*1536f/source.width,boardY=sy*1024f/source.height;
     if(boardX>=563&&boardX<599&&boardY>=651&&boardY<675){
      float edge=Mathf.Min(Mathf.Min(boardX-563,599-boardX),Mathf.Min(boardY-651,675-boardY));
      Color paper=source.GetPixelBilinear(sx/source.width,1-(boardY-27)/1024f);
      pixel=Color.Lerp(pixel,paper,Mathf.Clamp01(edge/2));
     }
    }
    pixels[(height-1-y)*width+x]=pixel;
   }
   var result=new Texture2D(width,height,TextureFormat.RGBA32,false){name="V2 portrait "+key,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
   result.SetPixels(pixels);result.Apply(false,false);return result;
  }
 }
}
