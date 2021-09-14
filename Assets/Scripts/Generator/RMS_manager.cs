using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TypeReferences;

// RMS.Generator needs access to private RMS fields, but C# has no friendship and Unity can't instantiate via UI components which are nested in another class.
// So RMS_manager is used as a dummy that forces the creation via code of an RMS.Generator component

[RequireComponent(typeof(RMS.Generator))]
public class RMS_manager : MonoBehaviour
	{
	private void Reset() { if (GetComponent<RMS.Generator>() == null) { gameObject.AddComponent<RMS.Generator>(); } }
	}

public abstract partial class RMS
	{
	[RequireComponent(typeof(Resources_manager), typeof(map_gen.Map_terrain))]
	public class Generator : MonoBehaviour
		{
		[Header("These fields should be consistent across the game, not depending on the individual map")]
		[SerializeField]
		public float tile_size = 1f;

		[ClassExtends(typeof(RMS))]
		public ClassTypeReference script = null;

		//Just for editor testing
		RMS instance = null;

		[EasyButtons.Button]
		public void generate(int players_count)
			{
			//var
			instance = utils.construct.type<RMS>(script);

			GetComponent<Textures_manager>().generate();
			instance.initialize(GetComponent<Resources_manager>(), GetComponent<Textures_manager>(), this, players_count);
			instance.run();
			instance.finalize();
			}

		[EasyButtons.Button]
		void debugger_time()
			{
			; //breakpoint
			}


		[SerializeField, Min(-1)]
		int highlight = -1;
		private void OnDrawGizmos()
			{
			if (instance != null)
				{
				// Highlighted cell
				if(highlight >= 0 && highlight < instance.regions.size)
					{
					var cell = instance.regions[highlight];

					cell.draw_gizmos_tiles();

					Gizmos.color = Color.white;
					((utils.geometry.Polygon)cell.polygon.bounding_box).draw_gizmos_rising();
					Gizmos.color = Color.black;
					cell.polygon.draw_gizmos_rising();
					cell.point.draw_gizmos(1.5f);
					}

				// Links to neighbours
				foreach (var cell in instance.regions.all)
					{
					Gizmos.color = Color.cyan;
					foreach (var neighbour in cell.neighbours)
						{
						new utils.geometry.Segment(cell.point, neighbour.point).draw_gizmos();
						}
					}

				// Points, spawn points and cell borders
				foreach (var cell in instance.regions.all)
					{
					Gizmos.color = Color.red;
					cell.point.draw_gizmos(1);

					Gizmos.color = Color.red;
					cell.polygon.draw_gizmos();
					}
				Gizmos.color = Color.magenta;
				foreach (var cell in instance.regions.spawn)
					{
					cell.point.draw_gizmos(2);
					}
				}
			}
		}
	}