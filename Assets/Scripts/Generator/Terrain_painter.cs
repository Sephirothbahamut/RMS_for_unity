using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Tile_indices = utils.Matrix<map_gen.Tile>.Indices;


public abstract partial class RMS
	{
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Per cell
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public class Terrain_painter_partial
		{
		public Terrain_painter_partial(HashSet<Cell> regions, RMS rms) =>
			(this.regions, this.rms) =
			(regions, rms);

		private HashSet<Cell> regions;
		private RMS rms;

		public Terrain_painter with(string terrain_name)
			{
			var terrain = rms.terrain(terrain_name);
			return new Terrain_painter(regions, rms, terrain);
			}
		}

	/// <summary>
	/// Generic regional fillers
	/// </summary>
	public class Terrain_painter : Modifier
		{
		public Terrain_painter(HashSet<Cell> regions, RMS rms, Rect terrain) : base(regions, rms) => (this.terrain) = (terrain);

		private Rect terrain;

		public override void action(ref map_gen.Tile tile, float weight) { if (weight == 1f || (Random.Range(0f, 1f) <= weight)) { tile.uv = terrain; } }
		}

	public Terrain_painter_partial paint(HashSet<Cell> from) { return new Terrain_painter_partial(from, this); }

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Per tile
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public class Terrain_tile_painter_partial
		{
		public Terrain_tile_painter_partial(HashSet<Tile_indices> tiles, RMS rms) => 
			(this.tiles, this.rms) = (tiles, rms);

		private HashSet<Tile_indices> tiles;
		private RMS rms;

		public Terrain_tile_painter with(string terrain_name)
			{
			var terrain = rms.terrain(terrain_name);
			return new Terrain_tile_painter(tiles, rms, terrain);
			}
		}

	/// <summary>
	/// Per-tile
	/// </summary>
	public class Terrain_tile_painter : Modifier_per_tile
		{
		public Terrain_tile_painter(HashSet<Tile_indices> tiles, RMS rms, Rect terrain) : base(tiles, rms) => (this.terrain) = (terrain);

		private Rect terrain;

		public override void action(ref map_gen.Tile tile, float weight) { tile.uv = terrain; }
		}


	public Terrain_tile_painter_partial paint(HashSet<Tile_indices> selector) { return new Terrain_tile_painter_partial(selector, this); }
	}