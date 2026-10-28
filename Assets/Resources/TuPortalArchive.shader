Shader "TU/PortalArchive"
{
 Properties {_MainTex("Original texture",2D)="black"{} _TintColor("Original tint",Color)=(1,1,1,1) _Scroll("Original UV rate",Vector)=(.5,0,0,0)}
 SubShader {Tags {"Queue"="Transparent+10" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha One
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex;float4 _MainTex_ST,_TintColor,_Scroll;
 struct appdata{float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};struct v2f{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
 v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=TRANSFORM_TEX(v.uv,_MainTex)+_Time.y*_Scroll.xy;o.color=v.color;return o;}
 float4 frag(v2f i):SV_Target{return 2*tex2D(_MainTex,i.uv)*i.color*_TintColor;}
 ENDCG}
 }
}
