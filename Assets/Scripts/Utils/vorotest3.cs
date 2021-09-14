using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using utils.geometry;
using utils.random;
using utils.extensions;


public class vorotest3 : MonoBehaviour
	{
	public class Cell : utils.geometry.Cell
		{
		public Cell(Point point, Polygon.Convex polygon, bool is_border) : base(point, polygon, is_border) { }

		public List<utils.Matrix<GameObject>.Indices> tiles_coords = new List<utils.Matrix<GameObject>.Indices>();
		}

	Voronoi<Cell> v = null;

	[EasyButtons.Button]
	void do_stuff()
		{
		var border = new Polygon.AABB(new Point(0f, 0f), 100f, 100f);

		var bounding_box = border.bounding_box;
		PoissonDiscSampler sampler = new PoissonDiscSampler(bounding_box, 10f);

		var points = new List<Point>();

		foreach (var sample in sampler.Samples())
			{
			points.emplace(bounding_box.x + sample.x, bounding_box.y + sample.y);
			}

		v = new Voronoi<Cell>(bounding_box, points);
		}

	[SerializeField]
	int draw_highlight = -1;
	private void OnDrawGizmos()
		{
		if (v != null)
			{
			foreach (var cell in v.cells)
				{
				Gizmos.color = Color.magenta;

				cell.point.draw_gizmos();

				Gizmos.color = Color.cyan;
				cell.polygon.draw_gizmos();

				Gizmos.color = Color.red;
				foreach (var neighbour in cell.neighbours)
					{
					new Segment(cell.point, neighbour.point).draw_gizmos();
					}

				}

			if (draw_highlight >= 0 && draw_highlight < v.cells.Count)
				{
				var cell = v.cells[draw_highlight];

				Gizmos.color = Color.red;
				cell.point.draw_gizmos(1f);

				Gizmos.color = Color.black;
				cell.polygon.draw_gizmos_rising();
				}
			}
		}
	}
