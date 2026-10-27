Shader "Maritime/ManualSeaV2/Ship" {
 Properties {_MainTex("Existing ship art",2D)="white"{} _Tint("Light tint",Color)=(1,1,1,1)}
 SubShader {Tags {"Queue"="AlphaTest" "RenderType"="TransparentCutout"} Cull Off ZWrite On
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex;float4 _Tint;
 struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.texcoord;return o;}
 fixed4 frag(v2f i):SV_Target{fixed4 c=tex2D(_MainTex,i.uv);clip(c.a-.18);c.rgb*=_Tint.rgb;return c;}
 ENDCG}}
}
