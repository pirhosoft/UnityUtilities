using System;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class Style
	{
		private GUIStyle _content;
		private Func<GUIStyle> _create;

		public GUIStyle Content
		{
			get
			{
				if (_content == null)
					_content = _create();

				return _content;
			}
		}

		public Style(Func<GUIStyle> create)
		{
			_create = create;
		}
	}
}
