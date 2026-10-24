Shader "Maritime/SeaReliefGround" {
 Properties {_MainTex("Painted map",2D)="white"{} _SeaData("Coast and depth",2D)="black"{}}
 SubShader {Tags {"RenderType"="Opaque" "Queue"="Geometry+1"} Offset -1,-1 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma target 3.5
 #include "UnityCG.cginc"
 #include "SeaMapTiles.cginc"
 struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float3 normal:TEXCOORD1;};sampler2D _MainTex,_SeaData;float4 _MainTex_TexelSize,_SeaData_TexelSize;
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.texcoord;o.normal=UnityObjectToWorldNormal(v.normal);return o;}
 float3 paint(float2 uv){return seaMapPaint(uv);}
 fixed4 frag(v2f i):SV_Target {
  float sdf=tex2D(_SeaData,i.uv).r;clip(-sdf);
  // Preserve registered paint coordinates. Pulling the coastline UVs inland stretched
  // small islands and erased their detail; shoreline elevation is handled by geometry.
  float3 c=paint(i.uv);
  // The illustration already contains its painterly shadows. Only a restrained matte shape cue is added.
  float shade=.90+.10*saturate(dot(normalize(i.normal),normalize(float3(-.4,.85,-.25))));
  return float4(c*shade,1);
 }
 ENDCG}}
}
