using System;
using UnityEngine;
using static Unity.Mathematics.math;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

using Random = UnityEngine.Random;

using F32  = System.Single;
using F32x3 = Unity.Mathematics.float3;
using I32  = System.Int32;

using Bool = System.Boolean;

namespace CoolBeans
{
    public sealed class FlowerPlacer : MonoBehaviour
    {
        [SerializeField] private F32  factorPrimantissa = 0.1f;
        [SerializeField] private AnimationCurve distributionCurve = AnimationCurve.EaseInOut(timeStart: 0, timeEnd: 1, valueStart: 0, valueEnd: 1);

        [SerializeField, HideInInspector] private LineRenderer lineRenderer;

        [SerializeField] private GameObject flowerPrefab;

        private void Reset()
        {
            lineRenderer = transform.GetComponent<LineRenderer>();
        }
        private void OnValidate()
        {
            lineRenderer = transform.GetComponent<LineRenderer>();
        }

        [Button]
        public void Place()
        {
            for (I32 __index = 1; __index < lineRenderer.positionCount - 1; __index += 1)
            {
                F32 __t = (__index / (F32) lineRenderer.positionCount);
                
                F32 __distribution = distributionCurve.Evaluate(__t);

                F32 __chance = (factorPrimantissa * __distribution);
                
                //if (Random.Range(0, 100) > (100 - randomFactor))
                if (Random.value <= __chance)
                {
                    Vector3 __lineRendererPosition = lineRenderer.GetPosition(index: __index);
                    
                    GameObject __spawnedFlower = Instantiate(original: flowerPrefab, position: __lineRendererPosition, rotation: Quaternion.identity);
                    
                    Vector3 __lt = Vector3.Normalize(lineRenderer.GetPosition(index: clamp(__index - 1, 0, lineRenderer.positionCount)) - __lineRendererPosition);
                    Vector3 __rt = Vector3.Normalize(lineRenderer.GetPosition(index: clamp(__index + 1, 0, lineRenderer.positionCount)) - __lineRendererPosition);
                        
                    F32 a = Angle(Vector3.up, __lt);
                    F32 b = Angle(__lt, __rt);
                    F32 c = a + (b * 0.5f);
                    if (b > 0)
                    {
                        c += 180;
                    }
                    
                    //50/50 chance at being flipped
                    Boolean __isFlipped = (Random.value < 0.5);
                    if (__isFlipped)
                    {
                        c += 180;
                    }
                    
                    __spawnedFlower.transform.rotation = Quaternion.Euler(0, 0, c);

                    F32 __offsetDistance = lineRenderer.widthCurve.Evaluate(__t);

                    __spawnedFlower.transform.position += __spawnedFlower.transform.up * __offsetDistance;
                    //* (__isFlipped ? -__offsetDistance : __offsetDistance);
                }
            }
        }
        
        private F32 Angle(Vector3 a, Vector3 b)
        {
            F32 __dot = dot(a, b);
            F32 __det = (a.x * b.y) - (b.x * a.y);
            return degrees(atan2(__det, __dot));
        }
    }
}
