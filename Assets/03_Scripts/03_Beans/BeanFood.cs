using System;
using UnityEngine;

namespace CoolBeans
{
    public class BeanFood : MonoBehaviour
    {
        [SerializeField] private GameObject beanPrefab;

        [SerializeField] private Int32 foodRequiredForBean = 3;
        
        [SerializeField] private Int32 foodCount = 0;

        [SerializeField] private Single foodCheckRadius = 2f;

        [SerializeField] private LayerMask foodLayerMask = (1 << 9);
        
        private readonly Collider2D[] _foodCheckBuffer = new Collider2D[3];
        private void Update()
        {
            Int32 __foundFoundCount = Physics2D.OverlapCircleNonAlloc(point: transform.position, radius: foodCheckRadius, results: _foodCheckBuffer, layerMask: foodLayerMask);
            
            for (Int32 __index = 0; __index < __foundFoundCount; __index += 1)
            {
                Collider2D __foundCollider = _foodCheckBuffer[__index];
                
                if(!__foundCollider.TryGetComponent(out FoodPickup __foodPickup)) continue;

                Debug.Log("Bean Found Food");

                __foodPickup.Eat();
                foodCount += 1;
                
                if (foodCount >= foodRequiredForBean)
                {
                    foodCount = 0;
                    
                    //Move player +0.5 to the left.
                    transform.position += Vector3.left * 0.5f;
                    
                    Vector3 __spawnPos = transform.position;
                    __spawnPos += Vector3.right * 0.5f;

                    Instantiate(beanPrefab, __spawnPos, Quaternion.identity);
                    foodCount = 0;
                    return;
                }
            }
        }
    }
}
