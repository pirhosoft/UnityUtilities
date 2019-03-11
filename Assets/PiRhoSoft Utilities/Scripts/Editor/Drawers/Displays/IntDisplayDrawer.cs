using PiRhoSoft.UtilityEngine;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[CustomPropertyDrawer(typeof(IntDisplayAttribute))]
	class IntDisplayDrawer : PropertyDrawer
	{
		private const string _invalidTypeWarning = "(UFDDIT) Invalid type for IntDisplay on field {0}: IntDisplay can only be used with int fields";
		private const string _invalidMaximumWarning = "(UFDDIM) Invalid property for MinMaxSlider IntDisplay on field {0}: the sibling field {1} could not be found";

		#region Static Property Interface

		public static float GetHeight(GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight;
		}

		public static void DrawPopup(GUIContent label, SerializedProperty property, GUIContent[] names, int[] values)
		{
			var height = GetHeight(label);
			var rect = EditorGUILayout.GetControlRect(false, height);

			DrawPopup(rect, label, property, names, values);
		}

		public static void DrawSlider(GUIContent label, SerializedProperty property, int minimum, int maximum, int snap)
		{
			var height = GetHeight(label);
			var rect = EditorGUILayout.GetControlRect(false, height);

			DrawSlider(rect, label, property, minimum, maximum, snap);
		}

		public static void DrawMinMaxSlider(GUIContent label, SerializedProperty minimumProperty, SerializedProperty maximumProperty, int minimum, int maximum, int snap)
		{
			var height = GetHeight(label);
			var rect = EditorGUILayout.GetControlRect(false, height);

			DrawMinMaxSlider(rect, label, minimumProperty, maximumProperty, minimum, maximum, snap);
		}

		public static void DrawPopup(Rect position, GUIContent label, SerializedProperty property, GUIContent[] names, int[] values)
		{
			if (property.propertyType == SerializedPropertyType.Integer)
			{
				property.intValue = DrawPopup(position, label, property.intValue, names, values);
			}
			else
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
				EditorGUI.PropertyField(position, property, label);
			}
		}

		public static void DrawSlider(Rect position, GUIContent label, SerializedProperty property, int minimum, int maximum, int snap)
		{
			if (property.propertyType == SerializedPropertyType.Integer)
			{
				property.intValue = DrawSlider(position, label, property.intValue, minimum, maximum, snap);
			}
			else
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
				EditorGUI.PropertyField(position, property, label);
			}
		}

		public static void DrawMinMaxSlider(Rect position, GUIContent label, SerializedProperty minimumProperty, SerializedProperty maximumProperty, int minimum, int maximum, int snap)
		{
			if (minimumProperty.propertyType != SerializedPropertyType.Integer)
			{
				Debug.LogWarningFormat(_invalidTypeWarning, minimumProperty.propertyPath);
				EditorGUI.PropertyField(position, minimumProperty, label);
			}
			else if (maximumProperty.propertyType != SerializedPropertyType.Integer)
			{
				Debug.LogWarningFormat(_invalidTypeWarning, maximumProperty.propertyPath);
				EditorGUI.PropertyField(position, maximumProperty, label);
			}
			else
			{
				var minimumValue = minimumProperty.intValue;
				var maximumValue = maximumProperty.intValue;

				DrawMinMaxSlider(position, label, ref minimumValue, ref maximumValue, minimum, maximum, snap);

				minimumProperty.intValue = minimumValue;
				maximumProperty.intValue = maximumValue;
			}
		}

		#endregion

		#region Static Object Interface

		public static int DrawPopup(GUIContent label, int value, GUIContent[] names, int[] values)
		{
			var height = GetHeight(label);
			var rect = EditorGUILayout.GetControlRect(false, height);

			return DrawPopup(rect, label, value, names, values);
		}

		public static int DrawSlider(GUIContent label, int value, int minimum, int maximum, int snap)
		{
			var height = GetHeight(label);
			var rect = EditorGUILayout.GetControlRect(false, height);

			return DrawSlider(rect, label, value, minimum, maximum, snap);
		}

		public static void DrawMinMaxSlider(GUIContent label, ref int minimumValue, ref int maximumValue, int minimum, int maximum, int snap)
		{
			var height = GetHeight(label);
			var rect = EditorGUILayout.GetControlRect(false, height);

			DrawMinMaxSlider(rect, label, ref minimumValue, ref maximumValue, minimumValue, maximumValue, snap);
		}

		public static int DrawPopup(Rect position, GUIContent label, int value, GUIContent[] names, int[] values)
		{
			// After selection in the editor the correct index will always be found with an equality check, but after
			// serialization or after changing the value from code the int representation might not be exactly
			// equivalent. And because the scale of values isn't known, a tolerance doesn't really work either, so
			// the index is found based on the closest value in the list. Not ideal if the value changes to something
			// not even close, but if that happens, a popup probably isn't the best option.

			var index = -1;
			var distance = int.MaxValue;

			for (var i = 0; i < values.Length; i++)
			{
				var thisDistance = Mathf.Abs(value - values[i]);

				if (thisDistance < distance)
				{
					index = i;
					distance = thisDistance;
				}
			}

			index = EditorGUI.Popup(position, label, index, names);

			if (index >= 0)
				value = values[index];

			return value;
		}

		public static int DrawSlider(Rect position, GUIContent label, int value, int minimum, int maximum, int snap)
		{
			value = EditorGUI.IntSlider(position, label, value, minimum, maximum);
			value = MathHelper.Snap(value, snap);

			return value;
		}

		public static void DrawMinMaxSlider(Rect position, GUIContent label, ref int minimumValue, ref int maximumValue, int minimum, int maximum, int snap)
		{
			if (minimum >= maximum)
			{
				var swap = minimum;
				minimum = maximum;
				maximum = swap;
			}

			position = EditorGUI.PrefixLabel(position, label);

			var minimumRect = RectHelper.TakeWidth(ref position, EditorGUIUtility.fieldWidth);
			var maximumRect = RectHelper.TakeTrailingWidth(ref position, EditorGUIUtility.fieldWidth);

			RectHelper.TakeWidth(ref position, 5.0f);
			RectHelper.TakeTrailingWidth(ref position, 5.0f);

			minimumValue = EditorGUI.IntField(minimumRect, minimumValue);
			maximumValue = EditorGUI.IntField(maximumRect, maximumValue);

			var minimumFloat = (float)minimumValue;
			var maximumFloat = (float)maximumValue;

			EditorGUI.MinMaxSlider(position, ref minimumFloat, ref maximumFloat, minimum, maximum);

			minimumValue = MathHelper.Snap(Mathf.RoundToInt(minimumFloat), snap);
			maximumValue = MathHelper.Snap(Mathf.RoundToInt(maximumFloat), snap);
		}

		#endregion

		#region Drawer Interface

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return GetHeight(label);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var intDisplay = attribute as IntDisplayAttribute;

			label.tooltip = Label.GetTooltip(fieldInfo);

			switch (intDisplay.Type)
			{
				case IntDisplayType.Popup:
				{
					var names = intDisplay.Names.Select(name => new GUIContent(name)).ToArray();
					DrawPopup(position, label, property, names, intDisplay.Values);
					break;
				}
				case IntDisplayType.Slider:
				{
					DrawSlider(position, label, property, intDisplay.Values[0], intDisplay.Values[1], intDisplay.Values[2]);
					break;
				}
				case IntDisplayType.MinMaxSlider:
				{
					var maximumPropertName = intDisplay.Names[0];
					var maximumProperty = PropertyHelper.GetSibling(property, maximumPropertName);

					if (maximumProperty != null)
					{
						DrawMinMaxSlider(position, label, property, maximumProperty, intDisplay.Values[0], intDisplay.Values[1], intDisplay.Values[2]);
					}
					else
					{
						DrawSlider(position, label, property, intDisplay.Values[0], intDisplay.Values[1], intDisplay.Values[2]);
						Debug.LogWarningFormat(_invalidMaximumWarning, property.propertyPath, maximumPropertName);
					}

					break;
				}
			}
		}

		#endregion
	}
}
