using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using utils.extensions;
using utils.geometry;
using Tile_indices = utils.Matrix<map_gen.Tile>.Indices;

public abstract partial class RMS
	{
	public class Selector_per_tile : IEnumerable<Tile_indices>
		{
		public Selector_per_tile(ICollection<Tile_indices> from, RMS rms) { selected = new HashSet<Tile_indices>(from); this.rms = rms; }
		public Selector_per_tile(Tile_indices from, RMS rms) { selected = new HashSet<Tile_indices> { from }; this.rms = rms; }
		public Selector_per_tile(Selector_per_tile from, RMS rms) => (this.selected, this.rms) = (from.get(), rms);
		private HashSet<Tile_indices> selected;
		private RMS rms;

		/// <summary> Selects [count] random regions. </summary>
		public Selector_per_tile random(int count = 1)
			{
			var ret = new HashSet<Tile_indices>();
			var from = new List<Tile_indices>(selected);

			for (uint i = 0; i < count && i < from.Count; i++)
				{
				int index = Random.Range(0, from.Count);
				ret.Add(from[index]);
				from.RemoveAt(index);
				}

			return new Selector_per_tile(ret, rms);
			}

		/// <summary> Selects all the regions which are adjacent to the currently selected regions. </summary>
		public Selector_per_tile adjacent()
			{
			var ret = new HashSet<Tile_indices>();

			var width  = rms.map.tiles.width;
			var height = rms.map.tiles.height;

			foreach (var tile in this)
				{
				/*tile_top_left    */
				if (tile.x > 0 && tile.y > 0) { ret.Add(new Tile_indices(tile.x - 1, tile.y - 1)); }
				/*tile_top         */
				if (tile.y > 0) { ret.Add(new Tile_indices(tile.x, tile.y - 1)); }
				/*tile_top_right   */
				if (tile.x < width - 1 && tile.y > 0) { ret.Add(new Tile_indices(tile.x + 1, tile.y - 1)); }
				/*tile_bottom_left */
				if (tile.x > 0 && tile.y < height - 1) { ret.Add(new Tile_indices(tile.x - 1, tile.y + 1)); }
				/*tile_bottom      */
				if (tile.y < height - 1) { ret.Add(new Tile_indices(tile.x, tile.y + 1)); }
				/*tile_bottom_right*/
				if (tile.x < width - 1 && tile.y < height - 1) { ret.Add(new Tile_indices(tile.x + 1, tile.y + 1)); }
				/*tile_right       */
				if (tile.x < width - 1) { ret.Add(new Tile_indices(tile.x + 1, tile.y)); }
				/*tile_left        */
				if (tile.x > 0) { ret.Add(new Tile_indices(tile.x - 1, tile.y)); }
				}
			return new Selector_per_tile(ret, rms).except(this);
			}

		/// <summary> Selects all the tiles with a certain delta height with their neighbours. <summary>
		public Selector_per_tile sloped(float minimum_delta_height, float maximum_delta_height)
			{
			var ret = new HashSet<Tile_indices>();
			foreach (var tile in this)
				{
				var current_slope = rms.tile_at(tile).slope;

				if (current_slope >= minimum_delta_height &&
					current_slope <= maximum_delta_height)
					{ ret.Add(tile); }
				}
			return new Selector_per_tile(ret, rms);
			}
		/// <summary> Selects all the tiles with a certain delta height with their neighbours. <summary>
		public Selector_per_tile sloped(float delta_height)
			{
			var ret = new HashSet<Tile_indices>();
			foreach (var tile in this)
				{
				if (rms.tile_at(tile).slope == delta_height)
					{ ret.Add(tile); }
				}
			return new Selector_per_tile(ret, rms);
			}

		/// <summary> Selects all the tiles at a certain height. <summary>
		public Selector_per_tile height(float minimum_height, float maximum_height)
			{
			var ret = new HashSet<Tile_indices>();
			foreach (var tile in this)
				{
				if (rms.tile_at(tile).height >= minimum_height &&
					rms.tile_at(tile).height <= maximum_height)
					{ ret.Add(tile); }
				}
			return new Selector_per_tile(ret, rms);
			}
		/// <summary> Selects all the tiles at a certain height. <summary>
		public Selector_per_tile height(float height)
			{
			var ret = new HashSet<Tile_indices>();
			foreach (var tile in this)
				{
				if (rms.tile_at(tile).height == height)
					{ ret.Add(tile); }
				}
			return new Selector_per_tile(ret, rms);
			}
		/// <summary> Selects all the tiles with a certain terrain. <summary>
		public Selector_per_tile terrain(string terrain_name)
			{
			var ret = new HashSet<Tile_indices>();
			foreach (var tile in this)
				{
				if (rms.tile_at(tile).uv == rms.terrain(terrain_name))
					{ ret.Add(tile); }
				}
			return new Selector_per_tile(ret, rms);
			}
		/// <summary> Selects all the tiles with a certain resource. <summary>
		public Selector_per_tile contains(string resource_name)
			{
			var ret = new HashSet<Tile_indices>();
			var resource = rms.resource(resource_name);
			foreach (var tile in this)
				{
				foreach (var obj in rms.tile_at(tile).objects)
					{
					if (resource.object_is(obj)) { ret.Add(tile); }
					}
				}
			return new Selector_per_tile(ret, rms);
			}

		/// <summary> Selects all the tiles along a line with the given thickness. <summary>
		public Selector_per_tile in_line(Selector froms, Selector tos, float thickness, float additional_length = 0f)
			{
			var ret = new HashSet<Tile_indices>();
			foreach (var region in froms)
				{
				foreach (var other_region in tos)
					{
					var poly = Polygon.Rectangle.from_segment(new Segment(region, other_region), thickness, additional_length);

					foreach (var tile in selected)
						{
						if (poly.contains(rms.tile_at(tile).center_position)) { ret.Add(tile); }
						}
					}
				}
			return new Selector_per_tile(ret, rms);
			}

		/// <summary> Removes the passed parameter tiles from the currently selected ones. <summary>
		public Selector_per_tile except(Selector_per_tile exclusions)
			{
			var from = new HashSet<Tile_indices>(selected);
			foreach (var exclude in exclusions) { from.Remove(exclude); }
			return new Selector_per_tile(from, rms);
			}

		/// <summary> Adds the passed parameter tiles to the currently selected ones. <summary>
		public Selector_per_tile include(Selector_per_tile inclusions)
			{
			var from = new HashSet<Tile_indices>(selected);
			foreach (var include in inclusions) { from.Add(include); }
			return new Selector_per_tile(from, rms);
			}

		public Selector_per_tile except(Tile_indices cell) { return except(new Selector_per_tile(cell, rms)); }
		public Selector_per_tile include(Tile_indices cell) { return include(new Selector_per_tile(cell, rms)); }

		public bool is_empty { get { return selected.Count != 0; } }
		public int count { get { return selected.Count; } }

		private HashSet<Tile_indices> get() { return selected; }
		public static implicit operator HashSet<Tile_indices>(Selector_per_tile selector) { return selector.selected; }

		public IEnumerator<Tile_indices> GetEnumerator() { return selected.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
		}

	public Selector_per_tile select_tiles_from(Tile_indices tile) { return new Selector_per_tile(tile, this); }
	public Selector_per_tile tiles_in_region(Cell cell) { return tiles_in_regions(new Selector(cell, this)); }
	public Selector_per_tile tiles_in_regions(Selector cells)
		{
		var ret = new HashSet<Tile_indices>();

		foreach (var cell in cells)
			{
			foreach (var tile in cell.tiles_indices.data())
				{
				if ((bool)tile) { ret.Add(tile); }
				}
			}

		return new Selector_per_tile(ret, this);
		}
	public Selector_per_tile tiles_in_line(Selector froms, Selector tos, float thickness) { return tiles_all.in_line(froms, tos, thickness); }

	public Selector_per_tile sloped(float minimum_delta_height, float maximum_delta_height) { return tiles_all.sloped(minimum_delta_height, maximum_delta_height); }
	public Selector_per_tile sloped(float delta_height                                    ) { return tiles_all.sloped(delta_height); }
	public Selector_per_tile height(float minimum_height,       float maximum_height)       { return tiles_all.sloped(minimum_height, maximum_height); }
	public Selector_per_tile height(float height                                          ) { return tiles_all.sloped(height); }

	public Selector_per_tile tiles_all 
		{ 
		get
			{
			var indices = new List<Tile_indices>(tiles.size);
			foreach (var tile in tiles.data())
				{
				indices.emplace(tile.matrix_x, tile.matrix_y);
				}
			return new Selector_per_tile(indices, this); 
			}
		}

	}