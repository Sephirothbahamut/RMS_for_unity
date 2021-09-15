using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using com.spacepuppy.Tween;

public abstract partial class RMS
	{
	/// <summary>
	/// Performs actions on selected regions.
	/// </summary>
	public abstract partial class Modifier
		{
		public Modifier(HashSet<Cell> regions, RMS rms) => (this.regions, this.rms) = (regions, rms);

		private HashSet<Cell> regions;
		private RMS rms;

		public abstract void action(ref map_gen.Tile tile, float weight = 1f);

		public void all()
			{
			_foreach_tile((ref map_gen.Tile tile, Cell cell) => { action(ref tile); });
			}

		public void uniform()
			{
			_foreach_tile((ref map_gen.Tile tile, Cell cell) => { action(ref tile, Random.Range(0f, 1f)); });
			}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ease_style">The easing function used</param>
		/// <param name="from_centre">How much distant (normalized between 0-1) from the centre will the easing function be used. From centre to this distance the value will be constant at inner_weight.</param>
		/// <param name="from_border">How much distant (normalized between 0-1) from the border will the easing function be used. From border to this distance the value will be constant at outer_weight.</param>
		/// <param name="inner_weight">The value between the centre and from_centre, and at the inner side of the easing function.</param>
		/// <param name="outer_weight">The value between the border and from_border, and at the outer side of the easing function.</param>
		public void radial(EaseStyle ease_style = EaseStyle.LinearEaseInOut, float from_centre = 0f, float from_border = 0f, float inner_weight = 1f, float outer_weight = 0f)
			{
			_foreach_tile((ref map_gen.Tile tile, Cell cell) =>
			{
				var dist = cell.proportional_distance(tile.center_position);
				float weight = 0f;
				if (dist <= from_centre) { weight = inner_weight; }
				else if (1f - dist <= from_border) { weight = outer_weight; }
				else
					{
					dist = utils.math.map(dist, from_centre, 1f - from_border, 0f, 1f);
					weight = EaseMethods.GetEase(ease_style)(dist, inner_weight, outer_weight - inner_weight, 1f); 
					}

				action(ref tile, weight);
			});
			}

		private void _foreach_tile(ActionRef function)
			{
			foreach (var cell in regions)
				{
				foreach (var tile_indices in cell.tiles_indices.data())
					{
					if ((bool)tile_indices)
						{
						function.Invoke(ref rms.tile_at(tile_indices), cell);
						}
					}
				}
			}

		delegate void ActionRef(ref map_gen.Tile tile, Cell cell);
		}
	}