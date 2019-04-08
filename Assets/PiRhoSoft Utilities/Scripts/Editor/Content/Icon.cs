using System;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class Icon
	{
		public const string Add = "Toolbar Plus";
		public const string CustomAdd = "Toolbar Plus More";
		public const string Remove = "Toolbar Minus";
		public const string Edit = "UnityEditor.InspectorWindow";
		public const string Expanded = "IN foldout focus on";
		public const string Collapsed = "IN foldout focus";
		public const string Refresh = "d_preAudioLoopOff";
		public const string Load = "SceneLoadIn";
		public const string Unload = "SceneLoadOut";
		public const string Close = "LookDevClose";

		private const string _invalidIconError = "(UIII) failed to create icon content: the built in icon {0} could not be loaded";
		private const string _invalidDataError = "(UIID) failed to create icon content: the supplied data is not a valid base 64 string";
		private const string _invalidContentError = "(UIIC) failed to create icon content: the supplied data could not be loaded";

		private string _name;
		private string _data;

		private Texture _content;

		public Texture Content
		{
			get
			{
				if (_content == null)
				{
					_content = EditorGUIUtility.whiteTexture; // set to something so if loading fails it only fails once

					if (!string.IsNullOrEmpty(_name))
					{
						var content = EditorGUIUtility.IconContent(_name);

						if (content != null)
							_content = content.image;
						else
							Debug.LogErrorFormat(_invalidIconError, _name);
					}
					else if (!string.IsNullOrEmpty(_data))
					{
						var content = new Texture2D(1, 1);
						var data = Convert.FromBase64String(_data);

						content.hideFlags = HideFlags.HideAndDontSave;

						if (data != null)
						{
							if (content.LoadImage(data))
								_content = content;
							else
								Debug.LogError(_invalidContentError);
						}
						else
						{
							Debug.LogError(_invalidDataError);
						}
					}
				}

				return _content;
			}
		}

		public static Icon BuiltIn(string name)
		{
			return new Icon { _name = name };
		}

		public static Icon Base64(string data)
		{
			return new Icon { _data = data };
		}
	}
}
