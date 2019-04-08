using PiRhoSoft.UtilityEngine;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class ChangeTriggerControl : PropertyScopeControl
	{
		private object[] _parameters = new object[0];
		private Type[] _types = new Type[0];
		private ParameterModifier[] _modifiers = new ParameterModifier[0];

		private const string _missingObjectWarning = "(UCTMO) failed to setup ChangeTrigger for '{0}': the object could not be resolved";
		private const string _missingMethodWarning = "(UCTMM) failed to setup ChangeTrigger for '{0}': a method named '{1}' with no parameters could not be found";

		private object _object;
		private MethodInfo _method;

		public override void Setup(SerializedProperty property, FieldInfo fieldInfo, PropertyAttribute attribute)
		{
			base.Setup(property, fieldInfo, attribute);

			var callback = (attribute as ChangeTriggerAttribute).Callback;

			_object = PropertyHelper.GetAncestor<object>(property, 1);
			_method = fieldInfo.DeclaringType.GetMethod(callback, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, _types, _modifiers);

			if (_object == null)
				Debug.LogWarningFormat(_missingObjectWarning, property.propertyPath);
			else if (_method == null)
				Debug.LogWarningFormat(_missingMethodWarning, property.propertyPath, callback);

			Undo.undoRedoPerformed += OnUndo;
		}

		public override float GetHeight(SerializedProperty property, GUIContent label)
		{
			return GetNextHeight(property, label);
		}

		public override void Draw(Rect position, SerializedProperty property, GUIContent label)
		{
			using (var changes = new EditorGUI.ChangeCheckScope())
			{
				DrawNext(position, property, label);

				if (changes.changed && _object != null && _method != null)
				{
					using (new EditObjectScope(property.serializedObject))
						_method.Invoke(_object, _parameters);
				}
			}
		}

		private void OnUndo()
		{
			if (_object != null && _method != null)
				_method.Invoke(_object, _parameters);
		}
	}

	[CustomPropertyDrawer(typeof(ChangeTriggerAttribute))]
	public class ChangeTriggerDrawer : PropertyDrawer<ChangeTriggerControl>
	{
	}
}
