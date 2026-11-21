Shader "Maritime/RegionalGround" {
 Properties{_MainTex("Ground",2D)="white"{} _Tint("Tint",Color)=(1,1,1,1) _Scale("Metres per texture",Float)=3}
 SubShader{Tags{"RenderType"="Opaque"} CGPROGRAM
 #pragma surface surf Lambert fullforwardshadows
 #pragma target 3.0
 sampler2D _MainTex;float4 _MainTex_TexelSize;fixed4 _Tint;float _Scale;
 struct Input{float3 worldPos;};
 void surf(Input IN,inout SurfaceOutput o){
  // Continuous world coordinates, mirrored at texture boundaries; tile rotations do not break edge UVs.
  float2 p=IN.worldPos.xz/max(_Scale,.1);float2 uv=1-abs(frac(p*.5)*2-1);
  uv=clamp(uv,_MainTex_TexelSize.xy*1.5,1-_MainTex_TexelSize.xy*1.5);
  float3 c=tex2D(_MainTex,uv).rgb*_Tint.rgb;o.Albedo=c*.85;o.Emission=c*.15;o.Alpha=1;
 }
 ENDCG}Fallback "Diffuse"
}
