Shader "Maritime/WorldSea/Land" {
 SubShader {Tags {"RenderType"="Opaque"} Cull Off Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 #include "ManualSeaV2SkyColor.cginc"
 float _WorldSeaSkyMode;
 float4 _WorldSeaFogColor,_WorldSeaLight,_WorldSeaTint;float2 _WorldSeaFogRange;
 struct input {float4 vertex:POSITION;float3 normal:NORMAL;float4 color:COLOR;};
 struct output {float4 pos:SV_POSITION;float3 world:TEXCOORD0;float4 color:COLOR;};
 output vert(input v){output o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;
 float lit=.55+.45*saturate(dot(UnityObjectToWorldNormal(v.normal),_WorldSeaLight.xyz));o.color=float4(v.color.rgb*lit*_WorldSeaTint.rgb,1);return o;}
 fixed4 frag(output i):SV_Target {float f=smoothstep(_WorldSeaFogRange.x,_WorldSeaFogRange.y,distance(i.world.xz,_WorldSpaceCameraPos.xz));float3 mist=seaSkyColor(i.world-_WorldSpaceCameraPos,_WorldSeaSkyMode,_WorldSeaLight.xyz,3,4);return float4(lerp(i.color.rgb,mist,f),1);}
 ENDCG}}
}
