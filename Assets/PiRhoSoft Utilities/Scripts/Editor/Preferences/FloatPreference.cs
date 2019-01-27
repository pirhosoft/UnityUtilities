using UnityEditor;

namespace PiRhoSoft.UtilityEditor
{
	public class FloatPreference
	{
		private string _name;
		private float _default;

		public FloatPreference(string name, float defaultValue)
		{
			_name = name;
			_default = defaultValue;
		}

		public float Value
		{
			get { return EditorPrefs.GetFloat(_name, _default); }
			set { EditorPrefs.SetFloat(_name, value); }
		}
	}
}
