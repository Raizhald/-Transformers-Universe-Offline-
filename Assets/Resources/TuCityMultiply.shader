Shader "TU/CityMultiply"
{
 Properties { _MainTex("Original decal texture",2D)="white"{} _Color("Primary multiplier",Color)=(1,1,1,1) }
 SubShader
 {
  Tags {"Queue"="Transparent" "IgnoreProjector"="true" "RenderType"="Transparent"}
  Cull Off ZWrite Off Blend Zero SrcColor
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
   v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=TRANSFORM_TEX(v.uv,_MainTex);o.color=v.color*_Color;UNITY_TRANSFER_FOG(o,o.pos);return o;}
   fixed4 frag(v2f i):SV_Target
   {
    fixed4 previous=tex2D(_MainTex,i.uv)*i.color;
    // Original fixed-function stage: previous lerp(previous) constant-white.
    fixed4 result=lerp(fixed4(1,1,1,1),previous,previous.a);
    UNITY_APPLY_FOG_COLOR(i.fogCoord,result,fixed4(1,1,1,1));return result;
   }
   ENDCG
  }
 }
}
