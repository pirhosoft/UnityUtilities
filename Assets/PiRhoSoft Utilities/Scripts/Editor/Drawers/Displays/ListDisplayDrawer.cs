using System.Reflection;
using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class ListDisplayControl : PropertyControl
	{
		private const string _invalidTypeWarning = "(ULDCIT) Invalid type for ListDisplay on field {0}: ListDisplay can only be applied to SerializedList or SerializedArray fields";

		private static Label _addButton = new Label(Icon.BuiltIn(Icon.Add), string.Empty, "Add an item to this list");
		private static Label _removeButton = new Label(Icon.BuiltIn(Icon.Remove), string.Empty, "Remove this item from the list");

		private SerializedProperty _property;
		private GUIContent _label;
		private PropertyListControl _listControl = new PropertyListControl();
		private PropertyDrawer _itemDrawer = null;

		private static string GetOpenPreference(SerializedProperty property)
		{
			return property.serializedObject.targetObject.GetType().Name + "." + property.propertyPath + ".IsOpen";
		}

		public override void Setup(SerializedProperty property, FieldInfo fieldInfo, PropertyAttribute attribute)
		{
			_property = property.FindPropertyRelative("_items");
			_itemDrawer = PropertyHelper.GetNextDrawer(fieldInfo, attribute);

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
					if (_itemDrawer != null)
						_listControl.MakeDrawable(DrawItem);

					if (display.AllowAdd)
						_listControl.MakeAddable(_addButton);

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

		private void DrawItem(Rect position, SerializedProperty property, int index)
		{
			_itemDrawer.OnGUI(position, property.GetArrayElementAtIndex(index), GUIContent.none);
		}
	}

	[CustomPropertyDrawer(typeof(ListDisplayAttribute))]
	public class ListDisplayDrawer : PropertyDrawer<ListDisplayControl>
	{
	}
}
