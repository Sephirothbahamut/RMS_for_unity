using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Textures_manager : MonoBehaviour
	{
	[System.Serializable]
	private struct entry
		{
		[SerializeField] public string name;
		[SerializeField] public Texture2D texture;
		[SerializeField] public Texture2D normal_map;
		}

	[SerializeField]
	List<entry> entries;

	[SerializeField, HideInInspector]
	public Texture2D atlas;
	[SerializeField, HideInInspector]
	public Rect[] rects;

	[SerializeField, HideInInspector]
	public Texture2D nm_atlas;

	//Regenerated each time because for some reason unity refuses to serialize
	public Dictionary<string, Rect> terrains_map;

	[EasyButtons.Button] //Not OnValidate because it's slow
	public void generate()
		{
		atlas = new Texture2D(8192, 8192);
		nm_atlas = new Texture2D(8192, 8192);

		var textures = new Texture2D[entries.Count];
		var normal_maps = new Texture2D[entries.Count];

		for (int i = 0; i < entries.Count; i++) { textures[i] = entries[i].texture; }
		rects = atlas.PackTextures(textures, 2);

		for (int i = 0; i < entries.Count; i++) { normal_maps[i] = entries[i].normal_map; }

		Rect[] nm_rects = nm_atlas.PackTextures(normal_maps, 2);

		for (int i = 0; i < rects.Length; i++)
			{
			if (rects[i] != nm_rects[i]) { Debug.LogError("Error generating the terrain's texture atlas. Textures used for normal maps do not match the ones used for base texture. Aborting."); return; }
			}

		terrains_map = new Dictionary<string, Rect>();
		for (int i = 0; i < entries.Count; i++)
			{
			var entry = entries[i];
			terrains_map.Add(entry.name, rects[i]);
			}
		}
	}
