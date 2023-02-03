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
        public static F32x2 WorldToCamera(this Camera camera, F32x2 position)
        {
            //manual implementation of Camera.WorldToScreenPoint
            F32x4x4 __worldToCameraMatrix = camera.worldToCameraMatrix;
            F32x4x4 __projectionMatrix    = camera.projectionMatrix;
            
            return mul(mul(__worldToCameraMatrix, __projectionMatrix), new F32x4(position, z: 0f, w: 1f)).xy;
        }
        [MethodImpl(AggressiveInlining)]
        public static F32x2 WorldToCamera(this Camera camera, F32x3 position)
        {
            //manual implementation of Camera.WorldToScreenPoint
            F32x4x4 __worldToCameraMatrix = camera.worldToCameraMatrix;
            F32x4x4 __projectionMatrix    = camera.projectionMatrix;
            
              return mul(mul(__worldToCameraMatrix, __projectionMatrix), new F32x4(position, w: 1f)).xy;
        }
        
        [MethodImpl(AggressiveInlining)]
        public static F32x2 CameraToWorld(this Camera camera, F32x2 position)
        {
            //manual implementation of Camera.ScreenToWorldPoint
            F32x4x4 __worldToCameraMatrix = camera.worldToCameraMatrix;
            F32x4x4 __projectionMatrix    = camera.projectionMatrix;
            
            return mul(mul(__worldToCameraMatrix, __projectionMatrix), new F32x4(position, z: 0f, w: 1f)).xy;
        }
        [MethodImpl(AggressiveInlining)]
        public static F32x3 CameraToWorld(this Camera camera, F32x3 position)
        {
            //manual implementation of Camera.ScreenToWorldPoint
            F32x4x4 __worldToCameraMatrix = camera.worldToCameraMatrix;
            F32x4x4 __projectionMatrix    = camera.projectionMatrix;
            
            return mul(mul(__worldToCameraMatrix, __projectionMatrix), new F32x4(position, w: 1f)).xyz;
        }
    }
}
