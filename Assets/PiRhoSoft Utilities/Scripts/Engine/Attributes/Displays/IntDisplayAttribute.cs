using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public enum IntDisplayType
	{
		Popup,
		Slider,
		MinMaxSlider
	}

	public class IntDisplayAttribute : PropertyAttribute
	{
		public IntDisplayType Type { get; private set; }
		public string[] Names { get; private set; }
		public int[] Values { get; private set; }

		public IntDisplayAttribute(string[] names, int[] values)
		{
			Type = IntDisplayType.Popup;
			Names = names;
			Values = values;
		}

		public IntDisplayAttribute(int minimum, int maximum, int snap)
		{
			Type = IntDisplayType.Slider;
			Names = null;
			Values = new int[] { minimum, maximum, snap };
		}

		public IntDisplayAttribute(string maximumProperty, int minimum, int maximum, int snap)
		{
			Type = IntDisplayType.MinMaxSlider;
			Names = new string[] { maximumProperty };
			Values = new int[] { minimum, maximum, snap };
		}
	}
}
