Shader "TU/ArchiveAdditive"
{
 Properties { _MainTex("Original glow texture",2D)="black"{} _TintColor("Original tint",Color)=(1,1,1,1) }
 SubShader {
  Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
  Cull Off ZWrite Off Blend SrcAlpha One
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   sampler2D _MainTex;float4 _MainTex_ST,_TintColor;
   struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
   struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};
   v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=TRANSFORM_TEX(v.uv,_MainTex);return o;}
   float4 frag(v2f i):SV_Target{return 2*tex2D(_MainTex,i.uv)*_TintColor;}
   ENDCG
  }
 }
}
