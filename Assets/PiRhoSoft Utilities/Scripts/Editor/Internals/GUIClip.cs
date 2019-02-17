using System;
using System.Reflection;
using PiRhoSoft.UtilityEngine;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public static class GUIClip
	{
		private static Func<Rect> _topmostRectGet;
		private static Func<Rect> _getTopRect;

		static GUIClip()
		{
			var assembly = Assembly.GetAssembly(typeof(GUI));
			var type = assembly?.GetType("UnityEngine.GUIClip", false);

			if (type != null)
			{
				_topmostRectGet = InternalHelper.CreateGetDelegate<Rect>(type, nameof(topmostRect));
				_getTopRect = InternalHelper.CreateDelegate<Func<Rect>>(type, nameof(GetTopRect));
			}
		}

		public static Rect topmostRect => _topmostRectGet();
		public static Rect GetTopRect() => _getTopRect();
	}
}
