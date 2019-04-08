using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class PropertyListControl : ListControl
	{
		private Action<SerializedProperty> _customAdd;
		private Action<SerializedProperty, int> _customEdit;
		private Action<SerializedProperty, int> _customRemove;
		private Action<Rect, SerializedProperty, int> _customDraw;

		public PropertyListControl Setup(SerializedProperty property)
		{
			var reorderableList = new ReorderableList(property.serializedObject, property, false, true, false, false);
			Setup(reorderableList);
			return this;
		}

		public PropertyListControl MakeDrawable(Action<Rect, SerializedProperty, int> callback)
		{
			_customDraw = callback;
			return this;
		}

		public PropertyListControl MakeAddable(Label button, Action<SerializedProperty> callback = null)
		{
			MakeHeaderButton(button, Add, Color.white);
			_customAdd = callback;
			return this;
		}

		public PropertyListControl MakeRemovable(Label button, Action<SerializedProperty, int> callback = null)
		{
			MakeItemButton(button, Remove, Color.white);
			_customRemove = callback;
			return this;
		}

		public PropertyListControl MakeEditable(Label button, Action<SerializedProperty, int> callback = null)
		{
			MakeItemButton(button, Edit, Color.white);
			_customEdit = callback;
			return this;
		}

		public void DoDefaultAdd()
		{
			List.serializedProperty.arraySize++;
		}

		public void DoDefaultEdit(int index)
		{
			var property = List.serializedProperty.GetArrayElementAtIndex(index);

			if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null)
				Selection.activeObject = property.objectReferenceValue;
		}

		public void DoDefaultRemove(int index)
		{
			var property = List.serializedProperty.GetArrayElementAtIndex(index);

			// If an element is removed from a SerializedProperty that is a list or array of Objects,
			// DeleteArrayElementAtIndex will set the entry to null instead of removing it. If the entry is already
			// null it will be removed as expected.
			if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null)
				List.serializedProperty.DeleteArrayElementAtIndex(index);

			List.serializedProperty.DeleteArrayElementAtIndex(index);
		}

		public void DoDefaultDraw(Rect rect, int index)
		{
			var property = List.serializedProperty.GetArrayElementAtIndex(index);
			EditorGUI.PropertyField(rect, property, GUIContent.none);
		}

		private void Add(Rect rect)
		{
			if (_customAdd != null)
				_customAdd(List.serializedProperty);
			else
				DoDefaultAdd();
		}

		private void Edit(Rect rect, int index)
		{
			if (_customEdit != null)
				_customEdit(List.serializedProperty, index);
			else
				DoDefaultEdit(index);
		}

		private void Remove(Rect rect, int index)
		{
			if (_customRemove != null)
				_customRemove(List.serializedProperty, index);
			else
				DoDefaultRemove(index);
		}

		protected override void Draw(Rect rect, int index)
		{
			if (_customDraw != null)
				_customDraw(rect, List.serializedProperty, index);
			else
				DoDefaultDraw(rect, index);
		}
	}
}
