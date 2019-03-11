using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public class DictionaryDisplayAttribute : PropertyAttribute
	{
		public bool AllowAdd = true;
		public bool AllowRemove = true;
		public bool AllowCollapse = true;
		public ListItemDisplayType ItemDisplay = ListItemDisplayType.Normal;
		public string AddLabel = null;
		public string EmptyText = null;

		public string AddMethod = null;
		public string RemoveMethod = null;
	}
}
