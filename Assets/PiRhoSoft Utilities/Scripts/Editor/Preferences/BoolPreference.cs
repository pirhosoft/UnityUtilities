using UnityEditor;

namespace PiRhoSoft.UtilityEditor
{
	public class BoolPreference
	{
		private string _name;
		private bool _default;

		public BoolPreference(string name, bool defaultValue)
		{
			_name = name;
			_default = defaultValue;
		}

		public bool Value
		{
			get { return EditorPrefs.GetBool(_name, _default); }
			set { EditorPrefs.SetBool(_name, value); }
		}
	}
}
