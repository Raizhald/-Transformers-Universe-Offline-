Shader "TU/CityTransparent" {
 Properties { _Color("Original colour",Color)=(1,1,1,1) _MainTex("Original diffuse",2D)="white"{} _BumpMap("Original normal",2D)="bump"{} _Illum("Original illumination",2D)="black"{} _UseNormal("Use normal",Float)=0 _UseIllum("Use illumination",Float)=0 _UseCutout("Use cutout",Float)=0 _Cutoff("Original alpha cutoff",Float)=0.5 _Glossiness("Adapted shininess",Range(0,1))=0.2 _Metallic("Metallic",Range(0,1))=0 }
 SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off
 CGPROGRAM
 #pragma surface surf Standard alpha:fade fullforwardshadows
 #pragma target 3.0
 sampler2D _MainTex,_BumpMap,_Illum;fixed4 _Color;half _UseNormal,_UseIllum,_UseCutout,_Cutoff,_Glossiness,_Metallic;
 struct Input {float2 uv_MainTex;float2 uv_BumpMap;float2 uv_Illum;};
 void surf(Input IN,inout SurfaceOutputStandard o){fixed4 c=tex2D(_MainTex,IN.uv_MainTex)*_Color;if(_UseCutout>0.5)clip(c.a-_Cutoff);o.Albedo=c.rgb;o.Normal=normalize(lerp(float3(0,0,1),UnpackNormal(tex2D(_BumpMap,IN.uv_BumpMap)),_UseNormal));o.Emission=c.rgb*tex2D(_Illum,IN.uv_Illum).a*_UseIllum;o.Metallic=_Metallic;o.Smoothness=_Glossiness;o.Alpha=c.a;}
 ENDCG
 } Fallback "Diffuse"
}