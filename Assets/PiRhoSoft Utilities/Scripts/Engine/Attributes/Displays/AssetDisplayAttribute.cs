using System;
using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	[Flags]
	public enum AssetDisplaySaveLocation
	{
		None,
		AssetRoot,
		Selectable
	}

	public class AssetDisplayAttribute : PropertyAttribute
	{
		public bool ShowNoneOption = true;
		public bool ShowEditButton = true;
		public AssetDisplaySaveLocation SaveLocation = AssetDisplaySaveLocation.None;
		public string DefaultName = null;
	}
}
