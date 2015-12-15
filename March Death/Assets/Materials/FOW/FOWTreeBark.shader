Shader "Fog Of War/TreeBark" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,0)
		_MainTex ("Main Texture", 2D) = "white" {}
		_BaseLight ("Base Light", Range(0, 1)) = 0.35
		_AO ("Amb. Occlusion", Range(0, 10)) = 2.4

		// These are here only to provide default values
		[HideInInspector] _TreeInstanceColor ("TreeInstanceColor", Vector) = (1,1,1,1)
		[HideInInspector] _TreeInstanceScale ("TreeInstanceScale", Vector) = (1,1,1,1)
		[HideInInspector] _SquashAmount ("Squash", Float) = 1
    }
  	SubShader {
      Tags {
        "IgnoreProjector"="True"
        "RenderType" = "TreeOpaque"
        "DisableBatching"="True"
      }
		ZWrite Off Cull Off
  		UsePass "Nature/Tree Soft Occlusion Bark/TREE1"
  		UsePass "Nature/Tree Soft Occlusion Bark/SHADOWCASTER"

  		CGPROGRAM
  		#pragma surface surf Lambert

  		sampler2D _MainTex;
      sampler2D _FOWTex;
    	float4 _FOWTex_ST;

  		struct Input {
  			float2 uv_MainTex;
    		float3 worldPos;
  		};

      half4 TransformColourFOW(half4 c, half3 fow) {
    		half3 t = c.rgb * ((saturate(fow.g * 5 - 2)* (1 + fow.b)) / 3);
    		return half4(lerp(dot(t, half3(0.5f, 0.4f, 0.1f)).rrr, t.rgb, fow.b), c.a);
    	}
  		void surf (Input IN, inout SurfaceOutput o) {
        half4 c = tex2D(_MainTex, IN.uv_MainTex);
    		half3 fow = tex2D(_FOWTex, TRANSFORM_TEX(IN.worldPos.xz, _FOWTex));
    		half4 t = TransformColourFOW(c, fow);

    		o.Albedo = t.rgb;
    		o.Alpha = t.a;
  		}
  		ENDCG
  	}
  	FallBack "Diffuse"
  }
