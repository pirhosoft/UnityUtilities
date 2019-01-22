using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public class SliderAttribute : PropertyAttribute
	{
		public float MinimumValue { get; private set; }
		public float MaximumValue { get; private set; }
		public float SnapValue { get; private set; }

		public SliderAttribute(float minValue, float maxValue, float snapValue = 0.0f)
		{
			MinimumValue = minValue;
			MaximumValue = maxValue;
			SnapValue = snapValue;
		}

		public SliderAttribute(int minValue, int maxValue, int snapValue = 0)
		{
			MinimumValue = minValue;
			MaximumValue = maxValue;
			SnapValue = snapValue;
		}
	}
}
