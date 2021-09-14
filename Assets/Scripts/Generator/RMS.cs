using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using utils.geometry;
using utils.random;
using utils.extensions;

public abstract partial class RMS
	{
	private Generator manager;
	private map_gen.Map map;
	private Resources_manager resources;
	private Textures_manager terrains;
	public int players_count { get; private set; }
	public Regions regions;
	public float tile_size { get { return map.tile_size; } }

	public void initialize(Resources_manager resources, Textures_manager terrains, Generator manager, int players_count)
		{
		this.resources       = resources;
		this.terrains        = terrains;
		this.manager         = manager;
		this.players_count   = players_count;

		utils.destruction.exterminate_children(manager);

		// Generates the matrix of unitary tiles
		map = new map_gen.Map(tiles_width, tiles_height, manager.tile_size, manager.transform, manager.GetComponent<map_gen.Map_terrain>());

		regions = new Regions(this, minimum_region_radius, players_count, minimum_player_starting_space);
		}
	public void finalize() 
		{
		map.apply();

		//Update height of spawned objects to that of their tile
		for (int i = 0; i < map.tiles.size; i++)
			{
			ref var tile = ref map.tiles[i];
			foreach (var obj in tile.objects)
				{
				obj.transform.position = new Vector3(obj.transform.position.x, tile.height, obj.transform.position.z);
				}
			}
		}

	protected virtual int   tiles_width                  { get { return 200; } }
	protected virtual int   tiles_height                 { get { return 200; } }
	protected virtual float minimum_region_radius        { get { return  10; } }
	protected virtual float minimum_player_starting_space{ get { return  20; } }

	public abstract void run();

	/// <summary> Smooths the current terrain. </summary>
	/// <param name="range"></param>
	public void smooth(int range) { if (range > 0) { map.smooth(range); } }

	/// <summary> Returns the tile that contains the position x, y; where x and y are in-world coordinates. </summary>
	public ref map_gen.Tile tile_at(float x, float y) { return ref map.tile_at(x, y); }
	/// <summary> Returns the tile that contains the position x, y; where x and y are in-world coordinates. </summary>
	public ref map_gen.Tile tile_at(Point position) { return ref tile_at(position.x, position.y); }

	/// <summary> Returns the tile at index [x, y] in memory, where x and y are indices in the matrix. </summary>
	public ref map_gen.Tile tile_at(int x, int y) { return ref map.tiles[x, y]; }
	public ref map_gen.Tile tile_at(utils.Matrix<map_gen.Tile>.Indices indices) { return ref map.tiles[indices]; }

	public utils.Matrix<map_gen.Tile> tiles { get { return map.tiles; } }

	public Resource resource(string resource_name) { return resources.resources_map[resource_name]; }
	public Rect     terrain (string terrain_name)  { return terrains.terrains_map[terrain_name]; }
	}