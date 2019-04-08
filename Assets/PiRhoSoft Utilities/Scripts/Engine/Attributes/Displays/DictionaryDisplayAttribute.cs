using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public class DictionaryDisplayAttribute : PropertyAttribute
	{
		public bool AllowAdd = true;
		public bool AllowRemove = true;
		public bool AllowCollapse = true;

		public string AddLabel = null;
		public string EmptyText = null;
	}
}
