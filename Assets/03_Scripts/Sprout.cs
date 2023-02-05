using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.InputSystem;
using static Unity.Mathematics.math;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

using F32   = System.Single;
using F32x2 = Unity.Mathematics.float2;
using F32x3 = Unity.Mathematics.float3;

using I32   = System.Int32;
using quaternion = Unity.Mathematics.quaternion;
using Random = UnityEngine.Random;

namespace CoolBeans
{
    public sealed class Sprout : MonoBehaviour
    {
        [SerializeField] private InputActionReference         steerInputReference;
        [SerializeField, HideInInspector] private InputAction steerInput;

        [SerializeField] private Transform sproutCameraPlaceHolder;

        #if ODIN_INSPECTOR
        [SuffixLabel(label: "metres/second", overlay: true)]
        #endif
        [SerializeField] private F32 sproutForwardSpeed  = 3f;
        
        #if ODIN_INSPECTOR
        [SuffixLabel(label: "degrees/second", overlay: true)]
        #endif
        [SerializeField] private F32 sproutSteeringSpeed = 60f;
        
        [SerializeField] private F32                  sproutMaxLength                      = 15;
        [SerializeField, HideInInspector] private F32 sproutMaxLengthSquared               = 225;
        [SerializeField] private F32                  distanceRequiredForNewSegment        = 1f;
        [SerializeField, HideInInspector] private F32 distanceSquaredRequiredForNewSegment = 1f;
        
        //Include layer 7 and 8 by default.
        [SerializeField] private LayerMask hitsLayerMask = 1 << 7 | 1 << 8;

        [SerializeField, HideInInspector] private SpriteShapeController sproutSpriteShapeController;
        [SerializeField, HideInInspector] private SpriteShapeRenderer   sproutSpriteShapeRenderer;
        [SerializeField, HideInInspector] private LineRenderer          lineRenderer;
        
        private readonly List<Vector3> _points = new();

        private F32x3 _forward;
        private F32x3 _tip;
        private F32x3 _lastTip;
        private F32   _lengthSquared;
        
        private Boolean _canGrow = true;

