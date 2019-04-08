using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public enum StringDisplayType
	{
		Field,
		Box,
		Area,
		Popup
	}

	public class StringDisplayAttribute : PropertyAttribute
	{
		public StringDisplayType Type { get; private set; }
		public string[] Options { get; private set; }

		public bool Foldout = false;
		public bool FullWidth = false;
		public bool WordWrap = false;
		public int MinimumLines = 3;
		public int MaximumLines = 5;

		public StringDisplayAttribute(StringDisplayType type)
		{
			Type = type;
			WordWrap = type == StringDisplayType.Area;
		}

		public StringDisplayAttribute(string[] options)
		{
			Type = StringDisplayType.Popup;
			Options = options;
		}
	}
}
