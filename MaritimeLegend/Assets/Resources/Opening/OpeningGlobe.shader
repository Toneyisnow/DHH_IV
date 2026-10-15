Shader "Maritime/OpeningGlobe" {
 Properties { _MainTex("World map",2D)="white"{} _Longitude("Longitude",Float)=0 _Radius("Radius",Float)=.9 }
 SubShader { Cull Off ZWrite Off ZTest Always
 Pass {
 CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex;
 float _Longitude, _Radius;
 fixed4 frag(v2f_img i):SV_Target {
   float2 screen=i.uv-float2(.5,.51);
   float2 p=screen*float2(1.7777778,1)/_Radius;
   float r2=dot(p,p);
   float3 backdrop=float3(.012,.025,.035);
   float3 col=backdrop;
   if(r2<1){
     float3 n=float3(p,sqrt(1-r2));
     // The positive longitude offset moves surface features right-to-left (east to west).
     float2 uv=float2(atan2(n.x,n.z)/6.2831853+.5+_Longitude,asin(n.y)/3.14159265+.5);
     // Blend the map's unverified seam over a narrow longitude band.
     uv.x=frac(uv.x);
     float edge=min(uv.x,1-uv.x);
     float3 paper=tex2D(_MainTex,uv).rgb;
     float3 seam=(tex2D(_MainTex,float2(.003,uv.y)).rgb+tex2D(_MainTex,float2(.997,uv.y)).rgb)*.5;
     paper=lerp(seam,paper,smoothstep(0,.018,edge));
     float diffuse=saturate(dot(n,normalize(float3(-.4,.55,1))));
     col=paper*(.32+.68*diffuse)*float3(.94,.88,.76);
     col=lerp(backdrop,col,smoothstep(0,.026,1-r2));
   }
   float vignette=1-.56*smoothstep(.23,.78,length(screen*float2(1.05,1.3)));
   return fixed4(col*vignette,1);
 }
 ENDCG
 }
 }
}
