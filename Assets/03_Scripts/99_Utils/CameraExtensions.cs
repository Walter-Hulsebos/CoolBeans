using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static System.Runtime.CompilerServices.MethodImplOptions;
using static Unity.Mathematics.math;

using F32     = System.Single;
using F32x2   = Unity.Mathematics.float2;
using F32x3   = Unity.Mathematics.float3;
using F32x4   = Unity.Mathematics.float4;
using F32x4x4 = Unity.Mathematics.float4x4;

using I32   = System.Int32;


namespace CoolBeans
{
    public static class CameraExtensions
    {
        [MethodImpl(AggressiveInlining)]
        public static F32x2 ToCameraSpace(this Camera camera, F32x2 worldSpacePosition)
        {
            //manual implementation of Camera.WorldToScreenPoint
            F32x4x4 __worldToCameraMatrix = camera.worldToCameraMatrix;
            F32x4x4 __projectionMatrix    = camera.projectionMatrix;
            
            return mul(mul(__worldToCameraMatrix, __projectionMatrix), new F32x4(worldSpacePosition, z: 0f, w: 1f)).xy;
        }
        [MethodImpl(AggressiveInlining)]
        public static F32x2 ToCameraSpace(this Camera camera, F32x3 worldSpacePosition)
        {
            //manual implementation of Camera.WorldToScreenPoint
            F32x4x4 __worldToCameraMatrix = camera.worldToCameraMatrix;
            F32x4x4 __projectionMatrix    = camera.projectionMatrix;
            
              return mul(mul(__worldToCameraMatrix, __projectionMatrix), new F32x4(worldSpacePosition, w: 1f)).xy;
        }
        
        [MethodImpl(AggressiveInlining)]
        public static F32x2 ToWorldSpace(this Camera camera, F32x2 cameraSpacePosition)
        {
            //manual implementation of Camera.ScreenToWorldPoint
            F32x4x4 __worldToCameraMatrix = camera.worldToCameraMatrix;
            F32x4x4 __projectionMatrix    = camera.projectionMatrix;
            
            return mul(mul(__worldToCameraMatrix, __projectionMatrix), new F32x4(cameraSpacePosition, z: 0f, w: 1f)).xy;
        }
        [MethodImpl(AggressiveInlining)]
        public static F32x3 ToWorldSpace(this Camera camera, F32x3 cameraSpacePosition)
        {
            //manual implementation of Camera.ScreenToWorldPoint
            F32x4x4 __worldToCameraMatrix = camera.worldToCameraMatrix;
            F32x4x4 __projectionMatrix    = camera.projectionMatrix;
            
            return mul(mul(__worldToCameraMatrix, __projectionMatrix), new F32x4(cameraSpacePosition, w: 1f)).xyz;
        }
    }
}
