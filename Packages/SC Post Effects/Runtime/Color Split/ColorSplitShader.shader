Shader "Hidden/SC Post Effects/Color Split"
{
	HLSLINCLUDE

	#include "../../Shaders/Pipeline/Pipeline.hlsl"

	float4 _Params;

	#define OFFSET _Params.x
	#define EDGEFADE _Params.y

	#define EDGE_SIZE 2.5
	#define EDGE_FALLOFF 2.0

	float4 FragSingle(Varyings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float red = ScreenColor(UV - float2(OFFSET, 0)).r;
		float4 original = SCREEN_COLOR(UV);
		float blue = ScreenColor(UV + float2(OFFSET, 0)).b;

		float4 splitColors = float4(red, original.g, blue, original.a);

		float mask = lerp(1.0, EdgeMask(UV, EDGE_SIZE, EDGE_FALLOFF), EDGEFADE);
		//return mask;

		return float4(lerp(original.rgb, splitColors.rgb, mask), original.a);
	}

	float4 FragDouble(Varyings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float redX = ScreenColor(UV - float2(OFFSET, 0)).r;
		float redY = ScreenColor(UV - float2(0, OFFSET)).r;

		float4 original = SCREEN_COLOR(UV);

		float blueX = ScreenColor(UV + float2(OFFSET, 0)).b;
		float blueY = ScreenColor(UV + float2(0, OFFSET)).b;

		float4 splitColorsX = float4(redX, original.g, blueX, original.a);
		float4 splitColorsY = float4(redY, original.g, blueY, original.a);

		float4 blendedColors = (splitColorsX + splitColorsY) * 0.5;

		float mask = lerp(1.0, EdgeMask(UV, EDGE_SIZE, EDGE_FALLOFF), EDGEFADE);

		return float4(lerp(original.rgb, blendedColors.rgb, mask), original.a);
	}

	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			Name "Color Split: Horizontal"
			HLSLPROGRAM
			#pragma multi_compile_vertex _ _USE_DRAW_PROCEDURAL
			#pragma exclude_renderers gles

			#pragma vertex Vert
			#pragma fragment FragSingle

			ENDHLSL
		}
		Pass
		{
			Name "Color Split: Horizontal + Vertical"
			HLSLPROGRAM
			#pragma multi_compile_vertex _ _USE_DRAW_PROCEDURAL
			#pragma exclude_renderers gles

			#pragma vertex Vert
			#pragma fragment FragDouble

			ENDHLSL
		}
	}
}