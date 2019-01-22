using System.Reflection;
using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class DictionaryDisplayAttributeControl : PropertyControl
	{
		public const string _invalidTypeWarning = "Invalid type for DictionaryDisplay on field {0}: DictionaryDisplay can only be applied to IEditableDictionary fields";

		private IEditableDictionary _dictionary;
		private DictionaryControl _dictionaryControl = new DictionaryControl();
		private GUIContent _label;

		private static BoolPreference GetOpenPreference(SerializedProperty property, bool openByDefault)
		{
			return new BoolPreference(property.serializedObject.targetObject.GetType().Name + "." + property.propertyPath + ".IsOpen", openByDefault);
		}

		public override void Setup(SerializedProperty property, FieldInfo fieldInfo)
		{
			var attribute = TypeHelper.GetAttribute<DictionaryDisplayAttribute>(fieldInfo);

			_dictionary = GetObject<IEditableDictionary>(property);

			if (_dictionary == null)
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
			}
			else
			{
				_dictionaryControl.Setup(property, _dictionary);

				if (attribute != null)
				{
					if (attribute.InlineChildren)
						_dictionaryControl.MakeDrawableInline();

					if (attribute.AllowAdd)
						_dictionaryControl.MakeAddable(IconButton.CustomAdd, string.IsNullOrEmpty(attribute.AddLabel) ? null : new Label(attribute.AddLabel));

					if (attribute.AllowRemove)
						_dictionaryControl.MakeRemovable(IconButton.Remove);

					if (attribute.AllowCollapse)
						_dictionaryControl.MakeCollapsable(GetOpenPreference(property, true));

					if (attribute.ShowEditButton)
						_dictionaryControl.MakeEditable(IconButton.Edit);

					if (attribute.EmptyText != null)
						_dictionaryControl.MakeEmptyLabel(new Label(attribute.EmptyText));
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
