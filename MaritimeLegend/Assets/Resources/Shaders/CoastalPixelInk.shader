Shader "Maritime/CoastalPixelInk" {
 Properties {_MainTex("Scene",2D)="white"{}}
 SubShader {Cull Off ZWrite Off ZTest Always Pass {
 CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 #pragma target 3.0
 #include "UnityCG.cginc"
 sampler2D _MainTex,_CoastalGeometry;float4 _MainTex_TexelSize;
 void geometry(float2 uv,out float depth,out float3 normal,out float coverage){
  #if UNITY_UV_STARTS_AT_TOP
  if(_MainTex_TexelSize.y<0)uv.y=1-uv.y;
  #endif
  float4 g=tex2D(_CoastalGeometry,uv);depth=g.z;normal=DecodeViewNormalStereo(g);coverage=g.w;
 }
 fixed4 frag(v2f_img i):SV_Target {
  float2 uv=i.uv;
  float3 c=tex2D(_MainTex,uv).rgb;
  float depth,coverage;float3 normal;geometry(uv,depth,normal,coverage);float edge=0;
  for(int j=0;j<4;j++){
   float2 delta=j==0?float2(1,0):j==1?float2(-1,0):j==2?float2(0,1):float2(0,-1);
   float other,otherCoverage;float3 n;geometry(uv+delta*abs(_MainTex_TexelSize.xy)*1.15,other,n,otherCoverage);
   // Foreground-side ink. Suppress small facets and gradual ground slopes.
   float silhouette=smoothstep(.0013,.0022,other-depth);
   float crease=smoothstep(.75,1.15,length(normal-n))*.30;
   edge=max(edge,max(silhouette,crease));
  }
  #ifndef UNITY_COLORSPACE_GAMMA
  c=LinearToGammaSpace(c);
  #endif
  // Absolute ink colors keep bright mountains as legible as dark foliage.
  float foliage=smoothstep(.015,.075,c.g-c.r);
  float3 ink=lerp(float3(.024,.027,.020),float3(.025,.075,.030),foliage);
  c=lerp(c,ink,edge*.96*coverage);
  #ifndef UNITY_COLORSPACE_GAMMA
  c=GammaToLinearSpace(c);
  #endif
  return float4(c,1);
 }
 ENDCG
 }
 Pass {
 CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment antialias
 #pragma target 3.0
 #include "UnityCG.cginc"
 sampler2D _MainTex;float4 _MainTex_TexelSize;
 float luminance(float3 c){return dot(c,float3(.299,.587,.114));}
 fixed4 antialias(v2f_img i):SV_Target{
  float2 uv=i.uv,px=abs(_MainTex_TexelSize.xy);
  float3 center=tex2D(_MainTex,uv).rgb;
  float nw=luminance(tex2D(_MainTex,uv+px*float2(-1,-1)).rgb);
  float ne=luminance(tex2D(_MainTex,uv+px*float2(1,-1)).rgb);
  float sw=luminance(tex2D(_MainTex,uv+px*float2(-1,1)).rgb);
  float se=luminance(tex2D(_MainTex,uv+px*float2(1,1)).rgb);
  float mid=luminance(center),lo=min(mid,min(min(nw,ne),min(sw,se))),hi=max(mid,max(max(nw,ne),max(sw,se)));
  // Skip low-contrast texture detail; soften only high-contrast stair steps.
  if(hi-lo<max(.035,hi*.12))return float4(center,1);
  float2 direction=float2(-((nw+ne)-(sw+se)),(nw+sw)-(ne+se));
  float reduce=max((nw+ne+sw+se)*.03125,.0078125);
  direction=clamp(direction/(min(abs(direction.x),abs(direction.y))+reduce),-6,6)*px;
  float3 a=.5*(tex2D(_MainTex,uv-direction/6).rgb+tex2D(_MainTex,uv+direction/6).rgb);
  float3 b=a*.5+.25*(tex2D(_MainTex,uv-direction*.5).rgb+tex2D(_MainTex,uv+direction*.5).rgb);
  float lb=luminance(b);return float4(lb<lo||lb>hi?a:b,1);
 }
 ENDCG
 }
 } Fallback Off
}
