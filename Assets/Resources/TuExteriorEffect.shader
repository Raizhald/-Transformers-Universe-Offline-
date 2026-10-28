Shader "TU/ExteriorEffect"
{
 Properties { _Color("Color",Color)=(1,1,1,1) _MainTex("Source texture",2D)="white"{} [HideInInspector]_SrcBlend("Source blend",Float)=5 [HideInInspector]_DstBlend("Destination blend",Float)=10 [HideInInspector]_ZWrite("Depth write",Float)=0 }
 SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite [_ZWrite] Blend [_SrcBlend] [_DstBlend]
 Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #include "UnityCG.cginc"
 sampler2D _MainTex;float4 _MainTex_ST;fixed4 _Color;
 struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;UNITY_FOG_COORDS(1)};
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=TRANSFORM_TEX(v.texcoord,_MainTex);UNITY_TRANSFER_FOG(o,o.pos);return o;}
 fixed4 frag(v2f i):SV_Target{fixed4 c=tex2D(_MainTex,i.uv)*_Color;UNITY_APPLY_FOG(i.fogCoord,c);return c;}
 ENDCG
 }
 }
}
