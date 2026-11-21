Shader "Maritime/RegionalCityElement" {
 Properties {
  _MainTex("Regional element",2D)="white"{}
  _Cutoff("Cutout threshold",Range(0,1))=.7
  _Amplitude("Wind amplitude (metres)",Float)=.05
  _Period("Wind cycle (seconds)",Float)=7
  _Phase("Phase offset",Float)=0
  _Bird("Wing motion",Float)=0
  _FlapSpeed("Wingbeat angular speed",Float)=4.2
 }
 SubShader {
  Tags {"Queue"="AlphaTest" "RenderType"="TransparentCutout"}
  Cull Off
  CGPROGRAM
  #pragma surface surf Lambert alphatest:_Cutoff vertex:vert addshadow fullforwardshadows
  #pragma target 3.0
  sampler2D _MainTex;
  float _Amplitude,_Period,_Phase,_Bird,_FlapSpeed;
  float _RegionalCityPreview,_RegionalCityTime;
  struct Input {float2 uv_MainTex;};
  void vert(inout appdata_full v){
   float t=lerp(_Time.y,_RegionalCityTime,_RegionalCityPreview);
   float a=t*6.2831853/max(_Period,.1)+_Phase;
   float wind=sin(a)+.22*sin(a*2+v.vertex.y*1.5);
   // Vertex red is an authored mobility mask: roots, poles and beams stay pinned.
   float displacement=_Amplitude*v.color.r;
   v.vertex.x+=wind*displacement*(1-_Bird);
   v.vertex.z+=sin(a+v.vertex.x*1.4)*displacement*.28*(1-_Bird);
   // Slow wingbeats alternate with a glide, retaining the bird body and tail.
   float beat=sin(t*_FlapSpeed+_Phase)*(.35+.65*smoothstep(.2,.8,sin(t*.6)*.5+.5));
   v.vertex.y+=beat*displacement*_Bird;
  }
  void surf(Input IN,inout SurfaceOutput o){
   fixed4 c=tex2D(_MainTex,IN.uv_MainTex);
   // Painted material shading is retained; the remaining light responds to scene shadows.
   o.Albedo=c.rgb*.65;o.Emission=c.rgb*.35;o.Alpha=c.a;
  }
  ENDCG
 }
 Fallback "Transparent/Cutout/Diffuse"
}
