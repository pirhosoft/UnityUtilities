using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class ControlDrawer<T> : PropertyDrawer where T : PropertyControl, new()
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

		private Dictionary<string, T> _controls = new Dictionary<string, T>();

		private T GetControl(SerializedProperty property)
		{
			var path = property.serializedObject.targetObject.name + property.propertyPath;

			if (!_controls.TryGetValue(path, out T control))
			{
				control = new T();
				control.Setup(property, fieldInfo);
				_controls.Add(path, control);
			}

			return control;
		}
	}
}
