Shader "Maritime/ManualSeaV2/Clouds" {
 SubShader {Tags {"Queue"="Transparent+10" "RenderType"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Back
 Pass {Tags {"LightMode"="ForwardBase"} CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #pragma target 3.0
 #include "UnityCG.cginc"
 #include "Lighting.cginc"
 struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct v2f {float4 pos:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;float nearFade:TEXCOORD2;};
 v2f vert(appdata v){UNITY_SETUP_INSTANCE_ID(v);v2f o;o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.normal=UnityObjectToWorldNormal(v.normal);o.pos=UnityObjectToClipPos(v.vertex);float3 center=mul(unity_ObjectToWorld,float4(0,4,0,1)).xyz;o.nearFade=smoothstep(18,38,distance(center,_WorldSpaceCameraPos));return o;}
 float4 frag(v2f i):SV_Target {
  float lighting=saturate(dot(normalize(i.normal),normalize(float3(-.45,.85,.35))));
  float shade=.16+.30*smoothstep(.05,.2,lighting)+.32*smoothstep(.35,.55,lighting)+.22*smoothstep(.72,.86,lighting);
  float3 color=lerp(float3(.38,.52,.70),float3(1,.99,.95),shade);
  color*=lerp(float3(.65,.72,.85),_LightColor0.rgb,.65);
  float distanceFade=1-smoothstep(180,350,length(i.world.xz-_WorldSpaceCameraPos.xz));
  return float4(color,.75*distanceFade*i.nearFade);
 }
 ENDCG}}
}
