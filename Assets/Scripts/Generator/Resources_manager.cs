using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// All this mess because unity doesn't manage dictionaries in the inspector
/// </summary>
public class Resources_manager : MonoBehaviour
	{
	[System.Serializable]
	private struct dict_entry
		{
		[SerializeField] public string name;
		[SerializeField] public RMS.Resource resource;
		}

	[SerializeField]
	private dict_entry[] resources = null;

	[SerializeField, HideInInspector]
	public Dictionary<string, RMS.Resource> resources_map;

	private void OnValidate()
		{
		resources_map = new Dictionary<string, RMS.Resource>();
		foreach (var entry in resources) { resources_map[entry.name] = entry.resource; }
		}
	}

public abstract partial class RMS
	{
	[System.Serializable]
	public class Resource
		{
		[SerializeField]
		private List<GameObject> variants;

		public void spawn(ref map_gen.Tile tile)
			{
			var index = Random.Range(0, variants.Count);
			tile.instantiate(variants[index]);
			}

		public bool object_is(GameObject obj)
			{
			foreach (var variant in variants) { if (obj.name == variant.name) { return true; } }
			return false;
			}
		}
	}