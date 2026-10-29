#ifndef SEA_V2_CLOUDS
#define SEA_V2_CLOUDS
float _SeaCloudEnabled,_SeaCloudTop;float4 _SeaCloudLight;
float seaHash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
float seaNoise(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(seaHash(a),seaHash(a+float2(1,0)),f.x),lerp(seaHash(a+float2(0,1)),seaHash(a+1),f.x),f.y);}
sampler2D _SeaCloudMask;float4 _SeaCloudOffset;
float seaCloud(float2 world){return tex2D(_SeaCloudMask,(world-_SeaCloudOffset.xy)/_SeaCloudOffset.zw+.5).r;}
float seaCloudShadow(float3 world){
 // The mask is the actual mesh projected to y=0, not an approximate ellipse.
 float2 source=world.xz-_SeaCloudLight.xz*world.y/max(.08,_SeaCloudLight.y);
 float distanceFade=1-smoothstep(180,350,length(world.xz-_WorldSpaceCameraPos.xz));
 return _SeaCloudEnabled*seaCloud(source)*distanceFade*(1-smoothstep(_SeaCloudTop-1,_SeaCloudTop,world.y));
}
#endif
