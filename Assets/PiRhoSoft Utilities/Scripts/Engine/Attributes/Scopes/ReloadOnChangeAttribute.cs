namespace PiRhoSoft.UtilityEngine
{
	public interface IReloadable
	{
		void OnEnable();
		void OnDisable();
	}

	public class ReloadOnChangeAttribute : PropertyScopeAttribute
	{
		public const int DefaultOrder = int.MaxValue - 50;

		public ReloadOnChangeAttribute() : base(DefaultOrder) { }
	}
}
