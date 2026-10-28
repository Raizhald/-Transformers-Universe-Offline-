Shader "TU/CityAdditive"
{
 Properties { _MainTex("Original glow texture",2D)="white"{} _Color("Original tint",Color)=(.5,.5,.5,.5) }
 SubShader
 {
  Tags {"Queue"="Transparent" "IgnoreProjector"="true" "RenderType"="Transparent"}
  Cull Off ZWrite Off Blend SrcAlpha One ColorMask RGB
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_fog
   #pragma target 3.0
   #include "UnityCG.cginc"
   sampler2D _MainTex;float4 _MainTex_ST;fixed4 _Color;
   struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
   struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;UNITY_FOG_COORDS(1)};
   v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=TRANSFORM_TEX(v.uv,_MainTex);o.color=v.color;UNITY_TRANSFER_FOG(o,o.pos);return o;}
   fixed4 frag(v2f i):SV_Target
   {
    fixed4 result=2*tex2D(_MainTex,i.uv)*i.color*_Color;clip(result.a-.01);
    // Additive fog must approach zero light; scene-coloured fog exposes the entire glow quad.
    UNITY_APPLY_FOG_COLOR(i.fogCoord,result,fixed4(0,0,0,0));return result;
   }
   ENDCG
  }
 }
}
