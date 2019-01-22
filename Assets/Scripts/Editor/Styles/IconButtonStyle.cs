using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class IconButtonStyle : StaticStyle
	{
		public static readonly IconButtonStyle Default = new IconButtonStyle();

		protected override GUIStyle Create()
		{
			var style = new GUIStyle(GUIStyle.none)
			{
				alignment = TextAnchor.MiddleCenter
			};

			return style;
		}
	}
}
