Shader "TU/Hologram"
{
 Properties {_MainTex("Original texture",2D)="white"{} _Tint("Tint",Color)=(1,.47,.025,1) _OceanTint("Ocean tint",Color)=(.3,.035,.85,1) _Layer("Layer",Float)=0 _HasTexture("Has texture",Float)=1}
 SubShader {
  Tags {"Queue"="Transparent" "RenderType"="Transparent"}
  Cull Off ZWrite Off Blend SrcAlpha One
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   sampler2D _MainTex;float4 _MainTex_ST,_Tint,_OceanTint;float _Layer,_HasTexture;
   struct appdata{float4 vertex:POSITION;float3 normal:NORMAL;float2 uv:TEXCOORD0;};
   struct v2f{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float3 normal:TEXCOORD1;float3 world:TEXCOORD2;};
   v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.normal=UnityObjectToWorldNormal(v.normal);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;return o;}
   float4 frag(v2f i):SV_Target{
    float4 tex=tex2D(_MainTex,i.uv*_MainTex_ST.xy+_MainTex_ST.zw+float2(_Time.y*.045,0));float mask=_HasTexture>.5?max(tex.r,max(tex.g,tex.b))*tex.a:1;
    float fresnel=pow(1-abs(dot(normalize(i.normal),normalize(_WorldSpaceCameraPos-i.world))),2);float scan=.92+.08*sin(i.uv.y*180-_Time.y*4);float flicker=.94+.06*sin(_Time.y*2.7+i.uv.x*20);
    float3 color=_Tint.rgb;float opacity=.25;
    if(_Layer>.5&&_Layer<1.5){color=_OceanTint.rgb;opacity=.015+fresnel*.055;mask*=.35;}
    if(_Layer>1.5&&_Layer<2.5){opacity=.38;mask=pow(mask,1.6);}
    if(_Layer>2.5)opacity=.035;return float4(color*(1.05+fresnel*.6)*scan*flicker,saturate(mask*opacity));
   }
   ENDCG
  }
 }
}
