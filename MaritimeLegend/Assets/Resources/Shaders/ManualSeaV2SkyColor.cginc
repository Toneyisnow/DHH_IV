#ifndef SEA_V2_SKY_COLOR
#define SEA_V2_SKY_COLOR
 float seaSkyHash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
 float3 seaSkyColor(float3 direction,float mode,float3 sunDirection,float starDensity,float starBrightness){
  float3 d=normalize(direction);float elevation=saturate(d.y);float alignment=dot(d,normalize(sunDirection));
  float3 c;
  if(mode<.5){c=lerp(float3(.55,.82,1),float3(.025,.30,.85),(pow(elevation,.45)*smoothstep(0,.025,elevation)));c+=float3(1,.85,.6)*pow(saturate(alignment),900)*2;}
  else if(mode<1.5){
   c=lerp(float3(.98,.36,.12),float3(.15,.12,.36),(pow(elevation,.45)*smoothstep(0,.025,elevation)));
   float band=sin(d.x*24+d.z*13+sin(d.z*27)*.8)*sin(d.y*95+d.x*7);
   float cloud=smoothstep(.1,.7,band)*exp(-pow((d.y-.18)*4,2));c=lerp(c,float3(.89,.24,.19),cloud*.42);
   c+=float3(1,.45,.08)*pow(saturate(alignment),35)*.55;
   float disk=smoothstep(.9990,.99925,alignment);c=lerp(c,float3(1,.62,.16),disk);
  } else {
   c=lerp(float3(.045,.075,.16),float3(.004,.009,.035),(pow(elevation,.4)*smoothstep(0,.025,elevation)));
   float3 cell=floor(d*650);float stars=step(1-.004*starDensity,seaSkyHash(cell))*pow(saturate(1-length(frac(d*650)-.5)*1.4),3);
   c+=stars*starBrightness*(.65+.35*sin(_Time.y*.6+seaSkyHash(cell)*20))*float3(.8,.9,1.0)*smoothstep(0,.15,d.y);
   c+=float3(.18,.26,.45)*pow(saturate(alignment),100)*.35;
   float moon=smoothstep(.99915,.99935,alignment);float craters=.83+.17*seaSkyHash(floor(d*550));c=lerp(c,float3(.89,.94,1)*craters,moon);
  }
  #ifndef UNITY_COLORSPACE_GAMMA
  c=GammaToLinearSpace(c);
  #endif
  return c;
 }
#endif
