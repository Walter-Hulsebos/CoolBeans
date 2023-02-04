using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.Mathematics.math;

using F32   = System.Single;
using F32x2 = Unity.Mathematics.float2;
using F32x3 = Unity.Mathematics.float3;

using I32   = System.Int32;
using Random = UnityEngine.Random;

namespace CoolBeans
{
    using CoolBeans.Selection;
    using static CoolBeans.Selection.Selection;
    
    public sealed class MovementManager : MonoBehaviour
    {
        [SerializeField] private InputActionReference         actInputReference;
        [SerializeField, HideInInspector] private InputAction actInput;
        
        [SerializeField] private F32 maxJumpDistance = 10f;

        [SerializeField] private new Camera camera;

        [SerializeField] private LayerMask stalkLayerMask = (LayerMask)(1 << 7);
        [SerializeField, HideInInspector] private I32 stalkLayerMaskValue;
        
        [SerializeField] private LayerMask flowerLayerMask = (LayerMask)(1 << 8);
        [SerializeField, HideInInspector] private I32 flowerLayerMaskValue;
        
        private F32x2 _screenSafeAreaSize;
        private F32x2 _screenSafeAreaSizeHalf;
        
        private F32x2 MousePositionCurrentCameraSpace => ((F32x2)Mouse.current.position.ReadValue());
        private F32x3 MousePositionCurrentWorldSpace  => camera.ScreenToWorldPoint(position: new F32x3(xy: MousePositionCurrentCameraSpace, z: camera.nearClipPlane));

        #if UNITY_EDITOR
        private void Reset()
        {
            camera = Camera.main;
            
            stalkLayerMaskValue  = stalkLayerMask.value;
            flowerLayerMaskValue = flowerLayerMask.value;
            
            actInput             = actInputReference.action;
        }
        private void OnValidate()
        {
            if (camera == null)
            {
                camera = Camera.main;
            }
            
            stalkLayerMaskValue  = stalkLayerMask.value;
            flowerLayerMaskValue = flowerLayerMask.value;
            
            actInput             = actInputReference.action;
        }
        #endif
        
        private void OnEnable()
        {
            actInput = actInputReference.action;

            actInput.Enable();
            
            actInput.started += OnInputStarted;
        }
        private void OnDisable()
        {
            actInput.started -= OnInputStarted;
            
            actInput.Disable();
        }
        
        private void Awake()
        {
            _screenSafeAreaSize     = (F32x2)Screen.safeArea.size;
            _screenSafeAreaSizeHalf = _screenSafeAreaSize * 0.5f;
        }

        private readonly RaycastHit2D[] _hitBuffer = new RaycastHit2D[8];
        private I32                     _hitCount  = 0;
        
        private void OnInputStarted(InputAction.CallbackContext ctx)
        {
            F32x3 __mousePositionCurrentWorldSpace = MousePositionCurrentWorldSpace;
            
            // if (Physics2D.CircleCast(
            //         origin:    MousePositionCurrentWorldSpace.xy, 
            //         radius:    1.0f, 
            //         direction: Vector2.right, //why does a circle cast need a direction?
            //         results:   _hitBuffer,
            //         distance:  Mathf.Infinity,
            //         layerMask: stalkLayerMaskValue | flowerLayerMaskValue)
            //     )

            _hitCount = Physics2D.CircleCastNonAlloc(
                origin:    __mousePositionCurrentWorldSpace.xy, 
                radius:    1.0f, 
                direction: Vector2.right, //why does a circle cast need a direction?
                results:   _hitBuffer,
                distance:  Mathf.Infinity,
                layerMask: flowerLayerMaskValue);
            
            //Do flowers first.
            if(_hitCount > 0)
            {
                Debug.Log("Has hit a flower, moving to position!");
                
                RaycastHit2D __hitInfo = _hitBuffer[0];

                JumpAllSelectedBeansToPoint(__hitInfo.point);
                return;
            }
            
            //Then stalks.
            Physics2D.OverlapPoint(__mousePositionCurrentWorldSpace.xy, layerMask: stalkLayerMaskValue);
            
            // _hitCount = Physics2D.CircleCastNonAlloc(
            //     origin:    __mousePositionCurrentWorldSpace.xy, 
            //     radius:    0.1f, 
            //     direction: Vector2.right, //why does a circle cast need a direction?
            //     results:   _hitBuffer,
            //     distance:  Mathf.Infinity,
            //     layerMask: stalkLayerMaskValue);
            
            Collider2D __collider2D = Physics2D.OverlapPoint(point: __mousePositionCurrentWorldSpace.xy, layerMask: stalkLayerMaskValue);
            if (__collider2D != null)
            {
                //Debug.Log("Has hit a stalk, moving to position!");
                //RaycastHit2D __hitInfo = _hitBuffer[0];

                JumpAllSelectedBeansToPoint(__mousePositionCurrentWorldSpace.xy);
            }
        }
        
