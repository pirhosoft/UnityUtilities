using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public abstract class AddNamedItemContent : IAddContent
	{
		private string _name = "";
		private bool _nameValid = true;
		private bool _focusName = true;

		protected abstract void Add_(string name);
		protected abstract bool IsNameInUse(string name);

		protected virtual float GetHeight_() => 0.0f;
		protected virtual bool Draw_(bool clean) => false;
		protected virtual bool Validate_() => true;
		protected virtual void Reset_() { }

		public float GetHeight()
		{
			return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + GetHeight_();
		}
		
		public bool Draw(bool clean)
		{
			var create = false;

			using (new InvalidScope(clean || _nameValid))
			{
				create |= EnterFieldDrawer.DrawString("NewItemName", GUIContent.none, ref _name);

				if (_focusName)
				{
					GUI.FocusControl("NewItemName");
					_focusName = false;
				}
			}

			create = Draw_(clean) || create;

			return create;
		}

		public bool Validate()
		{
			_nameValid = !string.IsNullOrEmpty(_name) && !IsNameInUse(_name);
			return Validate_() && _nameValid;
		}

		public void Add()
		{
			Add_(_name);
		}

		public void Reset()
		{
			_name = "";
			Reset_();
		}
	}
}
