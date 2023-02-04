using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.Mathematics.math;

using F32   = System.Single;
using F32x2 = Unity.Mathematics.float2;
using F32x3 = Unity.Mathematics.float3;

using I32   = System.Int32;

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
            _hitCount = Physics2D.CircleCastNonAlloc(
                origin:    __mousePositionCurrentWorldSpace.xy, 
                radius:    1.0f, 
                direction: Vector2.right, //why does a circle cast need a direction?
                results:   _hitBuffer,
                distance:  Mathf.Infinity,
                layerMask: stalkLayerMaskValue);

            if (_hitCount > 0)
            {
                Debug.Log("Has hit a stalk, moving to position!");
                
                RaycastHit2D __hitInfo = _hitBuffer[0];

                JumpAllSelectedBeansToPoint(__hitInfo.point);
            }
        }
        
        private void JumpAllSelectedBeansToPoint(F32x2 point)
        {
            HashSet<ISelectable> __beans = Instance.SelectedUnits;
            
            foreach (ISelectable __bean in __beans)
            {
                F32x2 __beanPosition = (Vector2)__bean.transform.position;
                
                F32x2 __direction = normalize(point - __beanPosition);
                F32   __distance  = distance(point, __beanPosition);
                
                //F32 __distanceClamped = clamp(__distance, 0f, maxJumpDistance);
                
                //F32x3 __targetPosition = __beanPosition + (__direction * __distanceClamped);
                
                //__bean.transform.position = (Vector3)(Vector2)point;
                
                JumpTowards(__bean.transform.GetComponent<Rigidbody2D>(), point);
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
