Shader "Custom/ModifiedSurfaceShader" {
	Properties{
		_MainTex("Main", 2D) = "white" {}
		_Color("Main Color", Color) = (1,1,1,1)
		_SpecTex("Spec Map", 2D) = "white" {}
		_GlossTex("Gloss Map", 2D) = "white" {}
		_BumpMap("Normalmap", 2D) = "bump" {}
	}
	SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 400

CGPROGRAM
#pragma surface surf BlinnPhong

		sampler2D _MainTex;
		sampler2D _SpecTex;
		sampler2D _GlossTex;
		sampler2D _BumpMap;
		fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
			float2 uv_SpecTex;
			float2 uv_GlossTex;
			float2 uv_BumpMap;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = tex.rgb * _Color.rgb;
			o.Gloss = tex2D(_GlossTex, IN.uv_GlossTex).a;
			o.Alpha = tex.a * _Color.a;
			o.Specular = tex2D(_SpecTex, IN.uv_SpecTex).g;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		}
ENDCG
	}
	FallBack "Diffuse"
}
