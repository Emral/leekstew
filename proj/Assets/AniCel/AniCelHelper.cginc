#ifndef ANICEL_SETUP_MASKS
#if ANICEL_MASK
#if ANICEL_SPECULAR
#define ANICEL_SETUP_MASKS float2 uv_ShadowBrushMask; float2 uv_SpecBrushMask;
#else
#define ANICEL_SETUP_MASKS float2 uv_ShadowBrushMask;
#endif
#else
#define ANICEL_SETUP_MASKS
#endif
#endif


#ifdef POINT
#define ANICEL_LIGHT_ATTENUATION(destName1, destName2, input, worldPos) \
    unityShadowCoord3 lightCoord = mul(unity_WorldToLight, unityShadowCoord4(worldPos, 1)).xyz; \
    fixed shadow = UNITY_SHADOW_ATTENUATION(input, worldPos); \
    fixed destName1 = tex2D(_LightTexture0, dot(lightCoord, lightCoord).rr).UNITY_ATTEN_CHANNEL; \
	fixed destName2 = destName1 * shadow;
#endif

#ifdef SPOT
#define ANICEL_LIGHT_ATTENUATION(destName1, destName2, input, worldPos) \
    unityShadowCoord4 lightCoord = mul(unity_WorldToLight, unityShadowCoord4(worldPos, 1)); \
    fixed shadow = UNITY_SHADOW_ATTENUATION(input, worldPos); \
    fixed destName1 = (lightCoord.z > 0) * UnitySpotCookie(lightCoord) * UnitySpotAttenuate(lightCoord.xyz); \
	fixed destName2 = destName1 * shadow;
#endif

#ifdef DIRECTIONAL
#define ANICEL_LIGHT_ATTENUATION(destName1, destName2, input, worldPos) fixed destName1 = 1; fixed destName2 = UNITY_SHADOW_ATTENUATION(input, worldPos);
#endif

#ifdef POINT_COOKIE
#define ANICEL_LIGHT_ATTENUATION(destName1, destName2, input, worldPos) \
    unityShadowCoord3 lightCoord = mul(unity_WorldToLight, unityShadowCoord4(worldPos, 1)).xyz; \
    fixed shadow = UNITY_SHADOW_ATTENUATION(input, worldPos); \
    fixed destName1 = tex2D(_LightTextureB0, dot(lightCoord, lightCoord).rr).UNITY_ATTEN_CHANNEL * texCUBE(_LightTexture0, lightCoord).w; \
	fixed destName2 = destName1 * shadow;
#endif

#ifdef DIRECTIONAL_COOKIE
#define ANICEL_LIGHT_ATTENUATION(destName1, destName2, input, worldPos) \
    unityShadowCoord2 lightCoord = mul(unity_WorldToLight, unityShadowCoord4(worldPos, 1)).xy; \
    fixed shadow = UNITY_SHADOW_ATTENUATION(input, worldPos); \
    fixed destName1 = tex2D(_LightTexture0, lightCoord).w; \
	fixed destName2 =  destName1 * shadow;
#endif


#ifndef ANICEL_SCREENCOORDS
#define ANICEL_SCREENCOORDS(screenPos) (screenPos.xy / max(screenPos.w,0.000001)*half2(1, _ScreenParams.y / _ScreenParams.x))
#endif