using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class Label
	{
		private const string _missingPropertyWarning = "(ULMP) unable to find property {0} on type {1} for Label";

		private string _label;
		private string _tooltip;

		private Icon _icon;
		private string _name;
		private GUIContent _content;

		public GUIContent Content
		{
			get
			{
				if (_content == null)
				{
					var label = !string.IsNullOrEmpty(_name) ? ObjectNames.NicifyVariableName(_name) : _label;// NicifyVariableName can't be called during startup
					var icon = _icon?.Content; // Texture's cannot be created during startup

					_content = new GUIContent(label, icon, _tooltip);
				}

				return _content;
			}
		}

		public Label(Type type, string property)
		{
			_name = property;
			_tooltip = GetTooltip(type, property);
		}

		public Label(string label, string tooltip = "")
		{
			_label = label;
			_tooltip = tooltip;
		}

		public Label(Icon icon, string label = "", string tooltip = "")
		{
			_icon = icon;
			_label = label;
			_tooltip = tooltip;
		}

		public Label(Icon icon, Type type, string property)
		{
			_icon = icon;
			_name = property;
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
				return string.Empty;
			}
			else
			{
				return GetTooltip(field);
			}
		}

		public static string GetTooltip(FieldInfo field)
		{
			return field?.GetCustomAttribute<TooltipAttribute>()?.tooltip ?? string.Empty;
		}

		#endregion
	}
}
