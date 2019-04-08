using System.Reflection;
using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class DictionaryDisplayControl : PropertyControl
	{
		private const string _invalidTypeWarning = "(UDDCIT) Invalid type for DictionaryDisplay on field {0}: DictionaryDisplay can only be applied to IEditableDictionary fields";

		private static Label _addButton = new Label(Icon.BuiltIn(Icon.CustomAdd), string.Empty, "Add an item to this dictionary");
		private static Label _removeButton = new Label(Icon.BuiltIn(Icon.Remove), string.Empty, "Remove this item from the dictionary");

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
			_itemDrawer = PropertyHelper.GetNextDrawer(fieldInfo, attribute);

			if (_dictionary == null)
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
			}
			else
			{
				_dictionaryControl.Setup(property, _dictionary);

				if (attribute is DictionaryDisplayAttribute display)
				{
					if (_itemDrawer != null)
						_dictionaryControl.MakeDrawable(DrawItem);

					if (display.AllowAdd)
						_dictionaryControl.MakeAddable(_addButton, display.AddLabel == null ? new GUIContent("Add Item") : (display.AddLabel == string.Empty ? GUIContent.none : new GUIContent(display.AddLabel)));

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
	public class DictionaryDisplayDrawer : PropertyDrawer<DictionaryDisplayControl>
	{
	}
}
