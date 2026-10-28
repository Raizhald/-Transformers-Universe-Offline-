Shader "TU/ExteriorLift"
{
 Properties {
 [MainTexture] _Emissive("Original glow",2D)="black"{} _Normal("Original normal",2D)="bump"{}
 _EmissiveColor("Source glow colour",Color)=(0.164,0,0,0.91) _FresnelColor("Source edge colour",Color)=(0.806,0,0,1)
 _DistortionPower("Source refraction",Float)=0.05224 _FresnelPower("Source edge power",Float)=2.56328
 }
 SubShader {
 Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off
 GrabPass {"_TuLiftBackground"}
 Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma target 3.0
 #include "UnityCG.cginc"
 sampler2D _Emissive,_Normal,_TuLiftBackground;float4 _Emissive_ST,_Normal_ST;
 fixed4 _EmissiveColor,_FresnelColor;float _DistortionPower,_FresnelPower;
 struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float3 normal:TEXCOORD1;float3 view:TEXCOORD2;float4 screen:TEXCOORD3;};
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.texcoord.xy;o.normal=UnityObjectToWorldNormal(v.normal);o.view=WorldSpaceViewDir(v.vertex);o.screen=ComputeGrabScreenPos(o.pos);return o;}
 fixed4 frag(v2f i):SV_Target{
 float3 bump=UnpackNormal(tex2D(_Normal,i.uv*_Normal_ST.xy+_Normal_ST.zw));
 float rim=pow(saturate(1-abs(dot(normalize(i.normal),normalize(i.view)))),_FresnelPower);
 float4 screen=i.screen;screen.xy+=bump.xy*_DistortionPower*.2*screen.w;
 fixed3 background=tex2Dproj(_TuLiftBackground,UNITY_PROJ_COORD(screen)).rgb;
 fixed3 glow=tex2D(_Emissive,i.uv*_Emissive_ST.xy+_Emissive_ST.zw).rgb*_EmissiveColor.rgb;
 return fixed4(background+glow*.35+_FresnelColor.rgb*rim*.25,1);
 }
 ENDCG
 }
 }
}
