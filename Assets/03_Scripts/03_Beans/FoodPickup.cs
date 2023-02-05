using ExtEvents;
using UnityEngine;

namespace CoolBeans
{
    public class FoodPickup : MonoBehaviour
    {
        [SerializeField] private ExtEvent onEaten = new();
        
        public void Eat()
        {
            onEaten?.Invoke();
            
            Destroy(gameObject);
        }
    }
}
