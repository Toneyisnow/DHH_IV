Shader "Maritime/SeaReliefWater" {
 Properties {_MainTex("Painted water",2D)="white"{} _SeaData("Coast and depth",2D)="black"{}}
 SubShader {Tags {"RenderType"="Opaque"} Cull Off Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma target 3.5
 #include "UnityCG.cginc"
 #include "SeaMapTiles.cginc"
 struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};sampler2D _MainTex,_SeaData;float4 _MainTex_TexelSize;
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.texcoord;return o;}
 fixed4 frag(v2f i):SV_Target {
  float depth=tex2D(_SeaData,i.uv).g;
  float3 c=lerp(float3(.10,.57,.63),float3(.035,.37,.48),smoothstep(0,25,depth));
  c=lerp(c,float3(.018,.22,.36),smoothstep(25,120,depth));
  c=lerp(c,float3(.012,.105,.23),smoothstep(120,550,depth));
  c=lerp(c,float3(.008,.045,.12),smoothstep(550,2200,depth));
  // Retain the source's angular, hand-painted wave marks; no glossy light overlay.
  float2 offset=float2(sin(_Time.y*.28),cos(_Time.y*.23))*_MainTex_TexelSize.xy*.18;
  float3 painted=seaMapPaint(i.uv+offset);
  float luminance=dot(painted,float3(.21,.72,.07));
  float local=dot(tex2Dlod(_MainTex,float4(i.uv,0,3)).rgb,float3(.21,.72,.07));
  float brush=clamp((luminance-local)*2.8,-.28,.28);
  c=saturate(c*(1+brush));
  #ifndef UNITY_COLORSPACE_GAMMA
  c=GammaToLinearSpace(c);
  #endif
  return float4(c,1);
 }
 ENDCG}}
}
