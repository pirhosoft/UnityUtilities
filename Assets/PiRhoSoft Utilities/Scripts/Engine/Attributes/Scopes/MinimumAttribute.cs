namespace PiRhoSoft.UtilityEngine
{
	public class MinimumAttribute : PropertyScopeAttribute
	{
		public const int DefaultOrder = int.MaxValue - 30;

		public float MinimumValue { get; private set; }

		public MinimumAttribute(float minimum) : base(DefaultOrder) => MinimumValue = minimum;
		public MinimumAttribute(int minimum) : base(DefaultOrder) => MinimumValue = minimum;
	}
}
