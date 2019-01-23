using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public class MaximumAttribute : PropertyAttribute
	{
		public float MaximumValue { get; private set; }

		public MaximumAttribute(float maximum) => MaximumValue = maximum;
		public MaximumAttribute(int maximum) => MaximumValue = maximum;
	}
}
