using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class DisableInInspectorControl : PropertyScopeControl
	{
		public override float GetHeight(SerializedProperty property, GUIContent label)
		{
			return GetNextHeight(property, label);
		}

		public override void Draw(Rect position, SerializedProperty property, GUIContent label)
		{
			using (new EditorGUI.DisabledScope())
				DrawNext(position, property, label);
		}
	}

	[CustomPropertyDrawer(typeof(DisableInInspectorAttribute))]
	public class DisableInInspectorDrawer : ControlDrawer<DisableInInspectorControl>
	{
	}
}
