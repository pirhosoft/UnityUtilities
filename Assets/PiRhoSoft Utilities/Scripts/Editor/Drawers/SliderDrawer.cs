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
				value = MathHelper.Snap(value, slider.SnapValue);
				property.floatValue = Mathf.Clamp(value, slider.MinimumValue, slider.MaximumValue);
			}
			else if (property.propertyType == SerializedPropertyType.Integer)
			{
				var minimum = Mathf.RoundToInt(slider.MinimumValue);
				var maximum = Mathf.RoundToInt(slider.MaximumValue);
				var snap = Mathf.RoundToInt(slider.SnapValue);

				var value = EditorGUI.IntSlider(position, label, property.intValue, minimum, maximum);
				value = MathHelper.Snap(value, snap);
				property.intValue = Mathf.Clamp(value, minimum, maximum);
			}
			else
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
				EditorGUI.PropertyField(position, property, label);
			}
		}
	}
}
