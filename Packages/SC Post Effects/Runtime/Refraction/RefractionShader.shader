Shader "Hidden/SC Post Effects/Refraction"
{
	HLSLINCLUDE

	#include "../../Shaders/Pipeline/Pipeline.hlsl"

	TEXTURE2D(_RefractionTex);
	uniform float _Amount;

	float4 Frag(Varyings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float4 dudv = SAMPLE_TEXTURE2D(_RefractionTex, Clamp, UV).rgba;

		float2 refraction = lerp(UV, (UV) * dudv.rg, _Amount * dudv.rg);

		return ScreenColor(refraction);
	}

	float4 FragNormalMap(Varyings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float4 dudv = SAMPLE_TEXTURE2D(_RefractionTex, Clamp, UV).rgba;

#if UNITY_VERSION >= 20172 //Pre 2017.2
		dudv.x *= dudv.w;
#else
		dudv.x = 1 - dudv.x;
#endif
		//Remap to -1,-1
		dudv.xy = dudv.xy * 2 - 1;

		float2 refraction = lerp(UV, (UV)* dudv.rg, _Amount * dudv.rg);

		return ScreenColor(refraction);
	}

	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			Name "Refraction"
			HLSLPROGRAM
			#pragma multi_compile_vertex _ _USE_DRAW_PROCEDURAL
			#pragma exclude_renderers gles

			#pragma vertex Vert
			#pragma fragment Frag

			ENDHLSL
		}
		Pass
		{
			Name "Refraction by normal map"
			HLSLPROGRAM
			#pragma multi_compile_vertex _ _USE_DRAW_PROCEDURAL
			#pragma exclude_renderers gles

			#pragma vertex Vert
			#pragma fragment FragNormalMap

			ENDHLSL
		}
	}
}