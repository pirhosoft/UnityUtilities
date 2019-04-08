using System.Reflection;
using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class MaximumControl : PropertyScopeControl
	{
		public const string _invalidTypeWarning = "(UMACIT) Invalid type for Maximum on field {0}: Maximum can only be applied to int or float fields";

		private float _maximum = 0.0f;

		public override void Setup(SerializedProperty property, FieldInfo fieldInfo, PropertyAttribute attribute)
		{
			base.Setup(property, fieldInfo, attribute);

			if (attribute is MaximumAttribute maximum)
				_maximum = maximum.MaximumValue;

			if (property.propertyType != SerializedPropertyType.Float && property.propertyType != SerializedPropertyType.Integer)
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
		}

		public override float GetHeight(SerializedProperty property, GUIContent label)
		{
			return GetNextHeight(property, label);
		}

		public override void Draw(Rect position, SerializedProperty property, GUIContent label)
		{
			DrawNext(position, property, label);

			if (property.propertyType == SerializedPropertyType.Integer)
				property.intValue = Mathf.Min(Mathf.RoundToInt(_maximum), property.intValue);
			else if (property.propertyType == SerializedPropertyType.Float)
				property.floatValue = Mathf.Min(_maximum, property.floatValue);
		}
	}

	[CustomPropertyDrawer(typeof(MaximumAttribute))]
	public class MaximumDrawer : PropertyDrawer<MaximumControl>
	{
	}
}
