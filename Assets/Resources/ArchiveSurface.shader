Shader "TU/ArchiveSurface"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Original diffuse texture", 2D) = "white" {}
        _Metallic ("Metallic", Range(0,1)) = 0
        _Glossiness ("Smoothness", Range(0,1)) = 0.2
        _EmissionColor ("Emission", Color) = (0,0,0,1)
        _EmissionMap ("Emission texture", 2D) = "black" {}
        _DetailTex ("Original gloss R / two-tone G / emission B", 2D) = "black" {}
        _UseDetail ("Use original channel map", Float) = 0
        _GlossFromAlpha ("Use original diffuse alpha gloss", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        sampler2D _MainTex, _EmissionMap, _DetailTex;
        fixed4 _Color, _EmissionColor;
        half _Metallic, _Glossiness, _UseDetail, _GlossFromAlpha;
        struct Input { float2 uv_MainTex; float2 uv_EmissionMap; float2 uv_DetailTex; };
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            fixed4 detail=tex2D(_DetailTex,IN.uv_DetailTex);
            half gloss=lerp(lerp(1,c.a,_GlossFromAlpha),detail.r,_UseDetail);
            o.Albedo=c.rgb; o.Alpha=1; o.Metallic=_Metallic; o.Smoothness=_Glossiness*gloss;
            o.Emission=lerp(tex2D(_EmissionMap, IN.uv_EmissionMap).rgb,detail.bbb,_UseDetail) * _EmissionColor.rgb;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
