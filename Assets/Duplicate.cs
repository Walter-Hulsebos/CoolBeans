using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoolBeans
{
    public class Duplicate : MonoBehaviour
    {
		public GameObject bean;
		public int food ;

        // Start is called before the first frame update
		public void InstantiateNewObject()
		{
			Instantiate(bean);
		} 

        void Start()
        {
			

		}

        // Update is called once per frame
        void Update()
        {
			if(food > 5)
			{
				InstantiateNewObject();
			}
        }


    }
}
