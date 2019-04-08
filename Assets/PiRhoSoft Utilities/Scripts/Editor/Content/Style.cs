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


	public class NewStyle
	{
		public bool? WordWrap;

		private GUIStyle _content;

		public GUIStyle GetContent(GUIStyle template)
		{
			if (_content == null)
			{
				_content = template != null ? new GUIStyle(template) : new GUIStyle();

				if (WordWrap.HasValue)
					_content.wordWrap = WordWrap.Value;
			}

			return _content;
		}
	}
}
