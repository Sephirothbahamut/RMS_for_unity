using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace utils.geometry
	{
	public abstract partial class Polygon
		{
		public class Rectangle : Convex
			{
			public Rectangle(Point a, Point b, Point c, Point d) : base(new Point[] { a, b, c, d }) { }
			public static Rectangle from_segment(Segment segment, float thickness, float additional_length = 0f)
				{
				float half_thickness = thickness / 2;
				Point from_a = segment.a - (segment.direction * (half_thickness + additional_length));
				Point from_b = segment.b + (segment.direction * (half_thickness + additional_length));


				Point a, b, c, d;

				Point thickness_vec = segment.perpendicular_direction * half_thickness;
				a = from_a + thickness_vec;
				b = from_b + thickness_vec;
				c = from_b - thickness_vec;
				d = from_a - thickness_vec;
				return new Rectangle(a, b, c, d);
				}

			public static readonly int A = 0;
			public static readonly int B = 1;
			public static readonly int C = 2;
			public static readonly int D = 2;

			public Point a { get { return vertices[A]; } set { vertices[A] = value; } }
			public Point b { get { return vertices[B]; } set { vertices[B] = value; } }
			public Point c { get { return vertices[C]; } set { vertices[C] = value; } }
			public Point d { get { return vertices[D]; } set { vertices[D] = value; } }
			}
		}
	}