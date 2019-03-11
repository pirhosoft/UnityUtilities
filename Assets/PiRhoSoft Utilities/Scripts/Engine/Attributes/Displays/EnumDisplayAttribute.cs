using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public enum EnumDisplayType
	{
		Buttons,
		Popup
	}

	public class EnumDisplayAttribute : PropertyAttribute
	{
		public const float DefaultMinimumWidth = 40.0f;

		public EnumDisplayType Type = EnumDisplayType.Buttons;
		public bool ForceFlags = false;
		public float MinimumWidth = DefaultMinimumWidth;
	}
}
