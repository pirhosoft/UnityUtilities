using System.Reflection;
using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class SnapControl : PropertyScopeControl
	{
		private const string _invalidTypeWarning = "Invalid type for MinMaxSlider on field {0}: MinMaxSlider can only be applied to a float or int field";

		private float _snapValue = 0.0f;

		public override void Setup(SerializedProperty property, FieldInfo fieldInfo, PropertyAttribute attribute)
		{
			base.Setup(property, fieldInfo, attribute);

			if (attribute is SnapAttribute snap)
				_snapValue = snap.SnapValue;

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

			if (property.propertyType == SerializedPropertyType.Float)
				property.floatValue = MathHelper.Snap(property.floatValue, _snapValue);
			else if (property.propertyType == SerializedPropertyType.Integer)
				property.intValue = MathHelper.Snap(property.intValue, Mathf.RoundToInt(_snapValue));
		}
	}

	[CustomPropertyDrawer(typeof(SnapAttribute))]
	public class SnapDrawer : ControlDrawer<SnapControl>
	{
	}
}
