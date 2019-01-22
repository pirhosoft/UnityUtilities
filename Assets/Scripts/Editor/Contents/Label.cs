using System;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class Label : StaticContent
	{
		public string Text;
		public string Tooltip;

		public Label(string text, string tooltip = "")
		{
			Text = text;
			Tooltip = tooltip;
		}

		public Label(string text, Type type, string property)
		{
			Text = text;
			Tooltip = GuiHelper.GetTooltip(type, property);
		}

		public Label(Type type, string property)
		{
			Text = ObjectNames.NicifyVariableName(property);
			Tooltip = GuiHelper.GetTooltip(type, property);
		}

		protected override GUIContent Create()
		{
			return new GUIContent(Text, Tooltip);
		}
	}
}
