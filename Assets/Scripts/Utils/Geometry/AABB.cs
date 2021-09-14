using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace utils.geometry
	{
	public abstract partial class Polygon
		{
		public class AABB : Polygon
			{
			public float top;
			public float left;
			public float bottom;
			public float right;

			public Point top_left { get { return new Point(left, top); } }
			public Point top_right { get { return new Point(right, top); } }
			public Point bottom_right { get { return new Point(right, bottom); } }
			public Point bottom_left { get { return new Point(left, bottom); } }

			public float x { get { return left; } }
			public float y { get { return top; } }
			public float x1 { get { return left; } }
			public float y1 { get { return top; } }
			public float x2 { get { return right; } }
			public float y2 { get { return bottom; } }
			public float width { get { return right - left; } }
			public float height { get { return bottom - top; } }

			public override Point[] vertices { get { return new Point[] { top_left, top_right, bottom_right, bottom_left }; } }

			public AABB(float left, float top, float right, float bottom) =>
				(this.left, this.top, this.right, this.bottom) = (left, top, right, bottom);

			public AABB(Point origin, float width, float height) : this(origin, origin + new Point(width, height)) { }
			public AABB(Point top_left, Point bottom_right) : this(top_left.x, top_left.y, bottom_right.x, bottom_right.y) { }

			public override bool contains(Point point)
				{
				return point.x >  left  && point.y >  top
					&& point.x <  right && point.y <  bottom;
				}
			public override bool contains_strict(Point point)
				{
				return point.x >= left  && point.y >= top
					&& point.x <= right && point.y <= bottom;
				}
			}
		}
	}