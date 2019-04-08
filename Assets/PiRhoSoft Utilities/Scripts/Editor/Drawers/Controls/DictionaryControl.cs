using System;
using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class DictionaryControl : ListControl
	{
		private SerializedProperty _rootProperty;
		private SerializedProperty _keysProperty;
		private SerializedProperty _valuesProperty;
		private IEditableDictionary _dictionary;
		
		private Action<IEditableDictionary, string> _customAdd;
		private Action<IEditableDictionary, string> _customRemove;
		private Action<Rect, SerializedProperty, int, string> _customDraw;

		private CreateNamedPopup _createPopup = new CreateNamedPopup();

		public DictionaryControl Setup(SerializedProperty property, IEditableDictionary dictionary)
		{
			_dictionary = dictionary;

			PrepareForEdit();

			_rootProperty = property;
			_keysProperty = property.FindPropertyRelative("_keys");
			_valuesProperty = property.FindPropertyRelative("_values");

			var reorderableList = new ReorderableList(property.serializedObject, _keysProperty, false, true, false, false);
			Setup(reorderableList);

			return this;
		}

		public DictionaryControl MakeDrawable(Action<Rect, SerializedProperty, int, string> callback)
		{
			_customDraw = callback;
			return this;
		}

		public DictionaryControl MakeAddable(Label button, GUIContent label, Action<IEditableDictionary, string> callback = null)
		{
			_createPopup.Setup(label, PopupCreate, PopupValidate);
			MakeHeaderButton(button, _createPopup, Color.white);
			_customAdd = callback;
			return this;
		}

		public DictionaryControl MakeRemovable(Label button, Action<IEditableDictionary, string> callback = null)
		{
			MakeItemButton(button, Remove, Color.white);
			_customRemove = callback;
			return this;
		}

		private void DoDefaultAdd(string key)
		{
			using (new UndoScope(_rootProperty.serializedObject))
			{
				_keysProperty.arraySize++;
				_valuesProperty.arraySize++;

				_keysProperty.GetArrayElementAtIndex(_keysProperty.arraySize - 1).stringValue = key;
			}
		}

		private void DoDefaultEdit(int index)
		{
			var value = _valuesProperty.GetArrayElementAtIndex(index);

			if (value.propertyType == SerializedPropertyType.ObjectReference && value.objectReferenceValue != null)
				Selection.activeObject = value.objectReferenceValue;
		}

		private void DoDefaultRemove(int index)
		{
			var property = _valuesProperty.GetArrayElementAtIndex(index);

			// see note on PropertyListControl.DoDefaultRemove
			if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null)
				_valuesProperty.DeleteArrayElementAtIndex(index);

			_keysProperty.DeleteArrayElementAtIndex(index);
			_valuesProperty.DeleteArrayElementAtIndex(index);
		}

		public void DoDefaultDraw(Rect rect, string key, int index)
		{
			var property = _valuesProperty.GetArrayElementAtIndex(index);
			var labelRect = RectHelper.TakeWidth(ref rect, rect.width * 0.25f);

			EditorGUI.LabelField(labelRect, key);
			EditorGUI.PropertyField(rect, property, GUIContent.none);
		}

		private void PrepareForEdit()
		{
			_dictionary.PrepareForEdit();
		}

		private void ApplyEdits()
		{
			_dictionary.ApplyEdits();
		}

		private void PopupCreate()
		{
			AddEntry(_createPopup.Name);
		}

		private bool PopupValidate()
		{
			_createPopup.IsNameValid = IsEntryInUse(_createPopup.Name);
			return _createPopup.IsNameValid;
		}

		private bool IsEntryInUse(string key)
		{
			return _dictionary.Contains(key);
		}

		private void AddEntry(string key)
		{
			if (_customAdd != null)
			{
				_customAdd(_dictionary, key);
				PrepareForEdit();
			}
			else
			{
				DoDefaultAdd(key);
				ApplyEdits();
			}
		}

		private void Remove(Rect rect, int index)
		{
			var key = _keysProperty.GetArrayElementAtIndex(index).stringValue;

			if (_customRemove != null)
			{
				_customRemove(_dictionary, key);
				PrepareForEdit();
			}
			else
			{
				DoDefaultRemove(index);
				ApplyEdits();
			}
		}

		protected override void Draw(Rect rect, int index)
		{
			var key = _keysProperty.GetArrayElementAtIndex(index).stringValue;

			if (_customDraw != null)
				_customDraw(rect, _valuesProperty, index, key);
			else
				DoDefaultDraw(rect, key, index);
		}
	}
}
