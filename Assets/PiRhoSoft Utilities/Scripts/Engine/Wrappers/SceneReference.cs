using System;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace PiRhoSoft.UtilityEngine
{
	public class SceneReferenceAttribute : Attribute
	{
		public string Name { get; private set; }
		public string Creator { get; private set; }

		public SceneReferenceAttribute(string name, string creator)
		{
			Name = name;
			Creator = creator;
		}
	}

	[Serializable]
	public class SceneReference
	{
		public string Path;

		public bool IsAssigned => !string.IsNullOrEmpty(Path);
		public bool IsLoaded => Scene.IsValid() && Scene.isLoaded;
		public Scene Scene => SceneManager.GetSceneByPath(Path);
		public int Index => SceneUtility.GetBuildIndexByScenePath(Path);

		#region Editor Support

#if UNITY_EDITOR

		public static Action<string, string> SceneMoved;

		private Object _owner;

		public void Setup(Object owner)
		{
			_owner = owner;
			SceneMoved += OnSceneMoved;
		}

		public void Teardown()
		{
			SceneMoved -= OnSceneMoved;
		}

		private void OnSceneMoved(string from, string to)
		{
			if (Path == from)
			{
				Path = to;

				if (_owner)
					UnityEditor.EditorUtility.SetDirty(_owner);
			}
		}

#else

		public void Setup() { }
		public void Teardown() { }

#endif

		#endregion
	}
}
