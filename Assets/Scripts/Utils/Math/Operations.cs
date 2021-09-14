using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace utils
	{
	public static partial class math
		{
		public static float map(float x, float in_min, float in_max, float out_min, float out_max)
			{
			return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
			}
		}
	}