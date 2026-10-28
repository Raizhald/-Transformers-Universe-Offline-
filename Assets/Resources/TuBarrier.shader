Shader "TU/Barrier"
{
 Properties { _Tint("Tint",Color)=(1,.035,.08,1) }
 SubShader {
  Tags {"Queue"="Transparent" "RenderType"="Transparent"}
  Cull Off ZWrite Off Blend SrcAlpha One
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   float4 _Tint;
   struct appdata{float4 vertex:POSITION;float3 normal:NORMAL;float2 uv:TEXCOORD0;};
   struct v2f{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float3 normal:TEXCOORD1;float3 world:TEXCOORD2;};
   v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.normal=UnityObjectToWorldNormal(v.normal);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;return o;}
   float4 frag(v2f i):SV_Target{float bands=pow(.5+.5*sin(i.uv.y*75-_Time.y*3+sin(i.uv.x*24+_Time.y)*.7),14);float ripple=.5+.5*sin(i.uv.x*12+i.uv.y*25-_Time.y*1.8);float rim=pow(1-abs(dot(normalize(i.normal),normalize(_WorldSpaceCameraPos-i.world))),2);return float4(_Tint.rgb*(.3+bands*1.3+rim*.7),.08+bands*.22+ripple*.03);}
   ENDCG
  }
 }
}
