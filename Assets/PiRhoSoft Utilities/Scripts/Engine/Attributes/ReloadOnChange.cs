namespace PiRhoSoft.UtilityEngine
{
	public interface IReloadable
	{
		void OnEnable();
		void OnDisable();
	}

	public class ReloadOnChangeAttribute : PropertyScopeAttribute
	{
		public ReloadOnChangeAttribute() : base(int.MaxValue - 50) { }

		public bool UseAssetPopup = true;
	}
}
