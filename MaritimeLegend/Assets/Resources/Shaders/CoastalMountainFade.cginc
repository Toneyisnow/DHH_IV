#ifndef COASTAL_MOUNTAIN_FADE
#define COASTAL_MOUNTAIN_FADE
sampler2D _Shore;
float _MountainFade, _Clay, _FadeHeight;
sampler2D _TerrainData;float4 _TerrainRect;float _UseTerrainMap,_TerrainBase;
float coastalGroundHeight(float2 p){
 if(_UseTerrainMap>.5)return tex2Dlod(_TerrainData,float4((p-_TerrainRect.xy)/_TerrainRect.zw,0,0)).r+_TerrainBase;
 float d=max(0,p.y-tex2Dlod(_Shore,float4((p.x+100)/200,.5,0,0)).r);
 float width=5.5+1.1*sin(p.x*.19)+.7*sin(p.x*.43+1);
 float t=saturate((d-width)/(5.5+1.5*sin(p.x*.17)));float ramp=t*t*(3-2*t);
 float h=.025*min(d,width)+(1+.25*sin(p.x*.16)*cos(p.y*.13))*ramp;
 h+=(.13*sin(p.x*.31+p.y*.12)+.09*sin(p.y*.48-p.x*.21))*ramp;
 if(p.y>5){float river=3+sin(p.y*.19)*2.3+sin(p.y*.41)*.5;
  float channel=saturate(abs(p.x-river)-(1.15+.25*sin(p.y*.3)));h=-.24+(h+.24)*channel;}
 return h;
}
float mountainOpacity(float3 world){
 if(_MountainFade<.5||_Clay>.5)return 1;
 return smoothstep(.02,max(.03,_FadeHeight),world.y-coastalGroundHeight(world.xz));
}
#endif
