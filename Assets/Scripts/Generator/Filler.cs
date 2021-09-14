using System.Collections;
using System.Collections.Generic;
using UnityEngine;



using Tile_indices = utils.Matrix<map_gen.Tile>.Indices;

public abstract partial class RMS
	{
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Per cell
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public class Filler_partial
		{
		public Filler_partial(HashSet<Cell> regions, RMS rms) => 
			(this.regions, this.rms) = 
			(regions,      rms);

		private HashSet<Cell> regions;
		private RMS rms;

		public Filler with(string resource_name)
			{
			var resource = rms.resource(resource_name);
			if (resource == null) { Debug.LogError("Resource \"" + resource_name + "\" not found."); }
			return new Filler(regions, rms, resource);
			}

		public void clear()
			{
			foreach (var cell in regions)
				{
				foreach (var tile_indices in cell.tiles_indices.data())
					{
					if (!(bool)tile_indices) { continue; }
					ref var tile = ref rms.tile_at(tile_indices);
					foreach (var obj in tile.objects) { utils.destruction.destroy(obj, rms.manager); }
					tile.objects.Clear();
					}
				}
			}
		}

	/// <summary>
	/// Generic regional fillers
	/// </summary>
	public class Filler : Modifier
		{
		public Filler(HashSet<Cell> regions, RMS rms, Resource resource) : base(regions, rms) => (this.resource) = (resource);

		private Resource resource;

		public override void action(ref map_gen.Tile tile, float weight) { if (weight == 1f || (Random.Range(0f, 1f) <= weight)) { resource.spawn(ref tile); } }
		}

	public Filler_partial populate(HashSet<Cell> from) { return new Filler_partial(from, this); }

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Per tile
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public class Filler_tile_partial
		{
		public Filler_tile_partial(HashSet<Tile_indices> tiles, RMS rms) =>
			(this.tiles, this.rms) = (tiles, rms);

		private HashSet<Tile_indices> tiles;
		private RMS rms;

		public Filler_tile with(string resource_name)
			{
			var resource = rms.resource(resource_name);
			return new Filler_tile(tiles, rms, resource);
			}

		public void clear()
			{
			foreach (var tile_indices in tiles)
				{
				ref var tile = ref rms.tile_at(tile_indices);
				foreach (var obj in tile.objects) { utils.destruction.destroy(obj, rms.manager); }
				tile.objects.Clear();
				}
			}
		}

	/// <summary>
	/// Per-tile
	/// </summary>
	public class Filler_tile : Modifier_per_tile
		{
		public Filler_tile(HashSet<Tile_indices> tiles, RMS rms, Resource resource) : base(tiles, rms) => (this.resource) = (resource);

		private Resource resource;

		public override void action(ref map_gen.Tile tile, float weight) { if (weight == 1f || (Random.Range(0f, 1f) <= weight)) { resource.spawn(ref tile); } }
		}


	public Filler_tile_partial populate(HashSet<Tile_indices> selector) { return new Filler_tile_partial(selector, this); }
	}