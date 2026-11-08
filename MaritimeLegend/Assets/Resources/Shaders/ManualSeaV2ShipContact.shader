Shader "Maritime/ManualSeaV2/ShipContact" {
 Properties {
  _FoamColor("Waterline foam",Color)=(.7,.94,.94,1)
  _Phase("Ship ripple phase",Float)=0
 }
 SubShader {
  Tags {"Queue"="Transparent-20" "RenderType"="Transparent"}
  Blend SrcAlpha OneMinusSrcAlpha
  ZWrite Off Cull Off
  Pass { CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   float4 _FoamColor;float _Phase;
   struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};
   v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.texcoord;return o;}
   fixed4 frag(v2f i):SV_Target {
    float2 p=(i.uv-.5)*2;
    float t=_Time.y*.35+_Phase;
    float r=length(p);
    // Soft hull contact stays on the water while the billboard bobs above it.
    float shadow=(1-smoothstep(.24,.72,r))*.30;
    float angle=atan2(p.y,p.x);
    float irregular=sin(angle*5+t)*.018+sin(angle*9-t*.7)*.012;
    float aa=max(fwidth(r),.009);
    // A persistent inner waterline keeps the hull enclosed between ripple pulses.
    float foam=(1-smoothstep(.012,.028+aa,abs(r-.70+irregular)))*.16;
    [unroll] for(int wave=0;wave<2;wave++){
     float age=frac(t*.38+wave*.5);
     float radius=.72+age*.20;
     float band=1-smoothstep(.012,.025+aa,abs(r-radius+irregular));
     float broken=.78+.22*sin(angle*7-t*1.4+wave*2);
     foam+=band*broken*sin(age*3.14159)*.30;
    }
    float alpha=shadow+foam*(1-shadow);
    float3 color=(float3(.015,.13,.17)*shadow*(1-foam)+_FoamColor.rgb*foam)/max(alpha,.0001);
    return float4(color,alpha);
   }
  ENDCG }
 }
 Fallback Off
}
