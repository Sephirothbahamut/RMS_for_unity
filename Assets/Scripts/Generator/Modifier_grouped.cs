using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using In_cell_tile_indices = utils.Matrix<utils.Matrix<map_gen.Tile>.Indices>.Indices;

public abstract partial class RMS
	{
	/// <summary>
	/// Performs actions on selected regions.
	/// </summary>
	public abstract partial class Modifier
		{

		public void centre() { foreach (var cell in regions) { action(ref rms.tile_at(cell.point)); } }

		public void grouped(uint amount)
			{
			foreach (var cell in regions)
				{
				In_cell_tile_indices? in_cell_tile_indices = null;
				//Pick a random tiles_index from the cell until a valid one is found
				while (in_cell_tile_indices == null)
					{
					var tmp = new In_cell_tile_indices(Random.Range(0, cell.tiles_indices.width), Random.Range(0, cell.tiles_indices.height));

					if((bool)cell.tiles_indices[tmp])
						{
						in_cell_tile_indices = tmp;
						}
					}

				_grouped(ref amount, cell, in_cell_tile_indices.Value);
				}
			}

		#region Grouped_private

		private void _grouped(ref uint missing, Cell cell, In_cell_tile_indices in_cell_tile_indices)
			{
			if (missing > 0) { missing--; }

			action(ref rms.tiles[cell.tiles_indices[in_cell_tile_indices]]);

			//Create a list of candidate tiles taking the tiles adjacent to the current one, IF they belong to a given cell
			var candidates_indices = new List<utils.Matrix<utils.Matrix<map_gen.Tile>.Indices>.Indices>();

			System.Action<int, int> add_candidate_indices = (int x, int y) =>
				{
				var indices = new In_cell_tile_indices(x, y);
				if((bool)cell.tiles_indices[indices]) { candidates_indices.Add(indices); }
				};

			if (in_cell_tile_indices.x > 0)                             { add_candidate_indices(in_cell_tile_indices.x - 1, in_cell_tile_indices.y    ); }
			if (in_cell_tile_indices.y > 0)                             { add_candidate_indices(in_cell_tile_indices.x,     in_cell_tile_indices.y - 1); }
			if (in_cell_tile_indices.x < cell.tiles_indices.width  - 1) { add_candidate_indices(in_cell_tile_indices.x + 1, in_cell_tile_indices.y    ); }
			if (in_cell_tile_indices.y < cell.tiles_indices.height - 1) { add_candidate_indices(in_cell_tile_indices.x,     in_cell_tile_indices.y + 1); }

			//while because if recursive calls fail to find a valid tile, we will keep looking for one upon stack unwinding
			while (missing > 0 && candidates_indices.Count > 0)
				{
				int index = Random.Range(0, candidates_indices.Count);
				var candidate_indices = candidates_indices[index];
				candidates_indices.RemoveAt(index);

				if ((bool)candidate_indices) { _grouped(ref missing, cell, candidate_indices); }
				}
			}

		utils.Matrix<map_gen.Tile>.Indices _random_adjacent_tile_indices(Cell cell, utils.Matrix<map_gen.Tile>.Indices from)
			{
			while (true)
				{
				int tmp_x = from.x + Random.Range(-1, 2);
				int tmp_y = from.y + Random.Range(-1, 2);

				if (tmp_x >= 0 && tmp_x < cell.tiles_indices.width &&
					tmp_y >= 0 && tmp_y < cell.tiles_indices.height)
					{
					var new_indices = cell.tiles_indices[tmp_x, tmp_y];
					if ((bool)new_indices) { return new_indices; }
					}
				}
			}
		}
	#endregion
	}