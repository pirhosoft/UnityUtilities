using UnityEditor;

namespace PiRhoSoft.UtilityEditor
{
	public class IntPreference
	{
		private string _name;
		private int _default;

		public IntPreference(string name, int defaultValue)
		{
			_name = name;
			_default = defaultValue;
		}

		public int Value
		{
			get { return EditorPrefs.GetInt(_name, _default); }
			set { EditorPrefs.SetInt(_name, value); }
		}
	}
}
