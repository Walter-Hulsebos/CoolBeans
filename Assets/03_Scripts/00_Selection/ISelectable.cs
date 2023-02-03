

using UnityEngine;

namespace CoolBeans.Selection
{
    public interface ISelectable
    {
        public Transform transform { get; }
        
        void Select();
        void Deselect();
    }
}
