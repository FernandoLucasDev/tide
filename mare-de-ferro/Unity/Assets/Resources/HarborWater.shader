Shader "MareDeFerro/Water" {
 Properties {
  _Color ("Deep water", Color) = (0.035,0.21,0.24,1)
  _Shallow ("Wave tips", Color) = (0.17,0.43,0.39,1)
 }
 SubShader {
  Tags { "RenderType"="Opaque" }
  LOD 300
  CGPROGRAM
  #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
  #pragma target 3.0
  fixed4 _Color, _Shallow;
  struct Input { float3 worldPos; float3 viewDir; };
  float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
  float noise(float2 p) { float2 i=floor(p),f=frac(p); f=f*f*(3-2*f); return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+float2(1,1)),f.x),f.y); }
  void vert(inout appdata_full v) {
   float x = v.vertex.x, z = v.vertex.z, t = _Time.y;
   v.vertex.y += sin(x * .42 + z * .27 + t*.8)*.105 + sin(z*.83-x*.25+t*1.3)*.045;
   float dx = cos(x*.42+z*.27+t*.8)*.0441-cos(z*.83-x*.25+t*1.3)*.01125;
   float dz = cos(x*.42+z*.27+t*.8)*.02835+cos(z*.83-x*.25+t*1.3)*.03735;
   v.normal = normalize(float3(-dx,1,-dz));
  }
  void surf(Input IN, inout SurfaceOutputStandard o) {
   float2 p=IN.worldPos.xz; float t=_Time.y;
   float n=noise(p*.23+float2(t*.025,t*.015));
   float ripple=sin(p.x*1.7+p.y*4.7+t*1.2+n*2);
   float swell=sin(p.x*.42+p.y*.27+t*.8)*.5+.5;
   float foam=pow(saturate(ripple*.5+.5),45)*.025*smoothstep(.72,.9,noise(p*1.3));
   float fresnel=pow(1-saturate(dot(normalize(IN.viewDir),float3(0,0,1))),3);
   o.Albedo=lerp(_Color.rgb,_Shallow.rgb,swell*.18+n*.23)+foam;
   o.Normal=normalize(float3(sin(p.x*2.3+p.y*.8+t*.8)*.045+sin(p.y*5.1+t)*.018,cos(p.y*2.1-p.x*.6+t*.7)*.055,1));
   o.Metallic=.08; o.Smoothness=.88;
   o.Emission=lerp(float3(.08,.18,.20),float3(.28,.36,.38),fresnel)*.2;
  }
  ENDCG
 }
 FallBack "Diffuse"
}
