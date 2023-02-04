using System;
using System.Collections.Generic;
using CoolBeans.Utils;
using HighlightPlus;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using static Unity.Mathematics.math;

using F32   = System.Single;
using F32x2 = Unity.Mathematics.float2;
using F32x3 = Unity.Mathematics.float3;

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
        
        [SerializeField] private Transform testCubeStart;
        [SerializeField] private Transform testCubeEnd;
        [SerializeField] private Transform testCubeBox;
        
        [SerializeField] private LayerMask unitLayerMask  = (LayerMask)(1 << 6);
        [SerializeField, HideInInspector] private I32 unitLayerMaskValue;
        
        [SerializeField] private LayerMask stalkLayerMask = (LayerMask)(1 << 0);
        [SerializeField, HideInInspector] private I32 floorLayerMaskValue;

        /// <summary>
        /// Minimum distance squared the mouse has to move (in pixels) before a drag is registered.
        /// </summary>
        private const F32 MIN_DISTANCE_SQR_FOR_DRAG = 20f;

        private readonly HashSet<ISelectable> _unitsToAddToSelection    = new();
        private readonly HashSet<ISelectable> _unitsToRemoveFromSelection = new();

        private F32x2 MousePositionStartCameraSpace           { get; set; }
        private F32x2 MousePositionStartCameraSpaceCentered   { get; set; } //Only made a property for consistency with MousePositionCurrentCameraSpaceCentered, but it's not really needed.
        private F32x3 MousePositionStartWorldSpace            => camera.ScreenToWorldPoint(position: new F32x3(xy: MousePositionStartCameraSpace, z: camera.nearClipPlane));
        private F32x3 MousePositionStartWorldSpaceCentered    => camera.ScreenToWorldPoint(position: new F32x3(xy: MousePositionCurrentCameraSpaceCentered, z: camera.nearClipPlane));

        private F32x2 MousePositionCurrentCameraSpace         => ((F32x2)Mouse.current.position.ReadValue());
        private F32x2 MousePositionCurrentCameraSpaceCentered => (MousePositionCurrentCameraSpace - _screenSafeAreaSizeHalf);
        private F32x3 MousePositionCurrentWorldSpace          => camera.ScreenToWorldPoint(position: new F32x3(xy: MousePositionCurrentCameraSpace, z: camera.nearClipPlane));
        private F32x3 MousePositionCurrentWorldSpaceCentered  => camera.ScreenToWorldPoint(position: new F32x3(xy: MousePositionCurrentCameraSpaceCentered, z: camera.nearClipPlane));
        
        
        private F32x2 _screenSafeAreaSize;
        private F32x2 _screenSafeAreaSizeHalf;
        
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

        private void Awake()
        {
            _screenSafeAreaSize     = (F32x2)Screen.safeArea.size;
            _screenSafeAreaSizeHalf = _screenSafeAreaSize * 0.5f;
        }

        private void Update()
        {
            HandleSelectionInputs();

            cursor.anchoredPosition = MousePositionCurrentCameraSpaceCentered;
        }

        private void HandleSelectionInputs()
        {
            // Selection Box
            if (selectAction.WasPressedThisFrame())
            {
                StartSelectionBox();
            }
            else if (selectAction.IsPressed() && lengthsq(MousePositionStartCameraSpaceCentered - MousePositionCurrentCameraSpaceCentered) > MIN_DISTANCE_SQR_FOR_DRAG)
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
            
            MousePositionStartCameraSpace         = MousePositionCurrentCameraSpace;
            MousePositionStartCameraSpaceCentered = MousePositionCurrentCameraSpaceCentered;
        }
        
        private Collider[]    _foundCollidersBuffer = new Collider[200];
        private I32           _foundCollidersCount  = 0;

        private void UpdateSelectionBox()
        {
            F32x2 __mouseDelta = (MousePositionCurrentCameraSpaceCentered - MousePositionStartCameraSpaceCentered);
            
            F32 __width  = __mouseDelta.x;
            F32 __height = __mouseDelta.y;
            
            selectionBox.anchoredPosition = (MousePositionStartCameraSpaceCentered + new F32x2(x: __width * 0.5f, y: __height * 0.5f));
            selectionBox.sizeDelta        = new(x: abs(__width), y: abs(__height));
            
            F32x3 __startTestPos = MousePositionCurrentWorldSpace;
            __startTestPos.z += 10;
            testCubeStart.position = __startTestPos;
            
            F32x3 __endTestPos = MousePositionStartWorldSpace;
            __endTestPos.z += 10;
            testCubeEnd.position = __endTestPos;

            F32x3 __boxTestPos = MousePositionStartWorldSpace.Middle(MousePositionCurrentWorldSpace);
            __boxTestPos.z += 10;
            testCubeBox.position = __boxTestPos;

            F32x3 __overlapBoxCenter = MousePositionStartWorldSpace.Middle(MousePositionCurrentWorldSpace);
            F32x3 __worldSpaceDelta  = MousePositionStartWorldSpace.Delta(MousePositionCurrentWorldSpace);
            F32x3 __size = new(xy: abs(__worldSpaceDelta.xy), z: 500);
            F32x3 __overlapBoxSize   = __size * 0.5f;
                //new(xy: , z: 100);

            _foundCollidersCount = Physics.OverlapBoxNonAlloc(
                center:      __overlapBoxCenter, 
                halfExtents: __overlapBoxSize, 
                results:     _foundCollidersBuffer,
                orientation: Quaternion.identity, 
                mask:        unitLayerMaskValue);
            
            //Debug.Log("Colliders within selection box: " + _foundCollidersCount);
            for (I32 __index = 0; __index < _foundCollidersCount; __index++)
            {
                Debug.Log("Collider: " + _foundCollidersBuffer[__index].name);
            }

            if (addToSelectionAction.IsPressed())
            {
                // If AddToSelection is pressed, we want to add to the selection, not replace it.
                AddToSelection();
            }
            else if (removeFromSelectionAction.IsPressed())
            {
                // If RemoveFromSelection is pressed, we want to remove from the selection, not replace it.
                 RemoveFromSelection();
            }
            else
            {
                // If neither is pressed, we want to replace the selection.
                ReplaceSelection();
            }
            
            HighlightUnits();
        }

        private void AddToSelection()
        {
            for (I32 __index = 0; __index < _foundCollidersCount; __index++)
            {
                Collider __foundCollider = _foundCollidersBuffer[__index];

                if (!__foundCollider.TryGetComponent(out ISelectable __selectable)) continue;
                
                //if it's already in the selection, skip.
                //if(Selection.Instance.SelectedUnits.Contains(__selectable)) continue;

                if(_unitsToRemoveFromSelection.Contains(__selectable))
                {
                    _unitsToRemoveFromSelection.Remove(__selectable);
                }

                if (_unitsToAddToSelection.Contains(__selectable)) continue;
                        
                _unitsToAddToSelection.Add(__selectable);
            }
        }

        private void RemoveFromSelection()
        {
            for (I32 __index = 0; __index < _foundCollidersCount; __index++)
            {
                Collider __foundCollider = _foundCollidersBuffer[__index];
                    
                if (__foundCollider.TryGetComponent(out ISelectable __selectable))
                {
                    //if it's not in the selection, skip.
                    if(!Selection.Instance.SelectedUnits.Contains(__selectable)) continue;
                    //if it's already in the units to remove, skip.
                    if (_unitsToRemoveFromSelection.Contains(__selectable)) continue;

                    if(_unitsToAddToSelection.Contains(__selectable))
                    {
                        _unitsToAddToSelection.Remove(__selectable);
                    }
                    _unitsToRemoveFromSelection.Add(__selectable);
                }
            }
        }

        private void ReplaceSelection()
        {
            //Everything within the selection area should be added to `_unitsToAddToSelection`.
            _unitsToAddToSelection.Clear();
            for (I32 __index = 0; __index < _foundCollidersCount; __index++)
            {
                Collider __foundCollider = _foundCollidersBuffer[__index];
                    
                if (__foundCollider.TryGetComponent(out ISelectable __selectable))
                {
                    if(_unitsToAddToSelection.Contains(__selectable)) continue;
                    
                    _unitsToAddToSelection.Add(__selectable);
                }
            }
            
            //Everything else should be added to `_unitsToRemoveFromSelection`, if it's currently selected.
            foreach (ISelectable __existingUnit in Selection.Instance.ExistingUnits)
            {
                if(_unitsToAddToSelection.Contains(__existingUnit)) continue;
                
                if(Selection.Instance.SelectedUnits.Contains(__existingUnit))
                {
                    _unitsToRemoveFromSelection.Add(__existingUnit);
                }
            }
        }

        private void HighlightUnits()
        {
            foreach (ISelectable __unit in Selection.Instance.ExistingUnits)
            {
                if(Selection.Instance.SelectedUnits.Contains(__unit))
                {
                    if (!__unit.gameObject.TryGetComponent(out HighlightEffect __highlightEffect)) continue;
                    
                    __highlightEffect.highlighted  = true;
                    __highlightEffect.outlineColor = Color.white;
                }
                else if (_unitsToAddToSelection.Contains(__unit))
                {
                    if (!__unit.gameObject.TryGetComponent(out HighlightEffect __highlightEffect)) continue;
                    
                    __highlightEffect.highlighted  = true;
                    __highlightEffect.outlineColor = Color.green;
                }
                else if (_unitsToRemoveFromSelection.Contains(__unit))
                {
                    if (!__unit.gameObject.TryGetComponent(out HighlightEffect __highlightEffect)) continue;
                    
                    __highlightEffect.highlighted  = true;
                    __highlightEffect.outlineColor = Color.red;
                }
                else
                {
                    if (!__unit.gameObject.TryGetComponent(out HighlightEffect __highlightEffect)) continue;
                    
                    __highlightEffect.highlighted  = false;
                }
            }
            
            // foreach (ISelectable __unitToAdd in _unitsToAddToSelection)
            // { 
            //     if (!__unitToAdd.gameObject.TryGetComponent(out HighlightEffect __highlightEffect)) continue;
            //     
            //     __highlightEffect.highlighted  = true;
            //     __highlightEffect.outlineColor = Color.green;
            // }
            //
            // foreach (ISelectable __unitToRemove in _unitsToRemoveFromSelection)
            // {
            //     if (!__unitToRemove.gameObject.TryGetComponent(out HighlightEffect __highlightEffect)) continue;
            //     
            //     __highlightEffect.highlighted  = true;
            //     __highlightEffect.outlineColor = Color.red;
            // }
            //
            // foreach (ISelectable __unit in Selection.Instance.SelectedUnits)
            // {
            //     if (!__unit.gameObject.TryGetComponent(out HighlightEffect __highlightEffect)) continue;
            //     
            //     __highlightEffect.highlighted  = true;
            //     __highlightEffect.outlineColor = Color.white;
            // }
            //
            // foreach (ISelectable __unit in Selection.Instance.ExistingUnits)
            // {
            //     if (!__unit.gameObject.TryGetComponent(out HighlightEffect __highlightEffect)) continue;
            //     
            //     if (Selection.Instance.SelectedUnits.Contains(__unit)) continue;
            //     if (_unitsToAddToSelection.Contains(__unit)) continue;
            //     if (_unitsToRemoveFromSelection.Contains(__unit)) continue;
            //     
            //     __highlightEffect.highlighted = false;
            // }
        }

        private void EndSelectionBox()
        {
            selectionBox.sizeDelta = Vector2.zero;
            selectionBox.gameObject.SetActive(value: false);

            foreach (ISelectable __unitToAdd in _unitsToAddToSelection)
            {
                Selection.Instance.Select(unit: __unitToAdd);
            }
            foreach (ISelectable __unitToRemove in _unitsToRemoveFromSelection)
            {
                Selection.Instance.Deselect(unit: __unitToRemove);
            }
            
            _unitsToAddToSelection.Clear();
            _unitsToRemoveFromSelection.Clear();
            
            HighlightUnits();

            // // Single Unit Selection
            // if (Physics.Raycast(ray: camera.ScreenPointToRay(pos: (Vector3)(Vector2)MousePositionCurrentCameraSpaceCentered), hitInfo: out RaycastHit __hit, maxDistance: unitLayerMaskValue)
            //     && __hit.collider.TryGetComponent(component: out ISelectable __unit))
            // {
            //     Debug.Log(message: __unit + " has ISelectable interface");
            //     
            //     if (addToSelectionAction.IsPressed())
            //     {
            //         Selection.Instance.Select(unit: __unit);
            //     }
            //     else if (removeFromSelectionAction.IsPressed())
            //     {
            //         Selection.Instance.Deselect(unit: __unit);
            //     }
            //     else
            //     {
            //         Selection.Instance.DeselectAll();
            //         Selection.Instance.Select(unit: __unit);
            //     }
            // }
        }
        
        // private static Boolean UnitIsInSelectionBox(F32x2 position, Bounds bounds)
        // {
        //     return position.x > bounds.min.x && 
        //            position.x < bounds.max.x && 
        //            position.y > bounds.min.y && 
        //            position.y < bounds.max.y;
        // }
    }
}