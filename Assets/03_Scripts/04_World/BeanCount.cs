using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CoolBeans
{
    public class BeanCount : MonoBehaviour
    {
        [SerializeField] private TMP_Text _tmpText;

        private void Reset()
        {
            _tmpText = transform.GetComponent<TMP_Text>();
        }
        
        private void Update()
        {
            _tmpText.text = Selection.Selection.Instance.ExistingUnits.Count.ToString(format: "000");
        }
    }
}
