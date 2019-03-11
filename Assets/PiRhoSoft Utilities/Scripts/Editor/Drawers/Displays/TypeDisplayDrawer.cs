using System;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public static class TypePopupDrawer
	{
		public static float GetHeight()
		{
			return EditorGUIUtility.singleLineHeight;
		}
		
		public static Type Draw<BaseType>(GUIContent label, Type type)
		{
			var height = GetHeight();
			var rect = EditorGUILayout.GetControlRect(false, height);

			return Draw<BaseType>(rect, label, type);
		}

		public static Type Draw<BaseType>(Rect position, GUIContent label, Type type)
		{
			var list = TypeHelper.GetTypeList<BaseType>(true);
			var index = list.GetIndex(type);

			index = EditorGUI.Popup(position, label, index, list.Names);

			return list.GetType(index);
		}
	}
}
