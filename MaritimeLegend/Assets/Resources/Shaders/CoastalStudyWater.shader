Shader "Maritime/CoastalStudyWater" {
 Properties {_Shore("Shore profile",2D)="black"{} _Clay("Geometry inspection",Range(0,1))=0 _Stylized("Painted lighting",Range(0,1))=1}
 SubShader {Tags {"RenderType"="Opaque"} Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma target 3.0
 #include "UnityCG.cginc"
 sampler2D _Shore;float _Clay,_Stylized;
 struct v2f{float4 pos:SV_POSITION;float3 world:TEXCOORD0;};
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;return o;}
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 fixed4 frag(v2f i):SV_Target {
  float2 p=i.world.xz;
  float shore=tex2D(_Shore,float2((p.x+100)/200,.5)).r;
  float riverx=3+sin(p.y*.19)*2.3+sin(p.y*.41)*.5;
  float distance=p.y<shore?shore-p.y:max(0,1.15+.25*sin(p.y*.3)-abs(p.x-riverx));
  float depth=smoothstep(0,18,distance);
  float3 c=lerp(float3(.12,.37,.39),float3(.045,.19,.29),depth);
  float t=_Time.y;
  float wave=sin(p.x*2.7+p.y*3.8+sin(p.x*.71)*2+t*.8);
  c*=.96+.10*smoothstep(.25,.42,wave);
  float sparkle=step(.92,hash(floor(p*1.4)))*smoothstep(.90,.995,wave)*smoothstep(3,10,distance);
  c=lerp(c,float3(.77,.86,.80),sparkle*.65);
  float bankInk=(1-smoothstep(.08,.32,distance))*_Stylized;
  c=lerp(c,c*float3(.48,.56,.51),bankInk*.72);
  c=lerp(c,float3(.26,.31,.33),_Clay);
  #ifndef UNITY_COLORSPACE_GAMMA
  c=GammaToLinearSpace(c);
  #endif
  return float4(c,1);
 }
 ENDCG
 }
 UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
 } Fallback "Diffuse"
}
