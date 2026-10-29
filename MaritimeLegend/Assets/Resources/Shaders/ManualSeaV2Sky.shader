Shader "Maritime/ManualSeaV2/Sky" {
 Properties {_Mode("0 sunny 1 sunset 2 night",Float)=0 _SunDirection("Celestial direction",Vector)=(0,.14,.99,0) _StarDensity("Star count multiplier",Range(1,6))=3 _StarBrightness("Star brightness multiplier",Range(1,8))=4}
 SubShader {Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"} Cull Off ZWrite Off
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 float _Mode,_StarDensity,_StarBrightness;float4 _SunDirection;
 struct v2f{float4 pos:SV_POSITION;float3 dir:TEXCOORD0;};
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.dir=v.vertex.xyz;return o;}
 #include "ManualSeaV2SkyColor.cginc"
 fixed4 frag(v2f i):SV_Target{return float4(seaSkyColor(i.dir,_Mode,_SunDirection.xyz,_StarDensity,_StarBrightness),1);}

 ENDCG}
 } Fallback Off
}
