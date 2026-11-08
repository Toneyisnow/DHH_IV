Shader "Maritime/ManualSeaV2/Port" {
 Properties { _MainTex("Town art",2D)="white"{} _Color("Atmosphere",Color)=(1,1,1,1) }
 SubShader {
  Tags {"Queue"="AlphaTest" "RenderType"="TransparentCutout"}
  Cull Off ZWrite On
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   sampler2D _MainTex;float4 _MainTex_ST;fixed4 _Color;
   struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};
   v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=TRANSFORM_TEX(v.texcoord,_MainTex);return o;}
   fixed4 frag(v2f i):SV_Target {fixed4 c=tex2D(_MainTex,i.uv);clip(c.a-.35);return fixed4(c.rgb*_Color.rgb,1);}
   ENDCG
  }
  Pass {
   Tags {"LightMode"="ShadowCaster"}
   ZWrite On ZTest LEqual
   CGPROGRAM
   #pragma vertex vertDepth
   #pragma fragment fragDepth
   #pragma multi_compile_shadowcaster
   #include "UnityCG.cginc"
   sampler2D _MainTex;float4 _MainTex_ST;
   struct depthOut {V2F_SHADOW_CASTER;float2 uv:TEXCOORD1;};
   depthOut vertDepth(appdata_base v){depthOut o;TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);o.uv=TRANSFORM_TEX(v.texcoord,_MainTex);return o;}
   float4 fragDepth(depthOut i):SV_Target {clip(tex2D(_MainTex,i.uv).a-.35);SHADOW_CASTER_FRAGMENT(i)}
   ENDCG
  }
 }
}