        private void JumpAllSelectedBeansToPoint(F32x2 targetPosition)
        {
            HashSet<ISelectable> __beans = Instance.SelectedUnits;
            
            foreach (ISelectable __bean in __beans)
            {
                F32x2 __beanPosition = (Vector2)__bean.transform.position;
                
                //Get random position around target position, on the stalk.

                Boolean __hasNoRandomPosOnStalk = true;
                F32x2   __randomPosOnStalk      = targetPosition;
                I32     __attempts              = 0;
                
                const F32 R_RADIUS = 1f;
                const I32 MAX_ATTEMPTS = 20;
                
                while (__hasNoRandomPosOnStalk || __attempts < MAX_ATTEMPTS)
                {
                    F32x2 __randomOffset = new(x: Random.Range(-R_RADIUS, +R_RADIUS), y: Random.Range(-R_RADIUS, +R_RADIUS));
                    F32x2 __randomSample = targetPosition + __randomOffset;
                    
                    //Check if position is on stalk.
                    Collider2D __collider2D = Physics2D.OverlapPoint(point: __randomSample, layerMask: stalkLayerMaskValue);
                    if (__collider2D != null)
                    {
                        __randomPosOnStalk      = __randomSample;
                        __hasNoRandomPosOnStalk = false;
                    }
                    
                    __attempts += 1;
                }

                
                F32x2 __direction = normalize(__randomPosOnStalk - __beanPosition);
                F32   __distance  = distance(__randomPosOnStalk, __beanPosition);
                
                Boolean __canReachTarget = (__distance <= maxJumpDistance);

                if (__canReachTarget)
                {
                    //Debug.Log("Can reach target!");
                    __bean.transform.DOJump(endValue: (Vector2)__randomPosOnStalk, jumpPower: 5f, numJumps: 1, duration: 0.5f);
                }
                else
                {
                    Vector3 __missedTargetPosition = (Vector2)(__randomPosOnStalk + new F32x2(x: 0, y: -10));

                    Debug.Log("Can't reach target, jumping to missed target position!");
                    //TODO: Add miss target effect/animation.
                    __bean.transform.DOJump(endValue: __missedTargetPosition, jumpPower: 5f, numJumps: 1, duration: 0.6f)
                        .onComplete += () =>
                    {
                        //Enable physics.
                        Debug.Log("Missed target, enabling physics!");
                    };
                }
                
                //F32 __distanceClamped = clamp(__distance, 0f, maxJumpDistance);
                
                //F32x3 targetPosition = __beanPosition + (__direction * __distanceClamped);
                
                //__bean.transform.position = (Vector3)(Vector2)targetPosition;
                //JumpTowards(__bean.transform.GetComponent<Rigidbody2D>(), targetPosition);
            }
        }
        
        public F32 jumpForce = 10f;
        public void JumpTowards(Rigidbody2D body, Vector2 targetPosition)
        {
            Vector2 direction     = (targetPosition - (Vector2)transform.position).normalized;
            float distance        = Vector2.Distance(transform.position, targetPosition);
            float jumpDistance    = Mathf.Min(distance, maxJumpDistance);
            Vector2 jumpDirection = direction * jumpDistance;
            body.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);
        }

    }
}
