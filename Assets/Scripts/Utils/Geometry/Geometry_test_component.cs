using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace utils.geometry
	{
	public class Geometry_test_component : MonoBehaviour
		{
		[SerializeField]
		Point vertex_a;
		[SerializeField]
		Point vertex_b;
		[SerializeField]
		Point vertex_c;
		[SerializeField]
		Polygon.Triangle triangle = new Polygon.Triangle(new Point(), new Point(), new Point());

		[SerializeField]
		Point point  = new Point();
		[SerializeField]
		Point segment_a  = new Point();
		[SerializeField]
		Point segment_b  = new Point();

		[SerializeField]
		float thickness = 0;

		[SerializeField]
		float dist;
		[SerializeField]
		bool point_inside_triangle;
		[SerializeField]
		bool point_inside_thick_segment;

		bool calculated = false;

		Polygon thick_segment = null;

		private void OnValidate()
			{
			triangle.a = vertex_a;
			triangle.b = vertex_b;
			triangle.c = vertex_c;
			calculated = true;
			thick_segment = Polygon.Rectangle.from_segment(new Segment(segment_a, segment_b), thickness);

			point_inside_triangle = triangle.contains(point);
			point_inside_thick_segment = thick_segment.contains(point);
			}

		private void OnDrawGizmos()
			{
			if (calculated)
				{
				Gizmos.color = Color.yellow;
				point.draw_gizmos();

				Gizmos.color = Color.red;
				triangle.draw_gizmos();

				dist = triangle.proportional_distance(point, Polygon.Triangle.A);

				var tri_base = triangle.edge(Polygon.Triangle.B, Polygon.Triangle.C);
				var shortest_segment = tri_base.shortest_segment_from(point);
				var perpendi_segment = tri_base.perpendicular_segment_from(point);
				var tri_height_line  = tri_base.perpendicular_segment_from(triangle.a);

				var perpendicular_point_to_height_segment = tri_height_line.perpendicular_segment_from(point);
				var reference_vertex_to_point_projected_on_height = new Segment(triangle.a, perpendicular_point_to_height_segment.b);

				Gizmos.color = Color.blue;
				tri_base.draw_gizmos();
				tri_height_line.draw_gizmos();

				Gizmos.color = Color.cyan;
				perpendicular_point_to_height_segment.draw_gizmos();
				reference_vertex_to_point_projected_on_height.draw_gizmos();

				Gizmos.color = Color.white;
				var segment = new Segment(segment_a, segment_b);
				segment.draw_gizmos();

				Gizmos.color = Color.black;
				thick_segment.draw_gizmos();
				}
			}
		}
	}