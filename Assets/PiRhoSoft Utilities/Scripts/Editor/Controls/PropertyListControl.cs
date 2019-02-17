using System;
using System.Collections.Generic;
using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class PropertyListControl : ListControl
	{
		private static readonly IconButton _collapseButton = new IconButton(IconButton.Expanded, "Collapse this item's fields");
		private static readonly IconButton _expandButton = new IconButton(IconButton.Collapsed, "Expand this item's fields");

		private Action<SerializedProperty> _customAdd;
		private Action<SerializedProperty, int> _customEdit;
		private Action<SerializedProperty, int> _customRemove;
		private Action<Rect, SerializedProperty, int> _customDraw;

		private ListItemDisplayType _itemDisplay = ListItemDisplayType.Normal;
		private List<bool> _isExpanded = new List<bool>();
		private Type _assetPopupType;

		public PropertyListControl Setup(SerializedProperty property)
		{
			var reorderableList = new ReorderableList(property.serializedObject, property, false, true, false, false);
			Setup(reorderableList);
			return this;
		}

		public PropertyListControl MakeDrawable(ListItemDisplayType itemDisplay, Type assetPopupType = null)
		{
			MakeCustomHeight(GetItemHeight);

			_itemDisplay = itemDisplay;
			_assetPopupType = assetPopupType;

			return this;
		}

		public PropertyListControl MakeDrawable(Action<Rect, SerializedProperty, int> callback)
		{
			_customDraw = callback;
			return this;
		}

		public PropertyListControl MakeAddable(IconButton icon, Action<SerializedProperty> callback = null)
		{
			MakeHeaderButton(icon, Add, Color.white);
			_customAdd = callback;
			return this;
		}

		public PropertyListControl MakeRemovable(IconButton icon, Action<SerializedProperty, int> callback = null)
		{
			MakeItemButton(icon, Remove, Color.white);
			_customRemove = callback;
			return this;
		}

		public PropertyListControl MakeEditable(IconButton icon, Action<SerializedProperty, int> callback = null)
		{
			MakeItemButton(icon, Edit, Color.white);
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

		public float GetItemHeight(int index)
		{
			var property = List.serializedProperty.GetArrayElementAtIndex(index);

			switch (_itemDisplay)
			{
				case ListItemDisplayType.Normal:
				{
					return EditorGUI.GetPropertyHeight(property);
				}
				case ListItemDisplayType.Inline:
				{
					return InlineDisplayDrawer.GetHeight(property);
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

		public void DoDefaultDraw(Rect rect, int index)
		{
			var property = List.serializedProperty.GetArrayElementAtIndex(index);

			switch (_itemDisplay)
			{
				case ListItemDisplayType.Normal:
				{
					EditorGUI.PropertyField(rect, property, GUIContent.none);
					break;
				}
				case ListItemDisplayType.Inline:
				{
					InlineDisplayDrawer.Draw(rect, property, null);
					break;
				}
				case ListItemDisplayType.Foldout:
				{
					var expanded = IsExpanded(index);
					var labelRect = expanded ? RectHelper.TakeLine(ref rect) : rect;
					var foldoutRect = RectHelper.TakeLeadingIcon(ref labelRect);

					using (ColorScope.Color(new Color(0.3f, 0.3f, 0.3f)))
					{
						if (GUI.Button(foldoutRect, expanded ? _collapseButton.Content : _expandButton.Content, GUIStyle.none))
							SetExpanded(index, !expanded);
					}

					EditorGUI.LabelField(labelRect, "Item " + index, EditorStyles.boldLabel);

					if (expanded)
					{
						using (new EditorGUI.IndentLevelScope(1))
							InlineDisplayDrawer.Draw(rect, property, null);
					}

					break;
				}
				case ListItemDisplayType.AssetPopup:
				{
					AssetPopupDrawer.Draw(rect, GUIContent.none, property, _assetPopupType, true, false, true);
					break;
				}
			}
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
			RemoveExpanded(index);

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

		protected override void ElementsMoved(ReorderableList list, int oldIndex, int newIndex)
		{
			base.ElementsMoved(list, oldIndex, newIndex);

			var expanded = IsExpanded(oldIndex);

			RemoveExpanded(oldIndex);
			AddExpanded(newIndex, expanded);
		}

		private void AddExpanded(int index, bool isExpanded)
		{
			EnsureExpandedCount(index);
			_isExpanded.Insert(index, isExpanded);
		}

		private void RemoveExpanded(int index)
		{
			if (index < _isExpanded.Count)
				_isExpanded.RemoveAt(index);
		}

		private void SetExpanded(int index, bool isExpanded)
		{
			EnsureExpandedCount(index);
			_isExpanded[index] = isExpanded;
		}

		private bool IsExpanded(int index)
		{
			EnsureExpandedCount(index);
			return _isExpanded[index];
		}

		private void EnsureExpandedCount(int count)
		{
			while (_isExpanded.Count <= count)
				_isExpanded.Add(false);
		}
	}
}
