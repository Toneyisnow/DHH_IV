Shader "Maritime/ManualSeaV2/Exploration" {
 Properties {_Exploration("Permanent hex states",2D)="black"{}}
 SubShader {Tags {"Queue"="Transparent+100" "RenderType"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off ZTest Always Cull Off
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma target 3.0
 #include "UnityCG.cginc"
 #include "ManualSeaV2Clouds.cginc"
 #include "ManualSeaV2SkyColor.cginc"
 float _FogSkyMode,_FogStarDensity,_FogStarBrightness;float4 _FogSunDirection;
 sampler2D _Exploration;UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
 struct v2f {float4 pos:SV_POSITION;float3 world:TEXCOORD0;float4 screen:TEXCOORD1;};
 v2f vert(appdata_base v){v2f o;o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.pos=UnityObjectToClipPos(v.vertex);o.screen=ComputeScreenPos(o.pos);return o;}
 float2 hex(float2 p){float2 ab=float2(.577350269*p.x-p.y/3,2*p.y/3)/12;float3 c=float3(ab,-ab.x-ab.y),r=round(c),d=abs(r-c);if(d.x>d.y&&d.x>d.z)r.x=-r.y-r.z;else if(d.y>d.z)r.y=-r.x-r.z;return r.xy;}
 float2 center(float2 h){return float2(1.7320508*(h.x+h.y*.5),1.5*h.y)*12;}
 float opacity(float2 h){float2 pixel=h+float2(48,32);if(any(pixel<0)||pixel.x>=96||pixel.y>=64)return 1;return tex2Dlod(_Exploration,float4((pixel+.5)/float2(96,64),0,0)).r;}
 float4 frag(v2f i):SV_Target {
  // Land depth also masks mountain silhouettes above the horizon when the camera is low.
  float3 ray=i.world-_WorldSpaceCameraPos;
  float eyeDepth=LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture,UNITY_PROJ_COORD(i.screen)));
  float eyeRay=max(.000001,-mul((float3x3)UNITY_MATRIX_V,ray).z);
  float3 hit=_WorldSpaceCameraPos+ray*(eyeDepth/eyeRay);
  float2 p;
  if(eyeDepth<_ProjectionParams.z*.999&&hit.y>=0)p=hit.xz;
  else {clip(-ray.y-.000001);float t=-_WorldSpaceCameraPos.y/min(ray.y,-.000001);p=(_WorldSpaceCameraPos+ray*t).xz;}
  p+=float2(seaNoise(p*.08+7),seaNoise(p*.08+39))*5-2.5;
  float2 h=hex(p),c=center(h),local=p-c;float a=opacity(h),sum=a,weight=1;
  const float2 neighbors[6]={float2(1,0),float2(0,1),float2(-1,1),float2(-1,0),float2(0,-1),float2(1,-1)};
  [unroll]for(int k=0;k<6;k++){float2 d=center(neighbors[k]);float edge=length(d)*.5-dot(local,normalize(d));float w=1-smoothstep(0,4,edge);sum+=opacity(h+neighbors[k])*w;weight+=w;}
  a=saturate(sum/weight);
  // Preserve perceptual 40% black in both gamma and linear renderers.
  #ifndef UNITY_COLORSPACE_GAMMA
  a=1-GammaToLinearSpace(float3(1-a,1-a,1-a)).r;
  #endif
  // Logical viewport height stays consistent when rendering into a flipped RT.
  float3 eye=mul((float3x3)UNITY_MATRIX_V,ray);
  float screenHeight=.5+.5*eye.y/max(.000001,-eye.z)*abs(UNITY_MATRIX_P[1][1]);
  float3 skyDirection=normalize(float3(ray.x,max(0,ray.y),ray.z));
  float3 horizon=seaSkyColor(skyDirection,_FogSkyMode,_FogSunDirection.xyz,_FogStarDensity,_FogStarBrightness);
  return float4(horizon*smoothstep(.75,.85,screenHeight),a);
 }
 ENDCG}}
}
