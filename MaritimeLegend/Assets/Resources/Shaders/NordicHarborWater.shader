Shader "Maritime/NordicHarborWater" {
 Properties{_Color("Harbor water",Color)=(.08,.25,.27,1)}
 SubShader{Tags{"RenderType"="Opaque"} Pass{CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #include "UnityCG.cginc"
 float4 _Color;float _RegionalCityPreview,_RegionalCityTime;
 struct v2f{float4 pos:SV_POSITION;float3 world:TEXCOORD0;UNITY_FOG_COORDS(1)};
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;UNITY_TRANSFER_FOG(o,o.pos);return o;}
 fixed4 frag(v2f i):SV_Target{
  float t=lerp(_Time.y,_RegionalCityTime,_RegionalCityPreview);
  float ripple=sin(i.world.x*2.1+i.world.z*3.7+t*.8)*sin(i.world.x*.9-i.world.z*2.4-t*.6);
  float shine=pow(saturate(ripple),16)*.045;
  float edge=min(abs(i.world.x+11),abs(i.world.x-10));float shore=pow(saturate(1-edge*3),4)*step(-12.5,i.world.z)*step(i.world.z,32.5)*.075;
  fixed4 c=float4(_Color.rgb*(1+ripple*.055)+float3(.5,.64,.61)*(shine+shore),1);UNITY_APPLY_FOG(i.fogCoord,c);return c;
 }ENDCG}}
}
