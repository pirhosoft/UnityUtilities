using System.Reflection;
using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class DictionaryDisplayAttributeControl : PropertyControl
	{
		public const string _invalidTypeWarning = "Invalid type for DictionaryDisplay on field {0}: DictionaryDisplay can only be applied to IEditableDictionary fields";

		private static IconButton _addButton = new IconButton(IconButton.CustomAdd, "Add an item to this dictionary");
		private static IconButton _editButton = new IconButton(IconButton.Edit, "Edit this item");
		private static IconButton _removeButton = new IconButton(IconButton.Remove, "Remove this item from the dictionary");

		private IEditableDictionary _dictionary;
		private DictionaryControl _dictionaryControl = new DictionaryControl();
		private GUIContent _label;

		private static string GetOpenPreference(SerializedProperty property)
		{
			return property.serializedObject.targetObject.GetType().Name + "." + property.propertyPath + ".IsOpen";
		}

		public override void Setup(SerializedProperty property, FieldInfo fieldInfo)
		{
			var attribute = TypeHelper.GetAttribute<DictionaryDisplayAttribute>(fieldInfo);

			_dictionary = PropertyHelper.GetObject<IEditableDictionary>(property);

			if (_dictionary == null)
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
			}
			else
			{
				_dictionaryControl.Setup(property, _dictionary);

				if (attribute != null)
				{
					if (attribute.AssetType != null)
						_dictionaryControl.MakeDrawable(ListItemDisplayType.AssetPopup, attribute.AssetType);
					else if (attribute.ItemDisplay != ListItemDisplayType.Normal)
						_dictionaryControl.MakeDrawable(attribute.ItemDisplay, null);

					if (attribute.AllowAdd)
						_dictionaryControl.MakeAddable(_addButton, attribute.AddLabel == null ? new GUIContent("Add Item") : (attribute.AddLabel == "" ? GUIContent.none : new GUIContent(attribute.AddLabel)));

					if (attribute.AllowRemove)
						_dictionaryControl.MakeRemovable(_removeButton);

					if (attribute.AllowCollapse)
						_dictionaryControl.MakeCollapsable(GetOpenPreference(property));

					if (attribute.ShowEditButton)
						_dictionaryControl.MakeEditable(_editButton);

					if (attribute.EmptyText != null)
						_dictionaryControl.MakeEmptyLabel(new GUIContent(attribute.EmptyText));
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
	}

	[CustomPropertyDrawer(typeof(DictionaryDisplayAttribute))]
	public class DictionaryDisplayAttributeDrawer : ControlDrawer<DictionaryDisplayAttributeControl>
	{
	}
}
