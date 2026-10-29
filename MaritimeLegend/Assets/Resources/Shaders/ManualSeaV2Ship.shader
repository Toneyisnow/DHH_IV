Shader "Maritime/ManualSeaV2/Ship" {
 Properties {_MainTex("Existing ship art",2D)="white"{} _SailTrim("Sail trim",Float)=0 _SailOpening("Sail opening",Range(0,1))=1 _WindFill("Wind fill",Range(0,1))=1 _Tint("Light tint",Color)=(1,1,1,1)}
 SubShader {Tags {"Queue"="AlphaTest" "RenderType"="TransparentCutout"} Cull Off ZWrite On
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex;float4 _Tint;float _SailTrim,_SailOpening,_WindFill;
 struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.texcoord;return o;}
 fixed4 frag(v2f i):SV_Target{float sail=smoothstep(.34,.55,i.uv.y);float2 uv=i.uv;
 uv.x=.5+(uv.x-.5)/lerp(1,.82+.18*abs(cos(_SailTrim)),sail);
 uv.x+=sail*(sin(_SailTrim)*.055+sin(_Time.y*2+uv.y*9)*.004*_SailOpening*_WindFill);
 uv.y=lerp(uv.y,.34+(uv.y-.34)/max(.25,_SailOpening),sail*(1-_SailOpening));
 fixed4 c=tex2D(_MainTex,uv);clip(c.a-.18);c.rgb*=_Tint.rgb;return c;}
 ENDCG}}
}
