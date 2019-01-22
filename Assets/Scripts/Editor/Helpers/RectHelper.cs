using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public enum RectVerticalAlignment
	{
		Top,
		Middle,
		Bottom
	}

	public enum RectHorizontalAlignment
	{
		Left,
		Center,
		Right
	}

	public static class RectHelper
	{
		public static float VerticalSpace = EditorGUIUtility.standardVerticalSpacing;
		public static float HorizontalSpace = EditorGUIUtility.standardVerticalSpacing;
		public static float HorizontalPadding = 4.0f;
		public static float IconWidth = EditorGUIUtility.singleLineHeight;
		public static float LineHeight = EditorGUIUtility.singleLineHeight + VerticalSpace;
		public static float IndentWidth = 15.0f;

		public static Rect TakeLine(ref Rect full)
		{
			var height = EditorGUIUtility.singleLineHeight;
			var rect = TakeHeight(ref full, height + VerticalSpace);

			return new Rect(rect.x, rect.y, rect.width, height);
		}

		public static Rect TakeHeight(ref Rect full, float height)
		{
			var rect = new Rect(full.x, full.y, full.width, height);
			full = new Rect(full.x, full.y + height, full.width, full.height - height);
			return rect;
		}

		public static Rect TakeTrailingHeight(ref Rect full, float height)
		{
			var offset = full.height - height;
			var rect = new Rect(full.x, full.y + offset, full.width, height);
			full = new Rect(full.x, full.y, full.width, offset);
			return rect;
		}

		public static Rect TakeVerticalSpace(ref Rect full)
		{
			return TakeHeight(ref full, VerticalSpace);
		}

		public static Rect TakeHorizontalSpace(ref Rect full)
		{
			return TakeWidth(ref full, HorizontalSpace);
		}

		public static Rect TakeHorizontalPadding(ref Rect full)
		{
			return TakeWidth(ref full, HorizontalPadding);
		}

		public static void TakeIndent(ref Rect full)
		{
			// this is not in the Unity documentation despite being publicly accessible
			full = EditorGUI.IndentedRect(full);
		}

		public static Rect TakeLabel(ref Rect full)
		{
			return TakeWidth(ref full, EditorGUIUtility.labelWidth);
		}

		public static Rect TakeIndentedLabel(ref Rect full)
		{
			var indent = full.width - EditorGUI.IndentedRect(full).width;
			var rect = new Rect(full.x + indent, full.y, EditorGUIUtility.labelWidth - indent, full.height);

			TakeWidth(ref full, EditorGUIUtility.labelWidth);

			return rect;
		}

		public static Rect TakeLeadingIcon(ref Rect full)
		{
			var rect = TakeWidth(ref full, IconWidth + HorizontalSpace);
			return new Rect(rect.x, rect.y, IconWidth, rect.height);
		}

		public static Rect TakeTrailingIcon(ref Rect full)
		{
			var rect = TakeTrailingWidth(ref full, IconWidth + HorizontalSpace);
			return new Rect(rect.x + HorizontalSpace, rect.y, IconWidth, rect.height);
		}

		public static Rect TakeWidth(ref Rect full, float width)
		{
			var rect = new Rect(full.x, full.y, width, full.height);
			full = new Rect(full.x + width, full.y, full.width - width, full.height);
			return rect;
		}

		public static Rect TakeTrailingWidth(ref Rect full, float width)
		{
			var offset = full.width - width;
			var rect = new Rect(full.x + offset, full.y, width, full.height);
			full = new Rect(full.x, full.y, offset, full.height);
			return rect;
		}

		public static Rect Adjust(Rect rect, float width, float height, RectHorizontalAlignment horizontal, RectVerticalAlignment vertical)
		{
			var h = AdjustWidth(rect, width, horizontal);
			var v = AdjustHeight(h, height, vertical);
			return v;
		}

		public static Rect AdjustHeight(Rect rect, float height, RectVerticalAlignment alignment)
		{
			var offset = 0.0f;

			switch (alignment)
			{
				case RectVerticalAlignment.Middle: offset = (rect.height - height) * 0.5f; break;
				case RectVerticalAlignment.Bottom: offset = rect.height - height; break;
			}

			return new Rect(rect.x, rect.y + offset, rect.width, height);
		}

		public static Rect AdjustWidth(Rect rect, float width, RectHorizontalAlignment alignment)
		{
			var offset = 0.0f;

			switch (alignment)
			{
				case RectHorizontalAlignment.Center: offset = (rect.width - width) * 0.5f; break;
				case RectHorizontalAlignment.Right: offset = rect.width - width; break;
			}

			return new Rect(rect.x + offset, rect.y, width, rect.height);
		}
	}
}
