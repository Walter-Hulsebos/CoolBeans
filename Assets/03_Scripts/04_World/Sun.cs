using System;
using CoolBeans.Selection;
using ExtEvents;
using UnityEngine;

namespace CoolBeans
{
    public class Sun : MonoBehaviour
    {
        [SerializeField] private float height = 150;

        [SerializeField, HideInInspector] private Camera cam;
        
        [SerializeField] private ExtEvent onBeanCrossesThreshold;
        
        [SerializeField] private Boolean beanHasCrossedThreshold = false;

        private void Reset()
        {
            cam = Camera.main;
        }

        private void OnValidate()
        {
            if (cam == null)
            {
                cam = Camera.main;
            }
        }
        
        private void Update()
        {
            if(beanHasCrossedThreshold) return;
            
            transform.position = new Vector3(cam.transform.position.x, height, transform.position.z);
            
            foreach(ISelectable __bean in Selection.Selection.Instance.SelectedUnits)
            {
                if (__bean.transform.position.y >= height)
                {
                    onBeanCrossesThreshold?.Invoke();
                    beanHasCrossedThreshold = true;
                }
            }
        }
    }
}
