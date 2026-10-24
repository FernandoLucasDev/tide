Shader "MareDeFerro/Sky" {
 Properties { _SunDir("Sun direction",Vector)=(.2,.3,.8,0) }
 SubShader { Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" } Cull Off ZWrite Off
 Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct v2f { float4 pos:SV_POSITION; float3 dir:TEXCOORD0; };
 float4 _SunDir;
 v2f vert(appdata_base v) { v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.dir=v.vertex.xyz; return o; }
 fixed4 frag(v2f i):SV_Target {
  float3 d=normalize(i.dir); float h=saturate(d.y);
  float3 horizon=float3(.87,.57,.35), zenith=float3(.19,.33,.43);
  float3 c=lerp(horizon,zenith,pow(h,.52));
  float sun=max(0,dot(d,normalize(_SunDir.xyz)));
  c+=float3(1,.54,.21)*pow(sun,26)*.28;
  c+=float3(1,.77,.40)*smoothstep(.9990,.9996,sun)*1.6;
  float cloud=sin(d.x*14+d.z*10+sin(d.z*18)*.6)*sin(d.z*23-d.x*9);
  float layer=smoothstep(.18,.55,cloud)*smoothstep(.02,.11,h)*(1-smoothstep(.20,.48,h));
  c=lerp(c,float3(.55,.48,.44),layer*.4);
  return float4(c,1);
 }
 ENDCG
 } }
}
