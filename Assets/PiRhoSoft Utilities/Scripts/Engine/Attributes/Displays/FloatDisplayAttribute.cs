using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public enum FloatDisplayType
	{
		Popup,
		Slider,
		MinMaxSlider
	}

	public class FloatDisplayAttribute : PropertyAttribute
	{
		public FloatDisplayType Type { get; private set; }
		public string[] Names { get; private set; }
		public float[] Values { get; private set; }

		public FloatDisplayAttribute(string[] names, float[] values)
		{
			Type = FloatDisplayType.Popup;
			Names = names;
			Values = values;
		}

		public FloatDisplayAttribute(float minimum, float maximum, float snap)
		{
			Type = FloatDisplayType.Slider;
			Names = null;
			Values = new float[] { minimum, maximum, snap };
		}

		public FloatDisplayAttribute(string maximumProperty, float minimum, float maximum, float snap)
		{
			Type = FloatDisplayType.MinMaxSlider;
			Names = new string[] { maximumProperty };
			Values = new float[] { minimum, maximum, snap };
		}
	}
}
