using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public enum ListItemDisplayType
	{
		Normal,
		Inline,
		Foldout
	}

	public class ListDisplayAttribute : PropertyAttribute
	{
		public bool AllowAdd = true;
		public bool AllowRemove = true;
		public bool AllowReorder = true;
		public bool AllowCollapse = true;
		public ListItemDisplayType ItemDisplay = ListItemDisplayType.Normal;

		public string EmptyText = null;

		public string AddMethod = null;
		public string RemoveMethod = null;
		public string ReorderCallback = null;

		public ListDisplayAttribute()
		{
			order = int.MaxValue;
		}
	}
}
