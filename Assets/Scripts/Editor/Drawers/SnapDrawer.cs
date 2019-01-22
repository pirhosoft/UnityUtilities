﻿using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[CustomPropertyDrawer(typeof(SnapAttribute))]
	public class SnapDrawer : PropertyDrawer
	{
		private const string _invalidTypeWarning = "Invalid type for MinMaxSlider on field {0}: MinMaxSlider can only be applied to a float or int fields";

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label.tooltip = GuiHelper.GetTooltip(fieldInfo);

			var snap = attribute as SnapAttribute;

			EditorGUI.PropertyField(position, property, label);

			if (property.propertyType == SerializedPropertyType.Float)
				property.floatValue = MathHelper.Snap(property.floatValue, snap.SnapValue);
			else if (property.propertyType == SerializedPropertyType.Integer)
				property.intValue = Mathf.RoundToInt(MathHelper.Snap(property.intValue, snap.SnapValue));
			else
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
		}
	}
}
