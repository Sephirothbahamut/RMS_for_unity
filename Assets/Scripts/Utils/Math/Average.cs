using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace utils
	{
	class cumulative_average
		{
		public void push(float value)
			{
			avg = ((avg * count) + value) / (count + 1);
			count++;
			}
		public float get_average() { return avg; }

		public void reset()
			{
			count = 0;
			avg = 0f;
			}

		public static implicit operator float(cumulative_average avg) { return avg.avg; }

		private uint count = 0;
		private float avg = 0f;
		};
	}