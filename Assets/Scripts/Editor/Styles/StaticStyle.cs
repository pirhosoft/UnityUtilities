using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public abstract class StaticStyle
	{
		public GUIStyle BaseStyle;

		private GUIStyle _style;

		protected abstract GUIStyle Create();

		public GUIStyle Style
		{
			get
			{
				if (_style == null)
					_style = Create();

				return _style;
			}
		}
	}
}
