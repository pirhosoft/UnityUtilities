using PiRhoSoft.UtilityEngine;
using UnityEngine;

namespace PiRhoSoft.UtilityExample
{
	[ExecuteInEditMode]
	[AddComponentMenu("PiRho Soft/Examples/String Display")]
	public class StringDisplayExample : MonoBehaviour
	{
		[StringDisplay(StringDisplayType.Field)] public string Field;
		[StringDisplay(StringDisplayType.Field, Foldout = true)] public string FoldoutField;
		[StringDisplay(StringDisplayType.Field, FullWidth = true)] public string FullField;
		[StringDisplay(StringDisplayType.Field, Foldout = true, FullWidth = true)] public string FoldoutAndFullField;

		[StringDisplay(StringDisplayType.Box)] public string Box;
		[StringDisplay(StringDisplayType.Box, Foldout = true)] public string FoldoutBox;
		[StringDisplay(StringDisplayType.Box, FullWidth = true)] public string FullBox;
		[StringDisplay(StringDisplayType.Box, Foldout = true, FullWidth = true)] public string FoldoutAndFullBox;

		[StringDisplay(StringDisplayType.Area)] public string Area;
		[StringDisplay(StringDisplayType.Area, Foldout = true)] public string FoldoutArea;
		[StringDisplay(StringDisplayType.Area, FullWidth = true)] public string FullArea;
		[StringDisplay(StringDisplayType.Area, Foldout = true, FullWidth = true)] public string FoldoutAndFullArea;

		[StringDisplay(new string[] { "One", "Two", "Three" })] public string Popup;
		[StringDisplay(new string[] { "One", "Two", "Three" }, Foldout = true)] public string FoldoutPopup;
		[StringDisplay(new string[] { "One", "Two", "Three" }, FullWidth = true)] public string FullPopup;
		[StringDisplay(new string[] { "One", "Two", "Three" }, Foldout = true, FullWidth = true)] public string FoldoutAndFullPopup;

		[StringDisplay(StringDisplayType.Box, WordWrap = true, MinimumLines = 4, MaximumLines = 4)] public string WrappingBox;
		[StringDisplay(StringDisplayType.Area, WordWrap = false, MinimumLines = 4, MaximumLines = 4)] public string NonWrappingArea;
	}
}
