using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using com.spacepuppy.Tween;


namespace random_map_scripts
	{

	public class Arabia : RMS
		{
		protected override int   tiles_width                  { get { return 160 + (20 * players_count); } }
		protected override int   tiles_height                 { get { return 160 + (20 * players_count); } }
		protected override float minimum_region_radius        { get { return 10; } }
		protected override float minimum_player_starting_space{ get { return 20; } }

		public override void run()
			{
			paint(regions.all).with("Grass").all();

			var hills_layer = create_new_regions_layer_with_spawn_positions(minimum_region_radius * 5);
			var candidate_hills = hills_layer.all;

			foreach (var spawn in regions.spawn)
				{
				var start_resources_regions = regions.empty.closest_to(spawn, include_targets: false, 5);

				populate(spawn).with("Town_center").centre();

				var forests = start_resources_regions.random(3);
				populate(forests).with("Tree").radial();
				paint(forests).with("Underbrush").radial();

				var gold = start_resources_regions.except(regions.filled).random();
				populate(gold).with("Gold").grouped(6);

				var berries = start_resources_regions.except(regions.filled);
				populate(berries).with("Berry").grouped(8);

				var secondary_mines = start_resources_regions.adjacent().except(regions.filled).random(2);
				var secondary_gold = secondary_mines.random(1);
				var stone = secondary_mines.except(secondary_gold);

				populate(secondary_gold).with("Gold").grouped(4);
				populate(stone).with("Stone").grouped(4);

				candidate_hills = candidate_hills.except(candidate_hills.touching(start_resources_regions).include(start_resources_regions.adjacent()));
				}

			populate(regions.empty.random(players_count * 2)).with("Relic").centre();
			populate(regions.empty.random(players_count)).with("Gold").grouped(3);
			populate(regions.empty.random(players_count)).with("Stone").grouped(3);

			var sparse_forests = regions.empty.random(regions.empty.count / 6);
			populate(sparse_forests).with("Tree").radial(EaseStyle.Linear, from_border: 0.3f);
			paint(sparse_forests).with("Underbrush").radial();

			for (int i = 0; i < players_count * 2; i++)
				{
				var sparse_hill = candidate_hills.random();
				candidate_hills = candidate_hills.except(sparse_hill).except(sparse_hill.adjacent());

				rise(sparse_hill).by(4).radial(EaseStyle.CubicEaseInOut, from_centre: 0.1f);
				}

			// Create a path to connect 2 random spawns
			var path_layer = create_new_regions_layer_with_spawn_positions(8);
			var connected_spawns = path_layer.spawn.random(2);
			var target_spawn = connected_spawns.random();
			var from = connected_spawns.except(target_spawn);

			if (target_spawn.count == 0 || from.count == 0) { return; }
			while (((Cell)from).point != ((Cell)target_spawn).point)
				{
				var next = from.adjacent().closest_to(target_spawn, include_targets: true);
				if (next.count == 0) { break; };
				var cells = next.include(from);
				var path = cells.tiles().in_line(from, next, tile_size * 2);
				paint(path).with("Shore").all();
				populate(path.except(path.contains("Town_center"))).clear();
				from = next;
				}
			}
		}
	}