using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public abstract class PropertyScopeControl : PropertyControl
	{
		private PropertyDrawer _nextDrawer = null;

		public override void Setup(SerializedProperty property, FieldInfo fieldInfo, PropertyAttribute attribute)
		{
			_nextDrawer = PropertyHelper.GetNextDrawer(fieldInfo, attribute);
		}

		protected float GetNextHeight(SerializedProperty property, GUIContent label)
		{
			return _nextDrawer == null ? EditorGUI.GetPropertyHeight(property) : _nextDrawer.GetPropertyHeight(property, label);
		}

		protected void DrawNext(Rect position, SerializedProperty property, GUIContent label)
		{
			if (_nextDrawer != null)
				_nextDrawer.OnGUI(position, property, label);
			else
				EditorGUI.PropertyField(position, property, label);
		}
	}
}
