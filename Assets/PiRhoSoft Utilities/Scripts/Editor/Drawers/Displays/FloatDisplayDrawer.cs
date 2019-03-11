using PiRhoSoft.UtilityEngine;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[CustomPropertyDrawer(typeof(FloatDisplayAttribute))]
	class FloatDisplayDrawer : PropertyDrawer
	{
		private const string _invalidTypeWarning = "(UFDDIT) Invalid type for FloatDisplay on field {0}: FloatDisplay can only be used with float fields";
		private const string _invalidMaximumWarning = "(UFDDIM) Invalid property for MinMaxSlider FloatDisplay on field {0}: the sibling field {1} could not be found";

		#region Static Property Interface

		public static float GetHeight(GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight;
		}

		public static void DrawPopup(GUIContent label, SerializedProperty property, GUIContent[] names, float[] values)
		{
			var height = GetHeight(label);
			var rect = EditorGUILayout.GetControlRect(false, height);

			DrawPopup(rect, label, property, names, values);
		}

		public static void DrawSlider(GUIContent label, SerializedProperty property, float minimum, float maximum, float snap)
		{
			var height = GetHeight(label);
			var rect = EditorGUILayout.GetControlRect(false, height);

			DrawSlider(rect, label, property, minimum, maximum, snap);
		}

		public static void DrawMinMaxSlider(GUIContent label, SerializedProperty minimumProperty, SerializedProperty maximumProperty, float minimum, float maximum, float snap)
		{
			var height = GetHeight(label);
			var rect = EditorGUILayout.GetControlRect(false, height);

			DrawMinMaxSlider(rect, label, minimumProperty, maximumProperty, minimum, maximum, snap);
		}

		public static void DrawPopup(Rect position, GUIContent label, SerializedProperty property, GUIContent[] names, float[] values)
		{
			if (property.propertyType == SerializedPropertyType.Float)
			{
				property.floatValue = DrawPopup(position, label, property.floatValue, names, values);
			}
			else
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
				EditorGUI.PropertyField(position, property, label);
			}
		}

		public static void DrawSlider(Rect position, GUIContent label, SerializedProperty property, float minimum, float maximum, float snap)
		{
			if (property.propertyType == SerializedPropertyType.Float)
			{
				property.floatValue = DrawSlider(position, label, property.floatValue, minimum, maximum, snap);
			}
			else
			{
				Debug.LogWarningFormat(_invalidTypeWarning, property.propertyPath);
				EditorGUI.PropertyField(position, property, label);
			}
		}

		public static void DrawMinMaxSlider(Rect position, GUIContent label, SerializedProperty minimumProperty, SerializedProperty maximumProperty, float minimum, float maximum, float snap)
		{
			if (minimumProperty.propertyType != SerializedPropertyType.Float)
			{
				Debug.LogWarningFormat(_invalidTypeWarning, minimumProperty.propertyPath);
				EditorGUI.PropertyField(position, minimumProperty, label);
			}
			else if (maximumProperty.propertyType != SerializedPropertyType.Float)
			{
				Debug.LogWarningFormat(_invalidTypeWarning, maximumProperty.propertyPath);
				EditorGUI.PropertyField(position, maximumProperty, label);
			}
			else
			{
				var minimumValue = minimumProperty.floatValue;
				var maximumValue = maximumProperty.floatValue;

				DrawMinMaxSlider(position, label, ref minimumValue, ref maximumValue, minimum, maximum, snap);

				minimumProperty.floatValue = minimumValue;
				maximumProperty.floatValue = maximumValue;
			}
		}

		#endregion

		#region Static Object Interface

		public static float DrawPopup(GUIContent label, float value, GUIContent[] names, float[] values)
		{
			var height = GetHeight(label);
			var rect = EditorGUILayout.GetControlRect(false, height);

			return DrawPopup(rect, label, value, names, values);
		}

		public static float DrawSlider(GUIContent label, float value, float minimum, float maximum, float snap)
		{
			var height = GetHeight(label);
			var rect = EditorGUILayout.GetControlRect(false, height);

			return DrawSlider(rect, label, value, minimum, maximum, snap);
		}

		public static void DrawMinMaxSlider(GUIContent label, ref float minimumValue, ref float maximumValue, float minimum, float maximum, float snap)
		{
			var height = GetHeight(label);
			var rect = EditorGUILayout.GetControlRect(false, height);

			DrawMinMaxSlider(rect, label, ref minimumValue, ref maximumValue, minimumValue, maximumValue, snap);
		}

		public static float DrawPopup(Rect position, GUIContent label, float value, GUIContent[] names, float[] values)
		{
			// After selection in the editor the correct index will always be found with an equality check, but after
			// serialization or after changing the value from code the float representation might not be exactly
			// equivalent. And because the scale of values isn't known, a tolerance doesn't really work either, so
			// the index is found based on the closest value in the list. Not ideal if the value changes to something
			// not even close, but if that happens, a popup probably isn't the best option.

			var index = -1;
			var distance = float.MaxValue;

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

		public static float DrawSlider(Rect position, GUIContent label, float value, float minimum, float maximum, float snap)
		{
			value = EditorGUI.Slider(position, label, value, minimum, maximum);
			value = MathHelper.Snap(value, snap);

			return value;
		}

		public static void DrawMinMaxSlider(Rect position, GUIContent label, ref float minimumValue, ref float maximumValue, float minimum, float maximum, float snap)
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

			minimumValue = EditorGUI.FloatField(minimumRect, minimumValue);
			maximumValue = EditorGUI.FloatField(maximumRect, maximumValue);

			EditorGUI.MinMaxSlider(position, ref minimumValue, ref maximumValue, minimum, maximum);

			minimumValue = MathHelper.Snap(minimumValue, snap);
			maximumValue = MathHelper.Snap(maximumValue, snap);
		}

		#endregion

		#region Drawer Interface

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return GetHeight(label);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var floatDisplay = attribute as FloatDisplayAttribute;

			label.tooltip = Label.GetTooltip(fieldInfo);

			switch (floatDisplay.Type)
			{
				case FloatDisplayType.Popup:
				{
					var names = floatDisplay.Names.Select(name => new GUIContent(name)).ToArray();
					DrawPopup(position, label, property, names, floatDisplay.Values);
					break;
				}
				case FloatDisplayType.Slider:
				{
					DrawSlider(position, label, property, floatDisplay.Values[0], floatDisplay.Values[1], floatDisplay.Values[2]);
					break;
				}
				case FloatDisplayType.MinMaxSlider:
				{
					var maximumPropertName = floatDisplay.Names[0];
					var maximumProperty = PropertyHelper.GetSibling(property, maximumPropertName);

					if (maximumProperty != null)
					{
						DrawMinMaxSlider(position, label, property, maximumProperty, floatDisplay.Values[0], floatDisplay.Values[1], floatDisplay.Values[2]);
					}
					else
					{
						DrawSlider(position, label, property, floatDisplay.Values[0], floatDisplay.Values[1], floatDisplay.Values[2]);
						Debug.LogWarningFormat(_invalidMaximumWarning, property.propertyPath, maximumPropertName);
					}

					break;
				}
			}
		}

		#endregion
	}
}
