Shader "Maritime/CityWater" {
Properties {_Color("Water",Color)=(.035,.19,.21,1) _Wind("Wind",Float)=1 _ReflectionTex("Reflection",2D)="black"{}}
SubShader {Tags {"RenderType"="Opaque"}
CGPROGRAM
#pragma surface surf Standard fullforwardshadows
#pragma target 3.0
float4 _Color;float _Wind;sampler2D _ReflectionTex;float4x4 _ReflectionVP;
struct Input {float3 worldPos;float3 viewDir;};
void surf(Input IN,inout SurfaceOutputStandard o){float2 p=IN.worldPos.xz;float wave=sin(p.y*20+sin(p.x*3)+_Time.y*_Wind*1.8);float ripple=pow(saturate(wave),24)*(.12+.08*sin(p.x*14+_Time.y));float glow=pow(saturate(sin(p.y*7+_Time.y*.6)),28)*exp(-abs(p.x+2)*.24);float breakup=saturate(sin(p.x*7+sin(p.y*2))*.5+.25);ripple*=breakup;glow*=breakup;float4 projection=mul(_ReflectionVP,float4(IN.worldPos,1));float2 uv=projection.xy/projection.w*.5+.5;uv.x+=sin(p.y*13+_Time.y)*.0015;float visible=step(0,uv.x)*step(uv.x,1)*step(0,uv.y)*step(uv.y,1)*step(.001,projection.w);float edge=saturate(min(min(uv.x,1-uv.x),min(uv.y,1-uv.y))*20);float3 reflected=tex2D(_ReflectionTex,clamp(uv,.002,.998)).rgb*visible*edge;float fresnel=.04+.10*pow(1-saturate(abs(normalize(IN.viewDir).z)),3);o.Albedo=_Color.rgb+float3(.12,.23,.22)*ripple;o.Emission=_Color.rgb*.12+float3(.65,.42,.12)*glow*.07+reflected*fresnel;o.Metallic=.1;o.Smoothness=.65;o.Alpha=1;}
ENDCG
}Fallback "Diffuse"}
