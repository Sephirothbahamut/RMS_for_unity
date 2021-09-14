using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace utils.geometry
	{
	public abstract partial class Polygon
		{
		public class Triangle : Convex
			{
			public Triangle(Point a, Point b, Point c) : base(new Point[] { a, b, c }) { }

			public static readonly int A = 0;
			public static readonly int B = 1;
			public static readonly int C = 2;

			public Point a { get { return vertices[A]; } set { vertices[A] = value; } }
			public Point b { get { return vertices[B]; } set { vertices[B] = value; } }
			public Point c { get { return vertices[C]; } set { vertices[C] = value; } }

			public float area2 { get { return ((b.x - a.x) * (c.y - a.y)) - ((c.x - a.x) * (b.y - a.y)); } }
			public float area { get { return Mathf.Sqrt(area2); } }

			/// <summary>
			/// Calculates the distance in the range [0-1] between a given point and an index along the triangle's height axis associated with that index
			/// </summary>
			public float proportional_distance2(Point point, int vertex_index)
				{
				var reference_vertex = vertex(vertex_index);
				var base_segment = edge((vertex_index + 1) % 3);
				var height_segment = base_segment.perpendicular_segment_from(reference_vertex);

				var perpendicular_point_to_height_segment = height_segment.perpendicular_segment_from(point);
				var reference_vertex_to_point_projected_on_height = new Segment(reference_vertex, perpendicular_point_to_height_segment.b);

				return reference_vertex_to_point_projected_on_height.length2 / height2(vertex_index);
				}
			public float proportional_distance(Point point, int vertex_index) { return Mathf.Sqrt(proportional_distance2(point, vertex_index)); }

			public float height(int vertex_index) { return a.distance_to_line(new Segment(b, c)); }
			public float height2(int vertex_index) { return a.distance_to_line2(new Segment(b, c)); }

			public override Point centroid { get { return (a + b + c) / 3; } }
			}
		}
	}