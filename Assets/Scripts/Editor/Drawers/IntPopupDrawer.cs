using PiRhoSoft.UtilityEngine;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[CustomPropertyDrawer(typeof(IntPopupAttribute))]
	class IntPopupDrawer : PropertyDrawer
	{
		private const string _invalidTypeWarning = "Invalid type for IntPopup on field {0}: IntPopup can only be applied to int fields";

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label.tooltip = Label.GetTooltip(fieldInfo);

			if (property.propertyType == SerializedPropertyType.Integer)
			{
				var values = (attribute as IntPopupAttribute).Values;
				var options = (attribute as IntPopupAttribute).Names.Select(option => new GUIContent(option)).ToArray();

				EditorGUI.IntPopup(position, property, options, values, label);
			}
			else
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
				EditorGUI.PropertyField(position, property, label);
			}
		}
	}
}
