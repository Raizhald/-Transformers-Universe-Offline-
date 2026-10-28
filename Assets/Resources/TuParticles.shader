Shader "TU/Particles"
{
 Properties { _DstBlend("Destination blend",Float)=1 }
 SubShader {
  Tags {"Queue"="Transparent" "RenderType"="Transparent"}
  Cull Off ZWrite Off Blend SrcAlpha [_DstBlend]
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct appdata{float4 vertex:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
   struct v2f{float4 pos:SV_POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
   v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.color=v.color;o.uv=v.uv;return o;}
   float4 frag(v2f i):SV_Target{float r=length((i.uv-.5)*2);float fade=pow(saturate(1-r*r),2);return float4(i.color.rgb,i.color.a*fade);}
   ENDCG
  }
 }
}
