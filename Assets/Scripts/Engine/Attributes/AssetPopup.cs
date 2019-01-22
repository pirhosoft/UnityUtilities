using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public class AssetPopupAttribute : PropertyAttribute
	{
		public bool ShowEdit { get; private set; }
		public AssetPopupAttribute(bool showEdit = true) => ShowEdit = showEdit;
	}
}
