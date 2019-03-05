using System;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class Base64Button : StaticContent
	{
		public string Data;
		public string Tooltip;

		public Base64Button(string data, string tooltip = "")
		{
			Data = data;
			Tooltip = tooltip;
		}

		protected override GUIContent Create()
		{
			var texture = new Texture2D(1, 1);
			texture.hideFlags = HideFlags.HideAndDontSave;
			texture.LoadImage(Convert.FromBase64String(Data));

			return new GUIContent(texture, Tooltip);
		}
	}
}
