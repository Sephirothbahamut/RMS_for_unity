using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace utils.geometry
	{
	[System.Serializable]
	public struct Segment
		{
		public readonly Point[] vertices;

		public static readonly int A = 0;
		public static readonly int B = 1;

		public Point a { get { return vertices[A]; } set { vertices[A] = value; } }
		public Point b { get { return vertices[B]; } set { vertices[B] = value; } }

		public Segment(Point a, Point b)
			{
			vertices = new Point[3];
			this.a = a;
			this.b = b;
			}

		public float length2 { get { return a.distance2(b); } }
		public float length { get { return a.distance(b); } }

		/// <summary>
		/// Returns a segment that goes from a given point to its closest point on this segment.
		/// a is the starting point, b is the calculated point.
		/// /// </summary>
		public Segment shortest_segment_from(Point point)
			{
			//http://csharphelper.com/blog/2016/09/find-the-shortest-distance-between-a-point-and-a-line-segment-in-c/
			Point closest;
			float dx = b.x - a.x;
			float dy = b.y - a.y;
			if ((dx == 0) && (dy == 0)) { closest = a; }
			else
				{
				// Calculate the t that minimizes the distance.
				float t = ((point.x - a.x) * dx + (point.y - a.y) * dy) / (dx * dx + dy * dy);

				// See if this represents one of the segment's
				// end points or a point in the middle.
				if (t < 0) { closest = new Point(a.x, a.y); }
				else if (t > 1) { closest = new Point(b.x, b.y); }
				else { closest = new Point(a.x + t * dx, a.y + t * dy); }
				}
			return new Segment(point, closest);
			}

		public float distance2(Point point) { return shortest_segment_from(point).length2; }
		public float distance(Point point) { return shortest_segment_from(point).length; }

		public Point perpendicular_direction { get { return -Vector2.Perpendicular(b - a).normalized; } }
		public Point direction { get { return (b - a) / length; } }

		/// <summary>
		/// Returns a segment that goes perpendicularly from a given point to the line to which this segment belongs.
		/// a is the starting point, b is the calculated point.
		/// </summary>
		public Segment perpendicular_segment_from(Point point)
			{
			//http://csharphelper.com/blog/2016/09/find-the-shortest-distance-between-a-point-and-a-line-segment-in-c/
			Point closest;
			float dx = b.x - a.x;
			float dy = b.y - a.y;
			if ((dx == 0) && (dy == 0)) { closest = a; }
			else
				{
				// Calculate the t that minimizes the distance.
				float t = ((point.x - a.x) * dx + (point.y - a.y) * dy) / (dx * dx + dy * dy);
				closest = new Point(a.x + t * dx, a.y + t * dy);
				}
			return new Segment(point, closest);
			}

		public bool contains(Point point)
			{
			if (!point.is_colinear(this)) { return false; }
			return point.x >= Mathf.Min(a.x, b.x) && point.x <= Mathf.Max(a.x, b.x)
				&& point.y >= Mathf.Min(a.y, b.y) && point.y <= Mathf.Max(a.y, b.y);
			}

		/// <summary> Returns true if there is an intersection between this segment and the parameter. </summary>
		public bool intersects(Segment segment)
			{
			return intersects_line(segment) && segment.intersects_line(this);
			}

		/// <summary> Returns true if there is an intersection between this segment and line to which the the parameter segment belongs. </summary>
		public bool intersects_line(Segment segment)
			{
			return (a.is_on_left(segment) && b.is_on_right(segment))
				|| (a.is_on_right(segment) && b.is_on_left(segment));
			}

		public bool intersects(Polygon polygon)
			{
			foreach (var edge in polygon.edges) { if (edge.intersects(this)) { return true; } }
			return false;
			}

		public void draw_gizmos() { Gizmos.DrawLine(a, b); }
		}
	}