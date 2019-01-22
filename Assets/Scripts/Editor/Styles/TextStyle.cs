using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class TextStyle : StaticStyle
	{
		public static readonly TextStyle TopLeftWhite = new TextStyle(Color.white, FontStyle.Bold, TextAnchor.UpperLeft);
		public static readonly TextStyle MiddleLeftWhite = new TextStyle(Color.white, FontStyle.Bold, TextAnchor.MiddleLeft);
		public static readonly TextStyle BottomLeftWhite = new TextStyle(Color.white, FontStyle.Bold, TextAnchor.LowerLeft);
		public static readonly TextStyle MiddleCenterWhite = new TextStyle(Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);

		public Color Color;
		public FontStyle FontStyle;
		public TextAnchor Alignment;

		public TextStyle(Color color, FontStyle style, TextAnchor alignment)
		{
			Color = color;
			FontStyle = style;
			Alignment = alignment;
		}

		protected override GUIStyle Create()
		{
			var style = BaseStyle == null ? new GUIStyle() : new GUIStyle(BaseStyle);
			style.normal.textColor = Color;
			style.fontStyle = FontStyle;
			style.alignment = Alignment;
			return style;
		}
	}
}
