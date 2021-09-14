using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace utils.extensions
	{
	public static class EVector2
		{
		public static UnityEngine.Vector2 from_rad(float rad)
			{
			return new UnityEngine.Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
			}

		public static UnityEngine.Vector2 from_deg(float deg)
			{
			return from_rad(deg * Mathf.Deg2Rad);
			}
		}
	}