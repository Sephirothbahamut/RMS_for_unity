using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace map_gen
	{
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
	[RequireComponent(typeof(Textures_manager))]
	public class Map_terrain : MonoBehaviour
		{
		Mesh mesh = null;
		utils.Matrix<Vector3> vertices = null;

		public void initialize(int width, int height, float tile_size)
			{
			if (mesh == null) { mesh = new Mesh(); } else { mesh.Clear(); }
			GetComponent<MeshRenderer>().sharedMaterial.mainTexture = GetComponent<Textures_manager>().atlas;
			GetComponent<MeshRenderer>().sharedMaterial.EnableKeyword("_NORMALMAP");
			GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_BumpMap", GetComponent<Textures_manager>().nm_atlas);

			mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

			// ╔═══════════════ Vertices matrix creation ═══════════════╗
			vertices = new utils.Matrix<Vector3>(width * 2, height * 2, Vector3.zero);

			for (int y = 0; y < vertices.height; y += 2)
				{
				for (int x = 0; x < vertices.width; x += 2)
					{
					var new_point = new utils.geometry.Point((x / 2) * tile_size, (y / 2) * tile_size);
					vertices[x    , y    ] = new_point;
					vertices[x + 1, y    ] = new utils.geometry.Point(new_point.x + tile_size, new_point.y);
					vertices[x    , y + 1] = new utils.geometry.Point(new_point.x,             new_point.y + tile_size);
					vertices[x + 1, y + 1] = new_point + tile_size;
					}
				}
			// ╚════════════════════════════════════════════════════════╝

			// ╔═══════════════ Triangles list creation  ═══════════════╗
			var triangles = new List<int>();

			for (int y = 0; y < vertices.height; y += 2)
				{
				for (int x = 0; x < vertices.width; x += 2)
					{
					//From above
					triangles.Add(vertices.get_index(x, y));
					triangles.Add(vertices.get_index(x + 1, y));
					triangles.Add(vertices.get_index(x, y + 1));

					triangles.Add(vertices.get_index(x + 1, y + 1));
					triangles.Add(vertices.get_index(x, y + 1));
					triangles.Add(vertices.get_index(x + 1, y));
					}
				}
			// ╚════════════════════════════════════════════════════════╝

			mesh.SetVertices(vertices.data());
			mesh.SetTriangles(triangles, 0);

			var normals = new List<Vector3>(vertices.size); for (int i = 0; i < vertices.size; i++) { normals.Add(Vector3.up); }
			mesh.SetNormals(normals);

			var uvs = new List<Vector3>(vertices.size); for (int i = 0; i < vertices.size; i++) { uvs.Add(new Vector2(vertices[i].x, vertices[i].z)); }
			mesh.SetUVs(0, uvs);

			GetComponent<MeshFilter>()  .sharedMesh = mesh;
			GetComponent<MeshCollider>().sharedMesh = mesh;
			}

		public void update_heights(utils.Matrix<float> heights)
			{
			if (heights.width != vertices.width / 2 || heights.height != vertices.height / 2) { Debug.LogError("Terrain update received a matrix of the wrong size."); return; }

			System.Func<float?, float?, float?, float?, float> average_vertex_height =
				(float? a, float? b, float? c, float? d) =>
				{
					var avg = new utils.cumulative_average();
					if(a.HasValue) { avg.push(a.Value); }
					if(b.HasValue) { avg.push(b.Value); }
					if(c.HasValue) { avg.push(c.Value); }
					if(d.HasValue) { avg.push(d.Value); }
					return avg;
				};

			for (int y = 0; y < heights.height; y++)
				{
				for (int x = 0; x < heights.width; x++)
					{
					float tile = heights[x, y];
					float? tile_top_left      = (x > 0                 && y > 0                 ) ? heights[x - 1, y - 1] : (float?)null;
					float? tile_top           = (                         y > 0                 ) ? heights[x    , y - 1] : (float?)null;
					float? tile_top_right     = (x < heights.width - 1 && y > 0                 ) ? heights[x + 1, y - 1] : (float?)null;
					float? tile_bottom_left   = (x > 0                 && y < heights.height - 1) ? heights[x - 1, y + 1] : (float?)null;
					float? tile_bottom        = (                         y < heights.height - 1) ? heights[x    , y + 1] : (float?)null;
					float? tile_bottom_right  = (x < heights.width - 1 && y < heights.height - 1) ? heights[x + 1, y + 1] : (float?)null;
					float? tile_right         = (x < heights.width - 1                          ) ? heights[x + 1, y    ] : (float?)null;
					float? tile_left          = (x > 0                                          ) ? heights[x - 1, y    ] : (float?)null;

					ref var vertex_top_left     = ref vertices[ x * 2,       y * 2     ];
					ref var vertex_top_right    = ref vertices[(x * 2) + 1,  y * 2     ];
					ref var vertex_bottom_left  = ref vertices[ x * 2,      (y * 2) + 1];
					ref var vertex_bottom_right = ref vertices[(x * 2) + 1, (y * 2) + 1];

					vertex_top_left    .y = average_vertex_height(tile, tile_top,    tile_left,  tile_top_left);
					vertex_top_right   .y = average_vertex_height(tile, tile_top,    tile_right, tile_top_right);
					vertex_bottom_left .y = average_vertex_height(tile, tile_bottom, tile_left,  tile_bottom_left);
					vertex_bottom_right.y = average_vertex_height(tile, tile_bottom, tile_right, tile_bottom_right);
					}
				}
			mesh.SetVertices(vertices.data());
			}
		public void update_uvs(utils.Matrix<Rect> uvs)
			{
			if (uvs.width != vertices.width / 2 || uvs.height != vertices.height / 2) { Debug.LogError("Terrain update received a matrix of the wrong size."); return; }

			var vertices_uv = new utils.Matrix<Vector2>(vertices.width, vertices.height, Vector3.zero);

			for (int y = 0; y < uvs.height; y++)
				{
				for (int x = 0; x < uvs.width; x++)
					{
					Rect rect = uvs[x, y];

					vertices_uv[ x * 2,       y * 2     ] = new Vector2(rect.x,              rect.y);
					vertices_uv[(x * 2) + 1,  y * 2     ] = new Vector2(rect.x + rect.width, rect.y);
					vertices_uv[ x * 2,      (y * 2) + 1] = new Vector2(rect.x,              rect.y + rect.height);
					vertices_uv[(x * 2) + 1, (y * 2) + 1] = new Vector2(rect.x + rect.width, rect.y + rect.height);
					}
				}
			mesh.SetUVs(0, vertices_uv.data());
			}

		public void update_unity_meshes()
			{
			GetComponent<MeshFilter>().sharedMesh = mesh;
			GetComponent<MeshCollider>().sharedMesh = mesh;

			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			}
		}
	}


