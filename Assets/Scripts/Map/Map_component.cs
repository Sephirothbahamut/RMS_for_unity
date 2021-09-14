using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace map_gen
	{
	class Map_component : MonoBehaviour
		{
		private Map map = null;

		[SerializeField]
		int width;
		[SerializeField]
		int height;
		[SerializeField]
		float tile_size;

		[EasyButtons.Button]
		public void generate()
			{
			map = new Map(width, height, tile_size, transform, GetComponent<Map_terrain>());
			}

		[EasyButtons.Button]
		public void test()
			{
			map.apply();
			}
		}
	}
