Shader "Hidden/SC Post Effects/Transition"
{
	HLSLINCLUDE

	#include "../../Shaders/Pipeline/Pipeline.hlsl"

	TEXTURE2D(_Gradient);
	SamplerState sampler_Gradient;

	float _Progress;

	float4 Frag(Varyings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float4 screenColor = SCREEN_COLOR(UV);

		float gradientTex = SAMPLE_TEXTURE2D(_Gradient, sampler_Gradient, UV).r;

		float alpha = smoothstep(gradientTex, _Progress, 1.01);

		return float4(lerp(screenColor.rgb, 0, alpha), screenColor.a);
	}

	ENDHLSL

	SubShader
	{
	Cull Off ZWrite Off ZTest Always

		Pass
		{
			Name "Transition"
			HLSLPROGRAM
			#pragma multi_compile_vertex _ _USE_DRAW_PROCEDURAL
			#pragma exclude_renderers gles

			#pragma vertex Vert
			#pragma fragment Frag

			ENDHLSL
		}
	}
}