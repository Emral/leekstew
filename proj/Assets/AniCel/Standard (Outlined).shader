Shader "AniCel/Standard (Outlined)" {
	Properties {
        _Color ("Main Colour", Color) = (1,1,1,1)
		_MainTex ("Albedo", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_SpecGlossMap ("Specular", 2D) = "white" {}
        _SpecColor ("Specular Colour", Color) = (0.5, 0.5, 0.5, 0.5)
		_Glossiness ("Smoothness", Range(0,1)) = 0
		[PowerSlider(1.5)]
        _SpecLevels ("Specular Levels", Range(1,30)) = 2
		[PowerSlider(3.0)]
        _SpecSharpness ("Specular Sharpness", Range(0,100)) = 80
		_SpecLineStroke("Stroke Density", Range(10,500)) = 50
		_SpecDotRadius("Dot Size", Range(0.001,0.05)) = 0.01
		_SpecDotDensity("Dot Density", Range(0.1,1)) = 0.8
		_SpecBrushMask("Stroke Mask", 2D) = "white" {}
		[PowerSlider(3.0)]
		_ShadowSharpness("Shadow Sharpness", Range(0,100)) = 80
		_ShadowDepth("Shadow Depth", Range(0,1)) = 1
		_ShadowOffset("Shadow Offset", Range(-1,1)) = 0
		_ShadowSaturation("Shadow Saturation", Range(0,10)) = 1
		_ShadowHue("Shadow Hue", Color) = (0,0,0,0)
		_ShadowLineStroke("Stroke Density", Range(10,300)) = 50
		_ShadowDotRadius("Dot Size", Range(0.001,0.05)) = 0.01
		_ShadowDotDensity("Dot Density", Range(0.1,1)) = 0.8
		_ShadowBrushMask("Stroke Mask", 2D) = "white" {}
		_FresColor("Fresnel Color", Color) = (0.5, 0.5, 0.5, 0.5)
		_FresPower("Fresnel Power", Range(2,20)) = 8
		_Cutoff("Alpha Cutoff", Range(0,1)) = 0.5

		[PowerSlider(2.0)]
		_OutlineWidth("Outline Width", Range(0,0.1)) = 0.01
		_OutlineSpace("Screen Space", Range(0,1)) = 0
		_OutlineColor("Outline Color", Color) = (0,0,0,0)

		[HideInInspector] _Cull("_Cull", Float) = 2 //0 = Off, 1 = Front, 2 = Back
		[HideInInspector] _SrcBlend("_SrcBlend", Float) = 1
		[HideInInspector] _DstBlend("_DstBlend", Float) = 0
		[HideInInspector] _ZWrite("_ZWrite", Float) = 1

	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
        Cull Back

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

            half _OutlineWidth;
            half4 _OutlineColor;
            half4 _Color;
			half _OutlineSpace;
			#if ANICEL_RENDER_CUTOFF
				half _Cutoff;
			#endif
            
            struct v2f 
            {
                float4 pos : POSITION;
				float2 texcoord0 : TEXCOORD0;
            };


			uniform sampler2D _MainTex;
			float4 _MainTex_ST;

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
               return o;
            }

            half4 frag( v2f i ) : COLOR
            {
                half4 c = tex2D(_MainTex, i.texcoord0);

				half cutoff = 1;
				#if ANICEL_RENDER_CUTOFF
					cutoff = c.a - _Cutoff;
				#endif
				clip((saturate(_OutlineWidth) * sign(cutoff))-0.001);

                c.rgb = lerp(c.rgb*_Color, 1, _OutlineColor.a)*_OutlineColor.rgb;
				c.a *= _Color.a;
                
                return c;
            }
            ENDCG
        }

		 UsePass "AniCel/Standard/FORWARD"
    }	   
    FallBack "AniCel/Standard"
	CustomEditor "AniCel_Outline"
}
