using System;
using UnityEngine;

namespace CoolBeans
{
    public class BeanFood : MonoBehaviour
    {
        [SerializeField] private GameObject beanPrefab;

        [SerializeField] private Int32 foodRequiredForBean = 3;
        
        private Int32 foodCount = 0;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Food"))
            {
                foodCount += 1;
                
                if (foodCount >= foodRequiredForBean)
                {
                    Instantiate(beanPrefab, transform.position, Quaternion.identity);
                    Destroy(gameObject);
                }
            }
        }
    }
}
