using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class PropertyDrawer<ControlType> : PropertyDrawer where ControlType : PropertyControl, new()
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var control = GetControl(property);
			return control.GetHeight(property, label);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// the label passed to PropertyDrawers from Unity does not have the tooltip resolved
			label.tooltip = Label.GetTooltip(fieldInfo);

			var control = GetControl(property);
			control.Draw(position, property, label);
		}

		private class ControlData
		{
			public SerializedObject Object;
			public ControlType Control;
		}

		private static Dictionary<string, ControlData> _controls = new Dictionary<string, ControlData>();

		private ControlType GetControl(SerializedProperty property)
		{
			var path = property.serializedObject.targetObject.name + property.propertyPath;

			// if an object is deleted, then a new object with the same name is created (or some other scenarios where
			// a new serialized object is created wrapping the same object), the property path won't change but the
			// data in most cases shouldn't be reused

			if (!_controls.TryGetValue(path, out ControlData control) || control.Object != property.serializedObject)
			{
				control = new ControlData
				{
					Object = property.serializedObject,
					Control = new ControlType()
				};

				control.Control.Setup(property, fieldInfo, attribute);
				_controls[path] = control;
			}

			return control.Control;
		}
	}
}
