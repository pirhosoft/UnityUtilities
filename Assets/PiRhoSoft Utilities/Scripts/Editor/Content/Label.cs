using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class Label : StaticContent
	{
		public string Text { get; private set; }
		public string Tooltip { get; private set; }

		public Label(Type type, string property)
		{
			Text = property;
			Tooltip = GetTooltip(type, property);
		}

		protected override GUIContent Create()
		{
			var text = ObjectNames.NicifyVariableName(Text);
			return new GUIContent(text, Tooltip);
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
