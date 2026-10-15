Shader "Maritime/CitySprite" {
Properties {_MainTex("Sprite",2D)="white"{} _Cutoff("Alpha cutoff",Range(0,1))=.4 _Tint("Tint",Color)=(1,1,1,1) _Cloth("Clothing",Color)=(.7,.15,.12,1) _Recolor("Recolor",Float)=0}
SubShader {Tags {"Queue"="AlphaTest" "RenderType"="TransparentCutout"} Cull Off
CGPROGRAM
#pragma surface surf Lambert alphatest:_Cutoff addshadow fullforwardshadows
#pragma target 3.0
sampler2D _MainTex;float4 _Tint,_Cloth;float _Recolor;
struct Input {float2 uv_MainTex;};
void surf(Input IN,inout SurfaceOutput o){fixed4 c=tex2D(_MainTex,IN.uv_MainTex);if(_Recolor>.5&&c.r>c.g*1.5&&c.r>c.b*1.3)c.rgb=lerp(c.rgb,_Cloth.rgb*(c.r+.15),.75);o.Albedo=c.rgb*_Tint.rgb;o.Emission=c.rgb*.20;o.Alpha=c.a;}
ENDCG
}Fallback "Transparent/Cutout/Diffuse"}
