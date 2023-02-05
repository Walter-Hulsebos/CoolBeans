using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.Mathematics.math;

using DG.Tweening;
using UnityEngine.UI;
using F32   = System.Single;
using F32x2 = Unity.Mathematics.float2;
using F32x3 = Unity.Mathematics.float3;

using I32   = System.Int32;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace CoolBeans
{
    using CoolBeans.Selection;
    using static CoolBeans.Selection.Selection;
    using CoolBeans.Sprouts;
    
    public sealed class MovementManager : MonoBehaviour
    {
        [SerializeField] private InputActionReference         actInputReference;
        [SerializeField, HideInInspector] private InputAction actInput;
        
        [SerializeField] private F32 maxJumpDistance = 10f;

        [SerializeField] private F32 jumpTimePerUnit = 0.08f;

        [SerializeField] private new Camera camera;

        [SerializeField] private LayerMask stalkLayerMask = (LayerMask)(1 << 7);
        [SerializeField, HideInInspector] private I32 stalkLayerMaskValue;
        
        [SerializeField] private LayerMask flowerLayerMask = (LayerMask)(1 << 8);
        [SerializeField, HideInInspector] private I32 flowerLayerMaskValue;
        
        [SerializeField] private Image cursorUI;

        [SerializeField] private Sprite cursorTextureAboveFlower;
        [SerializeField] private Sprite cursorTextureAboveStalk;
        [SerializeField] private Sprite cursorTextureAboveNothing;
        
        //[SerializeField] private ExtEvent onAllBeansAreDead 

        private F32x2 ScreenSafeAreaSize     => Screen.safeArea.size;
        private F32x2 ScreenSafeAreaSizeHalf => ScreenSafeAreaSize * 0.5f;
        
        private F32x2 ScreenSafeAreaHeightOffset => new F32x2(x: 0,                     y: ScreenSafeAreaSize.y);
        private F32x2 ScreenSafeAreaWidthOffset  => new F32x2(x: ScreenSafeAreaSize.x, y: 0);

        private F32x2 MousePositionCurrentCameraSpace => ((F32x2)Mouse.current.position.ReadValue());
        private F32x3 MousePositionCurrentWorldSpace  => camera.ScreenToWorldPoint(position: new F32x3(xy: MousePositionCurrentCameraSpace, z: camera.nearClipPlane));
        private F32x2 MousePositionCurrentCameraSpaceCentered => (MousePositionCurrentCameraSpace - ScreenSafeAreaHeightOffset);
        
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
            //actInput = actInputReference.action;

            actInput.Enable();
            actInput.started += OnInputStarted;
        }
        private void OnDisable()
        {
            actInput.started -= OnInputStarted;
            actInput.Disable();
        }
        
        // private void Awake()
        // {
        //     _screenSafeAreaSize     = (F32x2)Screen.safeArea.size;
        //     ScreenSafeAreaHeightOffset = new F32x2(x: 0,                     y: _screenSafeAreaSize.y);
        //     ScreenSafeAreaWidthOffset  = new F32x2(x: _screenSafeAreaSize.x, y: 0);
        //     _screenSafeAreaSizeHalf = _screenSafeAreaSize * 0.5f;
        // }

        private readonly Collider2D[] _flowerHitBuffer = new Collider2D[8];
        private I32                   _flowerHitCount  = 0;
        
        private Collider2D[] _flowerHitCache      = new Collider2D[8];
        private I32          _flowerHitCountCache = 0;

        private Boolean _mouseIsOverFlower  = false;
        private Boolean _mouseIsOverStalk   = false;
        private Boolean _mouseIsOverNothing = false;
        
        private void Update()
        {
            F32x3 __mousePositionCurrentWorldSpace = MousePositionCurrentWorldSpace;
            
            // if (Physics2D.CircleCast(
            //         origin:    MousePositionCurrentWorldSpace.xy, 
            //         radius:    1.0f, 
            //         direction: Vector2.right, //why does a circle cast need a direction?
            //         results:   _flowerHitBuffer,
            //         distance:  Mathf.Infinity,
            //         layerMask: stalkLayerMaskValue | flowerLayerMaskValue)
            //     )

            _flowerHitCount = Physics2D.OverlapPointNonAlloc(
                point:    __mousePositionCurrentWorldSpace.xy,
                results:   _flowerHitBuffer,
                layerMask: flowerLayerMaskValue);
            
            //Do flowers first.
            _mouseIsOverFlower = (_flowerHitCount > 0);
            if(_mouseIsOverFlower)
            {
                cursorUI.sprite = cursorTextureAboveFlower;
            }
            
            //Then stalks.
            // _flowerHitCount = Physics2D.CircleCastNonAlloc(
            //     origin:    __mousePositionCurrentWorldSpace.xy, 
            //     radius:    0.1f, 
            //     direction: Vector2.right, //why does a circle cast need a direction?
            //     results:   _flowerHitBuffer,
            //     distance:  Mathf.Infinity,
            //     layerMask: stalkLayerMaskValue);
            
            Collider2D __collider2D = Physics2D.OverlapPoint(point: __mousePositionCurrentWorldSpace.xy, layerMask: stalkLayerMaskValue);
            _mouseIsOverStalk = (__collider2D != null);
            if (_mouseIsOverStalk)
            {
                cursorUI.sprite = cursorTextureAboveStalk;

            }
            
            if(!_mouseIsOverFlower && !_mouseIsOverStalk)
            {
                cursorUI.sprite = cursorTextureAboveNothing;
            }

            cursorUI.rectTransform.anchoredPosition = MousePositionCurrentCameraSpaceCentered;
        }

        private void OnInputStarted(InputAction.CallbackContext ctx)
        {
            F32x3 __mousePositionCurrentWorldSpace = MousePositionCurrentWorldSpace;
            
            if (_mouseIsOverFlower)
            {
                Debug.Log("Has hit a flower, moving to position!");

                _flowerHitCache      = _flowerHitBuffer;
                _flowerHitCountCache = _flowerHitCount;
                JumpAllSelectedBeansToPoint(targetPosition: __mousePositionCurrentWorldSpace.xy, onMadeJumpFirst: OnMadeJumpToFlowerFirst);
            }
            else if (_mouseIsOverStalk)
            {
                //Debug.Log("Has hit a stalk, moving to position!");
                
                JumpAllSelectedBeansToPoint(targetPosition: __mousePositionCurrentWorldSpace.xy);
            }
            else
            {
                //Debug.Log("Has hit nothing!");
            }
        }

        private void OnMadeJumpToFlowerFirst(ISelectable bean)
        {
            Debug.Log("OnMadeJumpToFlowerFirst");
            
            if(_flowerHitCountCache > 0)
            {
                Debug.Log("There is a flower.");
                
                Collider2D __flowerHit = _flowerHitCache[0];

                if (!__flowerHit.transform.TryGetComponent(out Flower __flowerComponent)) return;
                
                __flowerComponent.Blossom();
                Destroy(bean.gameObject);
            }
        }

        private void JumpAllSelectedBeansToPoint(F32x2 targetPosition, Action<ISelectable> onMadeJumpFirst = null, Action<ISelectable> onMadeJumpOther = null)
        {
            //for (I32 __index = 0; __index < Instance.SelectedUnits.Count; __index++)
            I32 __index = 0;
            
            foreach (ISelectable __bean in Instance.SelectedUnits)
            {
                //ISelectable __bean = Instance.SelectedUnits[__index];

                F32x2 __beanPosition = (Vector2) __bean.transform.position;

                F32x2 __beanTargetPosition = targetPosition;

                //Get random position around target position, on the stalk.

                if (__index != 0)
                {
                    Boolean __hasNoRandomPosOnStalk = true;
                    F32x2 __randomPosOnStalk = __beanTargetPosition;
                    I32 __attempts = 0;

                    const F32 R_RADIUS = 2f;
                    const I32 MAX_ATTEMPTS = 20;

                    while (__hasNoRandomPosOnStalk || __attempts < MAX_ATTEMPTS)
                    {
                        F32x2 __randomOffset = new(
                            x: Random.Range(-R_RADIUS, +R_RADIUS), 
                            y: Random.Range(-R_RADIUS, +R_RADIUS));
                        F32x2 __randomSample = __beanTargetPosition + __randomOffset;

                        //Check if position is on stalk.
                        Collider2D __collider2D = Physics2D.OverlapPoint(point: __randomSample, layerMask: stalkLayerMaskValue);
                        if (__collider2D != null)
                        {
                            __randomPosOnStalk = __randomSample;
                            __hasNoRandomPosOnStalk = false;
                        }

                        __attempts += 1;
                    }

                    __beanTargetPosition = __randomPosOnStalk;
                }

                //F32x2 __direction = normalize(__randomPosOnStalk - __beanPosition);
                F32 __distance = distance(__beanTargetPosition, __beanPosition);
                F32 __jumpHeight = (__distance > 5f) ? 5f : (__distance * 0.75f);
                F32 __jumpTime = jumpTimePerUnit * __distance;

                Boolean __canReachTarget = (__distance <= maxJumpDistance);

                if (__canReachTarget)
                {
                    __bean.Jump(__beanTargetPosition, jumpHeight: __jumpHeight, jumpDuration: __jumpTime, onMadeJump: (__index == 0) ? onMadeJumpFirst : onMadeJumpOther);
                }
                else
                {
                    __bean.Fall(__beanTargetPosition, jumpHeight: __jumpHeight, jumpDuration: __jumpTime);
                }

                __index += 1;
            }
        }

        // public F32 jumpForce = 10f;
        // public void JumpTowards(Rigidbody2D body, Vector2 targetPosition)
        // {
        //     Vector2 direction     = (targetPosition - (Vector2)transform.position).normalized;
        //     float distance        = Vector2.Distance(transform.position, targetPosition);
        //     float jumpDistance    = Mathf.Min(distance, maxJumpDistance);
        //     Vector2 jumpDirection = direction * jumpDistance;
        //     body.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);
        // }

    }
}
