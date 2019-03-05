using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[CustomPropertyDrawer(typeof(InlineDisplayAttribute))]
	public class InlineDisplayDrawer : PropertyDrawer
	{
		#region Static Interface

		private static readonly GUIContent _emptyContent = new GUIContent(" "); // Need a space to force drawing of the blank prefix label

		public static float GetHeight(SerializedProperty property)
		{
			var height = 0.0f;
			var end = property.GetEndProperty();

			property.NextVisible(true);

			while (!SerializedProperty.EqualContents(property, end))
			{
				height += EditorGUI.GetPropertyHeight(property) + EditorGUIUtility.standardVerticalSpacing;
				property.NextVisible(false);
			}

			return height;
		}

		public static void Draw(SerializedProperty property, GUIContent label)
		{
			var rect = EditorGUILayout.GetControlRect(false, GetHeight(property));
			Draw(rect, property, label);
		}

		public static void Draw(Rect position, SerializedProperty property, GUIContent label)
		{
			var top = position.y;
			var end = property.GetEndProperty();
			var first = true;

			property.NextVisible(true);

			while (!SerializedProperty.EqualContents(property, end))
			{
				// Calling GetPropertyHeight changes label to contain the label for property unless the custom label is
				// passed in so do that if the label is being propagated.

				var height = label != null && first ? EditorGUI.GetPropertyHeight(property, label) : EditorGUI.GetPropertyHeight(property);
				var rect = RectHelper.TakeHeight(ref position, height);

				if (label != null)
					EditorGUI.PropertyField(rect, property, first ? label : _emptyContent);
				else
					EditorGUI.PropertyField(rect, property);

				RectHelper.TakeHeight(ref position, EditorGUIUtility.standardVerticalSpacing);
				property.NextVisible(false);

				first = false;
			}
		}

		#endregion

		#region Drawer Interface

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return GetHeight(property);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var useLabel = (attribute as InlineDisplayAttribute).PropagateLabel;

			if (useLabel)
				label.tooltip = Label.GetTooltip(fieldInfo);

			Draw(position, property, useLabel ? label : null);
		}

		#endregion
	}
}
