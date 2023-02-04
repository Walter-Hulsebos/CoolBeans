using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.InputSystem;
using static Unity.Mathematics.math;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

using Drawing;
using UnityEngine.Serialization;
using F32   = System.Single;
using F32x2 = Unity.Mathematics.float2;
using F32x3 = Unity.Mathematics.float3;

using I32   = System.Int32;
using quaternion = Unity.Mathematics.quaternion;

namespace CoolBeans
{
    public sealed class Sprout : MonoBehaviour
    {
        [SerializeField] private InputActionReference         steerInputReference;
        [SerializeField, HideInInspector] private InputAction steerInput;

        #if ODIN_INSPECTOR
        [SuffixLabel(label: "metres/second", overlay: true)]
        #endif
        [SerializeField] private F32 sproutForwardSpeed  = 3f;
        
        #if ODIN_INSPECTOR
        [SuffixLabel(label: "degrees/second", overlay: true)]
        #endif
        [SerializeField] private F32 sproutSteeringSpeed = 80f;
        
        [SerializeField] private F32 sproutMaxLength                                       = 20f;
        [SerializeField] private F32 distanceRequiredForNewSegment                         = 2f;
        [SerializeField, HideInInspector] private F32 distanceSquaredRequiredForNewSegment = 4f;
        
        //Include layer 7 and 8 by default.
        [SerializeField] private LayerMask hitsLayerMask = 1 << 7 | 1 << 8;

        [SerializeField, HideInInspector] private SpriteShapeController sproutSpriteShapeController;
        [SerializeField, HideInInspector] private LineRenderer          lineRenderer;
        
        // private Spline SproutSpline       => sproutSpriteShapeController.spline;
        // private F32x3  SproutEnd          => (F32x3)SproutSpline.GetPosition(index: SproutSpline.GetPointCount() - 1);
        // private F32x3  SproutEndForwardDirection    => normalize(-(SproutSpline.GetLeftTangent(index:  SproutSpline.GetPointCount() - 1)));
        // private F32x3  SproutEndLeftDirection       => normalize(cross(SproutEndForwardDirection, new(x: 0f, y: 0f, z: -1f)));
        // private F32x3  SproutEndRightDirection      => normalize(cross(SproutEndForwardDirection, new(x: 0f, y: 0f, z: +1f)));
        // private F32x3  SproutEndForward             => SproutEnd + SproutEndForwardDirection;
        // private F32x3  SproutEndLeft                => SproutEnd + SproutEndLeftDirection;
        // private F32x3  SproutEndRight               => SproutEnd + SproutEndRightDirection;

        private F32x3 _forward;
        private F32x3 _tip;
        private F32x3 _lastTip;
        
        private F32   _currentLength;
        
        private Boolean _canGrow = true;

        private void Reset()
        {
            sproutSpriteShapeController = GetComponent<SpriteShapeController>();
            lineRenderer                = GetComponent<LineRenderer>();
            
            distanceSquaredRequiredForNewSegment = distanceRequiredForNewSegment * distanceRequiredForNewSegment;
        }
        private void OnValidate()
        {
            sproutSpriteShapeController = GetComponent<SpriteShapeController>();
            lineRenderer                = GetComponent<LineRenderer>();
            
            distanceSquaredRequiredForNewSegment = distanceRequiredForNewSegment * distanceRequiredForNewSegment;
        }

        private void OnEnable()
        {
            steerInput = steerInputReference.action;
            steerInput.Enable();
        }
        private void OnDisable()
        {
            steerInput.Disable();
        }

        //spline.InsertPointAt(spline.GetPointCount(), insertPoint);
        //var newPointIndex = spline.GetPointCount() - 1;
        //spline.SetTangentMode(newPointIndex, ShapeTangentMode.Continuous);
        //spline.SetHeight(newPointIndex, 1.0f);
        //lastPosition = insertPoint;
        //spriteShapeController.BakeCollider();


        private void Start()
        {
            Transform __transform = transform;
            _forward = __transform.up;
            _tip     = __transform.position;
            
            lineRenderer.SetPosition(index: 0, _tip);
            lineRenderer.SetPosition(index: 1, _tip);
        }

        private void Update()
        {
            //Debug forward and righttangent.
            //Debug.DrawLine(start: SproutEnd, end: SproutEnd + SproutEndForwardDirection,   color: Color.red);
            //Debug.DrawLine(start: SproutEnd, end: SproutEnd + SproutEndRightDirection, color: Color.blue);

            //Debug.Log($"SproutEndForwardDirection: {SproutEndForwardDirection}");
            //Debug.Log($"SproutEndLeftDirection:    {SproutEndLeftDirection}");
            //Debug.Log($"SproutEndRightDirection:   {SproutEndRightDirection}");

            if (_canGrow)
            {
                Grow();
            }
        }

        private void Grow()
        {
            //TODO: When sprout hits a collider, stop growing.  Use Physics2D
            //TODO: When sprout is at max length, stop growing.
            
            //TODO: Grow locally forward, in the direction of the sprout's forward vector.
            //Steer perpendicular to that.

            // Debug.Log($"SproutEnd:        {SproutEnd}");
            // Debug.Log($"SproutEndForward: {SproutEndForward}");
            // Debug.Log($"SproutEndLeft:    {SproutEndLeft}");
            // Debug.Log($"SproutEndRight:   {SproutEndRight}");
            //
            // Draw.SolidCircleXY(center: SproutEnd,        radius: 0.1f, color: Color.green);
            // Draw.SolidCircleXY(center: SproutEndForward, radius: 0.1f, color: Color.red);
            // Draw.SolidCircleXY(center: SproutEndLeft,    radius: 0.1f, color: Color.yellow);
            // Draw.SolidCircleXY(center: SproutEndRight,   radius: 0.1f, color: Color.blue);

            F32  __steeringVector = steerInput.ReadValue<F32>();
            
            //-1 = left, 0 = straight, +1 = right
            F32   __steeringAngle     = __steeringVector * (sproutSteeringSpeed * Time.deltaTime);
            //F32x3 __leftDirection = -transform.right;
            
            //Rotate the forward vector by the steering angle.
            _forward = mul(quaternion.AxisAngle(axis: new F32x3(x: 0f, y: 0f, z: -1f), angle: radians(__steeringAngle)), _forward);
            
            _tip += _forward * (sproutForwardSpeed * Time.deltaTime);

            Draw.SolidCircleXY(center: _tip, radius: 0.1f, color: Color.magenta);

            lineRenderer.SetPosition(index: lineRenderer.positionCount - 1, position: _tip);

            // Add new point if we've moved far enough.
            if (distancesq(_lastTip, _tip) >= distanceSquaredRequiredForNewSegment)
            {
                lineRenderer.positionCount += 1;
                lineRenderer.SetPosition(index: lineRenderer.positionCount - 1, position: _tip);
                _lastTip = _tip;
            }
            
            

            //
            // I32 __newPointIndex = SproutSpline.GetPointCount();
            // SproutSpline.InsertPointAt( index: __newPointIndex, point: (Vector3)__newPosition);
            // SproutSpline.SetTangentMode(index: __newPointIndex, mode:  ShapeTangentMode.Continuous);
            // SproutSpline.SetHeight(     index:__newPointIndex,  value: 1.0f);
            // //_lastPosition = __newPosition;
            // sproutSpriteShapeController.BakeCollider();

        }

        private void StopGrowing()
        {
            //TODO: Construct the sprite shape from the line renderer, bake the collider, and disable the line renderer.
            
        }
    }
}