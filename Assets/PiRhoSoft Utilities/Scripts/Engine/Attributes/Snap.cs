namespace PiRhoSoft.UtilityEngine
{
	public class SnapAttribute : PropertyScopeAttribute
	{
		public float SnapValue { get; private set; }

		public SnapAttribute(int snapValue) : base(int.MaxValue - 10) => SnapValue = snapValue;
		public SnapAttribute(float snapValue) : base(int.MaxValue - 10) => SnapValue = snapValue;
	}
}
