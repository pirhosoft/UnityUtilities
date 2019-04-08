using System;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class CreatePopup : PopupWindowContent
	{
		public const float DefaultWidth = 200.0f;
		
		private static readonly Label _createButton = new Label(Icon.BuiltIn(Icon.Add), "Create", "Create this item");

		private GUIContent _label;
		private Action _creator;
		private Func<bool> _validator;

		protected bool IsValid { get; private set; } = true;
		protected bool HasChanged { get; private set; } = false;

		public virtual void Setup(GUIContent label, Action creator, Func<bool> validator)
		{
			_label = label;
			_creator = creator;
			_validator = validator;
		}

		protected virtual float GetContentHeight()
		{
			return 0.0f;
		}

		protected virtual bool DrawContent()
		{
			return false;
		}

		protected virtual void Reset()
		{
		}

		public override Vector2 GetWindowSize()
		{
			var labelHeight = _label != GUIContent.none ? EditorGUIUtility.singleLineHeight + 4 : 0.0f;
			var contentHeight = GetContentHeight();
			var buttonHeight = 20.0f;

			var width = DefaultWidth;
			var height = 4 + labelHeight + contentHeight + buttonHeight + 2;

			return new Vector2(width, height);
		}

		public override void OnGUI(Rect rect)
		{
			if (_label != GUIContent.none)
				EditorGUILayout.LabelField(_label);
			
			var create = false;

			using (var changes = new EditorGUI.ChangeCheckScope())
			{
				create = DrawContent();

				if (changes.changed)
					HasChanged = true;
			}

			using (new EditorGUI.DisabledScope(!IsValid))
				create |= GUILayout.Button(_createButton.Content);

			if (create)
			{
				IsValid = _validator();
				HasChanged = false;

				if (IsValid)
				{
					_creator();
					editorWindow.Close();
					Reset();
				}
			}
		}
	}

	public class CreateNamedPopup : CreatePopup
	{
		private bool _focusName = true;

		public string Name { get; private set; }
		public bool IsNameValid = true;

		protected override float GetContentHeight()
		{
			return EditorGUIUtility.singleLineHeight;
		}

		protected override bool DrawContent()
		{
			var create = false;

			using (new InvalidScope(!HasChanged || IsNameValid))
			{
				var name = Name;
				create |= EnterField.DrawString("NewItemName", GUIContent.none, ref name);
				Name = name;

				if (_focusName)
				{
					GUI.FocusControl("NewItemName");
					_focusName = false;
				}
			}

			return create;
		}

		protected override void Reset()
		{
			Name = string.Empty;
		}
	}
}
