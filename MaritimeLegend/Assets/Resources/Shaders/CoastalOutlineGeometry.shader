Shader "Maritime/CoastalOutlineGeometry" {
 Properties {_FadeHeight("Foot fade height",Float)=1.85 _FootDarkenHeight("Foot tint height",Float)=2.8 _FootColor("Foot color",Color)=(.075,.15,.035,1) _Shore("Shore profile",2D)="black"{} _MountainFade("Mountain foot fade",Float)=0 _Clay("Geometry inspection",Float)=0 _TerrainData("Terrain data",2D)="black"{} _TerrainRect("Terrain bounds",Vector)=(0,0,1,1) _TerrainBase("Terrain base",Float)=0 _UseTerrainMap("Use terrain map",Float)=0}
 SubShader {Pass {ZWrite On
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma target 3.0
 #include "UnityCG.cginc"
 #include "CoastalMountainFade.cginc"
 struct v2f{float4 pos:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;float depth:TEXCOORD2;};
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.normal=mul((float3x3)UNITY_MATRIX_IT_MV,v.normal);o.depth=-UnityObjectToViewPos(v.vertex).z*_ProjectionParams.w;return o;}
 float4 frag(v2f i):SV_Target{float opacity=mountainOpacity(i.world);clip(opacity-.025);return float4(EncodeViewNormalStereo(normalize(i.normal)),i.depth,smoothstep(.1,.85,opacity));}
 ENDCG
 }} Fallback Off
}
