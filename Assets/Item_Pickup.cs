using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoolBeans
{
	public class Item_Pickup : MonoBehaviour
	{
		public GameObject square;
		public int food;
		public Duplicate bean;

		// Start is called before the first frame update
		void Start()
		{
			bean = FindFirstObjectByType<Duplicate>();
			bean.transform.position = this.transform.position;

		}

		public void OnTriggerEnter2D(Collider2D other)
		{
			var x = other.GetComponent<Duplicate>();

			if (x != null)
			{

				GameObject square = this.gameObject;
				x.food++;
				Destroy(square);
				
				
				
			}
		}
	}
			
}
