using System;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class Base64Texture
	{
		public string Data { get; private set; }
		private Texture2D _texture;

		public Texture2D Texture
		{
			get
			{
				if (_texture == null)
				{
					_texture = new Texture2D(1, 1);
					_texture.hideFlags = HideFlags.HideAndDontSave;
					_texture.LoadImage(Convert.FromBase64String(Data));
				}

				return _texture;
			}
		}

		public Base64Texture(string base64)
		{
			Data = base64;
		}
	}
}
