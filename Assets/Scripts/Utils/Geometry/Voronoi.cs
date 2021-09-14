using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using utils.extensions;
using VoronoiLib.Structures;

namespace utils.geometry
	{
	public class Cell
		{
		public readonly Point point;
		public readonly Polygon.Convex polygon;
		public readonly bool is_border;
		public List<Cell> neighbours = new List<Cell>();

		public Point  [] vertices { get { return polygon.vertices; } }
		public Segment[] edges    { get { return polygon.edges; } }

		public Cell(Point point, Polygon.Convex polygon, bool is_border) =>
			(this.point, this.polygon, this.is_border) =
			(point,      polygon,      is_border);

		public float proportional_distance(Point position)
			{
			for (int i = 0; i < vertices.Length; i++)
				{
				var triangle = new Polygon.Triangle(point, vertices[i], vertices[(i + 1) % vertices.Length]);
				if (triangle.contains(position)) { return triangle.proportional_distance(position, Polygon.Triangle.A); }
				}
			return float.NaN;
			}
		public float proportional_distance_to_center(Point position)
			{
			for (int i = 0; i < vertices.Length; i++)
				{
				var triangle = new Polygon.Triangle(polygon.center, vertices[i], vertices[(i + 1) % vertices.Length]);
				if (triangle.contains(position)) { return triangle.proportional_distance(position, Polygon.Triangle.A); }
				}
			return float.NaN;
			}
		}

	public class Voronoi<Cell_t> where Cell_t : Cell
		{
		public List<Cell_t> cells = null;

		public Voronoi(Polygon border, ICollection<Point> points)
			{
			var sites = new List<FortuneSite>(points.Count);
			foreach (var point in points) { sites.Add(point); }

			var bounding_box = border.bounding_box;
			var edges = VoronoiLib.FortunesAlgorithm.Run(sites,
				(double)bounding_box.x1,
				(double)bounding_box.y1,
				(double)bounding_box.x2,
				(double)bounding_box.y2);

			//Add edges to the sites
			foreach (var edge in edges)
				{
				edge.Left .Cell.Add(edge);
				edge.Right.Cell.Add(edge);
				}

			cells = build(sites, bounding_box);
			}


		private class Cells_builder
			{
			private Dictionary<FortuneSite, Cell_t> builders = new Dictionary<FortuneSite, Cell_t>();
			private Polygon.AABB bb = null;
			public Cells_builder(Polygon.AABB bb) { this.bb = bb; }

			public Cell_t build(FortuneSite site)
				{
				var vertices = new List<Point>(site.Cell.Count);

				// Prepare edges by sorting them clockwise and filling the missing gaps with the general border
				var edges = new List<Segment>();

				foreach (var edge in site.Cell)
					{
					Point beg = edge.Start;
					Point end = edge.End;
					if (edge.Right == site) { edges.emplace(beg, end); }
					else { edges.emplace(end, beg); }
					}
				edges.Sort((segment_a, segment_b) => { return Point.compare_clockwise(site, segment_a.a, segment_b.a); });
				var is_border = complete_edges(ref edges);

				// Assign the vertices to the cell's polygon border
				foreach (var edge in edges) { vertices.Add(edge.a); }
				var polygon = new Polygon.Convex(vertices);
				
				Cell_t cell = construct.generic<Cell_t>((Point)site, polygon, is_border);

				// Assign neighbours
				// Note: Does NOT use site.Neighbours, because it includes voronoi cells which would be neighbours when extended outside of bounds.
				// By using the shared adges we make sure to not include those.

				//foreach (var neighbour_site in site.Neighbors)
				foreach (var edge in site.Cell)
					{
					FortuneSite neighbour_site = edge.Left == site ? edge.Right : edge.Left;

					Cell_t neighbour_cell = null;
					if (builders.TryGetValue(neighbour_site, out neighbour_cell))
						{
						cell.neighbours.Add(neighbour_cell);
						neighbour_cell.neighbours.Add(cell);
						}
					}

				builders.Add(site, cell);

				return cell;
				}

			private bool complete_edges(ref List<Segment> edges)
				{
				bool corner_ul = false;
				bool corner_ur = false;
				bool corner_dr = false;
				bool corner_dl = false;
				bool is_border = false;

				for (int i = 0; i < edges.Count; i++)
					{
					var edge_prev = edges[(i == 0)? edges.Count - 1 : i - 1];
					var edge_next = edges[i];

					var prev_end = edge_prev.b;
					var next_beg = edge_next.a;

					if (prev_end != next_beg)
						{
						is_border = true;
						Point beg = edge_prev.b;
						Point end;

						// UP-LL
						     if (prev_end.x == bb.x1 && prev_end.y > bb.y1 && next_beg.x > bb.x1 && !corner_ul) { end = new Point(bb.x1, bb.y1); corner_ul = true; }
						// UP-RR
						else if (prev_end.y == bb.y1 && prev_end.x < bb.x2 && next_beg.y > bb.y1 && !corner_ur) { end = new Point(bb.x2, bb.y1); corner_ur = true; }
						// DW-RR
						else if (prev_end.x == bb.x2 && prev_end.y < bb.y2 && next_beg.x < bb.x2 && !corner_dr) { end = new Point(bb.x2, bb.y2); corner_dr = true; }
						// DW-LL
						else if (prev_end.y == bb.y2 && prev_end.x > bb.x1 && next_beg.y < bb.y2 && !corner_dl) { end = new Point(bb.x1, bb.y2); corner_dl = true; }
						//Other cases:
						else
							{
							end = (edge_prev.b + edge_next.a) / 2;
							edges.Insert(i, new Segment(beg, end));
							edges.Insert(i + 1, new Segment(end, edge_next.a));
							continue;
							}

						edges.Insert(i, new Segment(beg, end));
						}
					}
				return is_border;
				}
			}

		public static List<Cell_t> build(IEnumerable<FortuneSite> sites, Polygon.AABB bb)
			{
			var builder = new Cells_builder(bb);

			var ret = new List<Cell_t>();
			foreach (var site in sites) { ret.Add(builder.build(site)); }

			return ret;
			}
		}
	}