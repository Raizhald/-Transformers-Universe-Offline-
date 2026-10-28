Shader "TU/AutobotSky"
{
 Properties {
  _FrontTex("Original front +Z",2D)="grey"{} _BackTex("Original back -Z",2D)="grey"{}
  _LeftTex("Original left +X",2D)="grey"{} _RightTex("Original right -X",2D)="grey"{}
  _UpTex("Original up +Y",2D)="grey"{} _DownTex("Original down -Y",2D)="grey"{}
 }
 SubShader {
  Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"}
  Cull Off ZWrite Off
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   sampler2D _FrontTex,_BackTex,_LeftTex,_RightTex,_UpTex,_DownTex;
   struct v2f {float4 pos:SV_POSITION;float3 direction:TEXCOORD0;};
   v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.direction=v.vertex.xyz*float3(-1,1,-1);return o;}
   fixed4 frag(v2f i):SV_Target {
    float3 d=i.direction,a=abs(d);float2 uv;fixed3 color;
    if(a.z>=a.x&&a.z>=a.y){uv=float2(d.z>0?d.x:-d.x,d.y)/a.z*.5+.5;color=d.z>0?tex2D(_FrontTex,uv).rgb:tex2D(_BackTex,uv).rgb;}
    else if(a.x>=a.y){uv=float2(d.x>0?-d.z:d.z,d.y)/a.x*.5+.5;color=d.x>0?tex2D(_LeftTex,uv).rgb:tex2D(_RightTex,uv).rgb;}
    else{uv=float2(d.x,d.y>0?-d.z:d.z)/a.y*.5+.5;color=d.y>0?tex2D(_UpTex,uv).rgb:tex2D(_DownTex,uv).rgb;}
    return fixed4(color,1);
   }
   ENDCG
  }
 }
}
