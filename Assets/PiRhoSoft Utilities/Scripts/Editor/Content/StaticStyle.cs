using System;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class StaticStyle
	{
		private GUIStyle _content;
		private Func<GUIStyle> _create;

		public StaticStyle(Func<GUIStyle> create)
		{
			_create = create;
		}

		public GUIStyle Style
		{
			get
			{
				if (_content == null)
					_content = _create();

				return _content;
			}
		}
	}
}
