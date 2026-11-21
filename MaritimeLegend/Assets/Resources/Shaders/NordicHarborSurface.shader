Shader "Maritime/NordicHarborSurface" {
 Properties {
  _MainTex("Paint texture",2D)="white"{}
  _Color("Material color",Color)=(1,1,1,1)
  _Tile("Atlas tile (-1 = full texture)",Float)=-1
  _TextureScale("World repeat scale",Float)=.5
  _Paint("Paint detail",Range(0,1))=1
  _Wind("Wind amplitude",Float)=0
  _Foliage("Foliage color detail",Float)=0
 }
 SubShader{Tags{"RenderType"="Opaque"} CGPROGRAM
 #pragma surface surf Lambert fullforwardshadows vertex:vert addshadow
 #pragma target 3.0
  sampler2D _MainTex;fixed4 _Color;float _Tile,_TextureScale,_Paint,_Wind,_Foliage;
 float _RegionalCityPreview,_RegionalCityTime;
 struct Input{float3 worldPos;float3 worldNormal;float4 color:COLOR;};
 void vert(inout appdata_full v){float t=lerp(_Time.y,_RegionalCityTime,_RegionalCityPreview);float w=pow(saturate(v.vertex.y/6),2);v.vertex.x+=sin(t*.85+v.vertex.y*.6)*_Wind*w;}
 float3 paint(float2 p){float2 uv=frac(p*_TextureScale);uv=(floor(uv*128)+.5)/128;if(_Tile>=0){float2 cell=float2(fmod(_Tile,3),1-floor(_Tile/3));uv=(cell+uv)/float2(3,2);}return tex2D(_MainTex,uv).rgb;}
 void surf(Input IN,inout SurfaceOutput o){float3 n=abs(normalize(IN.worldNormal));float3 w=pow(n,8);w/=max(.0001,w.x+w.y+w.z);float3 tex=paint(IN.worldPos.zy)*w.x+paint(IN.worldPos.xz)*w.y+paint(IN.worldPos.xy)*w.z;float l=dot(tex,float3(.2126,.7152,.0722));float detail=lerp(1,.4+saturate(l*1.65)*.85,_Paint);o.Albedo=_Color.rgb*lerp(detail.xxx,tex*2.1,_Foliage)*IN.color.rgb;o.Alpha=1;}
 ENDCG}Fallback "Diffuse"
}
