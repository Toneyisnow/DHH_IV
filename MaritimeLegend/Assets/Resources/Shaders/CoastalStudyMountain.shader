Shader "Maritime/CoastalStudyMountain" {
 Properties {_FadeHeight("Foot fade height",Float)=1.85 _FootDarkenHeight("Foot tint height",Float)=2.8 _FootColor("Foot color",Color)=(.075,.15,.035,1) _Shore("Shore profile",2D)="black"{} _MountainFade("Mountain foot fade",Float)=1 _Clay("Geometry inspection",Range(0,1))=0 _TerrainData("Terrain data",2D)="black"{} _TerrainRect("Terrain bounds",Vector)=(0,0,1,1) _TerrainBase("Terrain base",Float)=0 _UseTerrainMap("Use terrain map",Float)=0}
 SubShader {Tags {"RenderType"="Transparent" "Queue"="Transparent"}
 CGINCLUDE
 #include "UnityCG.cginc"
 #include "CoastalMountainFade.cginc"
 ENDCG
 // Only the solid mountain core writes depth. The transparent foot never hides the ground.
 Pass {ZWrite On ColorMask 0
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment depth
 #pragma target 3.0
 struct v2f{float4 pos:SV_POSITION;float3 world:TEXCOORD0;};
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;return o;}
 float4 depth(v2f i):SV_Target{clip(mountainOpacity(i.world)-.999);return 0;}
 ENDCG
 }
 Pass {Tags {"LightMode"="ForwardBase"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fwdbase
 #pragma target 3.0
 #include "Lighting.cginc"
 #include "AutoLight.cginc"
 struct v2f{float4 pos:SV_POSITION;float4 paint:COLOR;float3 normal:TEXCOORD0;float3 world:TEXCOORD1;SHADOW_COORDS(2)};
 v2f vert(appdata_full v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.paint=v.color;o.normal=UnityObjectToWorldNormal(v.normal);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;TRANSFER_SHADOW(o);return o;}
 float _FootDarkenHeight;float4 _FootColor;
 float4 frag(v2f i):SV_Target{
  float alpha=mountainOpacity(i.world);clip(alpha-.001);
  float height=i.world.y-coastalGroundHeight(i.world.xz);
  float3 c=lerp(_FootColor.rgb,i.paint.rgb,smoothstep(.02,_FootDarkenHeight,height));
  float lum=dot(c,float3(.299,.587,.114));c=lerp(lum.xxx,c,1.12);
  float ndl=saturate(dot(normalize(i.normal),normalize(_WorldSpaceLightPos0.xyz)));
  float light=.68+.17*smoothstep(.20,.31,ndl)+.23*smoothstep(.60,.71,ndl);
  c=lerp(c,float3(.57,.61,.57),_Clay)*light*lerp(.48,1,SHADOW_ATTENUATION(i));
  #ifndef UNITY_COLORSPACE_GAMMA
  c=GammaToLinearSpace(c);
  #endif
  return float4(c,alpha);
 }
 ENDCG
 }
 Pass {Tags {"LightMode"="ShadowCaster"} ZWrite On
 CGPROGRAM
 #pragma vertex shadowVert
 #pragma fragment shadowFrag
 #pragma multi_compile_shadowcaster
 #pragma target 3.0
 struct shadowOutput{V2F_SHADOW_CASTER;float3 world:TEXCOORD1;};
 shadowOutput shadowVert(appdata_base v){shadowOutput o;o.world=mul(unity_ObjectToWorld,v.vertex).xyz;TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);return o;}
 float4 shadowFrag(shadowOutput i):SV_Target{clip(mountainOpacity(i.world)-.55);SHADOW_CASTER_FRAGMENT(i)}
 ENDCG
 }
 } Fallback Off
}
