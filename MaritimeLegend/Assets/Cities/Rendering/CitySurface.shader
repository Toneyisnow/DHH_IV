Shader "Maritime/CitySurface" {
 Properties {
  _MainTex("Painted pixel atlas",2D)="white"{}
  _Color("Tint",Color)=(1,1,1,1)
  _EmissionColor("Emission",Color)=(0,0,0,0)
  _Painted("Use painted material",Float)=0
  _Tile("Atlas cell",Float)=0
  _TileScale("Repeats per metre",Vector)=(.333,.222,0,0)
  _Foliage("Leaf contrast",Float)=0
  _Cloth("Woven canvas",Float)=0
 }
 SubShader {
  Tags {"RenderType"="Opaque"}
  CGPROGRAM
  #pragma surface surf Lambert fullforwardshadows
  #pragma target 3.0
  sampler2D _MainTex;fixed4 _Color,_EmissionColor;
  float _Painted,_Tile,_Foliage,_Cloth;float4 _TileScale;
  struct Input {float3 worldPos;float3 worldNormal;};
  float3 painted(float2 p) {
   // Stable world-space pixel grid, independent of screen resolution and camera motion.
   float2 uv=frac(p*_TileScale.xy);
   uv=(floor(uv*float2(128,192))+.5)/float2(128,192);
   float2 cell=float2(fmod(_Tile,3),1-floor(_Tile/3));
   return tex2D(_MainTex,(cell+uv)/float2(3,2)).rgb;
  }
  void surf(Input IN,inout SurfaceOutput o) {
   float3 color=_Color.rgb;
   if(_Painted>.5) {
    float3 n=abs(normalize(IN.worldNormal));float3 weights=pow(n,8);weights/=max(.0001,weights.x+weights.y+weights.z);
    float3 tex=painted(IN.worldPos.zy)*weights.x+painted(IN.worldPos.xz)*weights.y+painted(IN.worldPos.xy)*weights.z;
    // Keep painted leaf highlights and deep recesses instead of flat green facets.
    float leafLight=dot(tex,float3(.2126,.7152,.0722));
    tex=lerp(tex,lerp(leafLight.xxx,tex,.65)*float3(.85,.90,.80),_Foliage);
    color*=tex;
   }
   if(_Cloth>.5) {
    float2 pixel=floor(IN.worldPos.xz*48);
    float weave=fmod(pixel.x+pixel.y,2);
    float fold=floor((sin(IN.worldPos.x*15)*.5+.5)*4)/4;
    color*=.73+fold*.22+weave*.07;
   }
   o.Albedo=color;o.Emission=_EmissionColor.rgb;o.Alpha=1;
  }
  ENDCG
 }
 Fallback "Diffuse"
}
