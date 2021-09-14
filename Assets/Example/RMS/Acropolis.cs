using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using com.spacepuppy.Tween;


namespace random_map_scripts
	{

	public class Acropolis : RMS
		{
		protected override int   tiles_width                  { get { return 160 + (20 * players_count); } }
		protected override int   tiles_height                 { get { return 160 + (20 * players_count); } }
		protected override float minimum_region_radius        { get { return 10; } }
		protected override float minimum_player_starting_space{ get { return 20; } }

		public override void run()
			{
			paint(regions.all).with("Grass").all();

			var hills_layer = create_new_regions_layer_with_spawn_positions(minimum_region_radius * 2);

			var risen_hills = hills_layer.none;

			foreach (var spawn in regions.spawn)
				{
				var start_resources_regions = regions.empty.closest_to(spawn, 5);

				// Create a large hill on the spawn
				var hill = hills_layer.all.touching(start_resources_regions.include(spawn));
				rise(hill).by(5).all();
				risen_hills = risen_hills.include(hill);

				populate(spawn).with("Town_center").center();

				var forests = start_resources_regions.random(3);
				populate(forests).with("Tree").radial(from_border: 0.5f);
				paint(forests).with("Underbush").radial();

				var gold = start_resources_regions.except(regions.filled).random();
				populate(gold).with("Gold").grouped(6);

				var berries = start_resources_regions.except(regions.filled);
				populate(berries).with("Berry").grouped(8);
				}

			var excluded = regions.all.touching(risen_hills);

			var remaining = regions.all.except(excluded);
			remaining = remaining.except(excluded.adjacent());

			var relics = remaining.random(players_count * 2);
			populate(relics).with("Relic").center();
			remaining = remaining.except(relics);

			var sparse_forests = remaining.random(remaining.count / 4);
			populate(sparse_forests).with("Tree").radial(EaseStyle.Linear);
			paint(sparse_forests).with("Underbush").radial();
			remaining = remaining.except(sparse_forests);

			var sparse_hills = hills_layer.all.except(risen_hills).except(risen_hills.adjacent());
			rise(sparse_hills).by(4).radial(EaseStyle.LinearEaseInOut, from_center: 0.1f);

			smooth(4);

			// Paint hills after smoothing
			var candidate_cliff = risen_hills.include(select_from(risen_hills).adjacent());
			var candidate_cliff_tiles = candidate_cliff.tiles();

			paint(candidate_cliff_tiles.sloped(0.5f, float.PositiveInfinity)).with("Cliff").all();

			// Leave grassy terrain paths (there's always at least 1 neighbour cell)
			foreach (var hill in risen_hills)
				{
				var path = candidate_cliff_tiles.in_line(select_from(hill), select_from(hill).adjacent(), tile_size * 5);
				paint(path).with("Grass").all();
				}
			}
		}
	}