        private void Reset()
        {
            sproutSpriteShapeController = GetComponent<SpriteShapeController>();
            sproutSpriteShapeRenderer   = GetComponent<SpriteShapeRenderer>();
            
            lineRenderer                = GetComponent<LineRenderer>();
            
            distanceSquaredRequiredForNewSegment = distanceRequiredForNewSegment * distanceRequiredForNewSegment;
            sproutMaxLengthSquared               = sproutMaxLength * sproutMaxLength;
        }
        private void OnValidate()
        {
            sproutSpriteShapeController = GetComponent<SpriteShapeController>();
            sproutSpriteShapeRenderer   = GetComponent<SpriteShapeRenderer>();
            
            lineRenderer                = GetComponent<LineRenderer>();
            
            distanceSquaredRequiredForNewSegment = distanceRequiredForNewSegment * distanceRequiredForNewSegment;
            sproutMaxLengthSquared               = sproutMaxLength * sproutMaxLength;
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

        private void Awake()
        {
            Transform __transform = transform;
            _forward = __transform.up;
            
            _tip     = __transform.position;
            _lastTip = _tip;
        }

        private void Start()
        {
            sproutSpriteShapeController.enabled = false;

            _points.Clear();
            _points.Add(_tip);
            _points.Add(_tip + _forward);
            
            lineRenderer.enabled = true;
        }

        private void Update()
        {
            if (_canGrow)
            {
                Grow();
            }
        }

        private void Grow()
        {
            //-1 = left, 0 = straight, +1 = right
            F32 __steeringVector = steerInput.ReadValue<F32>();
            F32 __randomSteeringNoise = Random.Range(minInclusive: -1f, maxInclusive: +1f);
            __steeringVector = clamp(__steeringVector + __randomSteeringNoise, -1, +1);
            
            F32 __steeringAngle  = __steeringVector * (sproutSteeringSpeed * Time.deltaTime);

            //Rotate the forward vector by the steering angle.
            _forward = mul(quaternion.AxisAngle(axis: new F32x3(x: 0f, y: 0f, z: -1f), angle: radians(__steeringAngle)), _forward);
            
            _tip += _forward * (sproutForwardSpeed * Time.deltaTime);
            sproutCameraPlaceHolder.position = _tip;

            _points[^1] = _tip;

            // Add new point if we've moved far enough.
            F32 __distToLastTipSquared = distancesq(_lastTip, _tip);
            if (__distToLastTipSquared >= distanceSquaredRequiredForNewSegment)
            {
                Debug.Log("Count = " + _points.Count);
                
                _points.Add(_tip);

                _lengthSquared += __distToLastTipSquared;
                _lastTip = _tip;
            }

            lineRenderer.positionCount = _points.Count;
            lineRenderer.SetPositions(positions: _points.ToArray());

            //Searches for collider at the tip of the sprout, ignoring the sprout's own collider.
            Collider2D __colliderAtTip     = Physics2D.OverlapPoint(point: _tip.xy, layerMask: hitsLayerMask);
            Boolean    __sproutIsColliding = (__colliderAtTip != null);
            if (__sproutIsColliding)
            {
                Debug.Log("Sprout is colliding.");
                StopGrowing();
            }

            Boolean __sproutIsAtMaxLength = (_lengthSquared + __distToLastTipSquared) >= sproutMaxLength;
            if (__sproutIsAtMaxLength)
            {
                Debug.Log("Sprout is at max length.");
                StopGrowing();
            }
        }

        
        private static I32 NextIndex(I32 index, I32 pointCount)     => Mod(index + 1, pointCount);
        private static I32 PreviousIndex(I32 index, I32 pointCount) => Mod(index - 1, pointCount);

        private static I32 Mod(I32 x, I32 m)
        {
            I32 r = x % m;
            return (r < 0) ? r + m : r;
        }
        private void Smoothen(I32 pointIndex)
        {
            Vector3 position     = sproutSpriteShapeController.spline.GetPosition(pointIndex);
            Vector3 positionNext = sproutSpriteShapeController.spline.GetPosition(NextIndex(pointIndex,     sproutSpriteShapeController.spline.GetPointCount()));
            Vector3 positionPrev = sproutSpriteShapeController.spline.GetPosition(PreviousIndex(pointIndex, sproutSpriteShapeController.spline.GetPointCount()));
            Vector3 forward      = gameObject.transform.forward;

            float scale = Mathf.Min((positionNext - position).magnitude, (positionPrev - position).magnitude) * 0.33f;

            Vector3 leftTangent  = (positionPrev - position).normalized * scale;
            Vector3 rightTangent = (positionNext - position).normalized * scale;

            sproutSpriteShapeController.spline.SetTangentMode(pointIndex, ShapeTangentMode.Continuous);
            SplineUtility.CalculateTangents(point: position, positionPrev, positionNext, forward, scale, out rightTangent, out leftTangent);

            sproutSpriteShapeController.spline.SetLeftTangent(pointIndex, leftTangent);
            sproutSpriteShapeController.spline.SetRightTangent(pointIndex, rightTangent);
        }
        
        private I32 boundsHash = 0;
        
        private void StopGrowing()
        {
            //TODO: Construct the sprite shape from the line renderer, bake the collider, and disable the line renderer.
            
            Debug.Log("Rughaar");
            _canGrow = false;
            
            //lineRenderer.enabled                = false;
            
            //sproutSpriteShapeController.enabled = true;
            //sproutSpriteShapeRenderer.enabled   = true;
            
            //Construct the sprite shape from the line renderer.
            //Vector3[] __positions = new Vector3[lineRenderer.positionCount];
            //lineRenderer.GetPositions(__positions);

            //sproutSpriteShapeController.spline.Clear();
            //Bounds __bounds = new();
            // for (I32 __index = 0; __index < lineRenderer.positionCount; __index += 1)
            // {
            //     //sproutSpriteShapeController.spline.InsertPointAt(index: __index, point: lineRenderer.GetPosition(index: __index));
            //     
            //     //Smoothen(__index);
            //     
            //     //__bounds.Encapsulate(sproutSpriteShapeController.spline.GetPosition(__index));
            // }
            //__bounds.Encapsulate(transform.position);
            
            // if (boundsHash != __bounds.GetHashCode())
            // {
            //     sproutSpriteShapeRenderer.SetLocalAABB(__bounds);
            //     boundsHash = __bounds.GetHashCode();
            // }

            //sproutSpriteShapeController.RefreshSpriteShape();
            //sproutSpriteShapeController.BakeCollider();
            
            //sproutSpriteShapeController.BakeMesh();

            //SpriteShapeRenderer spriteShapeRenderer = gameObject.AddComponent<SpriteShapeRenderer>();
            //sproutSpriteShapeRenderer = spriteShapeController.spriteShape;

            //sproutSpriteShapeRenderer.Prepare();
            //Bake the collider.
            //sproutSpriteShapeController.BakeCollider();

        }
    }
}