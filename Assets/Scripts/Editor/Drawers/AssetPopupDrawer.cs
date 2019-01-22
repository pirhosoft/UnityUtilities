using System;
using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[CustomPropertyDrawer(typeof(AssetPopupAttribute))]
	public class AssetPopupDrawer : PropertyDrawer
	{
		private const string _invalidTypeWarning = "Invalid type for AssetPopup on field {0}: AssetPopup can only be applied to ScriptableObject fields";

		#region Static Interface

		public static float GetHeight()
		{
			return EditorGUIUtility.singleLineHeight;
		}

		public static T Draw<T>(GUIContent label, T asset, bool showEditButton) where T : ScriptableObject
		{
			var rect = EditorGUILayout.GetControlRect(false, GetHeight());
			return Draw(rect, label, asset, showEditButton);
		}

		public static T Draw<T>(Rect position, GUIContent label, T asset, bool showEditButton) where T : ScriptableObject
		{
			return Draw(position, label, asset, typeof(T), showEditButton) as T;
		}

		public static void Draw(GUIContent label, SerializedProperty property, Type type, bool showEditButton)
		{
			var rect = EditorGUILayout.GetControlRect(false, GetHeight());
			Draw(rect, property, label, type, showEditButton);
		}

		public static void Draw(Rect position, SerializedProperty property, GUIContent label, Type type, bool showEditButton)
		{
			if (property.propertyType == SerializedPropertyType.ObjectReference && typeof(ScriptableObject).IsAssignableFrom(type))
			{
				property.objectReferenceValue = Draw(position, label, property.objectReferenceValue as ScriptableObject, type, showEditButton);
			}
			else
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
				EditorGUI.PropertyField(position, property, label);
			}
		}

		private static ScriptableObject Draw(Rect position, GUIContent label, ScriptableObject asset, Type type, bool showEditButton)
		{
			if (showEditButton && asset != null)
			{
				var editRect = RectHelper.TakeTrailingIcon(ref position);

				if (GuiHelper.IconButton(editRect, IconButton.Edit))
					Selection.activeObject = asset;
			}

			GuiHelper.AssetPopup(position, label, type, ref asset);
			return asset;
		}

		#endregion

		#region Virtual Interface

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return GetHeight();
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			Draw(position, property, label, fieldInfo.FieldType, (attribute as AssetPopupAttribute).ShowEdit);
		}

		#endregion
	}
}
