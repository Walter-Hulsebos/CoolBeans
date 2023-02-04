using ExtEvents;
using JetBrains.Annotations;
using UnityEngine;

namespace CoolBeans.Selection
{
    public sealed class SelectableBean : MonoBehaviour, ISelectable
    {
        #region Events

        [field:SerializeField] 
        public ExtEvent OnSelected   { get; [UsedImplicitly] private set; } = new();
        [field:SerializeField] 
        public ExtEvent OnDeselected { get; [UsedImplicitly] private set; } = new();

        #endregion
        
        
        #region Methods
        
        private void OnEnable()
        {
            Selection.Instance.ExistingUnits.Add(item: this);
        }
        private void OnDisable()
        {
            Selection.Instance.ExistingUnits.Remove(item: this);
        }

        public void Select()
        {
            //Debug.Log(message: "Selected " + gameObject.name, context: this);
            OnSelected.Invoke();
        }

        public void Deselect()
        {
            //Debug.Log(message: "Deselected " + gameObject.name, context: this);
            OnDeselected.Invoke();
        }

        #endregion
    }
}
