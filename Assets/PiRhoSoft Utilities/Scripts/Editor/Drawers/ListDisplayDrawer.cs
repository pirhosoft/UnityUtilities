using System.Reflection;
using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class ListDisplayAttributeControl : PropertyControl
	{
		private const string _invalidTypeWarning = "Invalid type for ListDisplay on field {0}: ListDisplay can only be applied to SerializedList or SerializedArray fields";

		private static IconButton _addButton = new IconButton(IconButton.Add, "Add an item to this list");
		private static IconButton _editButton = new IconButton(IconButton.Edit, "Edit this item");
		private static IconButton _removeButton = new IconButton(IconButton.Remove, "Remove this item from the list");

		private SerializedProperty _property;
		private GUIContent _label;
		private PropertyListControl _listControl = new PropertyListControl();

		private static string GetOpenPreference(SerializedProperty property)
		{
			return property.serializedObject.targetObject.GetType().Name + "." + property.propertyPath + ".IsOpen";
		}

		public override void Setup(SerializedProperty property, FieldInfo fieldInfo, PropertyAttribute attribute)
		{
			_property = property.FindPropertyRelative("_items");

			if (_property == null || !_property.isArray)
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
				_property = null;
			}
			else
			{
				_listControl.Setup(_property);

				if (attribute is ListDisplayAttribute display)
				{
					if (display.AssetType != null)
						_listControl.MakeDrawable(ListItemDisplayType.AssetPopup, display.AssetType);
					else if (display.ItemDisplay != ListItemDisplayType.Normal)
						_listControl.MakeDrawable(display.ItemDisplay, null);

					if (display.AllowAdd)
						_listControl.MakeAddable(_addButton);

					if (display.ShowEditButton)
						_listControl.MakeEditable(_editButton);

					if (display.AllowRemove)
						_listControl.MakeRemovable(_removeButton);

					if (display.AllowReorder)
						_listControl.MakeReorderable();

					if (display.AllowCollapse)
						_listControl.MakeCollapsable(GetOpenPreference(property));

					if (display.EmptyText != null)
						_listControl.MakeEmptyLabel(new GUIContent(display.EmptyText));
				}
			}
		}

		public override float GetHeight(SerializedProperty property, GUIContent label)
		{
			return _property != null ? _listControl.GetHeight() : EditorGUI.GetPropertyHeight(property, label);
		}

		public override void Draw(Rect position, SerializedProperty property, GUIContent label)
		{
			if (_property != null)
			{
				// During drawing the text of label changes so Unity must internally pool or reuse it for each element. In
				// any case, making a copy of it fixes the problem.

				if (_label == null)
					_label = new GUIContent(label);

				_listControl.Draw(position, _label);
			}
			else
			{
				EditorGUI.PropertyField(position, property, label);
			}
		}
	}

	[CustomPropertyDrawer(typeof(ListDisplayAttribute))]
	public class ListDisplayAttributeDrawer : ControlDrawer<ListDisplayAttributeControl>
	{
	}
}
