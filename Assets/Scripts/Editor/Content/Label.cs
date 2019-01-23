using System;
using System.Reflection;
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
			Tooltip = GetTooltip(type, property);
		}

		public Label(Type type, string property)
		{
			Text = ObjectNames.NicifyVariableName(property);
			Tooltip = GetTooltip(type, property);
		}

		protected override GUIContent Create()
		{
			return new GUIContent(Text, Tooltip);
		}

		#region Tooltips

		public static string GetTooltip(Type type, string propertyName)
		{
			var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			var field = type.GetField(propertyName, flags);

			return GetTooltip(field);
		}

		public static string GetTooltip(FieldInfo field)
		{
			return field?.GetCustomAttribute<TooltipAttribute>()?.tooltip ?? "";
		}

		#endregion
	}
}
