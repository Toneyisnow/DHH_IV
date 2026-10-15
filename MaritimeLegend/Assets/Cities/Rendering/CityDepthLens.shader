Shader "Maritime/CityDepthLens" {
Properties {_MainTex("Scene",2D)="white"{} _Focus("Focus",Float)=30 _Band("Sharp band",Float)=5 _Blur("Blur",Float)=3.5 _Fog("Fog",Float)=0.05}
SubShader {Cull Off ZWrite Off ZTest Always Pass {
CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#pragma target 3.0
#include "UnityCG.cginc"
sampler2D _MainTex;float4 _MainTex_TexelSize;UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
float _Focus,_Band,_Blur,_Fog,_DebugDepth;
float depth(float2 uv){float d=SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,uv);if(unity_OrthoParams.w>0.5){
#if defined(UNITY_REVERSED_Z)
d=1-d;
#endif
return lerp(_ProjectionParams.y,_ProjectionParams.z,d);}return LinearEyeDepth(d);}
fixed4 frag(v2f_img i):SV_Target {float eye=depth(i.uv);if(_DebugDepth>.5)return float4(((eye-15)/45).xxx,1);float coc=smoothstep(0,1,(abs(eye-_Focus)-_Band)/12);float2 step=_MainTex_TexelSize.xy*(_Blur*coc);float3 color=tex2D(_MainTex,i.uv).rgb;float3 sum=color;float weight=1;
// Dense disk sampling avoids the separated roof/window ghosts of a sparse 3x3 kernel.
for(int tap=0;tap<32;tap++){float radius=sqrt((tap+.5)/32.0);float angle=tap*2.399963;float2 uv=i.uv+float2(cos(angle),sin(angle))*radius*step;float d=depth(uv);float k=exp(-abs(d-eye)*.25)*(1-.5*radius);sum+=tex2D(_MainTex,uv).rgb*k;weight+=k;}color=lerp(color,sum/weight,saturate(coc*2));
float3 bloom=0;for(int bx=-1;bx<=1;bx++)for(int by=-1;by<=1;by++)bloom+=max(0,tex2D(_MainTex,i.uv+float2(bx,by)*_MainTex_TexelSize.xy*7).rgb-1.05);color+=bloom*.065;
float mist=saturate((eye-_Focus+1)*_Fog*.035);color=lerp(color,float3(.59,.58,.48),mist);float l=dot(color,float3(.2126,.7152,.0722));color=lerp(l.xxx,color,1.10);color*=float3(1.045,1.01,.94);float2 q=i.uv*2-1;color*=1-.12*pow(saturate(dot(q,q)*.5),1.4);return float4(color,1);}
ENDCG
}}}
