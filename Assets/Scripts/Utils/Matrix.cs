using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace utils
	{
	[Serializable]
	public class Matrix<T> 
		{
		public Matrix(int width, int height, T default_value)
			{
			this._width  = width;
			this._height = height;
			//arr = new List<T>(size);
			arr = new T[size];
			//arr.AddRange(Enumerable.Repeat<T>(default_value, size));
			for (int i = 0; i < arr.Length; i++) { arr[i] = default_value; }
			}

		public struct Indices
			{
			public Indices(int x, int y) { this.x = x; this.y = y; }
			public int x;
			public int y;

			public static explicit operator bool(Indices indices) { return indices.x >= 0 && indices.y >= 0; }
			}

		public int    get_index (Indices coords) { return coords.y * width + coords.x; }
		public int	  get_index (int x, int y)  { return y        * width + x; }
		public int    get_x     (int   index)   { return index % width; }
		public int    get_y     (int   index)   { return index / width; }
		public Indices get_coords(int   index)   { return new Indices(get_x(index), get_y(index)); }

		public ref T this[int index]      { get { return ref arr[index];             } }
		public ref T this[Indices coords] { get { return ref arr[get_index(coords)]; } }
		public ref T this[int x, int y]   { get { return ref arr[get_index(x, y)];   } }

		public int width   { get { return _width;  } }
		public int height  { get { return _height; } }
		public int size    { get { return _width * _height; } }

		//public List<T> data() { return arr; }
		public ref T[] data() { return ref arr; }

		[ReadOnly, SerializeField]
		private readonly int _width;
		[ReadOnly, SerializeField]
		private readonly int _height;
		[ReadOnly, SerializeField]
		private T[] arr = null;

		public override string ToString()
			{
			string ret = "";
			for (int y = 0; y < height; y++)
				{
				for (int x = 0; x < width - 1; x++) { ret += this[x, y] + ", "; }
				ret += this[width - 1, y];
				ret += "\n";
				}
			return ret;
			}
		}
	}