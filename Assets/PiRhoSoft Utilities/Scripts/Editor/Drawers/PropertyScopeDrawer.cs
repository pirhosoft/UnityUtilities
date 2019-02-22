using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public abstract class PropertyScopeControl : PropertyControl
	{
		private static readonly List<Type> _ignoredTypes = new List<Type>()
		{
			typeof(TooltipAttribute),
			typeof(ContextMenuItemAttribute)
		};

		private static Dictionary<Type, Type> _drawerLookup;
		private static FieldInfo _attributeField;
		private static FieldInfo _fieldInfoField;

		private static void SetupDrawerDictionary()
		{
			if (_drawerLookup == null)
			{
				_drawerLookup = new Dictionary<Type, Type>();

				var assembly = Assembly.GetAssembly(typeof(Editor));
				var internalType = assembly.GetType("UnityEditor.ScriptAttributeUtility", false);
				var drawerType = assembly.GetType("UnityEditor.ScriptAttributeUtility+DrawerKeySet", false);
				var dictionaryField = internalType.GetField("s_DrawerTypeForType", BindingFlags.Static | BindingFlags.NonPublic);
				var drawerField = drawerType.GetField("drawer", BindingFlags.Public | BindingFlags.Instance);
				var typeField = drawerType.GetField("type", BindingFlags.Public | BindingFlags.Instance);

				var dictionary = dictionaryField.GetValue(null) as IDictionary;

				foreach (var value in dictionary.Values)
				{
					var type = typeField.GetValue(value) as Type;
					var drawer = drawerField.GetValue(value) as Type;

					if (!_drawerLookup.ContainsKey(type))
						_drawerLookup.Add(type, drawer);
				}

				var propertyType = assembly.GetType("UnityEditor.PropertyDrawer", false);
				_fieldInfoField = propertyType.GetField("m_FieldInfo", BindingFlags.NonPublic | BindingFlags.Instance);
				_attributeField = propertyType.GetField("m_Attribute", BindingFlags.NonPublic | BindingFlags.Instance);
			}
		}

		private PropertyDrawer _nextDrawer = null;

		public override void Setup(SerializedProperty property, FieldInfo fieldInfo, PropertyAttribute attribute)
		{
			SetupDrawerDictionary();

			var nextAttribute = attribute;
			var found = false;

			while (!found)
			{
				var attributes = fieldInfo.GetCustomAttributes<PropertyAttribute>(true);
				var drawerAttribute = attributes.LastOrDefault(next => next.order < nextAttribute.order && !_ignoredTypes.Contains(next.GetType()));

				var typeToDraw = drawerAttribute == null ? fieldInfo.FieldType : drawerAttribute.GetType();

				if (_drawerLookup.TryGetValue(typeToDraw, out var drawer))
				{
					if (TypeHelper.IsCreatableAs<DecoratorDrawer>(drawer))
					{
						nextAttribute = drawerAttribute;
						continue;
					}

					_nextDrawer = Activator.CreateInstance(drawer) as PropertyDrawer;
					_fieldInfoField.SetValue(_nextDrawer, fieldInfo);
					_attributeField.SetValue(_nextDrawer, drawerAttribute);
				}

				found = true;
			}
		}

		protected float GetNextHeight(SerializedProperty property, GUIContent label)
		{
			return _nextDrawer == null ? EditorGUI.GetPropertyHeight(property) : _nextDrawer.GetPropertyHeight(property, label);
		}

		protected void DrawNext(Rect position, SerializedProperty property, GUIContent label)
		{
			if (_nextDrawer != null)
				_nextDrawer.OnGUI(position, property, label);
			else
				EditorGUI.PropertyField(position, property, label);
		}
	}
}
