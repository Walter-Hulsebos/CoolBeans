using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.MethodImplOptions;

using F32     = System.Single;
using F32x2   = Unity.Mathematics.float2;
using F32x3   = Unity.Mathematics.float3;
using F32x4   = Unity.Mathematics.float4;
using F32x4x4 = Unity.Mathematics.float4x4;

using I32   = System.Int32;

namespace CoolBeans.Utils
{
    public static class F32x3Extensions
    {
        /// <summary>
        /// Finds the middle point between two points
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static F32x3 Middle(this F32x3 a, F32x3 b)
        {
            return (a + b) * 0.5f;
        }
        
        [MethodImpl(AggressiveInlining)]
        public static F32x3 Delta(this F32x3 a, F32x3 b)
        {
            return (b - a);
        }
    }
}
