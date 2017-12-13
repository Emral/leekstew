Shader "Custom/shader_solidCutout" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Cutoff ("Cutoff", Range(0,1)) = 0.5
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader {
        Tags { "RenderType"="Opaque" "Queue"="AlphaTest" }
        Color [_Color]
        AlphaTest [_Cutoff]
	}
	FallBack "Diffuse"
}
