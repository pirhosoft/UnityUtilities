namespace PiRhoSoft.UtilityEngine
{
	public class DisableInInspectorAttribute : PropertyScopeAttribute
	{
		public DisableInInspectorAttribute() : base(int.MaxValue - 100) { }
	}
}
