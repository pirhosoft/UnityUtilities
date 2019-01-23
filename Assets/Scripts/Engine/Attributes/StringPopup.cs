using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public class StringPopupAttribute : PropertyAttribute
	{
		public string[] Options { get; private set; }

		public StringPopupAttribute(string[] options) => Options = options;
	}
}
