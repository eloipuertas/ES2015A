Shader "Fog Of War/TreeLeaves" {
	Properties {
			_Color ("Main Color", Color) = (1,1,1,1)
			_MainTex ("Main Texture", 2D) = "white" {  }
			_Cutoff ("Alpha cutoff", Range(0.25,0.9)) = 0.5
			_BaseLight ("Base Light", Range(0, 1)) = 0.35
			_AO ("Amb. Occlusion", Range(0, 10)) = 2.4
			_Occlusion ("Dir Occlusion", Range(0, 20)) = 7.5

			// These are here only to provide default values
			[HideInInspector] _TreeInstanceColor ("TreeInstanceColor", Vector) = (1,1,1,1)
			[HideInInspector] _TreeInstanceScale ("TreeInstanceScale", Vector) = (1,1,1,1)
			[HideInInspector] _SquashAmount ("Squash", Float) = 1
    }
  	SubShader {
				Tags{
				"Queue" = "Transparent-99"
				"IgnoreProjector" = "True"
				"RenderType" = "TreeTransparentCutout"
				"DisableBatching" = "True"
			}
			//Cull Off
			  UsePass "Nature/Tree Soft Occlusion Leaves/LEAVES1"
				UsePass "Nature/Tree Soft Occlusion Leaves/SHADOWCASTER"
  		CGPROGRAM
  		#pragma surface surf Lambert alphatest:_Cutoff

			struct Input {
				float2 uv_MainTex;
				float3 worldPos;
			};

			sampler2D _FOWTex;
    	float4 _FOWTex_ST;
  		sampler2D _MainTex;

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
		// This subshader is never actually used, but is only kept so
		// that the tree mesh still assumes that normals are needed
		// at build time (due to Lighting On in the pass). The subshader
		// above does not actually use normals, so they are stripped out.
		// We want to keep normals for backwards compatibility with Unity 4.2
		// and earlier.
		SubShader {
			Tags {
				"Queue" = "Transparent-99"
				"IgnoreProjector"="True"
				"RenderType" = "TransparentCutout"
			}
			Cull Off
			ColorMask RGB
			Pass {
				Tags { "LightMode" = "Vertex" }
				AlphaTest GEqual [_Cutoff]
				Lighting On
				Material {
					Diffuse [_Color]
					Ambient [_Color]
				}
				SetTexture [_MainTex] { combine primary * texture DOUBLE, texture }
			}
		}
		Dependency "BillboardShader" = "Hidden/TerrainEngine/BillboardTree"
		Fallback Off
  }
