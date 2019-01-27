using UnityEditor;

namespace PiRhoSoft.UtilityEditor
{
	public class StringPreference
	{
		private string _name;
		private string _default;

		public StringPreference(string name, string defaultValue)
		{
			_name = name;
			_default = defaultValue;
		}

		public string Value
		{
			get { return EditorPrefs.GetString(_name, _default); }
			set { EditorPrefs.SetString(_name, value); }
		}
	}
}
