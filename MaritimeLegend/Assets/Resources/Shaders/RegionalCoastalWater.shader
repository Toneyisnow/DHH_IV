Shader "Maritime/RegionalCoastalWater" {
 Properties{_Color("Sea",Color)=(.055,.24,.27,1)}
 SubShader{Tags{"RenderType"="Opaque"} Pass{CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 float4 _Color;float _RegionalCityPreview,_RegionalCityTime;
 struct v2f{float4 pos:SV_POSITION;float3 world:TEXCOORD0;};
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;return o;}
 fixed4 frag(v2f i):SV_Target{
  float t=lerp(_Time.y,_RegionalCityTime,_RegionalCityPreview);
  float ripple=sin(i.world.x*2.7+i.world.z*4.1+t*.85)*sin(i.world.x*.7-i.world.z*2.3-t*.6);
  float glint=pow(saturate(ripple),12)*.15;
  float foam=pow(saturate(1-abs(i.world.z+.18-sin(t*.7)*.09)*3),4)*.14;
  return float4(_Color.rgb*(1+ripple*.09)+float3(.35,.53,.48)*(glint+foam),1);
 }ENDCG}}
}
