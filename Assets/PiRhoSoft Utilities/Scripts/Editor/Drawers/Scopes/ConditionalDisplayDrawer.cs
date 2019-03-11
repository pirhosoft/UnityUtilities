using System.Reflection;
using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public abstract class ConditionalDisplayControl : PropertyScopeControl
	{
		private const string _invalidPropertyTypeWarning = "Invalid Property for ConditionalDisplay: the field {0} must be an int, bool, float, string, enum, or Object";

		protected ConditionalDisplayAttribute _attribute;

		public override void Setup(SerializedProperty property, FieldInfo fieldInfo, PropertyAttribute attribute)
		{
			base.Setup(property, fieldInfo, attribute);

			_attribute = attribute as ConditionalDisplayAttribute;
		}

		protected bool IsVisible(SerializedProperty property)
		{
			switch (property.propertyType)
			{
				case SerializedPropertyType.Integer: return _attribute.Invert ? property.intValue != _attribute.IntValue : property.intValue == _attribute.IntValue;
				case SerializedPropertyType.Boolean: return _attribute.Invert ? !property.boolValue : property.boolValue;
				case SerializedPropertyType.Float: return _attribute.Invert ? property.floatValue != _attribute.FloatValue : property.floatValue == _attribute.FloatValue;
				case SerializedPropertyType.String: return _attribute.Invert ? property.stringValue != _attribute.StringValue : property.stringValue == _attribute.StringValue;
				case SerializedPropertyType.ObjectReference: return _attribute.Invert ? property.objectReferenceValue == null : property.objectReferenceValue != null;
				case SerializedPropertyType.Enum: return _attribute.Invert ? property.intValue != _attribute.EnumValue : property.intValue == _attribute.EnumValue;
				default: Debug.LogWarningFormat(_invalidPropertyTypeWarning, property.propertyPath); break;
			}

			return true;
		}
	}

	public class ConditionalDisplaySelfControl : ConditionalDisplayControl
	{
		private const string _invalidPropertyNameWarning = "Invalid Property for ConditionalDisplaySelf on {0}: the referenced field {1} could not be found";

		public override float GetHeight(SerializedProperty property, GUIContent label)
		{
			return GetVisible(property) ? GetNextHeight(property, label) : 0.0f;
		}

		public override void Draw(Rect position, SerializedProperty property, GUIContent label)
		{
			if (GetVisible(property))
				DrawNext(position, property, label);
		}

		private bool GetVisible(SerializedProperty property)
		{
			var reference = PropertyHelper.GetSibling(property, _attribute.Property);

			if (reference != null)
				return IsVisible(reference);
			else
				Debug.LogWarningFormat(_invalidPropertyNameWarning, property.propertyPath, reference.propertyPath);

			return true;
		}
	}

	public class ConditionalDisplayOtherControl : ConditionalDisplayControl
	{
		private const string _invalidPropertyNameWarning = "Invalid Property for ConditionalDisplayOther on {0}: the referenced field {1} could not be found";

		public override float GetHeight(SerializedProperty property, GUIContent label)
		{
			var height = EditorGUI.GetPropertyHeight(property);

			if (IsVisible(property))
			{
				var reference = PropertyHelper.GetSibling(property, _attribute.Property);
				height += reference != null ? EditorGUI.GetPropertyHeight(reference) : 0;
			}

			return height;
		}

		public override void Draw(Rect position, SerializedProperty property, GUIContent label)
		{
			var rect = RectHelper.TakeHeight(ref position, EditorGUI.GetPropertyHeight(property));

			EditorGUI.PropertyField(rect, property, label);

			if (IsVisible(property))
			{
				var nextRect = RectHelper.TakeVerticalSpace(ref position);
				var reference = PropertyHelper.GetSibling(property, _attribute.Property);

				if (reference != null)
					EditorGUI.PropertyField(nextRect, reference);
				else
					Debug.LogWarningFormat(_invalidPropertyNameWarning, property.propertyPath, reference.propertyPath);
			}
		}
	}

	[CustomPropertyDrawer(typeof(ConditionalDisplaySelfAttribute))]
	public class ConditionalDisplaySelfDrawer : ControlDrawer<ConditionalDisplaySelfControl>
	{
	}

	[CustomPropertyDrawer(typeof(ConditionalDisplayOtherAttribute))]
	public class ConditionalDisplayOtherDrawer : ControlDrawer<ConditionalDisplayOtherControl>
	{
	}
}
