using PiRhoSoft.UtilityEngine;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[CustomPropertyDrawer(typeof(AssetNameAttribute))]
	public class AssetNameControl : PropertyScopeControl
	{
		private const string _invalidTypeWarning = "Invalid type for AssetName on field {0}: AssetName can only be applied to string fields";

		public override void Setup(SerializedProperty property, FieldInfo fieldInfo, PropertyAttribute attribute)
		{
			base.Setup(property, fieldInfo, attribute);

			if (property.propertyType != SerializedPropertyType.String)
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
		}

		public override float GetHeight(SerializedProperty property, GUIContent label)
		{
			return GetNextHeight(property, label);
		}

		public override void Draw(Rect position, SerializedProperty property, GUIContent label)
		{
			DrawNext(position, property, label);

			if (property.propertyType == SerializedPropertyType.String)
				property.serializedObject.targetObject.name = property.stringValue;
		}
	}

	[CustomPropertyDrawer(typeof(AssetNameAttribute))]
	public class AssetNameDrawer : ControlDrawer<AssetNameControl>
	{
	}
}
