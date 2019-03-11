namespace PiRhoSoft.UtilityEngine
{
	public class SnapAttribute : PropertyScopeAttribute
	{
		public const int DefaultOrder = int.MaxValue - 10;

		public float SnapValue { get; private set; }

		public SnapAttribute(int snapValue) : base(DefaultOrder) => SnapValue = snapValue;
		public SnapAttribute(float snapValue) : base(DefaultOrder) => SnapValue = snapValue;
	}
}
