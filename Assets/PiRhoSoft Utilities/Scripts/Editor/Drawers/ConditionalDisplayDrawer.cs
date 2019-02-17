using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class ConditionalDisplayDrawer : PropertyDrawer
	{
		private const string _invalidPropertyTypeWarning = "Invalid Property for ConditionalDisplay: the field {0} must be an int, bool, float, string, enum, or Object";

		protected bool IsVisible(SerializedProperty property)
		{
			var condition = attribute as ConditionalDisplayAttribute;

			switch (property.propertyType)
			{
				case SerializedPropertyType.Integer: return condition.Invert ? property.intValue != condition.IntValue : property.intValue == condition.IntValue;
				case SerializedPropertyType.Boolean: return condition.Invert ? !property.boolValue : property.boolValue;
				case SerializedPropertyType.Float: return condition.Invert ? property.floatValue != condition.FloatValue : property.floatValue == condition.FloatValue;
				case SerializedPropertyType.String: return condition.Invert ? property.stringValue != condition.StringValue : property.stringValue == condition.StringValue;
				case SerializedPropertyType.ObjectReference: return condition.Invert ? property.objectReferenceValue == null : property.objectReferenceValue != null;
				case SerializedPropertyType.Enum: return condition.Invert ? property.intValue != condition.EnumValue : property.intValue == condition.EnumValue;
				default: Debug.LogWarningFormat(_invalidPropertyTypeWarning, property.propertyPath); break;
			}

			return true;
		}
	}

	[CustomPropertyDrawer(typeof(ConditionalDisplaySelfAttribute))]
	public class ConditionalDisplaySelfDrawer : ConditionalDisplayDrawer
	{
		private const string _invalidPropertyNameWarning = "Invalid Property for ConditionalDisplaySelf on {0}: the referenced field {1} could not be found";

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return GetVisible(property) ? EditorGUI.GetPropertyHeight(property) : 0;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// EditorGUI.PropertyField internally knows not to use PropertyDrawers when inside a PropertyDrawer. That
			// does mean this will bypass any custom drawers for the field type, but at least it won't infinitely
			// recurse. If this is the case then use ConditionalDrawOtherAttribute instead.

			if (GetVisible(property))
			{
				label.tooltip = Label.GetTooltip(fieldInfo);
				EditorGUI.PropertyField(position, property, label, true);
			}
		}

		private bool GetVisible(SerializedProperty property)
		{
			var reference = PropertyHelper.GetSibling(property, (attribute as ConditionalDisplayAttribute).Property);

			if (reference != null)
				return IsVisible(reference);
			else
				Debug.LogWarningFormat(_invalidPropertyNameWarning, property.propertyPath, reference.propertyPath);

			return true;
		}
	}

	[CustomPropertyDrawer(typeof(ConditionalDisplayOtherAttribute))]
	public class ConditionalDisplayOtherDrawer : ConditionalDisplayDrawer
	{
		private const string _invalidPropertyNameWarning = "Invalid Property for ConditionalDisplayOther on {0}: the referenced field {1} could not be found";

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var height = EditorGUI.GetPropertyHeight(property);

			if (IsVisible(property))
			{
				var reference = PropertyHelper.GetSibling(property, (attribute as ConditionalDisplayAttribute).Property);
				height += reference != null ? EditorGUI.GetPropertyHeight(reference) : 0;
			}

			return height;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label.tooltip = Label.GetTooltip(fieldInfo);

			var rect = RectHelper.TakeHeight(ref position, EditorGUI.GetPropertyHeight(property));

			EditorGUI.PropertyField(rect, property, label);

			if (IsVisible(property))
			{
				var nextRect = RectHelper.TakeVerticalSpace(ref position);
				var reference = PropertyHelper.GetSibling(property, (attribute as ConditionalDisplayAttribute).Property);

				if (reference != null)
					EditorGUI.PropertyField(nextRect, reference);
				else
					Debug.LogWarningFormat(_invalidPropertyNameWarning, property.propertyPath, reference.propertyPath);
			}
		}
	}
}
