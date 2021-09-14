using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace utils.geometry
	{
	public abstract partial class Polygon
		{
		public class Convex : Concave
			{
			public Convex(ICollection<Point> points) : base(points) { }

			public override bool contains(Point point)
				{
				foreach (var edge in edges) { if (point.is_on_right(edge)) { return false; } }
				return true;
				}
			public override bool contains_strict(Point point)
				{
				foreach (var edge in edges) { if (point.is_on_right(edge) || point.is_colinear(edge)) { return false; } }
				return true;
				}
			}
		}
	}