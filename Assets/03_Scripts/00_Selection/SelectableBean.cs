using System;
using UnityEngine;

namespace CoolBeans.Selection
{
    public sealed class SelectableBean : MonoBehaviour, ISelectable
    {
        private void OnEnable()
        {
            //Debug.Log(message: "SelectableBean.OnEnable()");
            Selection.Instance.AvailableUnits.Add(item: this);
        }
        private void OnDisable()
        {
            //Debug.Log(message: "SelectableBean.OnDisable()");
            Selection.Instance.AvailableUnits.Remove(item: this);
        }

        public void Select()
        {
            Debug.Log(message: "SelectableBean.Select()");
            //throw new System.NotImplementedException();
        }

        public void Deselect()
        {
            Debug.Log(message: "SelectableBean.Deselect()");
            //throw new System.NotImplementedException();
        }
    }
}
