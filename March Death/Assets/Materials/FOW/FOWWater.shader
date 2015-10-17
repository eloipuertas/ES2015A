Shader "Fog Of War/Water" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
	_WaveScale("Wave scale", Range(0.02,0.15)) = 0.063
		_ReflDistort("Reflection distort", Range(0,1.5)) = 0.44
		_RefrDistort("Refraction distort", Range(0,1.5)) = 0.40
		_RefrColor("Refraction color", COLOR) = (.34, .85, .92, 1)
		[NoScaleOffset] _Fresnel("Fresnel (A) ", 2D) = "gray" {}
	[NoScaleOffset] _BumpMap("Normalmap ", 2D) = "bump" {}
	WaveSpeed("Wave speed (map1 x,y; map2 x,y)", Vector) = (19,9,-16,-7)
		[NoScaleOffset] _ReflectiveColor("Reflective color (RGB) fresnel (A) ", 2D) = "" {}
	_HorizonColor("Simple water horizon color", COLOR) = (.172, .463, .435, 1)
		[HideInInspector] _ReflectionTex("Internal Reflection", 2D) = "" {}
	[HideInInspector] _RefractionTex("Internal Refraction", 2D) = "" {}
	}
		SubShader{
		UsePass "FX/Water/WATERPASS"
		Tags{ "RenderType" = "Opaque" "WaterMode" = "Refractive" "RenderType" = "Opaque" }
		LOD 200
		ZWrite Off
		Blend DstColor Zero
		CGPROGRAM
#pragma surface surf Lambert

		sampler2D _MainTex;
	sampler2D _FOWTex;
	float4 _FOWTex_ST;

	struct Input {
		float2 uv_MainTex;
		float3 worldPos;
	};
	void GrayBrightFromFOW(half4 fow, out half lightness, out half grayscale) {
		grayscale = fow.b;
		fow.rg = saturate(fow.rg * 5 - 2);
		lightness = (fow.r + fow.g * (1 + fow.b)) / 3;
	}

	half4 TransformColourFOW(half4 c, half4 fow) {
		half lightness, grayscale;
		GrayBrightFromFOW(fow, lightness, grayscale);
		half3 t = c.rgb * lightness;
		return half4(lerp(dot(t, half3(0.5f, 0.4f, 0.1f)).rrr, t.rgb, grayscale), c.a);
	}
	void surf(Input IN, inout SurfaceOutput o) {
		half4 c = tex2D(_MainTex, IN.uv_MainTex);
		half4 fow = tex2D(_FOWTex, TRANSFORM_TEX(IN.worldPos.xz, _FOWTex));
		half4 t = TransformColourFOW(c, fow);

		o.Albedo = t.rgb;
		o.Alpha = t.a;
	}
	ENDCG
	}
		FallBack "Diffuse"
}
