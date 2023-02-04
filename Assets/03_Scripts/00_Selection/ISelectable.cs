

using UnityEngine;

namespace CoolBeans.Selection
{
    public interface ISelectable
    {
        public GameObject gameObject { get; }
        
        public Transform transform { get; }
        
        void Select();
        void Deselect();
    }
}
