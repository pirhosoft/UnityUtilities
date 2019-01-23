using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[CustomPropertyDrawer(typeof(ConditionalDisplayAttribute))]
	public class ConditionalDisplayDrawer : PropertyDrawer
	{
		private const string _invalidPropertyWarning = "Invalid Property for ConditionalDisplay on {0}: the referenced field {1} could not be found";
		private const string _invalidPropertyType = "Invalid Property for ConditionalDisplay on {0}: the referenced field {1} must be an int, bool, float, string, enum, or Object";

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return GetVisible(property) ? EditorGUI.GetPropertyHeight(property) : 0;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// EditorGUI.PropertyField internally knows not to use PropertyDrawers when inside a PropertyDrawer. That
			// does mean this will bypass any custom drawers for the field type, but at least it won't infinitely
			// recurse. If this is the case then use ConditionalDrawNextAttribute instead.

			if (GetVisible(property))
			{
				label.tooltip = Label.GetTooltip(fieldInfo);
				EditorGUI.PropertyField(position, property, label, true);
			}
		}

		private bool GetVisible(SerializedProperty property)
		{
			return GetVisible(property, attribute as ConditionalDisplayAttribute);
		}

		private bool GetVisible(SerializedProperty property, ConditionalDisplayAttribute attribute)
		{
			var path = property.propertyPath;
			var index = property.propertyPath.LastIndexOf('.');
			var conditionPath = index > 0 ? path.Substring(0, index) + "." + attribute.Property : attribute.Property;
			var conditionProperty = property.serializedObject.FindProperty(conditionPath);

			if (conditionProperty != null)
			{
				switch (conditionProperty.propertyType)
				{
					case SerializedPropertyType.Integer: return conditionProperty.intValue == attribute.IntValue;
					case SerializedPropertyType.Boolean: return conditionProperty.boolValue == attribute.BoolValue;
					case SerializedPropertyType.Float: return conditionProperty.floatValue == attribute.FloatValue;
					case SerializedPropertyType.String: return conditionProperty.stringValue == attribute.StringValue;
					case SerializedPropertyType.ObjectReference: return conditionProperty.objectReferenceValue == attribute.HasReference;
					case SerializedPropertyType.Enum: return conditionProperty.intValue == attribute.EnumValue;
					default: Debug.LogWarningFormat(_invalidPropertyType, property.propertyPath, attribute.Property); break;
				}
			}
			else
			{
				Debug.LogWarningFormat(_invalidPropertyWarning, property.propertyPath, attribute.Property);
			}

			return true;
		}
	}
}
