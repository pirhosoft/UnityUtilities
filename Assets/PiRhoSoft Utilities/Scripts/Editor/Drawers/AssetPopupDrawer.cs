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

		private static readonly IconButton _editButton = new IconButton(IconButton.Edit, "Edit this object");

		#region Static Interface

		public static float GetHeight()
		{
			return EditorGUIUtility.singleLineHeight;
		}

		public static AssetType Draw<AssetType>(GUIContent label, AssetType asset, bool showNoneOption, bool showEditButton, bool showCreateOptions) where AssetType : ScriptableObject
		{
			var height = GetHeight();
			var rect = EditorGUILayout.GetControlRect(false, height);

			return Draw(rect, label, asset, showNoneOption, showEditButton, showCreateOptions);
		}

		public static AssetType Draw<AssetType>(Rect position, GUIContent label, AssetType asset, bool showNoneOption, bool showEditButton, bool showCreateOptions) where AssetType : ScriptableObject
		{
			return Draw(position, label, asset, typeof(AssetType), showNoneOption, showEditButton, showCreateOptions) as AssetType;
		}

		public static void Draw(GUIContent label, SerializedProperty property, Type type, bool showNoneOption, bool showEditButton, bool showCreateOptions)
		{
			var height = GetHeight();
			var rect = EditorGUILayout.GetControlRect(false, height);

			Draw(rect, label, property, type, showNoneOption, showEditButton, showCreateOptions);
		}

		public static void Draw(Rect position, GUIContent label, SerializedProperty property, Type type, bool showNoneOption, bool showEditButton, bool showCreateOptions)
		{
			if (property.propertyType == SerializedPropertyType.ObjectReference && typeof(ScriptableObject).IsAssignableFrom(type))
			{
				property.objectReferenceValue = Draw(position, label, property.objectReferenceValue as ScriptableObject, type, showNoneOption, showEditButton, showCreateOptions);
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
			var assetPopup = attribute as AssetPopupAttribute;

			Draw(position, label, property, fieldInfo.FieldType, assetPopup.ShowNone, assetPopup.ShowEdit, assetPopup.ShowCreate);
		}

		#endregion

		#region Drawing

		private static ScriptableObject Draw(Rect position, GUIContent label, ScriptableObject asset, Type type, bool showNoneOption, bool showEditButton, bool showCreateOptions)
		{
			if (showEditButton)
			{
				var editRect = RectHelper.TakeTrailingIcon(ref position);

				if (asset != null)
				{
					if (GUI.Button(editRect, _editButton.Content, GUIStyle.none))
						Selection.activeObject = asset;
				}
			}

			var list = AssetHelper.GetAssetList(type, showNoneOption, showCreateOptions);
			var index = list.GetIndex(asset);

			index = EditorGUI.Popup(position, label, index, list.Names);

			asset = list.GetAsset(index);

			if (asset == null)
			{
				var createType = list.GetType(index);

				if (createType != null)
					asset = AssetHelper.CreateAsset(createType.Name, createType);
			}

			return asset;
		}

		#endregion
	}
}
