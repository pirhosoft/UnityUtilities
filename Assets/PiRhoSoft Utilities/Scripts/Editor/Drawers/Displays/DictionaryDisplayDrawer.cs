using System.Reflection;
using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class DictionaryDisplayAttributeControl : PropertyControl
	{
		public const string _invalidTypeWarning = "Invalid type for DictionaryDisplay on field {0}: DictionaryDisplay can only be applied to IEditableDictionary fields";

		private static Button _addButton = new Button(Icon.BuiltIn(Icon.CustomAdd), "", "Add an item to this dictionary");
		private static Button _removeButton = new Button(Icon.BuiltIn(Icon.Remove), "", "Remove this item from the dictionary");

		private IEditableDictionary _dictionary;
		private GUIContent _label;
		private DictionaryControl _dictionaryControl = new DictionaryControl();
		private PropertyDrawer _itemDrawer = null;

		private static string GetOpenPreference(SerializedProperty property)
		{
			return property.serializedObject.targetObject.GetType().Name + "." + property.propertyPath + ".IsOpen";
		}

		public override void Setup(SerializedProperty property, FieldInfo fieldInfo, PropertyAttribute attribute)
		{
			_dictionary = PropertyHelper.GetObject<IEditableDictionary>(property);
			_itemDrawer = PropertyScopeControl.GetNextDrawer(fieldInfo, attribute);

			if (_dictionary == null)
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
			}
			else
			{
				_dictionaryControl.Setup(property, _dictionary);

				if (attribute is DictionaryDisplayAttribute display)
				{
					if (display.ItemDisplay != ListItemDisplayType.Normal)
						_dictionaryControl.MakeDrawable(display.ItemDisplay);
					else if (_itemDrawer != null)
						_dictionaryControl.MakeDrawable(DrawItem);

					if (display.AllowAdd)
						_dictionaryControl.MakeAddable(_addButton, display.AddLabel == null ? new GUIContent("Add Item") : (display.AddLabel == "" ? GUIContent.none : new GUIContent(display.AddLabel)));

					if (display.AllowRemove)
						_dictionaryControl.MakeRemovable(_removeButton);

					if (display.AllowCollapse)
						_dictionaryControl.MakeCollapsable(GetOpenPreference(property));

					if (display.EmptyText != null)
						_dictionaryControl.MakeEmptyLabel(new GUIContent(display.EmptyText));
				}
			}
		}

		public override float GetHeight(SerializedProperty property, GUIContent label)
		{
			return _dictionary != null ? _dictionaryControl.GetHeight() : EditorGUI.GetPropertyHeight(property, label);
		}

		public override void Draw(Rect position, SerializedProperty property, GUIContent label)
		{
			if (_dictionary != null)
			{
				// During drawing the text of label changes so Unity must internally pool or reuse it for each element. In
				// any case, making a copy of it fixes the problem.

				if (_label == null)
					_label = new GUIContent(label);

				_dictionaryControl.Draw(position, _label);
			}
			else
			{
				EditorGUI.PropertyField(position, property, label);
			}
		}

		private void DrawItem(Rect position, SerializedProperty property, int index, string key)
		{
			_itemDrawer.OnGUI(position, property.GetArrayElementAtIndex(index), new GUIContent(key));
		}
	}

	[CustomPropertyDrawer(typeof(DictionaryDisplayAttribute))]
	public class DictionaryDisplayAttributeDrawer : ControlDrawer<DictionaryDisplayAttributeControl>
	{
	}
}
