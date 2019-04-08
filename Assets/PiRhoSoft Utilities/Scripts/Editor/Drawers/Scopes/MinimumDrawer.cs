using System.Reflection;
using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class MinimumControl : PropertyScopeControl
	{
		public const string _invalidTypeWarning = "(UMICIT) Invalid type for Minimum on field {0}: Minimum can only be applied to int or float fields";

		private float _minimum = 0.0f;

		public override void Setup(SerializedProperty property, FieldInfo fieldInfo, PropertyAttribute attribute)
		{
			base.Setup(property, fieldInfo, attribute);

			if (attribute is MinimumAttribute minimum)
				_minimum = minimum.MinimumValue;

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
				property.intValue = Mathf.Max(Mathf.RoundToInt(_minimum), property.intValue);
			else if (property.propertyType == SerializedPropertyType.Float)
				property.floatValue = Mathf.Max(_minimum, property.floatValue);
		}
	}

	[CustomPropertyDrawer(typeof(MinimumAttribute))]
	public class MinimumDrawer : PropertyDrawer<MinimumControl>
	{
	}
}
