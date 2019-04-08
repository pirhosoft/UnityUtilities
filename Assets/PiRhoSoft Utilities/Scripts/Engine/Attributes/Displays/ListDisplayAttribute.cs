using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public class ListDisplayAttribute : PropertyAttribute
	{
		public bool AllowAdd = true;
		public bool AllowRemove = true;
		public bool AllowReorder = true;
		public bool AllowCollapse = true;

		public string EmptyText = null;

		public ListDisplayAttribute()
		{
			order = int.MaxValue;
		}
	}
}
