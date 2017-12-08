Shader "AniCel/Ukiyo-e" {
	Properties {
        _Color ("Main Colour", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}
		[PowerSlider(3.0)]
		_ShadowSharpness("Shadow Sharpness", Range(0,100)) = 80
		_ShadowDepth("Shadow Depth", Range(0,1)) = 1
		_ShadowOffset("Shadow Offset", Range(-1,1)) = 0
		[PowerSlider(2.0)]
		_OutlineWidth("Outline Width", Range(0,0.1)) = 0.01
		_OutlineSpace("Screen Space Outline", Range(0,1)) = 0
		_OutlineBrightness("Outline Brightness", Range(0,1)) = 0
        _Brightness ("Brightness", Range(0,2)) = 1
        _Saturation ("Saturation", Range(0,5)) = 1
		_StrokeMask("Stroke Mask", 2D) = "gray" {}
		_StrokeStrength("Stroke Strength", Range(0,1)) = 0.3
		_Cutoff("Alpha Cutoff", Range(0,1)) = 0.5

		[HideInInspector] _Cull("_Cull", Float) = 2 //0 = Off, 1 = Front, 2 = Back
		[HideInInspector] _SrcBlend("_SrcBlend", Float) = 1
		[HideInInspector] _DstBlend("_DstBlend", Float) = 0
		[HideInInspector] _ZWrite("_ZWrite", Float) = 1
	}

	SubShader {
		Tags { "RenderType"="Opaque" }

        Pass
        {
            Name "BORDER"
			Tags{ "LightMode" = "Always" }
			Cull Front
			ZWrite [_ZWrite]
			ColorMask RGB
			Blend [_SrcBlend] [_DstBlend]

		    CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
			#pragma shader_feature ANICEL_RENDER_CUTOFF
            #include "UnityCG.cginc"
			#include "AniCelHelper.cginc"

			half4 _Color;
            half _OutlineWidth;
            half _OutlineBrightness;
			half _OutlineSpace;
			half _StrokeStrength;
			#if ANICEL_RENDER_CUTOFF
				half _Cutoff;
			#endif
            
            struct v2f 
            {
                float4 pos : POSITION;
				float2 texcoord0 : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
            };

			uniform sampler2D _StrokeMask, _MainTex;
			float4 _MainTex_ST;
			float4 _StrokeMask_ST;

            v2f vert (appdata_full v)
            {
               v2f o;

               v.vertex.xyz += v.normal * _OutlineWidth * (1-_OutlineSpace);
               o.pos = UnityObjectToClipPos(v.vertex); 

			   float2 offset = TransformViewToProjection(normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal)).xy);

				#ifdef UNITY_Z_0_FAR_FROM_CLIPSPACE
					o.pos.xy += offset * UNITY_Z_0_FAR_FROM_CLIPSPACE(o.pos.z) * _OutlineWidth * _OutlineSpace * 0.3;
				#else
					o.pos.xy += offset * o.pos.z * _OutlineWidth * _OutlineSpace * 0.3;
				#endif

			   o.texcoord0 = TRANSFORM_TEX(v.texcoord, _MainTex);
			   o.screenPos = ComputeScreenPos(o.pos);
               return o;
            }

            half4 frag( v2f i ) : COLOR
            {
				half a = (tex2D(_MainTex, i.texcoord0) * _Color).a;

				half4 c = lerp(tex2D(_StrokeMask, TRANSFORM_TEX(ANICEL_SCREENCOORDS(i.screenPos), _StrokeMask)), 1, _StrokeStrength);
				c.rgb *= _OutlineBrightness;
				c.a *= a;
				half cutoff = 1;
				#if ANICEL_RENDER_CUTOFF
					cutoff = c.a - _Cutoff;
				#endif
				clip((saturate(_OutlineWidth) * sign(cutoff))-0.001);
				return c;
            }
            ENDCG
        }


		Tags { "RenderType"="Opaque" }
		LOD 200
		Cull[_Cull]
		ZWrite[_ZWrite]
		Blend[_SrcBlend][_DstBlend]

		    CGPROGRAM
		    #pragma surface surf Ukiyoe fullforwardshadows keepalpha
			#pragma shader_feature ANICEL_UKIYOE_UNLIT
			#pragma shader_feature ANICEL_UKIYOE_SHADING
			#pragma shader_feature _ ANICEL_RENDER_CUTOFF
            #pragma target 3.0
            #include "UnityCG.glslinc"
            #include "AniCelLighting.cginc"

		    sampler2D _MainTex, _BumpMap, _StrokeMask;
            half4 _Color;
			half _StrokeStrength;
			half _Brightness;
			half _Saturation;
			#if ANICEL_RENDER_CUTOFF
				half _Cutoff;
			#endif
            
		    struct Input 
            {
		        float2 uv_MainTex;
		        float2 uv_BumpMap;
				float4 screenPos;
		    };

		    void surf (Input IN, inout UkiyoeSurfaceOutput o) 
            {
				half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				#if ANICEL_RENDER_CUTOFF
					clip(c.a - _Cutoff);
				#endif
                o.Normal = UnpackNormal( tex2D (_BumpMap, IN.uv_BumpMap));
			    o.Albedo = c.rgb * _Brightness;
				o.Stroke = tex2D(_StrokeMask, (IN.screenPos.xy / max(IN.screenPos.w, 0.000001)));
				o.StrokeStrength = _StrokeStrength;

				half lum = (o.Albedo.r + o.Albedo.g + o.Albedo.b) * 0.333;
				o.Albedo = lerp(saturate(lum + (o.Albedo - lum)*_Saturation), o.Stroke, o.StrokeStrength);
				#if ANICEL_UKIYOE_UNLIT
					o.Emission = o.Albedo;
					o.Albedo = 0;
				#endif
			    o.Alpha = c.a;
		    }
		    ENDCG 
    }	   
    FallBack "Standard"
	CustomEditor "AniCel_Ukiyoe"
}
