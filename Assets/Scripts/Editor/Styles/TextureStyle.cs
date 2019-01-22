using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class TextureStyle : StaticStyle
	{
		public static readonly TextureStyle Box = new TextureStyle(EditorGUIUtility.whiteTexture);

		private Texture2D _texture;

		public TextureStyle(Texture2D texture)
		{
			_texture = texture;
		}

		protected override GUIStyle Create()
		{
			var style = BaseStyle == null ? new GUIStyle() : new GUIStyle(BaseStyle);
			style.normal.background = _texture;
			return style;
		}
	}
}
