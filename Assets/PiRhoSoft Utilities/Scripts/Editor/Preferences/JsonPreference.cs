using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class JsonPreference<T>
	{
		private string _name;
		private string _default;

		public JsonPreference(string name)
		{
			_name = name;
			_default = "{}";
		}

		public T Value
		{
			get
			{
				var json = EditorPrefs.GetString(_name, _default);
				var state = JsonUtility.FromJson<T>(json);
				return state;
			}
			set
			{
				var json = JsonUtility.ToJson(value);
				EditorPrefs.SetString(_name, json);
			}
		}
	}
}
