using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public abstract partial class RMS
	{
	public class Terraformer_partial
		{
		public Terraformer_partial(HashSet<Cell> regions, RMS rms) =>
			(this.regions, this.rms) =
			(regions, rms);

		private HashSet<Cell> regions;
		private RMS rms;

		public Terraformer by(float delta_height)
			{
			return new Terraformer(regions, rms, delta_height);
			}
		}

	/// <summary>
	/// Generic regional fillers
	/// </summary>
	public class Terraformer : Modifier
		{
		public Terraformer(HashSet<Cell> regions, RMS rms, float delta_height) : base(regions, rms) => (this.delta_height) = (delta_height);

		private float delta_height;

		public override void action(ref map_gen.Tile tile, float weight) { tile.height += delta_height * weight; }
		}

	public Terraformer_partial rise(HashSet<Cell> from) { return new Terraformer_partial(from, this); }
	}