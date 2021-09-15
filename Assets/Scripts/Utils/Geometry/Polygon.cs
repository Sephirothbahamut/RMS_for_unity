using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace utils.geometry
	{
	public abstract partial class Polygon
		{
		public abstract Point  [] vertices { get; }
		public virtual  Segment[] edges
			{
			get
				{
				Segment[] ret = new Segment[vertices.Length];
				for (int i = 0; i < vertices.Length - 1; i++) { ret[i] = new Segment(vertices[i], vertices[i + 1]); }
				ret[vertices.Length - 1] = new Segment(vertices[vertices.Length - 1], vertices[0]);
				return ret;
				}
			}

		public Segment edge(int index_a, int index_b) { return new Segment(vertices[index_a], vertices[index_b]); }
		public Segment edge(int index) { var ret = edge(index, (index + 1) % vertices.Length); return ret; }
		public Point vertex(int index) { return vertices[index]; }

		public virtual bool contains(Polygon other)
			{
			foreach (var vertex in other.vertices) { if (!contains(vertex)) { return false; } }
			return true;
			}
		public bool touches(Polygon other) { return contains(other) || other.contains(this) || intersects(other); }

		public bool intersects(Polygon other)
			{
			foreach (var edge in other.edges) { if (edge.intersects(this)) { return true; } }
			return false;
			}

		public virtual bool contains(Point point)
			{
			// A point is inside a polygon if given a line in any direction, it intersects the polygon segments an uneven number of times
			bool is_inside = false;
			var tmp_segment = new Segment(point, new Point(point.x + bounding_box.x2, point.y)); //A segment which lies on a generic horizontal line

			foreach (var edge in edges) { if (edge.intersects(tmp_segment)) { is_inside = !is_inside; } }
			return is_inside;
			}
		public virtual bool contains_strict(Point point)
			{
			if (contains(point)) { return true; }
			foreach (var edge in edges) { if (edge.contains(point)) { return true; } }
			return false;
			}

		public virtual AABB bounding_box
			{
			get
				{
				float min_x = vertices[0].x;
				float max_x = vertices[0].x;
				float min_y = vertices[0].y;
				float max_y = vertices[0].y;
				for (int i = 1; i < vertices.Length; i++)
					{
					var vertex = vertices[i];
					min_x = Mathf.Min(vertex.x, min_x);
					max_x = Mathf.Max(vertex.x, max_x);
					min_y = Mathf.Min(vertex.y, min_y);
					max_y = Mathf.Max(vertex.y, max_y);
					}
				return new AABB(min_x, min_y, max_x, max_y);
				}
			}

		public Point centre
			{
			get
				{
				var ret = new Point(0f, 0f);
				foreach (var vertex in vertices) { ret += vertex; }
				ret /= vertices.Length;
				return ret;
				}
			}

		public virtual Point centroid
			{
			get
				{
				// https://stackoverflow.com/questions/2792443/finding-the-centroid-of-a-polygon
				Point centroid = new Point(0, 0);
				float signedArea = 0.0f;
				float x0 = 0.0f; // Current vertex X
				float y0 = 0.0f; // Current vertex Y
				float x1 = 0.0f; // Next vertex X
				float y1 = 0.0f; // Next vertex Y
				float a  = 0.0f; // Partial signed area

				// For all vertices except last
				int i=0;
				for (i = 0; i < vertices.Length - 1; ++i)
					{
					x0 = vertices[i].x;
					y0 = vertices[i].y;
					x1 = vertices[i + 1].x;
					y1 = vertices[i + 1].y;
					a = x0 * y1 - x1 * y0;
					signedArea += a;
					centroid.x += (x0 + x1) * a;
					centroid.y += (y0 + y1) * a;
					}

				// Do last vertex separately to avoid performing an expensive
				// modulus operation in each iteration.
				x0 = vertices[i].x;
				y0 = vertices[i].y;
				x1 = vertices[0].x;
				y1 = vertices[0].y;
				a = x0 * y1 - x1 * y0;
				signedArea += a;
				centroid.x += (x0 + x1) * a;
				centroid.y += (y0 + y1) * a;

				signedArea *= 0.5f;
				centroid.x /= (6.0f * signedArea);
				centroid.y /= (6.0f * signedArea);

				return centroid;
				}
			}


		public void draw_gizmos()
			{
			foreach (var edge in edges) { Gizmos.DrawLine(edge.a, edge.b); }
			}
		public void draw_gizmos_rising()
			{
			float height = 0;
			foreach (var edge in edges)
				{
				Gizmos.DrawLine
					(
					edge.a.to_vector3(height),
					edge.b.to_vector3(height + 1)
					);
				height++;
				}
			}
		}
	}