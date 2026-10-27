Shader "Maritime/ManualSeaV2/Water" {
 Properties {
 _DepthMap("Bathymetry",2D)="black"{} _MapBounds("Map min and size",Vector)=(-320,-240,640,480)
 _DeepColor("Azure sea",Color)=(.035,.43,.69,1) _ShallowColor("Turquoise",Color)=(.05,.73,.73,1)
 _WaveSpeed("Wave speed",Range(0,2))=.65 _WaveLength("Large swell wavelength",Range(4,40))=26
 _WaveStrength("Large swell shading",Range(0,1))=.30 _FoamStrength("Shore foam",Range(0,1))=.9
 _PreviewTime("Preview time (-1 live)",Float)=-1
 _SkyTint("Sky reflection",Color)=(.32,.66,.94,1) _SunColor("Sun reflection",Color)=(1,.97,.8,1)
 _SunDirection("Sun direction",Vector)=(0,.7,.7,0) _ViewForward("View forward",Vector)=(0,-.7,.7,0)
 _Evening("Sunset",Float)=0 _Night("Night",Float)=0
 }
 SubShader {Tags {"RenderType"="Opaque"} Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma target 3.0
 #include "UnityCG.cginc"
 sampler2D _DepthMap;float4 _MapBounds,_DeepColor,_ShallowColor,_SkyTint,_SunColor,_SunDirection,_ViewForward;
 float _WaveSpeed,_WaveLength,_WaveStrength,_FoamStrength,_PreviewTime,_Evening,_Night;
 struct v2f{float4 pos:SV_POSITION;float3 world:TEXCOORD0;};
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;return o;}
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float noise(float2 p){float2 a=floor(p),f=frac(p);f=f*f*f*(f*(f*6-15)+10);return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);}
 fixed4 frag(v2f i):SV_Target {
  float t=(_PreviewTime>=0?_PreviewTime:_Time.y)*_WaveSpeed;float2 p=i.world.xz;
  float2 uv=(p-_MapBounds.xy)/_MapBounds.zw;
  float h=tex2D(_DepthMap,uv).r*64-32;float outside=step(1,max(abs(uv.x-.5),abs(uv.y-.5))*2);float depth=lerp(max(0,-h),32,outside);
  float footprint=length(fwidth(p));float waveFade=1-smoothstep(.6,6,footprint);float detailFade=1-smoothstep(.12,1,footprint);
  // Unequal, incommensurate wavelengths and directions; derivative-filtered to avoid distant aliasing.
  float2 warp=float2(noise(p*.018+t*.012),noise(mul(float2x2(.8,-.6,.6,.8),p)*.023-t*.014))-.5;
  float2 q=p+warp*6;
  float k=6.283/max(_WaveLength,1);float swell=0;float2 slope=0;
  float2 dirs[4]={float2(.42,.9075),float2(-.26,.9656),float2(.86,.5103),float2(-.62,.7846)};
  float frequencies[4]={1,.613,1.731,2.973};
  float amplitudes[4]={.85,.52,.16,.035};
  [unroll] for(int wave=0;wave<4;wave++){
   float frequency=k*frequencies[wave];float phase=dot(q,dirs[wave])*frequency-sqrt(9.81*frequency)*t+wave*2.399;
   float filter=1-smoothstep(.7,3.0,footprint*frequency);
   float envelope=.85+.15*sin(dot(p,float2(.024+wave*.005,.031-wave*.002))+t*.08+wave*1.7);
   float a=amplitudes[wave]*envelope*filter;
   swell+=sin(phase)*a;slope+=dirs[wave]*cos(phase)*a*frequency;
  }
  float detail=sin(dot(q,float2(1.73,-2.27))-t*2.6)*detailFade;
  float3 normal=normalize(float3(-slope.x*_WaveStrength*2.5,1,-slope.y*_WaveStrength*2.5));
  float3 view=unity_OrthoParams.w>.5?normalize(-_ViewForward.xyz):normalize(_WorldSpaceCameraPos-i.world);
  float3 reflected=reflect(-view,normal);float fresnel=.10+.5*pow(1-saturate(dot(normal,view)),4);
  float3 col=lerp(_ShallowColor.rgb,_DeepColor.rgb,smoothstep(.1,11,depth));col*=.91+swell*.055+detail*.003;
  float skyMix=fresnel+_Evening*.16;
  col=lerp(col,_SkyTint.rgb,skyMix);
  float alignment=saturate(dot(reflected,normalize(_SunDirection.xyz)));
  float glint=pow(alignment,48)*.42+pow(alignment,12)*.08;
  float stripe=.8+.2*noise(q*.045+t*.025);
  col+=_SunColor.rgb*glint*stripe;
  // Broken, directional reflection for the 35-degree gameplay view.
  float facing=pow(saturate(dot(normalize(reflected.xz),normalize(_SunDirection.xz))),28);
  col+=_SunColor.rgb*facing*stripe*_Evening*.12;
  float distortion=noise(q*.47+t*.12);
  float caustic=pow(saturate(1-abs(sin(dot(q,float2(1.12,.67))+distortion*5+t*.4))),18)*detailFade;
  col+=float3(.025,.045,.03)*caustic*exp(-depth*.65)*(1-_Night*.8);
  float coastPhase=depth*5-t*1.9+noise(p*.9)*1.4;
  float foam=pow(saturate(.5+.5*sin(coastPhase)),10)*exp(-depth*.8)*smoothstep(.2,.65,noise(p*2+t*.15));
  col=lerp(col,lerp(float3(.89,.99,.95),float3(.28,.43,.59),_Night),saturate((foam*1.5+exp(-depth*12)*.55)*_FoamStrength));
  // No world-grid grain: continuous wave shading stays clean at close range.
  #ifndef UNITY_COLORSPACE_GAMMA
  col=GammaToLinearSpace(col);
  #endif
  return float4(col,1);
 }
 ENDCG}} Fallback Off
}



