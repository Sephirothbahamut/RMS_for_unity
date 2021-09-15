using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using vorli = VoronoiLib.Structures;

namespace utils.geometry
	{
	/// <summary>
	/// Bridges between Unity's Vector2 and other libraries vector/vertex/point/site
	/// </summary>
	[System.Serializable]
	public struct Point
		{
		[SerializeField] public float x;
		[SerializeField] public float y;

		public Point(float x, float y) { this.x = x; this.y = y; }

		public float distance2(Point other)
			{
			var dx = x - other.x;
			var dy = y - other.y;
			return (dx * dx) + (dy * dy);
			}
		public float distance(Point other) { return Mathf.Sqrt(distance2(other)); }

		// Operators
		public static Point operator +(Point a, Point b) { return new Point(a.x + b.x, a.y + b.y); }
		public static Point operator *(Point a, Point b) { return new Point(a.x * b.x, a.y * b.y); }
		public static Point operator -(Point a, Point b) { return new Point(a.x - b.x, a.y - b.y); }
		public static Point operator /(Point a, Point b) { return new Point(a.x / b.x, a.y / b.y); }
		public static Point operator +(Point a, float b) { return new Point(a.x + b, a.y + b); }
		public static Point operator *(Point a, float b) { return new Point(a.x * b, a.y * b); }
		public static Point operator -(Point a, float b) { return new Point(a.x - b, a.y - b); }
		public static Point operator /(Point a, float b) { return new Point(a.x / b, a.y / b); }
		public static Point operator +(float b, Point a) { return a + b; }
		public static Point operator *(float b, Point a) { return a * b; }
		public static Point operator -(float b, Point a) { return a - b; }
		public static Point operator /(float b, Point a) { return a / b; }

		public static bool operator ==(Point a, Point b) { return a.x == b.x && a.y == b.y; }
		public static bool operator !=(Point a, Point b) { return !(a == b); }

		public bool is_on_left (Segment segment) { return new Polygon.Triangle(this, segment.a, segment.b).area2 >  0; }
		public bool is_on_right(Segment segment) { return new Polygon.Triangle(this, segment.a, segment.b).area2 <  0; }
		public bool is_colinear(Segment segment) { return new Polygon.Triangle(this, segment.a, segment.b).area2 == 0; }

		public static int compare_clockwise(Point centre, Point a, Point b)
			{
			//https://stackoverflow.com/questions/6989100/sort-points-in-clockwise-ordervar a = first.Start;

			if (a.x - centre.x >= 0 && b.x - centre.x <  0) { return +1; }
			if (a.x - centre.x <  0 && b.x - centre.x >= 0) { return -1; }
			if (a.x - centre.x == 0 && b.x - centre.x == 0)
				{
				if (a.y - centre.y >= 0 || b.y - centre.y >= 0) { return a.y > b.y ? +1 : -1; }
				return b.y > a.y ? +1 : -1;
				}

			// compute the cross product of vectors (centre -> a) x (centre -> b)
			double det = (a.x - centre.x) * (b.y - centre.y) - (b.x - centre.x) * (a.y - centre.y);
			if (det < 0) { return +1; }
			if (det > 0) { return -1; }

			// points a and b are on the same line from the centre
			// check which point is closer to the centre
			double d1 = (a.x - centre.x) * (a.x - centre.x) + (a.y - centre.y) * (a.y - centre.y);
			double d2 = (b.x - centre.x) * (b.x - centre.x) + (b.y - centre.y) * (b.y - centre.y);
			return d1 > d2 ? +1 : -1;
			}
		public static int compare_counterclockwise(Point centre, Point a, Point b) { return compare_clockwise(centre, b, a); }

		public float distance_to_line2(Segment line) { return line.perpendicular_segment_from(this).length2; }
		public float distance_to_line (Segment line) { return line.perpendicular_segment_from(this).length; }

		// Cast operators
		public static implicit operator Point(Vector2 other)            { return new Point             (        other.x,         other.y); }
		public static implicit operator Vector2(Point other)            { return new Vector2           (        other.x,         other.y); }
		public static implicit operator Point(Vector3 other)            { return new Point             (        other.x,        -other.z); } // Unity's z goes upwards, conventional 2d y goes downwards
		public static implicit operator Vector3(Point other)            { return new Vector3           (        other.x, 0,     -other.y); } // Unity's z goes upwards, conventional 2d y goes downwards
		public static implicit operator Point(vorli::VPoint other)      { return new Point             ((float) other.X, (float) other.Y); }
		public static implicit operator vorli::VPoint(Point other)      { return new vorli::VPoint     ((double)other.x, (double)other.y); }
		public static implicit operator Point(vorli::FortuneSite other) { return new Point             ((float) other.X, (float) other.Y); }
		public static implicit operator vorli::FortuneSite(Point other) { return new vorli::FortuneSite((double)other.x, (double)other.y); }

		public Vector3 to_vector3(float height = 0f) { return new Vector3(x, height, -y); }

		public void draw_gizmos(float radius = .2f) { Gizmos.DrawSphere(this, radius); }

		// Auto generated
		public override bool Equals(object obj)
			{
			return obj is Point point &&
				   x == point.x &&
				   y == point.y;
			}

		public override int GetHashCode()
			{
			int hashCode = 1502939027;
			hashCode = hashCode * -1521134295 + x.GetHashCode();
			hashCode = hashCode * -1521134295 + y.GetHashCode();
			return hashCode;
			}
		}
	}