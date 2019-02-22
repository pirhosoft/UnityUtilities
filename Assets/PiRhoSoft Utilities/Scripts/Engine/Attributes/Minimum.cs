namespace PiRhoSoft.UtilityEngine
{
	public class MinimumAttribute : PropertyScopeAttribute
	{
		public float MinimumValue { get; private set; }

		public MinimumAttribute(float minimum) : base(int.MaxValue - 30) => MinimumValue = minimum;
		public MinimumAttribute(int minimum) : base(int.MaxValue - 30) => MinimumValue = minimum;
	}
}
