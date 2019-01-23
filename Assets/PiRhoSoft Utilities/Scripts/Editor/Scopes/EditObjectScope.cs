using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class EditObjectScope : GUI.Scope
	{
		private SerializedObject _serializedObject;

		public EditObjectScope(SerializedObject serializedObject)
		{
			_serializedObject = serializedObject;
			serializedObject.ApplyModifiedProperties();
		}

		protected override void CloseScope()
		{
			_serializedObject.Update();
		}
	}
}
