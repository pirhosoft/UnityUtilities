using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public static class GuiHelper
	{
		public static string GetTooltip(Type type, string propertyName)
		{
			var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			var field = type.GetField(propertyName, flags);

			return GetTooltip(field);
		}

		public static string GetTooltip(FieldInfo field)
		{
			return field?.GetCustomAttribute<TooltipAttribute>()?.tooltip ?? "";
		}

		public static void Space()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight);
		}

		public static void HalfSpace()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);
		}

		public static void Box(Rect box, Color color)
		{
			using (ColorScope.Color(color))
				GUI.Box(box, GUIContent.none, TextureStyle.Box.Style);
		}

		public static bool IconButton(IconButton button)
		{
			return GUILayout.Button(button.Content, GUIStyle.none);
		}

		public static bool IconButton(Rect rect, IconButton button)
		{
			return GUI.Button(rect, button.Content, GUIStyle.none);
		}

		public static void AssetPopup(GUIContent label, Type type, ref ScriptableObject asset, GUIStyle style = null, params GUILayoutOption[] options)
		{
			var list = AssetHelper.GetAssetList(type, true, false);
			AssetPopup(list, label, ref asset, style, options);
		}

		public static void AssetPopup(Rect rect, GUIContent label, Type type, ref ScriptableObject asset, GUIStyle style = null)
		{
			var list = AssetHelper.GetAssetList(type, true, false);
			AssetPopup(list, rect, label, ref asset, style);
		}

		public static void AssetPopup<AssetType>(GUIContent label, ref AssetType asset, GUIStyle style = null, params GUILayoutOption[] options) where AssetType : ScriptableObject
		{
			var list = AssetHelper.GetAssetList<AssetType>(true, false);
			AssetPopup(list, label, ref asset, style, options);
		}

		public static void AssetPopup<AssetType>(Rect rect, GUIContent label, ref AssetType asset, GUIStyle style = null) where AssetType : ScriptableObject
		{
			var list = AssetHelper.GetAssetList<AssetType>(true, false);
			AssetPopup(list, rect, label, ref asset, style);
		}

		private static void AssetPopup<AssetType>(AssetList list, GUIContent label, ref AssetType asset, GUIStyle style, params GUILayoutOption[] options) where AssetType : ScriptableObject
		{
			var index = list.GetIndex(asset);
			index = EditorGUILayout.Popup(label, index, list.Names, style ?? EditorStyles.popup, options);
			asset = list.GetAsset(index) as AssetType;
		}

		private static void AssetPopup<AssetType>(AssetList list, Rect rect, GUIContent label, ref AssetType asset, GUIStyle style) where AssetType : ScriptableObject
		{
			var index = list.GetIndex(asset);
			index = EditorGUI.Popup(rect, label, index, list.Names, style ?? EditorStyles.popup);
			asset = list.GetAsset(index) as AssetType;
		}

		private static ListedType[] _noneTypeList = new ListedType[] { new ListedType("None"), new ListedType("") };
		private static ListedType[] _gameObjectTypeList = new ListedType[] { new ListedType(typeof(GameObject)), new ListedType("") };

		public static void DerivedTypePopup<BaseType>(GUIContent label, ref Type type, GUIStyle style = null, params GUILayoutOption[] options)
		{
			var list = TypeHelper.GetTypeList<BaseType>(_noneTypeList);
			var index = ArrayUtility.IndexOf(list.Types, type);
			index = EditorGUILayout.Popup(label, index, list.Names, style ?? EditorStyles.popup, options);

			if (index >= 0)
				type = list.Types[index];
		}

		public static void DerivedTypePopup<BaseType>(Rect rect, GUIContent label, ref Type type, GUIStyle style = null)
		{
			var list = TypeHelper.GetTypeList<BaseType>(_noneTypeList);
			var index = ArrayUtility.IndexOf(list.Types, type);
			index = EditorGUI.Popup(rect, label, index, list.Names, style ?? EditorStyles.popup);

			if (index >= 0)
				type = list.Types[index];
		}

		public static void GameObjectTypePopup(Rect rect, GUIContent label, ref Type type, GUIStyle style = null)
		{
			var list = TypeHelper.GetTypeList<MonoBehaviour>(_gameObjectTypeList);
			var index = ArrayUtility.IndexOf(list.Types, type);
			index = EditorGUI.Popup(rect, label, index, list.Names, style ?? EditorStyles.popup);

			if (index >= 0)
				type = list.Types[index];
		}

		public static bool TextEnterField(string controlName, GUIContent label, ref string text, GUIStyle style = null, params GUILayoutOption[] options)
		{
			GUI.SetNextControlName(controlName);
			text = EditorGUILayout.TextField(label, text, style ?? EditorStyles.textField, options);
			return WasEnterPressed(controlName);
		}

		public static bool FloatEnterField(string controlName, GUIContent label, ref float value, GUIStyle style = null, params GUILayoutOption[] options)
		{
			GUI.SetNextControlName(controlName);
			value = EditorGUILayout.FloatField(label, value, style ?? EditorStyles.numberField, options);
			return WasEnterPressed(controlName);
		}

		public static bool IntEnterField(string controlName, GUIContent label, ref int value, GUIStyle style = null, params GUILayoutOption[] options)
		{
			GUI.SetNextControlName(controlName);
			value = EditorGUILayout.IntField(label, value, style ?? EditorStyles.numberField, options);
			return WasEnterPressed(controlName);
		}

		private static bool WasEnterPressed(string controlName)
		{
			return Event.current.type == EventType.KeyUp && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return) && GUI.GetNameOfFocusedControl() == controlName;
		}
	}
}
