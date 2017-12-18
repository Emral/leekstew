
            #include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "AniCelHelper.cginc"

			half _ShadowSharpness, _ShadowOffset, _ShadowDepth, _ShadowSaturation;
            half4 _ShadowHue;
			#if ANICEL_SPECULAR
				float _SpecLevels;
				half _SpecSharpness, _Glossiness;
			#endif
			#if ANICEL_FRESNEL
				half4 _FresColor;
				half _FresPower;
			#endif

			#if ANICEL_LINES || ANICEL_LINES_DYNAMIC
				half _ShadowLineStroke;
				half _ShadowLineDensity;
				#if ANICEL_SPECULAR
					half _SpecLineStroke;
					half _SpecLineDensity;
				#endif
			#elif ANICEL_DOTS || ANICEL_DOTS_DYNAMIC
				half _ShadowDotRadius, _ShadowDotDensity;
				#if ANICEL_SPECULAR
					half _SpecDotRadius, _SpecDotDensity;
				#endif
			#elif ANICEL_MASK
				sampler2D _ShadowBrushMask; 
				uniform float4 _ShadowBrushMask_ST;
				#if ANICEL_SPECULAR
					sampler2D _SpecBrushMask;
					uniform float4 _SpecBrushMask_ST;
				#endif
			#endif

			struct AnimeSurfaceOutput
			{
				half3 Albedo;
				half3 Normal;
				half3 WorldPos;
				half2 ScreenPos;
				half3 Emission;
				half3 Specular;
				half Gloss;
				half Alpha;
			};

			struct UkiyoeSurfaceOutput
			{
				half3 Albedo;
				half3 Normal;
				half StrokeStrength;
				half3 Stroke;
				half3 Emission;
				half Alpha;
			};

			/*
			float3 FresnelSchlick(float3 spec, float3 e, float3 h)
			{
				return spec + (1 - spec) * exp2(-8.656170 * saturate(dot(e, h)));
			}*/

			inline half ComputeLuminance(half ndotl, half atten)
			{
				half l = saturate((ndotl + _ShadowOffset) * (_ShadowSharpness + 1));
				return lerp(1, saturate(atten*l*l*(3 - 2 * l)),_ShadowDepth);
			}

			inline half3 ComputeShadowTone(half3 diff, half lum, half atn)
			{
				return diff * lerp(1, lerp(_ShadowHue.rgb, lum, saturate(1 + lum - _ShadowHue.a)), atn);
			}

			inline half3 ComputeShadowSaturation(half3 diff, half atn)
			{
				half greyscale = (diff.r + diff.g + diff.b) * 0.3333;
				return lerp(diff*atn, saturate(greyscale + (diff - greyscale)*_ShadowSaturation), atn);
			}

			inline half ComputeSpecularHighlights(half gloss, half ndotl, half3 normal, half3 lightDir, half3 viewDir)
			{
				#if ANICEL_SPECULAR
					half power = exp2(12 * gloss + 1);
					half spec = ((power + 2) / 8) * pow(saturate(dot(normal, normalize(lightDir + viewDir))), power);

					spec = max(pow(spec, 0.125), 0) * ndotl;
					half sp = max(floor(spec*_SpecLevels) / _SpecLevels, 0);
					return lerp(sp, max((floor(spec*_SpecLevels) + 1) / _SpecLevels, 0), pow(frac(spec*_SpecLevels), _SpecSharpness*_SpecSharpness + 1)) ;
				#else
					return 0;
				#endif
			}

			inline half3 ComputeFresnel(half3 normal, half3 viewDir, half3 lightDir, half lum, half atten)
			{
				#if ANICEL_FRESNEL
					half3 fres = pow(1 - dot(normal, viewDir), lerp(_FresPower*0.5, _FresPower, lum))*_FresColor.rgb*_FresColor.a*lerp(1, 0.15, lum);

					return fres*lerp(1, 0, (dot(viewDir, lightDir)+1)*0.5)*_LightColor0.rgb*atten;
				#else
					return half3(0, 0, 0);
				#endif
			}

			inline half4 Internal_LightingAnime(AnimeSurfaceOutput s, half3 lightDir, half3 viewDir, half atten, half shadowlessatten)
			{
				half3 diff = s.Albedo*0.75;
				half ndotl = clamp(dot(s.Normal, lightDir) + _ShadowOffset,-1,1);
				half satten = saturate((atten)*(_ShadowSharpness + 1));
				#if SPOT
					shadowlessatten = satten;
				#endif
				#if ANICEL_LINES || ANICEL_LINES_DYNAMIC
					//Base shadow calculation
					{
						#if ANICEL_LINES_DYNAMIC
							half midlum = ComputeLuminance(ndotl, satten);
							half lum = lerp(1, midlum, saturate(round(fmod(abs(s.ScreenPos.x + _ScreenParams.y - s.ScreenPos.y)*_ShadowLineStroke, 2) - (1.5 * midlum))));
						#else
							half lum = lerp(1, ComputeLuminance(ndotl, satten), saturate(round(fmod(abs(s.ScreenPos.x + _ScreenParams.y - s.ScreenPos.y)*_ShadowLineStroke, 2) - (1.5*(1-_ShadowLineDensity)))));
						#endif
						diff = lerp(ComputeShadowSaturation(saturate(ComputeShadowTone(diff, lum, shadowlessatten)), shadowlessatten), diff, lum * shadowlessatten);
						diff *= lerp(1, _LightColor0.rgb, shadowlessatten);
						diff += ComputeFresnel(s.Normal, viewDir, lightDir, lum, shadowlessatten);
					}

					//Specular calculation
					#if ANICEL_SPECULAR
						#if ANICEL_LINES_DYNAMIC
							half3 sbase = ComputeSpecularHighlights(s.Gloss, ndotl, s.Normal, lightDir, viewDir);
							diff += sbase*(s.Specular*satten)*saturate(round(fmod(abs(s.ScreenPos.x + _ScreenParams.y - s.ScreenPos.y)*_SpecLineStroke, 2) - (1.5 * (1-sbase))));
						#else
							diff += ComputeSpecularHighlights(s.Gloss, ndotl, s.Normal, lightDir, viewDir)*(s.Specular*satten)*saturate(round(fmod(abs(s.ScreenPos.x + _ScreenParams.y - s.ScreenPos.y)*_SpecLineStroke, 2) - (1.5 * (1 - _SpecLineDensity))));
						#endif
					#endif

					return half4(diff, s.Alpha);
				#elif ANICEL_DOTS || ANICEL_DOTS_DYNAMIC
					//Base shadow calculation
					{
						half2 pos = fmod(s.ScreenPos.xy, 2 * _ShadowDotRadius) - _ShadowDotRadius;
						#if ANICEL_DOTS_DYNAMIC
							half d = 1-ComputeLuminance(ndotl, satten);
							half lum = lerp(0, 1, saturate(sign(length(pos) - _ShadowDotRadius*d)));
						#else
							half lum = lerp(ComputeLuminance(ndotl, satten), 1, saturate(sign(length(pos) - _ShadowDotRadius*_ShadowDotDensity)));
						#endif
						diff = lerp(ComputeShadowSaturation(saturate(ComputeShadowTone(diff, lum, shadowlessatten)), shadowlessatten), diff, lum * shadowlessatten);
						diff *= lerp(1, _LightColor0.rgb, shadowlessatten);
						diff += ComputeFresnel(s.Normal, viewDir, lightDir, lum, shadowlessatten);
					}
					
					//Specular calculation
					#if ANICEL_SPECULAR
					{
						half2 pos = fmod(s.ScreenPos.xy, 2 * _SpecDotRadius) - _SpecDotRadius;
						#if ANICEL_DOTS_DYNAMIC
							half3 d = ComputeSpecularHighlights(s.Gloss, ndotl, s.Normal, lightDir, viewDir)*(s.Specular*atten);
							diff += 1-saturate(sign(length(pos) - _SpecDotRadius*d));
						#else
							diff += ComputeSpecularHighlights(s.Gloss, ndotl, s.Normal, lightDir, viewDir)*(s.Specular*satten)*(1-saturate(sign(length(pos) - _SpecDotRadius*_SpecDotDensity)));
						#endif
					}
					#endif

					return half4(diff, s.Alpha);

				#elif ANICEL_MASK
					//Base shadow calculation
					{
						half lum = ComputeLuminance(ndotl, satten);
						half3 nd = lerp(diff, ComputeShadowTone(diff, lum, shadowlessatten), tex2D(_ShadowBrushMask, TRANSFORM_TEX(s.ScreenPos.xy, _ShadowBrushMask)).r);
						diff = lerp(ComputeShadowSaturation(nd, shadowlessatten), diff, lum * shadowlessatten);
						diff *= lerp(1, _LightColor0.rgb, shadowlessatten);
						diff += ComputeFresnel(s.Normal, viewDir, lightDir, lum, shadowlessatten);
					}
					
					//Specular calculation
					#if ANICEL_SPECULAR
					{
						diff += ComputeSpecularHighlights(s.Gloss, ndotl, s.Normal, lightDir, viewDir)*(s.Specular*satten)*tex2D(_SpecBrushMask, TRANSFORM_TEX(s.ScreenPos.xy, _SpecBrushMask)).r;
					}
					#endif

					return half4(diff, s.Alpha);
				#else
					//Base shadow calculation
					{
						half lum = ComputeLuminance(ndotl, satten);
						diff = ComputeShadowTone(diff, lum, shadowlessatten);
						diff = lerp(ComputeShadowSaturation(diff, shadowlessatten), diff, lum * shadowlessatten);
						diff *= lerp(1, _LightColor0.rgb, lum);
						diff += ComputeFresnel(s.Normal, viewDir, lightDir, lum, shadowlessatten);
					}

					//Specular calculation
					#if ANICEL_SPECULAR
						diff += ComputeSpecularHighlights(s.Gloss, ndotl, s.Normal, lightDir, viewDir)*(s.Specular*satten);
					#endif

					//Physically-based colour blend
					//spec*FresnelSchlick(s.Specular, lightDir, normalize(lightDir + viewDir));

					return half4(diff, s.Alpha);
				#endif
			}

			inline half4 LightingAnime(AnimeSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
			{
				return Internal_LightingAnime(s, lightDir, viewDir, atten, atten);
			}

			inline half4 LightingUkiyoe (UkiyoeSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) 
            {
              
				half3 diff = s.Albedo * 0.75; 
				#if !ANICEL_UKIYOE_UNLIT
					diff = lerp(diff, s.Stroke, s.StrokeStrength);
					#if ANICEL_UKIYOE_SHADING
						diff *= lerp(1, saturate((dot(s.Normal, lightDir) + _ShadowOffset) * (_ShadowSharpness + 1)), _ShadowDepth);
					#endif
					diff *= atten * _LightColor0.rgb;
				#endif
				return half4(diff, s.Alpha);
            }