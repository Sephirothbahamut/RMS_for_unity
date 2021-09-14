using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace utils.geometry
	{
	public abstract partial class Polygon
		{
		public class Concave : Polygon
			{
			private Point[] _vertices;

			public Concave(ICollection<Point> vertices) { this._vertices = new Point[vertices.Count]; vertices.CopyTo(this.vertices, 0); }

			public override Point  [] vertices { get { return _vertices; } }
			}
		}
	}