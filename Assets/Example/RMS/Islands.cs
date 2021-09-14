using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using com.spacepuppy.Tween;


namespace random_map_scripts
	{

	public class Islands : RMS
		{
		protected override int   tiles_width                  { get { return 200 + (20 * players_count); } }
		protected override int   tiles_height                 { get { return 200 + (20 * players_count); } }
		protected override float minimum_region_radius        { get { return 10; } }
		protected override float minimum_player_starting_space{ get { return 50; } }

		public override void run()
			{
			paint(regions.all).with("Grass").all();

			var sea = regions.all;

			foreach (var spawn in regions.spawn)
				{
				var start_resources_regions = regions.empty.closest_to(spawn, 5);

				populate(spawn).with("Town_center").center();

				var forests = start_resources_regions.random(3);
				populate(forests).with("Tree").radial();
				paint(forests).with("Underbush").radial();

				var gold = start_resources_regions.except(regions.filled).random();
				populate(gold).with("Gold").grouped(6);

				var berries = start_resources_regions.except(regions.filled);
				populate(berries).with("Berry").grouped(8);

				var surroundings = start_resources_regions.adjacent();
				var secondary_mines = surroundings.except(regions.filled).random(2);
				var secondary_gold = secondary_mines.random(1);
				var stone = secondary_mines.except(secondary_gold);

				populate(secondary_gold).with("Gold").grouped(4);
				populate(stone).with("Stone").grouped(4);

				var shore = surroundings.adjacent().except(start_resources_regions);
				paint(shore).with("Shore");
				rise(shore.random(1)).by(2).radial();

				var few_trees = shore.random(5);
				populate(few_trees).with("Tree").radial(from_border: 0.3f);
				paint(few_trees).with("Underbush").radial();

				sea = sea.except(start_resources_regions).except(surroundings).except(shore);
				}

			rise(sea).by(-1).all();
			paint(sea).with("Water").all();
			}
		}
	}