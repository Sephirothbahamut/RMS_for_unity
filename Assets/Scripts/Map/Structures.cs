using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading.Tasks;

using utils.geometry;

namespace map_gen
	{
	public struct Tile
		{
		private readonly Map map;

		public readonly int matrix_x;
		public readonly int matrix_y;
		public utils.Matrix<Tile>.Indices matrix_indices { get { return new utils.Matrix<Tile>.Indices(matrix_x, matrix_y); } }

		public float world_x         { get { return matrix_x * map.tile_size; } }
		public float world_y         { get { return matrix_y * map.tile_size; } }
		public float world_x_center  { get { return world_x + (map.tile_size / 2); } }
		public float world_y_center  { get { return world_y + (map.tile_size / 2); } }
		public Point position        { get { return new Point(world_x, world_y); } }
		public Point center_position { get { return new Point(world_x_center, world_y_center); } }

		public float height;
		public Rect uv;

		public List<GameObject> objects;

		public bool assigned; //Flag if tile has been taken by a cell

		public Tile(int matrix_x, int matrix_y, Map map) => 
			(this.matrix_x, this.matrix_y, this.map, height, uv,         objects,                assigned) = 
			(matrix_x,      matrix_y,      map,      0f,     new Rect(), new List<GameObject>(), false);

		public GameObject instantiate(GameObject prefab, bool center = true)
			{
			var spawn_position = center ? center_position : position;
			var obj = GameObject.Instantiate(prefab, spawn_position.to_vector3(height), Quaternion.identity, map.parent);
			obj.name = prefab.name;
			objects.Add(obj);
			return obj;
			}

		public float slope
			{ 
			get
				{
				float height = this.height;
				float max_slope = 0;

				System.Action<Tile> update_minmax =
				(Tile tile) =>
				{
					float delta = Mathf.Abs(tile.height - height);
					max_slope = Mathf.Max(max_slope, delta);
				};
				/*tile_top_left    */ if(matrix_x > 0                   && matrix_y > 0                   ) { update_minmax(map.tiles[matrix_x - 1, matrix_y - 1]); }
				/*tile_top         */ if(                                  matrix_y > 0                   ) { update_minmax(map.tiles[matrix_x    , matrix_y - 1]); }
				/*tile_top_right   */ if(matrix_x < map.tiles.width - 1 && matrix_y > 0                   ) { update_minmax(map.tiles[matrix_x + 1, matrix_y - 1]); }
				/*tile_bottom_left */ if(matrix_x > 0                   && matrix_y < map.tiles.height - 1) { update_minmax(map.tiles[matrix_x - 1, matrix_y + 1]); }
				/*tile_bottom      */ if(                                  matrix_y < map.tiles.height - 1) { update_minmax(map.tiles[matrix_x    , matrix_y + 1]); }
				/*tile_bottom_right*/ if(matrix_x < map.tiles.width - 1 && matrix_y < map.tiles.height - 1) { update_minmax(map.tiles[matrix_x + 1, matrix_y + 1]); }
				/*tile_right       */ if(matrix_x < map.tiles.width - 1                                   ) { update_minmax(map.tiles[matrix_x + 1, matrix_y    ]); }
				/*tile_left        */ if(matrix_x > 0                                                     ) { update_minmax(map.tiles[matrix_x - 1, matrix_y    ]); }

				return max_slope;
				}
			}

		public void draw_gizmos() { Gizmos.DrawCube(center_position, new Vector3(1, 1, 1)); }
		}

	public class Map
		{
		public readonly utils.Matrix<Tile> tiles;
		public Transform parent;

		private map_gen.Map_terrain terrain;

		public readonly float tile_size;

		private Texture2D terrain_texture;

		/// <summary>
		/// Assumes the map is square
		/// </summary>
		public Map(int width, int height, float tile_size, Transform parent, Map_terrain terrain)
			{
			tiles = new utils.Matrix<Tile>(width, height, new Tile(0, 0, this));

			for (int y = 0; y < tiles.height; y++) { for (int x = 0; x < tiles.width; x++) { tiles[x, y] = new Tile(x, y, this); } }

			this.tile_size = tile_size;
			this.parent    = parent;
			terrain.initialize(width, height, tile_size);
			this.terrain = terrain;
			}

		public void apply()
			{
			var heights = new utils.Matrix<float>(tiles.width, tiles.height, 0);
			for (int y = 0; y < tiles.height; y++) { for (int x = 0; x < tiles.width; x++) { heights[x, y] = tiles[x, y].height; } }
			terrain.update_heights(heights);

			var uvs = new utils.Matrix<Rect>(tiles.width, tiles.height, new Rect());
			for (int y = 0; y < tiles.height; y++) { for (int x = 0; x < tiles.width; x++) { uvs[x, y] = tiles[x, y].uv; } }
			terrain.update_uvs(uvs);

			terrain.update_unity_meshes();
			}

		public void smooth(int range)
			{
			var new_heights = new utils.Matrix<float>(tiles.width, tiles.height, 0);

			Parallel.ForEach(tiles.data(), tile =>
				{
					var avg = new utils.cumulative_average();
					avg.push(tile.height);

					var neighbours = new utils.Matrix<Tile?>(range, range, null);
					for (int y = 0; y < neighbours.height; y++)
						{
						for (int x = 0; x < neighbours.width; x++)
							{
							int other_x = tile.matrix_x - (x - range);
							int other_y = tile.matrix_y - (y - range);
							if (other_x >= 0 && other_x < tiles.width &&
								other_y >= 0 && other_y < tiles.height)
								{
								neighbours[x, y] = tiles[other_x, other_y];
								}
							}
						}

					foreach (var neighbour in neighbours.data()) { if (neighbour.HasValue) { avg.push(neighbour.Value.height); } }
					new_heights[tile.matrix_x, tile.matrix_y] = avg;
				});

			for (int i = 0; i < tiles.size; i++) { tiles[i].height = new_heights[i]; }
			}

		public float width  { get { return tiles.width  * tile_size; } }
		public float height { get { return tiles.height * tile_size; } }

		public ref Tile tile_at(float world_x, float world_y)
			{
			int x = Mathf.FloorToInt(world_x / tile_size);
			int y = Mathf.FloorToInt(world_y / tile_size);
			if (x < 0) { x = 0; }
			else if (x >= tiles.width) { x = tiles.width - 1; }
			if (y < 0) { y = 0; }
			else if (y >= tiles.height) { y = tiles.height - 1; }
			return ref tiles[x, y];
			}
		}
	}