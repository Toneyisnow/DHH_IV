Shader "Maritime/ManualSeaV2/Land"
{
 Properties { _Grain("Paint texture",Range(0,1)) = .35 }
 SubShader {
 Tags {"RenderType"="Opaque"}
 CGPROGRAM
 #pragma surface surf Lambert fullforwardshadows vertex:vert
 #pragma target 3.0
 #include "ManualSeaV2Clouds.cginc"
 float _Grain;
 struct Input {float4 color:COLOR;float3 worldPos;};
 void vert(inout appdata_full v,out Input o) {UNITY_INITIALIZE_OUTPUT(Input,o);o.color=v.color;}
 float hash(float2 p) {return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 void surf(Input i,inout SurfaceOutput o) {
  float grain=hash(floor(i.worldPos.xz*22));
  float strokes=hash(floor(i.worldPos.xz*4));
  o.Albedo=i.color.rgb*(1+((grain-.5)*.3+(strokes-.5)*.22)*_Grain);
  o.Albedo*=1-.52*seaCloudShadow(i.worldPos);
  o.Alpha=1;
 }
 ENDCG
 } Fallback "Diffuse"
}
