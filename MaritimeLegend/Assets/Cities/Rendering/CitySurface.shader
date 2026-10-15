Shader "Maritime/CitySurface" {Properties {_MainTex("Texture",2D)="white"{} _Color("Color",Color)=(1,1,1,1) _EmissionColor("Emission",Color)=(0,0,0,0)} SubShader {Tags {"RenderType"="Opaque"} CGPROGRAM
#pragma surface surf Lambert fullforwardshadows
sampler2D _MainTex;fixed4 _Color,_EmissionColor;struct Input {float3 worldPos;float3 worldNormal;};void surf(Input IN,inout SurfaceOutput o){float2 uv=abs(IN.worldNormal.y)>.5?IN.worldPos.xz:float2(IN.worldPos.x+IN.worldPos.z,IN.worldPos.y);o.Albedo=tex2D(_MainTex,uv*.25).rgb*_Color.rgb;o.Emission=_EmissionColor.rgb;o.Alpha=1;}
ENDCG
}Fallback "Diffuse"}
