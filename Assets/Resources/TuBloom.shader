Shader "TU/Bloom"
{
 Properties {_MainTex("Source",2D)="black"{} _BloomTex("Bloom",2D)="black"{}}
 SubShader {
  Cull Off ZWrite Off ZTest Always
  CGINCLUDE
  #include "UnityCG.cginc"
  sampler2D _MainTex,_BloomTex;float4 _MainTex_TexelSize;
  float4 extract(v2f_img i):SV_Target{float3 c=tex2D(_MainTex,i.uv).rgb;float brightness=max(c.r,max(c.g,c.b));return float4(c*max(0,brightness-.9)/max(brightness,.0001),1);}
  float4 blur(v2f_img i):SV_Target{float2 d=_MainTex_TexelSize.xy*1.5;return (tex2D(_MainTex,i.uv+float2(d.x,d.y))+tex2D(_MainTex,i.uv+float2(-d.x,d.y))+tex2D(_MainTex,i.uv+float2(d.x,-d.y))+tex2D(_MainTex,i.uv-d))*.25;}
  float4 combine(v2f_img i):SV_Target{return float4(tex2D(_MainTex,i.uv).rgb+tex2D(_BloomTex,i.uv).rgb*.32,1);}
  ENDCG
  Pass { CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment extract
   ENDCG
  }
  Pass { CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment blur
   ENDCG
  }
  Pass { CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment combine
   ENDCG
  }
 }
 Fallback Off
}
