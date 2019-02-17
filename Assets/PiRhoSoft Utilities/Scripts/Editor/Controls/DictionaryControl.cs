using System;
using System.Collections.Generic;
using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class DictionaryControl : ListControl
	{
		private static readonly IconButton _collapseButton = new IconButton(IconButton.Expanded, "Collapse this entry's fields");
		private static readonly IconButton _expandButton = new IconButton(IconButton.Collapsed, "Expand this entry's fields");

		private SerializedProperty _rootProperty;
		private SerializedProperty _keysProperty;
		private SerializedProperty _valuesProperty;
		private IEditableDictionary _dictionary;
		
		private Action<IEditableDictionary, string> _customAdd;
		private Action<IEditableDictionary, string> _customEdit;
		private Action<IEditableDictionary, string> _customRemove;
		private Action<Rect, IEditableDictionary, string> _customDraw;

		private ListItemDisplayType _itemDisplay = ListItemDisplayType.Normal;
		private List<bool> _isExpanded = new List<bool>();
		private Type _assetPopupType;

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

		public DictionaryControl MakeDrawable(ListItemDisplayType itemDisplay, Type assetPopupType = null)
		{
			MakeCustomHeight(GetItemHeight);

			_itemDisplay = itemDisplay;
			_assetPopupType = assetPopupType;

			return this;
		}

		public DictionaryControl MakeDrawable(Action<Rect, IEditableDictionary, string> callback)
		{
			_customDraw = callback;
			return this;
		}

		public DictionaryControl MakeAddable(IconButton icon, GUIContent label, Action<IEditableDictionary, string> callback = null)
		{
			MakeHeaderButton(icon, new AddPopup(new AddItemContent(this), label), Color.white);
			_customAdd = callback;
			return this;
		}

		public DictionaryControl MakeRemovable(IconButton icon, Action<IEditableDictionary, string> callback = null)
		{
			MakeItemButton(icon, Remove, Color.white);
			_customRemove = callback;
			return this;
		}

		public DictionaryControl MakeEditable(IconButton icon, Action<IEditableDictionary, string> callback = null)
		{
			MakeItemButton(icon, Edit, Color.white);
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

		public float GetItemHeight(int index)
		{
			var property = _valuesProperty.GetArrayElementAtIndex(index);

			switch (_itemDisplay)
			{
				case ListItemDisplayType.Normal:
				{
					return EditorGUI.GetPropertyHeight(property);
				}
				case ListItemDisplayType.Inline:
				{
					using (new EditorGUI.IndentLevelScope(1))
						return RectHelper.LineHeight + InlineDisplayDrawer.GetHeight(property);
				}
				case ListItemDisplayType.Foldout:
				{
					using (new EditorGUI.IndentLevelScope(1))
					{
						var expanded = IsExpanded(index);
						return expanded ? RectHelper.LineHeight + InlineDisplayDrawer.GetHeight(property) : EditorGUIUtility.singleLineHeight;
					}
				}
				case ListItemDisplayType.AssetPopup:
				{
					return AssetPopupDrawer.GetHeight();
				}
			}

			return 0.0f;
		}

		public void DoDefaultDraw(Rect rect, string key, int index)
		{
			var property = _valuesProperty.GetArrayElementAtIndex(index);

			switch (_itemDisplay)
			{
				case ListItemDisplayType.Normal:
				{
					var labelRect = RectHelper.TakeWidth(ref rect, rect.width * 0.25f);

					EditorGUI.LabelField(labelRect, key);
					EditorGUI.PropertyField(rect, property, GUIContent.none);

					break;
				}
				case ListItemDisplayType.Inline:
				{
					var labelRect = RectHelper.TakeLine(ref rect);

					EditorGUI.LabelField(labelRect, key, EditorStyles.boldLabel);
					
					using (new EditorGUI.IndentLevelScope(1))
						InlineDisplayDrawer.Draw(rect, property, null);

					break;
				}
				case ListItemDisplayType.Foldout:
				{
					var expanded = IsExpanded(index);
					var labelRect = expanded ? RectHelper.TakeLine(ref rect) : RectHelper.TakeWidth(ref rect, rect.width * 0.25f);
					var foldoutRect = RectHelper.TakeLeadingIcon(ref labelRect);

					using (ColorScope.Color(new Color(0.3f, 0.3f, 0.3f)))
					{
						if (GUI.Button(foldoutRect, expanded ? _collapseButton.Content : _expandButton.Content, GUIStyle.none))
							SetExpanded(index, !expanded);
					}

					EditorGUI.LabelField(labelRect, key, EditorStyles.boldLabel);

					if (expanded)
					{
						using (new EditorGUI.IndentLevelScope(1))
							InlineDisplayDrawer.Draw(rect, property, null);
					}

					break;
				}
				case ListItemDisplayType.AssetPopup:
				{
					var labelRect = RectHelper.TakeWidth(ref rect, rect.width * 0.25f);

					EditorGUI.LabelField(labelRect, key);
					AssetPopupDrawer.Draw(rect, GUIContent.none, property, _assetPopupType, true, false, true);

					break;
				}
			}
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

		private bool IsExpanded(int index)
		{
			return index < _isExpanded.Count ? _isExpanded[index] : false;
		}

		private void SetExpanded(int index, bool isExpanded)
		{
			while (_isExpanded.Count <= index)
				_isExpanded.Add(false);

			_isExpanded[index] = isExpanded;
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
