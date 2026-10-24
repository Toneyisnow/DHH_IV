// 7 x 4 tiles, 256 source pixels per core, 8 source pixels of shared gutters.
UNITY_DECLARE_TEX2DARRAY(_MapTiles);
float3 seaMapPaint(float2 uv) {
 float2 p=clamp(float2(uv.x,1-uv.y)*float2(1672,941),float2(0,0),float2(1671.999,940.999));
 float2 cell=floor(p/256);
 float2 local=(p-cell*256+8)/272;
 local.y=1-local.y;
 float2 dx=ddx(p*4),dy=ddy(p*4);
 float lod=clamp(.5*log2(max(max(dot(dx,dx),dot(dy,dy)),1)),0,3);
 return UNITY_SAMPLE_TEX2DARRAY_LOD(_MapTiles,float3(local,cell.y*7+cell.x),lod).rgb;
}
