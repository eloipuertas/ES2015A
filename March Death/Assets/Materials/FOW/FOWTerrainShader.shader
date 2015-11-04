Shader "Fog Of War/Terrain" {
	Properties{
		[HideInInspector] _Control("Control (RGBA)", 2D) = "red" {}
		[HideInInspector] _Splat3("Layer 3 (A)", 2D) = "white" {}
		[HideInInspector] _Splat2("Layer 2 (B)", 2D) = "white" {}
		[HideInInspector] _Splat1("Layer 1 (G)", 2D) = "white" {}
		[HideInInspector] _Splat0("Layer 0 (R)", 2D) = "white" {}


		[HideInInspector] _MainTex("BaseMap (RGB)", 2D) = "white" {}
		[HideInInspector] _Color("Main Color", Color) = (1,1,1,1)
	}
	SubShader{
		Tags{
			"SplatCount" = "4"
            "Queue" = "Geometry-100"
			"RenderType" = "Opaque" 
		}
		LOD 200
		Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM
			
		#pragma target 4.0
		#pragma surface surf Lambert exclude_path:prepass

		sampler2D _FOWTex;
		float4 _FOWTex_ST;

		struct Input {
			float3 worldPos;
			float2 uv_Control : TEXCOORD0;
			float2 uv_Splat0 : TEXCOORD1;
			float2 uv_Splat1 : TEXCOORD2;
			float2 uv_Splat2 : TEXCOORD3;
			float2 uv_Splat3 : TEXCOORD4;
		};

		half3 TransformColourFOW(half3 c, half3 fow) {
			half3 t = c.rgb * ((saturate(fow.g * 5 - 2)* (1 + fow.b)) / 3);
			return lerp(dot(t, half3(0.5f, 0.4f, 0.1f)).rrr, t.rgb, fow.b);
		}

		sampler2D _Control;
		sampler2D _Splat0, _Splat1, _Splat2, _Splat3;

		void surf(Input IN, inout SurfaceOutput o) {
			
			half4 splat_control = tex2D(_Control, IN.uv_Control);
			half4 firstSplat = tex2D(_Splat0, IN.uv_Splat0);
			half3 c;

			c = splat_control.r * tex2D(_Splat0, IN.uv_Splat0).rgb;
			c += splat_control.g * tex2D(_Splat1, IN.uv_Splat1).rgb;
			c += splat_control.b * tex2D(_Splat2, IN.uv_Splat2).rgb;
			c += splat_control.a * tex2D(_Splat3, IN.uv_Splat3).rgb;
			half3 fow = tex2D(_FOWTex, TRANSFORM_TEX(IN.worldPos.xz, _FOWTex));

			o.Albedo = TransformColourFOW(c, fow);
			o.Alpha = 1;
		}
		ENDCG
	}
	Dependency "AddPassShader" = "Fog Of War/TerrainAddPass"
	FallBack "Diffuse"
}