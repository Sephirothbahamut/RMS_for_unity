using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace utils
	{
	/// <summary> Uniforms destruction calls between OnValidate (which can't destroy nor destroyImmediate, hence the coroutine) and ingame. </summary>
	public static class destruction
		{
		public static void exterminate_children(MonoBehaviour something)
			{
			foreach (Transform child in something.gameObject.transform)
				{
				if (Application.isPlaying) { Object.Destroy(child.gameObject); }
				else if (Application.isEditor) { something.StartCoroutine(_private.destroy(child.gameObject)); }
				}
			}

		/// <summary> Destroys obj; starter can be anything, is responsible of starting the coroutine in editor mode. </summary>
		/// <param name="obj"></param>
		/// <param name="starter"></param>
		public static void destroy(GameObject obj, MonoBehaviour starter)
			{
			if (Application.isPlaying) { Object.Destroy(obj); }
			else if (Application.isEditor) { starter.StartCoroutine(_private.destroy(obj)); }
			}

		public static void destroy(MonoBehaviour any_component_of_the_object_to_be_destroyed_except_transform)
			{
			if (Application.isPlaying)
				{ Object.Destroy(any_component_of_the_object_to_be_destroyed_except_transform.gameObject); }
			else if (Application.isEditor)
				{ any_component_of_the_object_to_be_destroyed_except_transform.StartCoroutine(_private.destroy(any_component_of_the_object_to_be_destroyed_except_transform.gameObject)); }
			}

		private static partial class _private
			{
			public static IEnumerator destroy(GameObject game_object)
				{
				yield return new WaitForEndOfFrame();
				Object.DestroyImmediate(game_object);
				}
			}
		}
	}
