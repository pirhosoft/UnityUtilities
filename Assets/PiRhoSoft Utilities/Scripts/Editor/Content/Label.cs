using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class Label
	{
		private const string _missingPropertyWarning = "(ULMP) unable to find property {0} on type {1} for Label";

		private string _text;
		private string _tooltip;
		private GUIContent _content;

		public GUIContent Content
		{
			get
			{
				if (_content == null)
				{
					var text = ObjectNames.NicifyVariableName(_text); // this is the reason the content can't be created in the constructor - NicifyVariableName can't be called on startup
					_content = new GUIContent(text, _tooltip);
				}

				return _content;
			}
		}

		public Label(Type type, string property)
		{
			_text = property;
			_tooltip = GetTooltip(type, property);
		}

		#region Tooltips

		public static string GetTooltip(Type type, string propertyName)
		{
			var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			var field = type.GetField(propertyName, flags);

			if (field == null)
			{
				Debug.LogWarningFormat(_missingPropertyWarning, propertyName, type.Name);
				return "";
			}
			else
			{
				return GetTooltip(field);
			}
		}

		public static string GetTooltip(FieldInfo field)
		{
			return field?.GetCustomAttribute<TooltipAttribute>()?.tooltip ?? "";
		}

		#endregion
	}
}
