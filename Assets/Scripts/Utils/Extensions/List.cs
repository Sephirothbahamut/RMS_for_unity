using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace utils.extensions
	{
	public static class EList
		{
		public static void emplace<S>(this IList<S> list, params object[] parameters)
			{
			list.Add((S)System.Activator.CreateInstance(typeof(S), parameters));
			}
		}
	}