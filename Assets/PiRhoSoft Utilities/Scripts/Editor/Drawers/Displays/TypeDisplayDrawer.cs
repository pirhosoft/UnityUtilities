using PiRhoSoft.UtilityEngine;
using System;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[CustomPropertyDrawer(typeof(TypeDisplayAttribute))]
	public class TypeDisplayDrawer : PropertyDrawer
	{
		private static readonly Icon _defaultTypeIcon = Icon.BuiltIn("cs Script Icon");
		private const string _invalidTypeWarning = "(UASDDIT) Invalid type for TypeDisplay of field {0}: TypeDisplay can only be used with string fields";

		#region Static Property Interface

		public static float GetHeight(GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight;
		}

		public static void Draw(GUIContent label, SerializedProperty property, Type rootType, bool showNoneOption, bool showAbstractOptions)
		{
			var height = GetHeight(label);
			var rect = EditorGUILayout.GetControlRect(false, height);

			Draw(rect, label, property, rootType, showNoneOption, showAbstractOptions);
		}

		public static void Draw(Rect position, GUIContent label, SerializedProperty property, Type rootType, bool showNoneOption, bool showAbstractOptions)
		{
			if (property.propertyType == SerializedPropertyType.String)
			{
				var type = Type.GetType(property.stringValue, false);
				type = Draw(position, label, type, rootType, showNoneOption, showAbstractOptions);
				property.stringValue = type != null ? type.AssemblyQualifiedName : string.Empty;
			}
			else
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
				EditorGUI.PropertyField(position, property, label);
			}
		}

		#endregion

		#region Static Generic Interface

		public static Type Draw<RootType>(GUIContent label, Type type, bool showNoneOption, bool showAbstractOptions)
		{
			return Draw(label, type, typeof(RootType), showNoneOption, showAbstractOptions);
		}

		public static Type Draw<RootType>(Rect position, GUIContent label, Type type, bool showNoneOption, bool showAbstractOptions)
		{
			return Draw(position, label, type, typeof(RootType), showNoneOption, showAbstractOptions);
		}

		#endregion

		#region Static Type Interface

		public static Type Draw(GUIContent label, Type type, Type rootType, bool showNoneOption, bool showAbstractOptions)
		{
			var height = GetHeight(label);
			var rect = EditorGUILayout.GetControlRect(false, height);

			return Draw(rect, label, type, rootType, showNoneOption, showAbstractOptions);
		}

		public static Type Draw(Rect position, GUIContent label, Type type, Type rootType, bool showNoneOption, bool showAbstractOptions)
		{
			var rect = EditorGUI.PrefixLabel(position, label);
			var list = TypeHelper.GetTypeList(rootType, showNoneOption, showAbstractOptions);
			var index = list.GetIndex(type);
			var thumbnail = type != null ? (AssetPreview.GetMiniTypeThumbnail(type) ?? _defaultTypeIcon.Content) : null;
			var popupLabel = type != null ? new GUIContent(type.Name, thumbnail) : new GUIContent("None");

			var selection = SelectionPopup.Draw(rect, popupLabel, new SelectionState { Tab = 0, Index = index }, list.Tree);
			return list.GetType(selection.Index);
		}

		#endregion

		#region Drawer Interface

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return GetHeight(label);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var typeDisplay = attribute as TypeDisplayAttribute;

			Draw(position, label, property, typeDisplay.RootType, typeDisplay.ShowNoneOption, typeDisplay.ShowAbstractOptions);
		}

		#endregion
	}
}
