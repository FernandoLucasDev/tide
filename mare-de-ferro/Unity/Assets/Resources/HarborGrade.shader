Shader "Hidden/MareDeFerro/Grade" {
 Properties { _MainTex("Image",2D)="white"{} }
 SubShader { Cull Off ZWrite Off ZTest Always
 Pass { CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex; float4 _MainTex_TexelSize;
 fixed4 frag(v2f_img i):SV_Target {
  float3 c=tex2D(_MainTex,i.uv).rgb;
  float3 glow=0;
  for(int x=-2;x<=2;x++) for(int y=-2;y<=2;y++) {
   float3 s=tex2D(_MainTex,i.uv+float2(x,y)*_MainTex_TexelSize.xy*3).rgb;
   glow+=max(s-.75,0)/25;
  }
  c+=glow*.20;
  float lum=dot(c,float3(.2126,.7152,.0722));
  c=lerp(lum.xxx,c,1.08); c=(c-.5)*1.045+.5;
  float vignette=1-dot(i.uv-.5,i.uv-.5)*.32;
  return float4(saturate(c*vignette),1);
 }
 ENDCG }
 } FallBack Off
}
