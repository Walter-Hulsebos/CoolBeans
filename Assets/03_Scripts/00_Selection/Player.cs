using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using static Unity.Mathematics.math;

using F32   = System.Single;
using F32x2 = Unity.Mathematics.float2;

using I32   = System.Int32;

namespace CoolBeans.Selection
{
    public sealed class Player : MonoBehaviour
    {
        [SerializeField] private InputActionReference         selectActionReference;
        [SerializeField, HideInInspector] private InputAction selectAction;
        
        [SerializeField] private InputActionReference         addToSelectionActionReference;
        [SerializeField, HideInInspector] private InputAction addToSelectionAction;
        
        [SerializeField] private InputActionReference         removeFromSelectionActionReference;
        [SerializeField, HideInInspector] private InputAction removeFromSelectionAction;
    
        [SerializeField] private new Camera camera;
        
        [SerializeField] private RectTransform selectionBox;
        [SerializeField] private RectTransform cursor;
        
        [SerializeField] private LayerMask unitLayerMask  = (LayerMask)(1 << 6);
        [SerializeField, HideInInspector] private I32 unitLayerMaskValue;
        
        [SerializeField] private LayerMask stalkLayerMask = (LayerMask)(1 << 0);
        [SerializeField, HideInInspector] private I32 floorLayerMaskValue;

        /// <summary>
        /// Minimum distance squared the mouse has to move (in pixels) before a drag is registered.
        /// </summary>
        private const F32 MIN_DISTANCE_SQR_FOR_DRAG = 20f;

        private readonly HashSet<ISelectable> _newlySelectedUnits = new();
        private readonly HashSet<ISelectable> _deselectedUnits    = new();

        private F32x2 _mousePositionStart;
        private static F32x2 MousePositionCurrent => (F32x2)(Mouse.current.position.ReadValue() - Screen.safeArea.size * 0.5f);
        
        #if UNITY_EDITOR
        private void Reset()
        {
            camera = Camera.main;
            
            unitLayerMaskValue  = unitLayerMask.value;
            floorLayerMaskValue = stalkLayerMask.value;
        }
        private void OnValidate()
        {
            unitLayerMask.value  = unitLayerMaskValue;
            stalkLayerMask.value = floorLayerMaskValue;
        }
        #endif

        private void OnEnable()
        {
            selectAction              = selectActionReference.action;
            addToSelectionAction      = addToSelectionActionReference.action;
            removeFromSelectionAction = removeFromSelectionActionReference.action;
            
            selectAction.Enable();
            addToSelectionAction.Enable();
            removeFromSelectionAction.Enable();
        }

        private void OnDisable()
        {
            removeFromSelectionAction.Disable();
            addToSelectionAction.Disable();
            selectAction.Disable();
        }

        private void Update()
        {
            HandleSelectionInputs();
            HandleMovementInputs();
            
            cursor.anchoredPosition = MousePositionCurrent;
        }

        private void HandleMovementInputs()
        {
            if (selectAction.WasReleasedThisFrame() && Selection.Instance.SelectedUnits.Count > 0)
            {
                // if (Physics.Raycast(ray: camera.ScreenPointToRay(pos: Input.mousePosition), hitInfo: out RaycastHit __hit, layerMask: floorLayerMaskValue))
                // {
                //     foreach (ISelectable __unit in Selection.Instance.SelectedUnits)
                //     {
                //         //unit.MoveTo(Hit.point);
                //     }
                // }
            }
        }

        private void HandleSelectionInputs()
        {
            // Selection Box
            if (selectAction.WasPressedThisFrame())
            {
                StartSelectionBox();
            }
            else if (selectAction.IsPressed() && lengthsq(_mousePositionStart - MousePositionCurrent) > MIN_DISTANCE_SQR_FOR_DRAG)
            {
                UpdateSelectionBox();
            }
            else if (selectAction.WasReleasedThisFrame())
            {
                EndSelectionBox();
            }
        }

        private void StartSelectionBox()
        {
            selectionBox.sizeDelta = Vector2.zero;
            selectionBox.gameObject.SetActive(value: true);
            _mousePositionStart = MousePositionCurrent;
            //_mouseDownTime         = Time.time;
        }
        
        private void UpdateSelectionBox()
        {
            //F32 __width  = Input.mousePosition.x - _mousePositionStart.x;
            //F32 __height = Input.mousePosition.y - _mousePositionStart.y;
            
            F32 __width  = MousePositionCurrent.x - _mousePositionStart.x;
            F32 __height = MousePositionCurrent.y - _mousePositionStart.y;

            selectionBox.anchoredPosition = (_mousePositionStart + new F32x2(x: __width * 0.5f, y: __height * 0.5f));
            selectionBox.sizeDelta = new(x: abs(__width), y: abs(__height));

            Bounds __bounds = new(center: selectionBox.anchoredPosition, size: selectionBox.sizeDelta);

            foreach (ISelectable __selectable in Selection.Instance.AvailableUnits)
            {
                //(F32x2) camera.WorldToScreenPoint(position: __selectable.transform.position)
                if (UnitIsInSelectionBox(position: camera.WorldToCamera(__selectable.transform.position), bounds: __bounds))
                {
                    if (!Selection.Instance.IsSelected(unit: __selectable))
                    {
                        _newlySelectedUnits.Add(item: __selectable);
                    }
                    _deselectedUnits.Remove(item: __selectable);
                }
                else
                {
                    _deselectedUnits.Add(item: __selectable);
                    _newlySelectedUnits.Remove(item: __selectable);
                }
            }
        }
        
        private void EndSelectionBox()
        {
            selectionBox.sizeDelta = Vector2.zero;
            selectionBox.gameObject.SetActive(value: false);

            foreach (ISelectable __newUnit in _newlySelectedUnits)
            {
                Selection.Instance.Select(unit: __newUnit);
            }
            foreach (ISelectable __deselectedUnit in _deselectedUnits)
            {
                Selection.Instance.Deselect(unit: __deselectedUnit);
            }

            _newlySelectedUnits.Clear();
            _deselectedUnits.Clear();

            // Single Unit Selection
            if (Physics.Raycast(ray: camera.ScreenPointToRay(pos: (Vector3)(Vector2)MousePositionCurrent), hitInfo: out RaycastHit __hit, maxDistance: unitLayerMaskValue)
                && __hit.collider.TryGetComponent(component: out ISelectable __unit))
            {
                Debug.Log(message: __unit + " has ISelectable interface");
                
                if (addToSelectionAction.IsPressed())
                {
                    Selection.Instance.Select(unit: __unit);
                }
                else if (removeFromSelectionAction.IsPressed())
                {
                    Selection.Instance.Deselect(unit: __unit);
                }
                else
                {
                    Selection.Instance.DeselectAll();
                    Selection.Instance.Select(unit: __unit);
                }
            }
            
            // else if (_mouseDownTime + dragDelay > Time.time)
            // {
            //     Selection.Instance.DeselectAll();
            // }

            // _mouseDownTime = 0;
        }
        
        private static Boolean UnitIsInSelectionBox(F32x2 position, Bounds bounds)
        {
            return position.x > bounds.min.x && 
                   position.x < bounds.max.x && 
                   position.y > bounds.min.y && 
                   position.y < bounds.max.y;
        }
    }
}