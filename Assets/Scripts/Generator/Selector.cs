using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading.Tasks;

using System.Linq;
using utils.geometry;

public abstract partial class RMS
	{
	public class Selector : IEnumerable<Cell>
		{
		public Selector(ICollection<Cell> from, RMS rms) { selected = new HashSet<Cell>(from); this.rms = rms; }
		public Selector(Cell              from, RMS rms) { selected = new HashSet<Cell>{from}; this.rms = rms; }
		public Selector(Selector          from, RMS rms) => (this.selected, this.rms) = ((HashSet<Cell>)from, rms);
		private HashSet<Cell> selected;
		private RMS rms;

		/// <summary> Selects [count] random regions. </summary>
		public Selector random(int count = 1)
			{
			var ret = new HashSet<Cell>();
			var from = new List<Cell>(selected);

			for (uint i = 0; i < count && i < from.Count; i++)
				{
				int index = Random.Range(0, from.Count);
				ret.Add(from[index]);
				from.RemoveAt(index);
				}

			return new Selector(ret, rms);
			}

		/// <summary> Selects all the regions which are adjacent to the currently selected regions. </summary>
		public Selector adjacent()
			{
			var ret = new HashSet<Cell>();
			foreach (var cell in this) { foreach (var neigh in cell.neighbours) { ret.Add((Cell)neigh); } }
			return new Selector(ret, rms).except(this);
			}

		/// <summary> Filters only empty cells. Faster than .except(filled) </summary>
		public Selector empty()
			{
			var ret = new HashSet<Cell>();
			foreach (var cell in this) { if (cell.is_empty) { ret.Add(cell); } }
			return new Selector(ret, rms);
			}

		/// <summary> Filters only non empty cells. Faster than .except(empty) </summary>
		public Selector filled()
			{
			var ret = new HashSet<Cell>();
			foreach (var cell in this) { if (cell.is_not_empty) { ret.Add(cell); } }
			return new Selector(ret, rms);
			}

		/// <summary> Filters only empty cells. Faster than .except(inner) </summary>
		public Selector border()
			{
			var ret = new HashSet<Cell>();
			foreach (var cell in this) { if (cell.is_border) { ret.Add(cell); } }
			return new Selector(ret, rms);
			}

		/// <summary> Filters only non empty cells. Faster than .except(border) </summary>
		public Selector inner()
			{
			var ret = new HashSet<Cell>();
			foreach (var cell in this) { if (!cell.is_border) { ret.Add(cell); } }
			return new Selector(ret, rms);
			}

		/// <summary> Removes the passed parameter regions from the currently selected ones. <summary>
		public Selector except(Selector exclusions)
			{
			var from = new HashSet<Cell>(selected);
			foreach (var exclude in exclusions) { from.Remove(exclude); }
			return new Selector(from, rms);
			}

		/// <summary> Adds the passed parameter regions to the currently selected ones. <summary>
		public Selector include(Selector inclusions)
			{
			var from = new HashSet<Cell>(selected);
			foreach (var include in inclusions) { from.Add(include); }
			return new Selector(from, rms);
			}

		/// <summary> Selects [count] regions amongst all the default regions that are the closest to the passed parameter. </summary>
		public Selector closest(int count = 1) { return closest_amongst(rms.regions.all.except(this), false, count); }

		/// <summary> Selects [count] regions amongst the selected cells that are the closest to the passed parameter. </summary>
		public Selector closest_to(Selector to, bool include_targets, int count = 1) { return to.closest_amongst(this, include_targets, count); }

		/// <summary> Selects [count] regions amongst the passed parameter that are the closest to the selected regions. </summary>
		public Selector closest_amongst(Selector amongst, bool include_sources, int count = 1)
			{
			var ret = new HashSet<Cell>();

			var candidates_selector = new Selector(amongst, rms);
			if (!include_sources) { candidates_selector = candidates_selector.except(this); }
			var candidates =  (HashSet<Cell>)candidates_selector;

			for (int i = 0; i < count; i++)
				{
				float closest_dist = float.PositiveInfinity;
				Cell chosen = null;
				foreach (var this_cell in this)
					{
					foreach (var candidate_cell in candidates)
						{
						float candidate_dist = candidate_cell.point.distance(this_cell.point);
						if (candidate_dist < closest_dist)
							{
							closest_dist = candidate_dist;
							chosen = candidate_cell;
							}
						}
					}
				ret.Add(chosen);
				candidates.Remove(chosen);
				}
			return new Selector(ret, rms);
			}

		/// <summary> 
		/// Selects all regions amongst the selected regions which are touching (intersect or contain) the passed parameter. 
		/// Note: this is meant to be used with regions from different layers.
		/// </summary>
		public Selector touching(Selector what)
			{
			var ret = new HashSet<Cell>();

			foreach (var selected_cell in this)
				{
				foreach (var what_cell in what)
					{
					if (selected_cell.polygon.touches(what_cell.polygon)) { ret.Add(selected_cell); break; }
					}
				}
			return new Selector(ret, rms);
			}


		/// <summary> 
		/// Selects all regions amongst the selected regions which contains at least one region of the passed parameter. 
		/// Note: this is meant to be used with regions from different layers.
		/// </summary>
		public Selector containing_one_of(Selector what)
			{
			var ret = new HashSet<Cell>();

			foreach (var selected_cell in this)
				{
				foreach (var what_cell in what)
					{
					if (selected_cell.polygon.contains(what_cell.polygon)) { ret.Add(selected_cell); break; }
					}
				}
			return new Selector(ret, rms);
			}

		/// <summary> 
		/// Selects all regions amongst the selected regions which contains all the regions of the passed parameter. 
		/// Note: this is meant to be used with regions from different layers.
		/// </summary>
		public Selector containing(Selector what)
			{
			var ret = new HashSet<Cell>();

			foreach (var selected_cell in this)
				{
				bool contains_all = true;
				foreach (var what_cell in what)
					{
					if (!selected_cell.polygon.contains(what_cell.polygon)) { contains_all = false; break; }
					}
				if (contains_all) { ret.Add(selected_cell); }
				}
			return new Selector(ret, rms);
			}

		public Selector except          (Cell cell)                                      { return except          (new Selector(cell, rms)); }
		public Selector include         (Cell cell)                                      { return include         (new Selector(cell, rms)); }
		public Selector closest_to      (Cell cell, bool include_targets, int count = 1) { return closest_to      (new Selector(cell, rms), include_targets, count); }
		public Selector closest_amongst (Cell cell, bool include_sources, int count = 1) { return closest_amongst (new Selector(cell, rms), include_sources, count); }
		public Selector touching        (Cell cell)                                      { return touching        (new Selector(cell, rms)); }
		public Selector containing      (Cell cell)                                      { return containing      (new Selector(cell, rms)); }

		public bool is_empty { get { return selected.Count != 0; } }
		public int  count    { get { return selected.Count; } }

		public static implicit operator Cell(Selector selector) { return selector.selected.First(); }
		public static implicit operator HashSet<Cell>(Selector selector) { return selector.selected; }

		public Selector_per_tile tiles() { return rms.tiles_in_regions(this); }

		public IEnumerator<Cell> GetEnumerator() { return selected.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
		}

	public Selector select_from(Cell cell) { return new Selector(cell, this); }
	}