using System;
using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public enum ListItemDisplayType
	{
		Normal,
		Inline,
		Foldout,
		AssetPopup
	}

	public class ListDisplayAttribute : PropertyAttribute
	{
		public bool AllowAdd = true;
		public bool AllowRemove = true;
		public bool AllowReorder = true;
		public bool AllowCollapse = true;
		public bool ShowEditButton = false;
		public ListItemDisplayType ItemDisplay = ListItemDisplayType.Normal;
		public Type AssetType = null;
		public string EmptyText = null;
	}
}
