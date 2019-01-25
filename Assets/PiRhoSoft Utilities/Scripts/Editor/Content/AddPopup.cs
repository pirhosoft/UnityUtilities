using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public interface IAddContent
	{
		float GetHeight();
		bool Draw(bool clean);
		bool Validate();
		void Add();
		void Reset();
	}

	public class AddPopup : PopupWindowContent
	{
		private static readonly TextButton _createButton = new TextButton("Create", "Create this item", IconButton.Add);

		private IAddContent _content;
		private GUIContent _label;
		private bool _valid = true;

		public AddPopup(IAddContent content, GUIContent label)
		{
			_content = content;
			_label = label;
		}

		public override Vector2 GetWindowSize()
		{
			var width = 200.0f;
			var height = 4 + _content.GetHeight() + 20 + 2; // padding, content, button, padding

			if (_label != GUIContent.none)
				height += EditorGUIUtility.singleLineHeight + 4; // label, spacing

			return new Vector2(width, height);
		}

		public override void OnGUI(Rect rect)
		{
			if (_label != GUIContent.none)
				EditorGUILayout.LabelField(_label);
			
			var create = false;

			using (var changes = new EditorGUI.ChangeCheckScope())
			{
				create = _content.Draw(_valid);

				if (changes.changed)
					_valid = true;
			}

			using (new EditorGUI.DisabledScope(!_valid))
				create |= GUILayout.Button(_createButton.Content);

			if (create)
			{
				_valid = _content.Validate();

				if (_valid)
				{
					_content.Add();
					editorWindow.Close();
					_content.Reset();
				}
			}
		}
	}
}
