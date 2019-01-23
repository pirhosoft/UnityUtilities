using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[CustomPropertyDrawer(typeof(SliderAttribute))]
	public class SliderDrawer : PropertyDrawer
	{
		private const string _invalidTypeWarning = "Invalid type for MinMaxSlider on field {0}: MinMaxSlider can only be applied to a float or int fields";

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label.tooltip = Label.GetTooltip(fieldInfo);

			var slider = attribute as SliderAttribute;

			if (property.propertyType == SerializedPropertyType.Float)
			{
				var value = EditorGUI.Slider(position, label, property.floatValue, slider.MinimumValue, slider.MaximumValue);
				value = SnapDrawer.Snap(value, slider.SnapValue);
				property.floatValue = Mathf.Clamp(value, slider.MinimumValue, slider.MaximumValue);
			}
			else if (property.propertyType == SerializedPropertyType.Integer)
			{
				var value = EditorGUI.IntSlider(position, label, property.intValue, Mathf.RoundToInt(slider.MinimumValue), Mathf.RoundToInt(slider.MaximumValue));
				value = Mathf.RoundToInt(SnapDrawer.Snap(value, slider.SnapValue));
				property.intValue = Mathf.Clamp(value, Mathf.RoundToInt(slider.MinimumValue), Mathf.RoundToInt(slider.MaximumValue));
			}
			else
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
				EditorGUI.PropertyField(position, property, label);
			}
		}
	}
}
