using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace utils
	{
	//Allows to construct generic types with parameters
	public static class construct
		{
		public static T generic<T>(params object[] parameters)
			{
			return (T)System.Activator.CreateInstance(typeof(T), parameters);
			}
		public static T generic<T>()
			{
			return (T)System.Activator.CreateInstance(typeof(T));
			}
		public static T type<T>(System.Type type, params object[] parameters)
			{
			return (T)System.Activator.CreateInstance(type, parameters);
			}
		public static T type<T>(System.Type type)
			{
			return (T)System.Activator.CreateInstance(type);
			}
		}
	}