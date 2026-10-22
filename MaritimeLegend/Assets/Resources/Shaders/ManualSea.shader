Shader "Maritime/ManualSea" {
 Properties { _MainTex("Map",2D)="white" {} _Center("Center",Vector)=(.38,.32,0,0) }
 SubShader { Tags { "Queue"="Transparent" "RenderType"="Transparent" } Cull Off ZWrite Off ZTest Always
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
 struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;};
 sampler2D _MainTex;float4 _Center;float4 _Projection;
 v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
 fixed4 frag(v2f i):SV_Target {
  float2 p=float2(i.uv.x*1920,(1-i.uv.y)*1080);
  float3 haze=float3(.81,.91,.84);
  if(p.y<162){float t=saturate(p.y/162);float3 sky=lerp(float3(.18,.43,.67),haze,pow(t,1.8));
   float cloud=exp(-pow((t-.42)/.10,2))*pow(saturate(sin(i.uv.x*19+_Time.y*.008)*.5+.5),10);
   return float4(sky+cloud*.1,1);}
  float q=max(.015,(p.y-162)/378);
  float2 uv=_Center.xy+float2((p.x-960)/(_Projection.x*q),_Projection.y*(1-pow(q,-.75)));
  float inside=step(0,uv.x)*step(uv.x,1)*step(0,uv.y)*step(uv.y,1);
  float3 col=lerp(float3(.035,.24,.36),tex2D(_MainTex,float2(uv.x,1-uv.y)).rgb,inside);
  float water=saturate((col.b-col.r)*5);
  float ripple=sin(uv.x*3300+uv.y*2700+_Time.y*1.7)*sin(uv.y*1800-_Time.y*.8);
  col+=water*ripple*.012;
  float fog=exp(-(p.y-162)/100);
  col=lerp(col,haze,fog*.96);col+=float3(.19,.15,.065)*exp(-pow((p.y-164)/12,2));
  return float4(col,1);
 }
 ENDCG }
 }
}
