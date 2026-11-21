Shader "Maritime/WorldSea/Water" {
 Properties {_DistanceMap("Coast distance",2D)="white"{} _HasCoast("Has coast",Float)=0 _ChunkOrigin("Local chunk origin",Vector)=(0,0,160,160)}
 SubShader {Tags {"RenderType"="Opaque"} Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma target 3.0
 #include "UnityCG.cginc"
 #include "ManualSeaV2SkyColor.cginc"
 float _WorldSeaSkyMode;
 sampler2D _DistanceMap;float _HasCoast;float4 _ChunkOrigin;
 float4 _WorldSeaFogColor,_WorldSeaOrigin,_WorldSeaLight,_WorldSeaTint;float2 _WorldSeaFogRange;
 struct output {float4 pos:SV_POSITION;float3 world:TEXCOORD0;};
 output vert(appdata_base v){output o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;return o;}
 fixed4 frag(output i):SV_Target {
 float2 p=i.world.xz+_WorldSeaOrigin.xy;
 // Integer X frequencies ensure waves meet at the 11520-unit world seam.
 float t=_Time.y;float a=p.x*(6.2831853*443/11520)+p.y*.18-t*.7,b=p.x*(6.2831853*231/11520)-p.y*.29+t*.6;
 float fade=1-smoothstep(.5,5,length(fwidth(p)));float wave=(sin(a)+sin(b))*.5*fade;
 float2 uv=((i.world.xz-_ChunkOrigin.xy)/160*64+1.5)/67;
 float d=lerp(80,tex2D(_DistanceMap,uv).r,_HasCoast);
 float3 col=lerp(float3(.07,.57,.55),float3(.025,.28,.44),smoothstep(0,24,d));
 col*=.94+wave*.06;
 float3 n=normalize(float3(-cos(a)*.15*fade,1,-cos(b)*.15*fade));
 float3 view=normalize(_WorldSpaceCameraPos-i.world);
 float shine=pow(saturate(dot(reflect(-view,n),_WorldSeaLight.xyz)),65);
 col+=shine*float3(.6,.64,.55);
 float foam=exp(-abs(d)*1.4)*(.55+.25*sin(d*4-t*1.6));col=lerp(col,float3(.72,.84,.77),foam);
 col*=_WorldSeaTint.rgb;
 float fog=smoothstep(_WorldSeaFogRange.x,_WorldSeaFogRange.y,distance(i.world.xz,_WorldSpaceCameraPos.xz));
 float3 mist=seaSkyColor(i.world-_WorldSpaceCameraPos,_WorldSeaSkyMode,_WorldSeaLight.xyz,3,4);
 return float4(lerp(col,mist,fog),1);
 }
 ENDCG}}
}
