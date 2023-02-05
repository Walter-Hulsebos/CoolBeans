using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using F32 = System.Single;

namespace CoolBeans
{
    public class BeanEra : MonoBehaviour
    {
        [SerializeField] private F32 romanHeight    = 40;
        [SerializeField] private F32 medievalHeight = 80;
        [SerializeField] private F32 cowboyHeight   = 120;
        [SerializeField] private F32 modernHeight   = 160;
        [SerializeField] private F32 futureHeight   = 200;
        
        [Space]
        
        [SerializeField] private GameObject futureObj;
        [SerializeField] private GameObject modernObj;
        [SerializeField] private GameObject cowboyObj;
        [SerializeField] private GameObject medievalObj;
        [SerializeField] private GameObject romanObj;
        [SerializeField] private GameObject caveManObj;
        
        private void Start()
        {
            F32 __height = transform.position.y;

            if (__height >= futureHeight)
            {
                futureObj.SetActive(true);
            }
            else if (__height >= modernHeight)
            {
                modernObj.SetActive(true);
            }
            else if (__height >= cowboyHeight)
            {
                cowboyObj.SetActive(true);
            }
            else if (__height >= medievalHeight)
            {
                medievalObj.SetActive(true);
            }
            else if (__height >= romanHeight)
            {
                romanObj.SetActive(true);
            }
            else
            {
                caveManObj.SetActive(true);
            }
        }
    }
}
