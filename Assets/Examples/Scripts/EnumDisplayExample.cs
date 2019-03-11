using PiRhoSoft.UtilityEngine;
using System;
using UnityEngine;

namespace PiRhoSoft.UtilityExample
{
	public enum ExampleEnum
	{
		Zero = 0,
		One = 1,
		Two = 2,
		Three = 3,
		Four = 4,
		Five = 5,
		Six = 6,
		Seven = 7,
		Eight = 8
	}

	[Flags]
	public enum ExampleFlags
	{
		None = 0,
		One = 0x1,
		Two = 0x2,
		Three = 0x4,
		Four = 0x8,
		Five = 0x10,
		Six = 0x20,
		Seven = 0x40,
		Eight = 0x80,
		All = ~0
	}

	[AddComponentMenu("PiRho Soft/Examples/Enum Display")]
	public class EnumDisplayExample : MonoBehaviour
	{
		public ExampleEnum Normal;

		[EnumDisplay(Type = EnumDisplayType.Popup)] public ExampleEnum EnumPopup;
		[EnumDisplay(Type = EnumDisplayType.Popup)] public ExampleFlags FlagsPopup;
		[EnumDisplay(Type = EnumDisplayType.Popup, ForceFlags = true)] public ExampleEnum ForcedFlagsPopup;

		[EnumDisplay(Type = EnumDisplayType.Buttons)] public ExampleEnum EnumButtons;
		[EnumDisplay(Type = EnumDisplayType.Buttons)] public ExampleFlags FlagsButtons;
		[EnumDisplay(Type = EnumDisplayType.Buttons, ForceFlags = true)] public ExampleEnum ForcedFlagsButtons;

		[EnumDisplay(MinimumWidth = 100)] public ExampleEnum CustomWidth;
	}
}
