using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[CustomPropertyDrawer(typeof(ClassDisplayAttribute))]
	public class ClassDisplayDrawer : PropertyDrawer
	{
		private static readonly GUIContent _emptyContent = new GUIContent(" "); // need a space to force drawing of the blank prefix label

		#region Static Interface

		public static float GetHeightOfChildren(SerializedProperty property)
		{
			var height = 0.0f;
			var end = property.GetEndProperty();

			property.NextVisible(true);

			while (!SerializedProperty.EqualContents(property, end))
			{
				height += EditorGUI.GetPropertyHeight(property) + RectHelper.VerticalSpace;
				property.NextVisible(false);
			}

			return height;
		}

		public static void DrawChildren(Rect position, SerializedProperty property, GUIContent firstLabel)
		{
			firstLabel = firstLabel != null ? new GUIContent(firstLabel) : null; // GetPropertyHeight overwrites the most recent label

			var end = property.GetEndProperty();
			var useLabel = firstLabel != null;

			property.NextVisible(true);

			while (!SerializedProperty.EqualContents(property, end))
			{
				var height = EditorGUI.GetPropertyHeight(property);
				var rect = RectHelper.TakeHeight(ref position, height);

				if (useLabel)
				{
					// passing label to PropertyField would overwrite the tooltip
					rect = EditorGUI.PrefixLabel(rect, firstLabel);
					EditorGUI.PropertyField(rect, property, GUIContent.none);
				}
				else
				{
					EditorGUI.PropertyField(rect, property);
				}

				RectHelper.TakeVerticalSpace(ref position);
				property.NextVisible(false);
				firstLabel = _emptyContent;
			}
		}

		public static float GetHeight(GUIContent label, SerializedProperty property, ClassDisplayType type)
		{
			if (type == ClassDisplayType.Inline || type == ClassDisplayType.Propogated)
			{
				return GetHeightOfChildren(property);
			}
			else if (type == ClassDisplayType.Foldout && !property.isExpanded)
			{
				return EditorGUIUtility.singleLineHeight;
			}
			else
			{
				using (new EditorGUI.IndentLevelScope())
					return RectHelper.LineHeight + GetHeightOfChildren(property);
			}
		}

		public static void Draw(GUIContent label, SerializedProperty property, ClassDisplayType type)
		{
			var height = GetHeight(label, property, type);
			var rect = EditorGUILayout.GetControlRect(false, height);

			Draw(rect, label, property, type);
		}

		public static void Draw(Rect position, GUIContent label, SerializedProperty property, ClassDisplayType type)
		{
			if (type == ClassDisplayType.Inline)
			{
				DrawChildren(position, property, null);
			}
			else if (type == ClassDisplayType.Propogated)
			{
				DrawChildren(position, property, label);
			}
			else if (type == ClassDisplayType.Foldout)
			{
				var labelRect = RectHelper.TakeLine(ref position);

				property.isExpanded = FoldoutSection.Draw(labelRect, label, property.isExpanded);

				if (property.isExpanded)
				{
					using (new EditorGUI.IndentLevelScope())
						DrawChildren(position, property, null);
				}
			}
			else
			{
				var labelRect = RectHelper.TakeLine(ref position);
				EditorGUI.LabelField(labelRect, label);

				using (new EditorGUI.IndentLevelScope())
					DrawChildren(position, property, null);
			}
		}

		#endregion

		#region Drawer Interface

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var type = (attribute as ClassDisplayAttribute).Type;
			return GetHeight(label, property, type);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label.tooltip = Label.GetTooltip(fieldInfo);

			var type = (attribute as ClassDisplayAttribute).Type;
			Draw(position, label, property, type);
		}

		#endregion
	}
}
