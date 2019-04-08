using System.Reflection;
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
		public static float IconWidth = EditorGUIUtility.singleLineHeight;
		public static float LineHeight = EditorGUIUtility.singleLineHeight + VerticalSpace;

		// these margin values seem consistent but it is hard to track down where they are actually set in order to
		// find the style(s) they are read from
		public static float LeftMargin = 14.0f;
		public static float RightMargin = 4.0f;
		public static float Indent = 15.0f; // this is the value of kIndentPerLevel at the time of writing

		public static float CurrentIndentWidth => GetIndentWidth(EditorGUI.indentLevel);
		public static float CurrentLabelWidth => EditorGUIUtility.labelWidth - CurrentIndentWidth;
		public static float CurrentFieldWidth => CurrentViewWidth - EditorGUIUtility.labelWidth;
		public static float CurrentViewWidth => CurrentContextWidth - LeftMargin - RightMargin - ContextMargin;
		public static float CurrentContextWidth => GetContextWidth();

		public static float ContextMargin { get; set; }

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

		public static void TakeIndent(ref Rect full)
		{
			TakeWidth(ref full, CurrentIndentWidth);
		}

		public static Rect TakeLabel(ref Rect full)
		{
			TakeIndent(ref full);
			return TakeWidth(ref full, CurrentLabelWidth);
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

		public static Rect Inset(Rect rect, float padding)
		{
			TakeWidth(ref rect, padding);
			TakeTrailingWidth(ref rect, padding);
			TakeHeight(ref rect, padding);
			TakeTrailingHeight(ref rect, padding);
			return rect;
		}

		public static Rect Inset(Rect rect, float left, float right, float top, float bottom)
		{
			TakeWidth(ref rect, left);
			TakeTrailingWidth(ref rect, right);
			TakeHeight(ref rect, top);
			TakeTrailingHeight(ref rect, bottom);
			return rect;
		}

		public static Rect Adjust(Rect rect, float width, float height, RectHorizontalAlignment horizontal, RectVerticalAlignment vertical)
		{
			var h = AdjustWidth(rect, width, horizontal);
			var v = AdjustHeight(h, height, vertical);
			return v;
		}

		public static Rect Union(Rect one, Rect two)
		{
			var left = Mathf.Min(one.xMin, two.xMin);
			var right = Mathf.Max(one.xMax, two.xMax);
			var top = Mathf.Min(one.yMin, two.yMin);
			var bottom = Mathf.Max(one.yMax, two.yMax);

			return new Rect(left, top, right - left, bottom - top);
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

		public static float GetIndentWidth(int levels)
		{
			var indent = Indent;
			var prop = typeof(EditorGUI).GetProperty("kIndentPerLevel", BindingFlags.NonPublic | BindingFlags.Static);

			if (prop != null)
			{
				try
				{
					indent = (float)prop.GetValue(null);
				}
				catch
				{
				}
			}

			return indent * levels;
		}

		private static float GetContextWidth()
		{
			var prop = typeof(EditorGUIUtility).GetProperty("contextWidth", BindingFlags.NonPublic | BindingFlags.Static);

			if (prop != null)
			{
				try
				{
					return (float)prop.GetValue(null);
				}
				catch
				{
				}
			}

			// this doesn't account for the scroll bar but at least it's a good estimate if the internal representation
			// of contextWidth changes in future releases
			return EditorGUIUtility.currentViewWidth;
		}
	}
}
