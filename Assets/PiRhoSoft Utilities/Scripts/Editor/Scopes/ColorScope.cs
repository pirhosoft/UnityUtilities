using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class ColorScope : GUI.Scope
	{
		private Color _color;
		private Color _backgroundColor;
		private Color _contentColor;

		public static ColorScope Color(Color color)
		{
			return new ColorScope(color, GUI.backgroundColor, GUI.contentColor);
		}

		public static ColorScope BackgroundColor(Color backgroundColor)
		{
			return new ColorScope(GUI.color, backgroundColor, GUI.contentColor);
		}

		public static ColorScope ContentColor(Color contentColor)
		{
			return new ColorScope(GUI.color, GUI.backgroundColor, contentColor);
		}

		public ColorScope(Color color, Color backgroundColor, Color contentColor)
		{
			_color = GUI.color;
			_backgroundColor = GUI.backgroundColor;
			_contentColor = GUI.contentColor;

			GUI.color = color;
			GUI.backgroundColor = backgroundColor;
			GUI.contentColor = contentColor;
		}

		protected override void CloseScope()
		{
			GUI.color = _color;
			GUI.backgroundColor = _backgroundColor;
			GUI.contentColor = _contentColor;
		}
	}
}
