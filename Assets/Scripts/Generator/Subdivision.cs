using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using utils.geometry;
using utils.random;
using utils.extensions;

public abstract partial class RMS
	{
	public class Regions
		{
		private Voronoi<Cell> voronoi;
		private HashSet<Cell> _spawn;
		private RMS rms;

		/// <summary>
		/// Additional layers
		/// </summary>
		public Regions(RMS rms, float minimum_region_radius)
			{
			this.rms = rms;
			var bounding_box = new Polygon.AABB(0, 0, rms.map.width, rms.map.height);

			// Sample random points
			PoissonDiscSampler sampler = new PoissonDiscSampler(bounding_box, minimum_region_radius);

			var points = new List<Point>();
			foreach (var sample in sampler.Samples()) { points.emplace(bounding_box.x + sample.x, bounding_box.y + sample.y); }

			// Create voronoi cells with the given points
			voronoi = new Voronoi<Cell>(bounding_box, points);

			bool take_tiles_from_toggle_state = rms.map.tiles[0, 0].assigned;
			_spawn = new HashSet<Cell>();
			foreach (var cell in voronoi.cells)
				{
				// Initialize cell user data
				cell.rms = rms;
				// Register the tiles contained by each region
				cell.take_regions(take_tiles_from_toggle_state);
				// Add the cells associated with spawn_points to the spawn regions set
				foreach (var spawn_region in rms.regions._spawn)
					{
					if (cell.polygon.contains_strict(spawn_region.point)) { _spawn.Add(cell); break; }
					}
				}
			}
		/// <summary>
		/// Additional layers
		/// </summary>
		public Regions(RMS rms, float minimum_region_radius, bool ignore)
			{
			this.rms = rms;
			var bounding_box = new Polygon.AABB(0, 0, rms.map.width, rms.map.height);

			var spawn_points = new List<Point>();
			foreach (var region in rms.regions.spawn) { spawn_points.Add(region.point); }

			// Sample random points
			PoissonDiscSampler sampler = new PoissonDiscSampler(bounding_box, minimum_region_radius, spawn_points);

			var points = new List<Point>();
			foreach (var sample in sampler.Samples()) { points.emplace(bounding_box.x + sample.x, bounding_box.y + sample.y); }

			// Create voronoi cells with the given points
			voronoi = new Voronoi<Cell>(bounding_box, points);

			bool take_tiles_from_toggle_state = rms.map.tiles[0, 0].assigned;
			_spawn = new HashSet<Cell>();
			foreach (var cell in voronoi.cells)
				{
				// Initialize cell user data
				cell.rms = rms;
				// Register the tiles contained by each region
				cell.take_regions(take_tiles_from_toggle_state);
				// Add the cells associated with spawn_points to the spawn regions set
				foreach (var spawn_region in rms.regions._spawn)
					{
					if (cell.polygon.contains_strict(spawn_region.point)) { _spawn.Add(cell); break; }
					}
				}
			}

		/// <summary>
		/// Base regions
		/// </summary>
		public Regions(RMS rms, float minimum_region_radius, int players_count, float minimum_player_starting_space)
			{
			var bounding_box = new Polygon.AABB(0, 0, rms.map.width, rms.map.height);

			// Sample random points
			PoissonDiscSampler sampler = new PoissonDiscSampler(bounding_box, minimum_region_radius);

			var points = new List<Point>();
			foreach (var sample in sampler.Samples()) { points.emplace(bounding_box.x + sample.x, bounding_box.y + sample.y); }

			// Select reference points as spawns in clockwise from centre of map
			var centre = (Vector2)((Polygon)bounding_box).centre;
			var reference_spawn_points = new List<Point>(players_count);

			var starting_angle = Random.Range(0f, 360f);
			var degrees_between_spawns = 360f / (float)players_count;
			var dist_from_center = (Mathf.Min(rms.map.width, rms.map.height) / 2f) - minimum_player_starting_space;

			var angle = starting_angle;
			for (int i = 0; i < players_count; i++, angle += degrees_between_spawns)
				{
				reference_spawn_points.Add(centre + (EVector2.from_deg(angle) * dist_from_center));
				}

			// Select the closest poisson points
			var candidate_spawn_points = new HashSet<Point>(points);
			var spawn_points = new HashSet<Point>();

			foreach (var reference_spawn_point in reference_spawn_points)
				{
				var closest_dist = float.PositiveInfinity;
				Point? closest_candidate = null;
				foreach (var candidate_spawn_point in candidate_spawn_points)
					{
					var tmp_dist = reference_spawn_point.distance(candidate_spawn_point);
					if (tmp_dist < closest_dist)
						{
						closest_dist = tmp_dist;
						closest_candidate = candidate_spawn_point;
						}
					}
				if (closest_candidate.HasValue)
					{
					candidate_spawn_points.Remove(closest_candidate.Value);
					spawn_points.Add(closest_candidate.Value);
					}
				}

			//TODO future expension: remove all points closer to the spawns than minimum_player_starting_space();

			// Create voronoi cells with the given points
			voronoi = new Voronoi<Cell>(bounding_box, points);

			_spawn = new HashSet<Cell>();
			foreach (var cell in voronoi.cells)
				{
				// Initialize cell user data
				cell.rms = rms;
				// Register the tiles contained by each region
				cell.take_regions();
				// Add the cells associated with spawn_points to the spawn regions set
				if (spawn_points.Contains(cell.point))
					{
					spawn_points.Remove(cell.point);
					_spawn.Add(cell);
					}
				}
			}


		public int size { get { return voronoi.cells.Count; } }
		public Cell this[int i] { get { return voronoi.cells[i]; } }

		/////////////////////////////////////////////////////////////////////// Selectors

		/// <summary> All the regions. </summary>
		public Selector all { get { return new Selector(voronoi.cells, rms); } }

		/// <summary> An empty selector. </summary>
		public Selector none { get { return new Selector(new HashSet<Cell>(), rms); } }

		/// <summary> All the regions with at least one edge at the border of the map. </summary>
		public Selector border { get { return all.border(); } }

		/// <summary> All the regions that don't reach the border of the map. </summary>
		public Selector inner { get { return all.inner(); } }

		/// <summary> All the regions on which at least a resource has been spawned. </summary>
		public Selector filled { get { return all.filled(); } }

		/// <summary> All the regions on which at no resource has been spawned. </summary>
		public Selector empty { get { return all.empty(); } }

		/// <summary> The regions containing spawn regions. </summary>
		public Selector spawn { get { return new Selector(_spawn, rms); } }
		}

	/// <summary>
	/// Creates another terrain subdivision which is independant from the default one, and can have different regions sizes.
	/// </summary>
	public Regions create_new_regions_layer(float minimum_region_radius) { return new Regions(this, minimum_region_radius); }
	public Regions create_new_regions_layer_with_spawn_positions(float minimum_region_radius) { return new Regions(this, minimum_region_radius, true); }
	}