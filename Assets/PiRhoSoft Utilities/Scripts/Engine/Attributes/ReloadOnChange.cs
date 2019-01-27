using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public interface IReloadable
	{
		void OnEnable();
		void OnDisable();
	}

	public class ReloadOnChangeAttribute : PropertyAttribute
	{
		public bool UseAssetPopup = true;
	}
}
