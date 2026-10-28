Shader "TU/CitySky" {
 Properties { _MainTex("Original sky panorama",2D)="black"{} _Rotation("Source world rotation",Float)=180 }
 SubShader { Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"} Cull Off ZWrite Off
 Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma target 3.0
 #include "UnityCG.cginc"
 sampler2D _MainTex;float _Rotation;
 struct v2f {float4 pos:SV_POSITION;float3 direction:TEXCOORD0;};
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);float a=radians(_Rotation);float c=cos(a),s=sin(a);o.direction=float3(c*v.vertex.x+s*v.vertex.z,v.vertex.y,-s*v.vertex.x+c*v.vertex.z);return o;}
 fixed4 frag(v2f i):SV_Target{float3 d=normalize(i.direction);float2 uv=float2(atan2(d.z,d.x)/6.28318530718+.5,asin(clamp(d.y,-1,1))/3.14159265359+.5);return fixed4(tex2D(_MainTex,uv).rgb,1);}
 ENDCG
 }
 }
}
