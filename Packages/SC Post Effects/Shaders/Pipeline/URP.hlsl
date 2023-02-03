// SC Post Effects
// Staggart Creations
// http://staggart.xyz
// Copyright protected under Unity Asset Store EULA

//Libraries
#define URP
#ifdef UNITY_GRAPHFUNCTIONS_LW_INCLUDED //Shader Graph
#define SHADER_GRAPH
#endif

//Sampling macro's have changed
#define VERSION_2_2_1

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "../Blending.hlsl"

#ifndef SHADER_GRAPH //Shader Graph
//Vertex shader
#if UNITY_VERSION >= 202220
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
#elif UNITY_VERSION >= 202020 
#include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Fullscreen.hlsl"
#elif UNITY_VERSION >= 202210
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
#else
#include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
#endif

#if UNITY_VERSION >= 202220 
#define UV input.texcoord.xy
#else
#define UV input.uv.xy
#endif
#endif

#ifndef UNITY_CORE_BLIT_INCLUDED //Core SRP Blit library, currently not used
SamplerState sampler_LinearClamp;
SamplerState sampler_LinearRepeat;
#endif

#define Clamp sampler_LinearClamp
#define Repeat sampler_LinearRepeat

TEXTURE2D_X(_MainTex); /* Always included */
SAMPLER(sampler_MainTex);
float4 _MainTex_TexelSize;

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#define SAMPLE_DEPTH(uv) SampleSceneDepth(uv) //Use function from DeclareDepthTexture, uses a float sampler
#define LINEAR_DEPTH(depth) Linear01Depth(depth, _ZBufferParams)
#define LINEAR_EYE_DEPTH(depth) LinearEyeDepth(depth, _ZBufferParams)

#if ((UNITY_VERSION < 202020))
//Depth normals pass not available
#define _RECONSTRUCT_NORMAL 1
#endif

#if _RECONSTRUCT_NORMAL
TEXTURE2D_X(_CameraDepthNormalsTexture);
#else
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
#endif

#define SAMPLE_DEPTH_NORMALS(uv) SampleDepthNormals(uv)

float4 SampleDepthNormals(float2 uv)
{
	float3 normals = 0;
	
	#if _RECONSTRUCT_NORMAL
	normals = SAMPLE_TEXTURE2D_X(_CameraDepthNormalsTexture, Clamp, uv).rgb;
	#else
	normals = SampleSceneNormals(uv);
	normals.z = 0;
	#endif
	
	//Remap to -1/+1 ranges
	normals = float3(normals.x * 2.0 - 1.0, normals.y * 2.0 - 1.0, 0);
	
	//Mirror behavior of Built-in RP, where linear depth is stored in the .W component
	float linearDepth = LINEAR_DEPTH(SAMPLE_DEPTH(uv));
	
	return float4(normals, linearDepth);
}

#define SCREEN_COLOR(uv) SAMPLE_TEXTURE2D_X(_MainTex, Clamp, uv);

//Shorthand for sampling MainTex
float4 ScreenColor(float2 uv) {
	return SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv);
}

float4 ScreenColorTiled(float2 uv) {
	return SAMPLE_TEXTURE2D_X(_MainTex, Repeat, uv);
}

//Generic functions
#include "../SCPE.hlsl"
#define WorldViewDirection -GetWorldToViewMatrix()[2].xyz

//unity_MatrixVP and unity_MatrixV aren't consistently set up, so it's being done through script
//Works fine in Single Pass Instanced VR rendering

#if _USE_DRAW_PROCEDURAL
float4x4 viewProjectionArray[2];
#undef UNITY_MATRIX_VP
#define UNITY_MATRIX_VP viewProjectionArray[unity_StereoEyeIndex]
#else
float4x4 viewProjection;
#undef UNITY_MATRIX_VP
#define UNITY_MATRIX_VP viewProjection

//Camera center, so same data for both eyes
float4x4 viewMatrix;
#undef UNITY_MATRIX_V
#define UNITY_MATRIX_V viewMatrix
#endif