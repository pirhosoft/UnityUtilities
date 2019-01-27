using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PiRhoSoft.UtilityEngine
{
	public static class ComponentHelper
	{
		public static T GetComponentInScene<T>(int sceneIndex, bool includeDisabled) where T : Component
		{
			var objects = includeDisabled ? Resources.FindObjectsOfTypeAll<T>() : Object.FindObjectsOfType<T>();

			foreach (var o in objects)
			{
				if (o.gameObject.scene.buildIndex == sceneIndex)
					return o;
			}

			return null;
		}

		public static void GetComponentsInScene<T>(int sceneIndex, List<T> components, bool includeDisabled) where T : Component
		{
			var objects = includeDisabled ? Resources.FindObjectsOfTypeAll<T>() : Object.FindObjectsOfType<T>();

			foreach (var o in objects)
			{
				if (o.gameObject.scene.buildIndex == sceneIndex)
					components.Add(o);
			}
		}

		public static GameObject FindObject(string name, int sceneIndex = -1)
		{
			// This is slower than the built in GameObject.Find however it will find both active and inactive objects.

			var objects = Resources.FindObjectsOfTypeAll<GameObject>();

			foreach (var o in objects)
			{
				if (o.name == name && (sceneIndex < 0 || o.scene.buildIndex == sceneIndex))
					return o;
			}

			return null;
		}

		// The following functions are used to 'cast' GameObjects to any of their Component types and Components to
		// GameObject or any of their sibling component types. Inheritance hierarchy is considered as well for
		// Components and ScriptableObjects.

		public static bool HasType(Object unityObject, Type type)
		{
			if (type == typeof(GameObject))
				return GetAsGameObject(unityObject) != null;
			else if (typeof(Component).IsAssignableFrom(type))
				return GetAsGameObject(unityObject)?.GetComponent(type) != null;
			else
				return type.IsAssignableFrom(unityObject.GetType());
		}

		public static Object GetAsBaseObject(Object unityObject)
		{
			// The 'base' object is the GameObject for Component types and the actual object for all other types.

			var component = unityObject as Component;
			return component != null ? component.gameObject : unityObject;
		}

		public static T GetAsObject<T>(Object unityObject) where T : Object
		{
			var t = unityObject as T;
			if (t != null)
				return t;

			if (typeof(T) == typeof(GameObject))
				return GetAsGameObject(unityObject) as T;

			if (typeof(Component).IsAssignableFrom(typeof(T)))
				return GetAsComponent<T>(unityObject);

			return null;
		}

		public static GameObject GetAsGameObject(Object unityObject)
		{
			var gameObject = unityObject as GameObject;
			if (gameObject != null)
				return gameObject;

			var component = unityObject as Component;
			if (component != null)
				return component.gameObject;

			return null;
		}

		public static T GetAsComponent<T>(Object unityObject) where T : Object
		{
			var t = unityObject as Component as T;
			if (t != null)
				return t;

			var gameObject = unityObject as GameObject;
			if (gameObject != null)
				return gameObject.GetComponent<T>();

			var component = unityObject as Component;
			if (component != null)
				return component.GetComponent<T>();

			return null;
		}

		public static Component GetAsComponent(Object unityObject, string componentName)
		{
			var gameObject = unityObject as GameObject;
			if (gameObject != null)
				return gameObject.GetComponent(componentName);

			var component = unityObject as Component;
			if (component != null)
				return component.GetComponent(componentName);

			return null;
		}
	}
}
