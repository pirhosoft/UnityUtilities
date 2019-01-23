using PiRhoSoft.UtilityEngine;
using System;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[CustomPropertyDrawer(typeof(FlagsPopupAttribute))]
	class FlagsPopupDrawer : PropertyDrawer
	{
		private const string _invalidTypeWarning = "Invalid type for EnumFlagsDisplayDrawer on field {0}: EnumFlagsDisplay can only be applied to Enum fields";

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (property.propertyType == SerializedPropertyType.Enum)
			{
				return EditorGUIUtility.singleLineHeight;
			}
			else
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
				return EditorGUI.GetPropertyHeight(property);
			}
		}
	
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label.tooltip = Label.GetTooltip(fieldInfo);

			if (property.propertyType == SerializedPropertyType.Enum)
			{
				var value = Enum.Parse(fieldInfo.FieldType, property.intValue.ToString());
				property.intValue = Convert.ToInt32(EditorGUI.EnumFlagsField(position, label, (Enum)value));
			}
			else
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
				EditorGUI.PropertyField(position, property, label);
			}
		}
	}
}
