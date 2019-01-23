using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class UndoScope : GUI.Scope
	{
		private SerializedObject _serializedObject;
		private Object _object;

		private bool _forceDirty;
		private int _group;

		public UndoScope(Object objectToTrack, bool forceDirty)
		{
			_object = objectToTrack;
			_forceDirty = forceDirty;
			_group = Undo.GetCurrentGroup();

			EditorGUI.BeginChangeCheck();
			Undo.RecordObject(objectToTrack, objectToTrack.name);
		}

		public UndoScope(SerializedObject serializedObject)
		{
			_serializedObject = serializedObject;
			_group = Undo.GetCurrentGroup();

			serializedObject.Update();
		}

		protected override void CloseScope()
		{
			if (_serializedObject != null)
			{
				_serializedObject.ApplyModifiedProperties();
			}
			else
			{
				Undo.FlushUndoRecordObjects();

				var changed = EditorGUI.EndChangeCheck();

				if (_object && (changed || _forceDirty))
					EditorUtility.SetDirty(_object);
			}

			Undo.CollapseUndoOperations(_group);
		}
	}
}