// Previous version; had no UV values, there were vertices being shared across tiles
/*
namespace map_gen
	{
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
	[RequireComponent(typeof(Textures_manager))]
	public class Map_terrain : MonoBehaviour
		{
		Mesh mesh = null;
		utils.Matrix<Vector3> vertices = null;

		public void initialize(int width, int height, float tile_size)
			{
			if (mesh == null) { mesh = new Mesh(); } else { mesh.Clear(); }

			mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

			// ╔═══════════════ Vertices matrix creation ═══════════════╗
			vertices = new utils.Matrix<Vector3>(width + 1, height + 1, Vector3.zero);

			for (int y = 0; y < vertices.height; y++)
				{
				for (int x = 0; x < vertices.width; x++)
					{
					vertices[x, y] = new utils.geometry.Point(x * tile_size, y * tile_size);
					}
				}
			// ╚════════════════════════════════════════════════════════╝

			// ╔═══════════════ Triangles list creation  ═══════════════╗
			var triangles = new List<int>();

			for (int y = 0; y < vertices.height - 1; y++)
				{
				for (int x = 0; x < vertices.width - 1; x++)
					{
					//From above
					triangles.Add(vertices.get_index(x, y));
					triangles.Add(vertices.get_index(x + 1, y));
					triangles.Add(vertices.get_index(x, y + 1));

					triangles.Add(vertices.get_index(x + 1, y + 1));
					triangles.Add(vertices.get_index(x, y + 1));
					triangles.Add(vertices.get_index(x + 1, y));
					}
				}
			// ╚════════════════════════════════════════════════════════╝

			mesh.SetVertices(vertices.data());
			mesh.SetTriangles(triangles, 0);

			var normals = new List<Vector3>(vertices.size); for (int i = 0; i < vertices.size; i++) { normals.Add(Vector3.up); }
			mesh.SetNormals(normals);

			var uvs = new List<Vector3>(vertices.size); for (int i = 0; i < vertices.size; i++) { uvs.Add(new Vector2(vertices[i].x, vertices[i].z)); }
			mesh.SetUVs(0, uvs);

			GetComponent<MeshFilter>()  .sharedMesh = mesh;
			GetComponent<MeshCollider>().sharedMesh = mesh;
			}

		public void update_heights(utils.Matrix<float> heights)
			{
			if (heights.width != vertices.width - 1 || heights.height != vertices.height - 1) { Debug.LogError("Terrain update received a matrix of the wrong size."); return; }

			//For each vertex, assign as height the average between the 4 tiles around it
			for (int y = 0; y < vertices.height; y++)
				{
				for (int x = 0; x < vertices.width; x++)
					{
					var avg = new utils.cumulative_average();
					// bottom right
					if (x < heights.width && y < heights.height) { avg.push(heights[x,     y    ]); }
					// top left
					if (x > 0             && y > 0)              { avg.push(heights[x - 1, y - 1]); }
					// top right
					if (x < heights.width && y > 0)              { avg.push(heights[x,     y - 1]); }
					//bottom left
					if (x > 0             && y < heights.height) { avg.push(heights[x - 1, y    ]); }

					float val = avg;
					vertices[x, y].y = val;
					}
				}
			update_unity_meshes();
			}
		public void update_uvs(utils.Matrix<Rect> uvs)
			{
			if (uvs.width != vertices.width - 1 || uvs.height != vertices.height - 1) { Debug.LogError("Terrain update received a matrix of the wrong size."); return; }
			}

		void update_unity_meshes()
			{
			mesh.SetVertices(vertices.data());

			GetComponent<MeshFilter>().sharedMesh = mesh;
			GetComponent<MeshCollider>().sharedMesh = mesh;

			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			}
		}
	}*/