using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public enum StringDisplayType
	{
		TextBox,
		TextArea,
		FoldoutBox,
		FoldoutArea,
		Popup
	}

	public class StringDisplayAttribute : PropertyAttribute
	{
		public StringDisplayType Type { get; private set; }
		public string[] Options { get; private set; }

		public bool WordWrap = true;
		public int MinimumLines = 3;
		public int MaximumLines = 5;

		public StringDisplayAttribute(StringDisplayType type)
		{
			Type = type;
		}

		public StringDisplayAttribute(string[] options)
		{
			Type = StringDisplayType.Popup;
			Options = options;
		}
	}
}
