using System;
using System.Reflection;
using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class ReloadOnChangeControl : PropertyScopeControl
	{
		private const string _notReloadableWarning = "Invalid owner for ReloadOnChange on field {0}: A class containing a field with ReloadOnChange must implement IReloadable";

		private Type _assetType;

		public override void Setup(SerializedProperty property, FieldInfo fieldInfo, PropertyAttribute attribute)
		{
			base.Setup(property, fieldInfo, attribute);

			_assetType = (attribute as ReloadOnChangeAttribute).UseAssetPopup && property.propertyType == SerializedPropertyType.ObjectReference && typeof(ScriptableObject).IsAssignableFrom(fieldInfo.FieldType) ? fieldInfo.FieldType : null;
		}

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

				if (_assetType != null)
					AssetPopupDrawer.Draw(position, label, property, _assetType, true, true, true);
				else
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
