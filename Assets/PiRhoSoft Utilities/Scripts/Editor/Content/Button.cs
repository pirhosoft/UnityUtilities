using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class Button
	{
		private Icon _icon;
		private string _label;
		private string _tooltip;

		private GUIContent _content;

		public GUIContent Content
		{
			get
			{
				if (_content == null)
					_content = new GUIContent(_label, _icon?.Content, _tooltip);

				return _content;
			}
		}

		public Button(Icon icon, string label = "", string tooltip = "")
		{
			_icon = icon;
			_label = label;
			_tooltip = tooltip;
		}
	}
}
