Shader "TU/HexShield"
{
 Properties { _Tint("Shield color",Color)=(.65,.12,1.7,1) _MainTex("Original electricity branches",2D)="white"{} }
 SubShader {
  Tags {"Queue"="Transparent" "RenderType"="Transparent"}
  Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass {
   CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment frag
   #include "UnityCG.cginc"
   sampler2D _MainTex;float4 _Tint;
   // Object-space UV animation, using the recovered electricity artwork.
   float4 frag(v2f_img i):SV_Target {
    float2 uv=i.uv;float2 drift=float2(_Time.y*.036,-_Time.y*.052);
    float a=tex2D(_MainTex,uv*1.65+drift).a;
    float b=tex2D(_MainTex,float2(uv.y,-uv.x)*2.1-drift*.7).a;
    float branches=saturate(a+b*.7);
    float rim=pow(saturate(length((uv-.5)*2)),5);
    float pulse=.85+.15*sin(_Time.y*1.8);
    float3 color=lerp(float3(.08,.008,.22),_Tint.rgb,branches)*pulse;
    return float4(color+_Tint.rgb*.3*rim,.38+branches*.4+rim*.15);
   }
   ENDCG
  }
 }
}
