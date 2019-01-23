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

		public static AssetType Draw<AssetType>(GUIContent label, AssetType asset, bool showEditButton) where AssetType : ScriptableObject
		{
			var height = GetHeight();
			var rect = EditorGUILayout.GetControlRect(false, height);

			return Draw(rect, label, asset, showEditButton);
		}

		public static AssetType Draw<AssetType>(Rect position, GUIContent label, AssetType asset, bool showEditButton) where AssetType : ScriptableObject
		{
			return Draw(position, label, asset, typeof(AssetType), showEditButton) as AssetType;
		}

		public static void Draw(GUIContent label, SerializedProperty property, Type type, bool showEditButton)
		{
			var height = GetHeight();
			var rect = EditorGUILayout.GetControlRect(false, height);

			Draw(rect, label, property, type, showEditButton);
		}

		public static void Draw(Rect position, GUIContent label, SerializedProperty property, Type type, bool showEditButton)
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

		#endregion

		#region Drawer Interface

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return GetHeight();
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			Draw(position, label, property, fieldInfo.FieldType, (attribute as AssetPopupAttribute).ShowEdit);
		}

		#endregion

		#region Drawing

		private static ScriptableObject Draw(Rect position, GUIContent label, ScriptableObject asset, Type type, bool showEditButton)
		{
			if (showEditButton)
			{
				var editRect = RectHelper.TakeTrailingIcon(ref position);

				if (asset != null)
				{
					if (GUI.Button(editRect, IconButton.Edit.Content, GUIStyle.none))
						Selection.activeObject = asset;
				}
			}

			var list = AssetHelper.GetAssetList(type, true, false);
			var index = list.GetIndex(asset);

			index = EditorGUI.Popup(position, label, index, list.Names);

			return list.GetAsset(index);
		}

		#endregion
	}
}
