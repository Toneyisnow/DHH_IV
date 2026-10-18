Shader "Maritime/Character04Toon" {
 Properties { _Color("Base Color",Color)=(1,1,1,1) }
 SubShader {
  Tags { "RenderType"="Opaque" "Queue"="Geometry" }
  Pass {
   Cull Off
   CGPROGRAM
   #pragma target 3.0
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; };
   struct v2f { float4 pos:SV_POSITION; float3 normal:TEXCOORD0; };
   float4 _Color;
   v2f vert(appdata v) { v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.normal=UnityObjectToWorldNormal(v.normal);return o; }
   fixed4 frag(v2f i, fixed facing:VFACE):SV_Target {
    float n=dot(normalize(i.normal)*(facing>0?1:-1),normalize(float3(-.5,.8,-.6)));
    float shade=n>.35?1:(n>-.25?.77:.52);
    return fixed4(_Color.rgb*shade,1);
   }
   ENDCG
  }
 }
}
