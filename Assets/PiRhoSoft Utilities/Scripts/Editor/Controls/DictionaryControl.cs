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

		private bool _drawInline = false;
		private Action<IEditableDictionary, string> _customAdd;
		private Action<IEditableDictionary, string> _customEdit;
		private Action<IEditableDictionary, string> _customRemove;
		private Action<Rect, IEditableDictionary, string> _customDraw;

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

		public DictionaryControl MakeDrawableInline()
		{
			MakeCustomHeight(GetItemInlineHeight);
			_drawInline = true;
			return this;
		}

		public DictionaryControl MakeDrawable(Action<Rect, IEditableDictionary, string> callback)
		{
			_customDraw = callback;
			return this;
		}

		public DictionaryControl MakeAddable(IconButton icon, GUIContent label, Action<IEditableDictionary, string> callback = null)
		{
			MakeHeaderButton(icon, new AddPopup(new AddItemContent(this), label));
			_customAdd = callback;
			return this;
		}

		public DictionaryControl MakeRemovable(IconButton icon, Action<IEditableDictionary, string> callback = null)
		{
			MakeItemButton(icon, Remove);
			_customRemove = callback;
			return this;
		}

		public DictionaryControl MakeEditable(IconButton icon, Action<IEditableDictionary, string> callback = null)
		{
			MakeItemButton(icon, Edit);
			_customEdit = callback;
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

		private float GetItemInlineHeight(int index)
		{
			var property = _valuesProperty.GetArrayElementAtIndex(index);
			return InlineDisplayDrawer.GetHeight(property);
		}

		public void DoDefaultDraw(Rect rect, string key, int index)
		{
			var labelRect = RectHelper.TakeWidth(ref rect, rect.width * 0.25f);
			var value = _valuesProperty.GetArrayElementAtIndex(index);

			EditorGUI.LabelField(labelRect, key);

			if (_drawInline)
				InlineDisplayDrawer.Draw(rect, value, null);
			else
				EditorGUI.PropertyField(rect, value, GUIContent.none);
		}

		private void PrepareForEdit()
		{
			_dictionary.PrepareForEdit();
		}

		private void ApplyEdits()
		{
			_dictionary.ApplyEdits();
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

		private void Edit(Rect rect, int index)
		{
			var key = _keysProperty.GetArrayElementAtIndex(index).stringValue;
			var value = _valuesProperty.GetArrayElementAtIndex(index);

			if (_customEdit != null)
				_customEdit(_dictionary, key);
			else
				DoDefaultEdit(index);
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
				_customDraw(rect, _dictionary, key);
			else
				DoDefaultDraw(rect, key, index);
		}

		private class AddItemContent : AddNamedItemContent
		{
			private DictionaryControl _control;

			public AddItemContent(DictionaryControl control)
			{
				_control = control;
			}

			protected override void Add_(string name)
			{
				_control.AddEntry(name);
			}

			protected override bool IsNameInUse(string name)
			{
				return _control.IsEntryInUse(name);
			}
		}
	}
}
