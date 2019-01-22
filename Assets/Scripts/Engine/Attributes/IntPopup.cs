using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public class IntPopupAttribute : PropertyAttribute
	{
		public int[] Values { get; private set; }
		public string[] Options { get; private set; }
		public IntPopupAttribute(int[] values, string[] options)
		{
			Values = values;
			Options = options;
		}
	}
}
