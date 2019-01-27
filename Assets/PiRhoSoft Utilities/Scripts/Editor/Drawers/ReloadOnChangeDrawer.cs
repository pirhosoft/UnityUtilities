using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[CustomPropertyDrawer(typeof(ReloadOnChangeAttribute))]
	public class ReloadOnChangeDrawer : PropertyDrawer
	{
		private const string _notReloadableWarning = "Invalid owner for ReloadOnChange on field {0}: A class containing a field with ReloadOnChange must implement IReloadable";

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			using (var changes = new EditorGUI.ChangeCheckScope())
			{
				var reloadable = property.serializedObject.targetObject as IReloadable;

				if (reloadable == null)
					Debug.LogWarningFormat(_notReloadableWarning, property.propertyPath);

				if (property.propertyType == SerializedPropertyType.ObjectReference && typeof(ScriptableObject).IsAssignableFrom(fieldInfo.FieldType) && (attribute as ReloadOnChangeAttribute).UseAssetPopup)
					AssetPopupDrawer.Draw(position, label, property, fieldInfo.FieldType, true, true, true);
				else
					EditorGUI.PropertyField(position, property, label);

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
}
