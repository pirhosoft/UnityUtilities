using System;

namespace PiRhoSoft.UtilityEngine
{
	public class FoldoutStringAttribute : Attribute
	{
		public float ExpandedHeight = 100.0f;
	}

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
