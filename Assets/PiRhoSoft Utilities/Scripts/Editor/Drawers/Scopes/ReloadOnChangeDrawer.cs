using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class ReloadOnChangeControl : PropertyScopeControl
	{
		private const string _notReloadableWarning = "Invalid owner for ReloadOnChange on field {0}: A class containing a field with ReloadOnChange must implement IReloadable";

		public override float GetHeight(SerializedProperty property, GUIContent label)
		{
			return GetNextHeight(property, label);
		}

		public override void Draw(Rect position, SerializedProperty property, GUIContent label)
		{
			using (var changes = new EditorGUI.ChangeCheckScope())
			{
				var reloadable = property.serializedObject.targetObject as IReloadable;

				if (reloadable == null)
					Debug.LogWarningFormat(_notReloadableWarning, property.propertyPath);

				DrawNext(position, property, label);

				if (changes.changed && reloadable != null)
				{
					using (new EditObjectScope(property.serializedObject))
					{
						reloadable.OnDisable();
						reloadable.OnEnable();
					}
				}
			}
		}
	}

	[CustomPropertyDrawer(typeof(ReloadOnChangeAttribute))]
	public class ReloadOnChangeDrawer : ControlDrawer<ReloadOnChangeControl>
	{
	}
}
