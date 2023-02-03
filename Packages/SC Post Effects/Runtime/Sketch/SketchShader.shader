Shader "Hidden/SC Post Effects/Sketch"
{
	HLSLINCLUDE

	#define REQUIRE_DEPTH
	#include "../../Shaders/Pipeline/Pipeline.hlsl"

	TEXTURE2D(_Strokes);

	uniform float4 _Params;
	//X: Projection mode
	//Y: Blending mode
	//Z: Intensity
	//W: Tiling
	uniform float2 _Brightness;

	float Hatching(float2 uv, float NdotL) {
		half hatch = saturate(1 - NdotL);

		half3 tex = SAMPLE_TEXTURE2D(_Strokes, Repeat, uv).rgb;

		float dark = smoothstep(hatch, 0, tex.r) + _Brightness.x;
		float light = smoothstep(0, hatch, tex.g) * _Brightness.y;

		hatch = lerp(dark, light, NdotL);

		return saturate(hatch);
	}

	float3 Blend(float3 color, float3 hatch, float lum) {
		float3 col = color.rgb;

		//Effect-only
		if (_Params.y == 0)
			col = lerp(color.rgb, hatch.rgb, _Params.z);
		//Multiply
		if (_Params.y == 1)
			col = lerp(color.rgb, color.rgb * hatch.rgb, _Params.z);
		//Add
		if (_Params.y == 2)
		{
			col = lerp(color.rgb, color.rgb + hatch.rgb, _Params.z);
		}

		return saturate(col);
	}

	float4 FragWorldSpace(Varyings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		float4 screenColor = SCREEN_COLOR(UV);

		float depth = (SAMPLE_DEPTH(UV));

		//return DebugEyeIndex();

		float3 worldPos = GetWorldPosition(UV, depth);
		//return float4(frac(worldPos), 1.0);
		
		float3 worldUV = worldPos.xyz * 0.01 * _Params.w;
		float2 uvX = worldUV.yz;
		float2 uvY = worldUV.xz;
		float2 uvZ = worldUV.xy;

		//Use luminance to create a psuedo diffuse light weight
		float luminance = SRGBToLinear(Luminance(screenColor.rgb));

		//return luminance;

		float hatchX = Hatching(uvX, luminance);
		float hatchY = Hatching(uvY, luminance);
		float hatchZ = Hatching(uvZ, luminance);

		//float3 hatch = (hatchX * hatchY * hatchZ);
		float3 hatch = (hatchX + hatchY + hatchZ) * 0.33;
		hatch = saturate(hatch);

		if (LINEAR_DEPTH(depth) > 0.99) hatch = 1.0;

		float3 col = Blend(screenColor.rgb, hatch.rgb, luminance);

		return float4(col.rgb, screenColor.a);
	}

	float4 FragScreenSpace(Varyings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		float4 screenColor = SCREEN_COLOR(UV);

		half luminance = SRGBToLinear(Luminance(screenColor.rgb));

		UV.x *= _ScreenParams.x / _ScreenParams.y;
		float3 hatch = Hatching(UV * _Params.w, luminance);

		float3 col = Blend(screenColor.rgb, hatch.rgb, luminance);

		return float4(col.rgb, screenColor.a);
	}

	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass //0
		{
			HLSLPROGRAM
			#pragma multi_compile_vertex _ _USE_DRAW_PROCEDURAL
			#pragma exclude_renderers gles
			
			#pragma vertex Vert
			#pragma fragment FragWorldSpace

			ENDHLSL
		}

		Pass //1
		{
			HLSLPROGRAM
			#pragma multi_compile_vertex _ _USE_DRAW_PROCEDURAL
			#pragma exclude_renderers gles

			#pragma vertex Vert
			#pragma fragment FragScreenSpace

			ENDHLSL
		}
	}
}