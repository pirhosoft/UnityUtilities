namespace PiRhoSoft.UtilityEngine
{
	public class MaximumAttribute : PropertyScopeAttribute
	{
		public const int DefaultOrder = int.MaxValue - 20;

		public float MaximumValue { get; private set; }

		public MaximumAttribute(float maximum) : base(DefaultOrder) => MaximumValue = maximum;
		public MaximumAttribute(int maximum) : base(DefaultOrder) => MaximumValue = maximum;
	}
}
