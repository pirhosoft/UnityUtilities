using PiRhoSoft.UtilityEngine;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[CustomPropertyDrawer(typeof(StringPopupAttribute))]
	class StringPopupDrawer : PropertyDrawer
	{
		private const string _invalidTypeWarning = "Invalid type for StringPopup on field {0}: StringPopup can only be applied to string fields";

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label.tooltip = Label.GetTooltip(fieldInfo);

			if (property.propertyType == SerializedPropertyType.String)
			{
				var options = (attribute as StringPopupAttribute).Options;
				var index = EditorGUI.Popup(position, label, Array.IndexOf(options, property.stringValue), options.Select(option => new GUIContent(option)).ToArray());
				if (index >= 0 && index < options.Length)
					property.stringValue = options[index];
			}
			else
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
				EditorGUI.PropertyField(position, property, label);
			}
		}
	}
}
