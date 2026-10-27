Shader "Maritime/ManualSeaV2/Sky" {
 Properties {_Mode("0 sunny 1 sunset 2 night",Float)=0 _SunDirection("Celestial direction",Vector)=(0,.14,.99,0) _StarDensity("Star count multiplier",Range(1,6))=3 _StarBrightness("Star brightness multiplier",Range(1,8))=4}
 SubShader {Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"} Cull Off ZWrite Off
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 float _Mode,_StarDensity,_StarBrightness;float4 _SunDirection;
 struct v2f{float4 pos:SV_POSITION;float3 dir:TEXCOORD0;};
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.dir=v.vertex.xyz;return o;}
 float hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
 fixed4 frag(v2f i):SV_Target{
  float3 d=normalize(i.dir);float elevation=saturate(d.y);float alignment=dot(d,normalize(_SunDirection.xyz));
  float3 c;
  if(_Mode<.5){c=lerp(float3(.55,.82,1),float3(.025,.30,.85),pow(elevation,.45));c+=float3(1,.85,.6)*pow(saturate(alignment),900)*2;}
  else if(_Mode<1.5){
   c=lerp(float3(.98,.36,.12),float3(.15,.12,.36),pow(elevation,.45));
   float band=sin(d.x*24+d.z*13+sin(d.z*27)*.8)*sin(d.y*95+d.x*7);
   float cloud=smoothstep(.1,.7,band)*exp(-pow((d.y-.18)*4,2));c=lerp(c,float3(.89,.24,.19),cloud*.42);
   c+=float3(1,.45,.08)*pow(saturate(alignment),35)*.55;
   float disk=smoothstep(.9990,.99925,alignment);c=lerp(c,float3(1,.62,.16),disk);
  } else {
   c=lerp(float3(.045,.075,.16),float3(.004,.009,.035),pow(elevation,.4));
   float3 cell=floor(d*650);float stars=step(1-.004*_StarDensity,hash(cell))*pow(saturate(1-length(frac(d*650)-.5)*1.4),3);
   c+=stars*_StarBrightness*(.65+.35*sin(_Time.y*.6+hash(cell)*20))*float3(.8,.9,1.0)*smoothstep(0,.15,d.y);
   c+=float3(.18,.26,.45)*pow(saturate(alignment),100)*.35;
   float moon=smoothstep(.99915,.99935,alignment);float craters=.83+.17*hash(floor(d*550));c=lerp(c,float3(.89,.94,1)*craters,moon);
  }
  #ifndef UNITY_COLORSPACE_GAMMA
  c=GammaToLinearSpace(c);
  #endif
  return float4(c,1);
 }
 ENDCG}
 } Fallback Off
}
