using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ExampleClass))]
public class ExampleClassDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		var flags = property.FindPropertyRelative(nameof(ExampleClass.Buttons));

		return EditorGUI.GetPropertyHeight(flags, GUIContent.none);
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var flags = property.FindPropertyRelative(nameof(ExampleClass.Buttons));

		EditorGUI.PropertyField(position, flags, GUIContent.none);
	}
}
