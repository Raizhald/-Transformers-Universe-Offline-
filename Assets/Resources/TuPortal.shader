Shader "TU/Portal"
{
 Properties { _EnergyColor ("Energy",Color)=(.65,.06,1,1) _Halo ("Halo",Float)=0 }
 SubShader {
  Tags { "Queue"="Transparent" "RenderType"="Transparent" }
  Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   struct appdata { float4 vertex:POSITION;float2 uv:TEXCOORD0; };
   struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};
   float4 _EnergyColor;float _Halo;
   v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
   float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
   float4 frag(v2f i):SV_Target {
    float2 p=(i.uv-.5)*2;float r=length(p),a=atan2(p.y,p.x),t=_Time.y*.65;
    float n=noise(float2(r*9-t,a*3+t));float arcs=pow(max(0,sin(a*5+r*28-t*5+n*5)),12);
    float cloud=noise(p*6+float2(sin(t),cos(t)))+.5*noise(p*15-t);float ring=exp(-pow((r-.86)*23,2));
    float3 core=lerp(float3(.12,.005,.04),_EnergyColor.rgb,saturate(r+cloud*.35));
    float concentric=pow(saturate(.5+.5*sin(r*64-t*3+n*.8)),10)*smoothstep(.22,.55,r)*(1-smoothstep(.82,.92,r));
    float center=exp(-r*r*9);float3 centerColor=float3(.6,.015,.12)*(cloud*.7+arcs*.4)*center;
    float3 color=core*(.16+cloud*.38)+_EnergyColor.rgb*(arcs*1.5+ring*2+concentric*.85)+centerColor;float alpha=(1-smoothstep(.88,1,r))*.96;
    if(_Halo>.5){color=_EnergyColor.rgb*.65;alpha=exp(-r*r*5)*.16;}return float4(color,alpha);
   }
   ENDCG
  }
 }
}
