Shader "Maritime/CoastalStudyGround" {
 Properties {_Color("Tint",Color)=(1,1,1,1) _Clay("Geometry inspection",Range(0,1))=0 _Stylized("Painted lighting",Range(0,1))=1 _GrassTex("Painted grass",2D)="gray"{} _GroundDetail("Terrain texture",Range(0,1))=0}
 SubShader {Tags {"RenderType"="Opaque"} Pass {Tags {"LightMode"="ForwardBase"}
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fwdbase
 #pragma target 3.0
 #include "UnityCG.cginc"
 #include "Lighting.cginc"
 #include "AutoLight.cginc"
 struct v2f {float4 pos:SV_POSITION;float4 paint:COLOR;float3 normal:TEXCOORD0;float3 world:TEXCOORD1;SHADOW_COORDS(2)};
 float4 _Color;float _Clay,_Stylized,_GroundDetail;sampler2D _GrassTex;
 v2f vert(appdata_full v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.paint=v.color;o.normal=UnityObjectToWorldNormal(v.normal);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;TRANSFER_SHADOW(o);return o;}
 fixed4 frag(v2f i):SV_Target {
  float ndl=saturate(dot(normalize(i.normal),normalize(_WorldSpaceLightPos0.xyz)));
  float bands=.68+.17*smoothstep(.20,.31,ndl)+.23*smoothstep(.60,.71,ndl);
  float shadow=SHADOW_ATTENUATION(i);
  float painted=bands*lerp(.48,1,shadow);
  float modeled=(.48+.60*ndl)*lerp(.63,1,shadow);
  float3 c=i.paint.rgb*_Color.rgb;
  float grass=smoothstep(-.01,.10,i.paint.g-i.paint.r)*_GroundDetail;
  float3 turf=tex2D(_GrassTex,i.world.xz/22).rgb;
  #ifndef UNITY_COLORSPACE_GAMMA
  turf=LinearToGammaSpace(turf);
  #endif
  float turfValue=dot(turf,float3(.25,.65,.10));
  float detail=clamp(.63+turfValue*.82,.65,1.2);
  c*=lerp(1,detail,grass*.90);
  // Fine sand variation without relief, highlights or large regular bands.
  float grain=frac(sin(dot(floor(i.world.xz*16),float2(127.1,311.7)))*43758.5453);
  c*=lerp(1,.95+grain*.10,_GroundDetail*(1-grass));
  float luminance=dot(c,float3(.299,.587,.114));c=lerp(luminance.xxx,c,1.12);
  c=lerp(c,float3(.57,.61,.57),_Clay);
  c*=lerp(modeled,painted,_Stylized*(1-_Clay));
  #ifndef UNITY_COLORSPACE_GAMMA
  c=GammaToLinearSpace(c);
  #endif
  return float4(c,1);
 }
 ENDCG
 }
 UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
 } Fallback "Diffuse"
}
