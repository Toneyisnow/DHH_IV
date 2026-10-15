Shader "Maritime/CityWater" {
Properties {_Color("Water",Color)=(.035,.19,.21,1) _Wind("Wind",Float)=1 _ReflectionTex("Reflection",2D)="black"{}}
SubShader {Tags {"RenderType"="Opaque"}
CGPROGRAM
#pragma surface surf Standard fullforwardshadows
#pragma target 3.0
float4 _Color;float _Wind;sampler2D _ReflectionTex;float4x4 _ReflectionVP;
struct Input {float3 worldPos;};
void surf(Input IN,inout SurfaceOutputStandard o){float2 p=IN.worldPos.xz;float wave=sin(p.y*20+sin(p.x*3)+_Time.y*_Wind*1.8);float ripple=pow(saturate(wave),24)*(.12+.08*sin(p.x*14+_Time.y));float glow=pow(saturate(sin(p.y*7+_Time.y*.6)),28)*exp(-abs(p.x+2)*.24);float breakup=saturate(sin(p.x*7+sin(p.y*2))*.5+.25);ripple*=breakup;glow*=breakup;float4 projection=mul(_ReflectionVP,float4(IN.worldPos,1));float2 uv=projection.xy/projection.w*.5+.5;uv.x+=sin(p.y*13+_Time.y)*.0015;float3 reflected=tex2D(_ReflectionTex,uv).rgb;o.Albedo=_Color.rgb+float3(.12,.23,.22)*ripple;o.Emission=_Color.rgb*.12+float3(.65,.42,.12)*glow*.07+reflected*.20;o.Metallic=.1;o.Smoothness=.85;o.Alpha=1;}
ENDCG
}Fallback "Diffuse"}
