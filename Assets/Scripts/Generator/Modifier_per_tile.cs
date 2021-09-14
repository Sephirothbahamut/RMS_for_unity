using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Tile_indices = utils.Matrix<map_gen.Tile>.Indices;

public abstract partial class RMS
	{
	/// <summary>
	/// Performs actions on selected regions.
	/// </summary>
	public abstract partial class Modifier_per_tile
		{
		public Modifier_per_tile(HashSet<Tile_indices> tiles, RMS rms) => (this.tiles, this.rms) = (tiles, rms);

		private HashSet<Tile_indices> tiles;
		private RMS rms;

		public abstract void action(ref map_gen.Tile tile, float weight = 1f);

		public void all()
			{
			_foreach_tile((Tile_indices tile) => { action(ref rms.tile_at(tile)); });
			}

		public void uniform()
			{
			_foreach_tile((Tile_indices tile) => { action(ref rms.tile_at(tile), Random.Range(0f, 1f)); });
			}

		private void _foreach_tile(ActionRef function)
			{
			foreach (var tile in tiles) { function.Invoke(tile); }
			}

		delegate void ActionRef(Tile_indices tile);
		}
	}