using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using utils.geometry;

public abstract partial class RMS
	{
	public class Cell : utils.geometry.Cell
		{
		public Cell(Point point, Polygon.Convex polygon, bool is_border) : base(point, polygon, is_border) { }

		public utils.Matrix<utils.Matrix<map_gen.Tile>.Indices> tiles_indices;
		public RMS rms;

		public void take_regions(bool from_toggle_state = false)
			{
			var bb = polygon.bounding_box;

			var ul_most_tile = rms.tile_at(bb.top_left);
			var dr_most_tile = rms.tile_at(bb.bottom_right);

			tiles_indices = new utils.Matrix<utils.Matrix<map_gen.Tile>.Indices>(
				(dr_most_tile.matrix_x - ul_most_tile.matrix_x) + 1,
				(dr_most_tile.matrix_y - ul_most_tile.matrix_y) + 1,
				new utils.Matrix<map_gen.Tile>.Indices(-1, -1));

			// Populate the matrix with contained cells
			for (int y = ul_most_tile.matrix_y; y <= dr_most_tile.matrix_y; y++)
				{
				for (int x = ul_most_tile.matrix_x; x <= dr_most_tile.matrix_x; x++)
					{
					ref var tile = ref rms.map.tiles[x, y];
					if (polygon.contains_strict(tile.center_position) && tile.assigned == from_toggle_state) 
						{
						tile.assigned = !from_toggle_state;
						tiles_indices[x - ul_most_tile.matrix_x, y - ul_most_tile.matrix_y] = tile.matrix_indices;
						}
					}
				}
			}

		public bool is_empty
			{
			get
				{
				foreach (var index in tiles_indices.data())
					{
					if ((bool)index)
						{
						if (rms.tile_at(index).objects.Count != 0) { return false; }
						}
					}
				return true;
				}
			}
		public bool is_not_empty
			{ 
			get
				{
				foreach (var index in tiles_indices.data())
					{
					if ((bool)index)
						{
						if (rms.tile_at(index).objects.Count != 0) { return true; }
						}
					}
				return false;
				}
			}

		//For selector and filler
		public static implicit operator HashSet<Cell>(Cell cell) { return new HashSet<Cell> { cell }; }

		public static implicit operator Polygon(Cell cell) { return cell.polygon; }
		public static implicit operator Point(Cell cell)   { return cell.point; }

		public void draw_gizmos_tiles()
			{

			foreach (var tile_indices in tiles_indices.data())
				{
				if ((bool)tile_indices)
					{
					ref var tile = ref rms.map.tiles[tile_indices];
					var c = proportional_distance(tile.center_position);
					Gizmos.color = new Color(c, c, c, 0.8f);
					tile.draw_gizmos();
					}
				}
			}
		}
	}