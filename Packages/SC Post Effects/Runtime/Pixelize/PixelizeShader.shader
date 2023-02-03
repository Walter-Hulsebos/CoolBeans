Shader "Hidden/SC Post Effects/Pixelize"
{
	HLSLINCLUDE

	#include "../../Shaders/Pipeline/Pipeline.hlsl"

	float _Resolution;
	float _Scale;
	float _PixelScale;

	float4 Frag(Varyings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		const float offset = 0.5;

		float2 scale = (_PixelScale / _ScreenParams.xy) * _Scale;
		float x = round((UV.x / scale.x) + offset) * scale.x;
		float y = round((UV.y / scale.y) + offset) * scale.y;

		return ScreenColor(float2(x,y));
	}

	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			Name "Pixelize"
			HLSLPROGRAM
			#pragma multi_compile_vertex _ _USE_DRAW_PROCEDURAL
			#pragma exclude_renderers gles

			#pragma vertex Vert
			#pragma fragment Frag

			ENDHLSL
		}
	}
}