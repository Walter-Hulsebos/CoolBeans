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
			bean.transform.position = new Vector2(square.transform.position.x, bean.transform.position.y);

		}

		// Update is called once per frame
		void Update()
		{

		}

		public void OnTriggerEnter2D(Collider2D other)
		{

			if (other.GetComponent<Duplicate>()!= null)
			{

				GameObject square = this.gameObject;
				square.SetActive(false);
				food++;
				
				return;
			}
		}
	}
			
}
