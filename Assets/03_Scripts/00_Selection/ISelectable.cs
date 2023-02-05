using System;

using UnityEngine;

using F32   = System.Single;                
using F32x2 = Unity.Mathematics.float2;     
using F32x3 = Unity.Mathematics.float3;     

namespace CoolBeans.Selection
{
    public interface ISelectable
    {
        public GameObject gameObject { get; }
        
        public Transform transform { get; }
        
        void Select();
        void Deselect();

        public void Jump(F32x2 targetPosition, F32 jumpHeight, F32 jumpDuration, Action<ISelectable> onMadeJump = null);
        public void Fall(F32x2 targetPosition, F32 jumpHeight, F32 jumpDuration);
    }
}
