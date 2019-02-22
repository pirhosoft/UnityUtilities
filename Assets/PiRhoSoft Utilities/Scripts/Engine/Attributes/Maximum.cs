namespace PiRhoSoft.UtilityEngine
{
	public class MaximumAttribute : PropertyScopeAttribute
	{
		public float MaximumValue { get; private set; }

		public MaximumAttribute(float maximum) : base(int.MaxValue - 20) => MaximumValue = maximum;
		public MaximumAttribute(int maximum) : base(int.MaxValue - 20) => MaximumValue = maximum;
	}
}
