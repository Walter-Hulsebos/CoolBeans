using System;
using Cinemachine;
using DG.Tweening;
using ExtEvents;
using JetBrains.Annotations;
using UnityEngine;

using F32   = System.Single;
using F32x2 = Unity.Mathematics.float2;
using F32x3 = Unity.Mathematics.float3;

namespace CoolBeans.Selection
{
    public sealed class SelectableBean : MonoBehaviour, ISelectable
    {
        #region Events

        [field:SerializeField] 
        public ExtEvent OnSelected   { get; [UsedImplicitly] private set; } = new();
        [field:SerializeField] 
        public ExtEvent OnDeselected { get; [UsedImplicitly] private set; } = new();
        
        [field:SerializeField]                                                           
        public ExtEvent OnJump       { get; [UsedImplicitly] private set; } = new();     
        
        [field:SerializeField]
        public ExtEvent OnFall       { get; [UsedImplicitly] private set; } = new();           

        #endregion
        
        private CinemachineTargetGroup _targetGroup;

        #region Methods
        
        private void OnEnable()
        {
            Selection.Instance.Add(unit: this);
        }
        private void OnDisable()
        {
            Selection.Instance.Remove(unit: this);
            
            _targetGroup.RemoveMember(transform);
        }

        private void Start()
        {
            _targetGroup = FindObjectOfType<CinemachineTargetGroup>();
            _targetGroup.AddMember(transform, weight: 1, radius: 4);
        }

        public void Select()
        {
            //Debug.Log(message: "Selected " + gameObject.name, context: this);
            OnSelected.Invoke();
        }

        public void Deselect()
        {
            //Debug.Log(message: "Deselected " + gameObject.name, context: this);
            OnDeselected.Invoke();
        }

        public void Jump(F32x2 targetPosition, F32 jumpHeight, F32 jumpDuration, Action<ISelectable> onMadeJump = null)
        {
            transform.DOJump(endValue: (Vector2)targetPosition, jumpPower: jumpHeight, numJumps: 1, duration: jumpDuration).onComplete += () => onMadeJump?.Invoke(obj: this);
        }

        public void Fall(F32x2 targetPosition, F32 jumpHeight, F32 jumpDuration)
        {
            Vector3 __missedTargetPosition = (Vector2)(targetPosition.xy + new F32x2(x: 0, y: -10));
            
            transform.DOJump(endValue: __missedTargetPosition, jumpPower: jumpHeight, numJumps: 1, duration: jumpDuration)
                .onComplete += () =>
            {
                //Enable physics.
                Debug.Log("Missed target, enabling physics!");
            };
        }

        #endregion
    }
}
