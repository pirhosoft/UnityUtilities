using System;

namespace PiRhoSoft.UtilityEngine
{
	[Serializable]
	public class FoldoutString
	{
		public string String;

		#region Editor Support
#if UNITY_EDITOR
		[NonSerialized] public bool IsExpanded = false;
#endif
		#endregion
	}
}
