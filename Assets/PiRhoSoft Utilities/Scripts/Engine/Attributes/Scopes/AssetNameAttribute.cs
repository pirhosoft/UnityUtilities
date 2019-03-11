namespace PiRhoSoft.UtilityEngine
{
	public class AssetNameAttribute : PropertyScopeAttribute
	{
		public const int DefaultOrder = int.MaxValue - 10;

		public AssetNameAttribute() : base(DefaultOrder) { }
	}
}
