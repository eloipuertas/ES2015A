Shader "Fog Of War/Terrain" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
	}
	SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

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
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
