Shader "Maritime/IndianOceanWater" {
 Properties {_TerrainData("Coast and depth",2D)="black"{}}
 SubShader {Tags {"RenderType"="Opaque"} Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma target 3.0
 #include "UnityCG.cginc"
 sampler2D _TerrainData;
 struct v2f{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float2 world:TEXCOORD1;};
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);float3 p=mul(unity_ObjectToWorld,v.vertex).xyz;o.world=p.xz;o.uv=float2(p.x/1672,-p.z/941);return o;}
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float noise(float2 p){float2 a=floor(p),t=frac(p);t=t*t*(3-2*t);return lerp(lerp(hash(a),hash(a+float2(1,0)),t.x),lerp(hash(a+float2(0,1)),hash(a+1),t.x),t.y);}
 fixed4 frag(v2f i):SV_Target{
  float4 data=tex2D(_TerrainData,i.uv);float depth=saturate(log2(1+max(0,data.b))/12);
  float3 c=lerp(float3(.08,.43,.45),float3(.018,.12,.22),smoothstep(.1,.95,depth));
  float breakup=noise(i.world*.58+float2(_Time.y*.08,0));
  float wave=sin(i.world.x*2.7+i.world.y*3.8+noise(i.world*.32)*8+_Time.y*.8);
  c*=.91+.13*noise(i.world*1.3)+.10*smoothstep(.60,.85,wave)*smoothstep(.3,.65,breakup);
  float grain=frac(sin(dot(floor(i.world*1.4),float2(127.1,311.7)))*43758.5453);
  c=lerp(c,float3(.72,.86,.86),step(.97,grain)*smoothstep(.94,.995,wave)*.50);
  c*=lerp(.50,1,smoothstep(0,1.2,data.g));
  #ifndef UNITY_COLORSPACE_GAMMA
  c=GammaToLinearSpace(c);
  #endif
  return float4(c,1);
 }
 ENDCG
 }} Fallback Off
}
