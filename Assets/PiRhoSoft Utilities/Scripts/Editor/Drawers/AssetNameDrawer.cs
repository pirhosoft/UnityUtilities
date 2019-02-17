using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[CustomPropertyDrawer(typeof(AssetNameAttribute))]
	class AssetNameDrawer : PropertyDrawer
	{
		private const string _invalidTypeWarning = "Invalid type for AssetNameDrawer on field {0}: AssetName can only be applied to string fields";

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label.tooltip = Label.GetTooltip(fieldInfo);

			EditorGUI.PropertyField(position, property, label);

			if (property.propertyType == SerializedPropertyType.String)
				property.serializedObject.targetObject.name = property.stringValue;
			else
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
		}
	}
}
