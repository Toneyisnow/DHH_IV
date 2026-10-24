Shader "Maritime/SeaAtmosphere" {
 Properties {_MainTex("Scene",2D)="white"{}}
 SubShader {Tags {"Queue"="Transparent"} Cull Off ZWrite Off ZTest Always Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};sampler2D _MainTex;
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.texcoord;return o;}
 fixed4 frag(v2f i):SV_Target {float y=(1-i.uv.y)*1080;float3 haze=float3(.79,.89,.86);if(y<162){float t=y/162;float3 sky=lerp(float3(.23,.49,.72),haze,t*t);float cloud=exp(-pow((t-.42)/.13,2))*pow(saturate(sin(i.uv.x*21+_Time.y*.008)*.5+.5),10);return float4(sky+cloud*.10,1);}float3 c=tex2D(_MainTex,i.uv).rgb;float fog=exp(-(y-162)/60);c=lerp(c,haze,fog);c+=float3(.15,.13,.07)*exp(-pow((y-164)/11,2));return float4(c,1);}
 ENDCG}}
}
