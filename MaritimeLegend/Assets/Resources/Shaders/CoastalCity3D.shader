Shader "Maritime/ManualSeaV2/CoastalCity3D" {
 Properties {_Color("Atmosphere",Color)=(1,1,1,1)}
 SubShader {
  Tags {"RenderType"="Opaque"}
  CGPROGRAM
  #pragma surface surf Lambert
  #pragma target 3.0
  fixed4 _Color;
  struct Input {float4 color:COLOR;};
  void surf(Input IN,inout SurfaceOutput o){o.Albedo=IN.color.rgb*_Color.rgb;o.Alpha=1;}
  ENDCG
 }
 Fallback "Diffuse"
}
