using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PiRhoSoft.UtilityEditor
{
	public class SceneList
	{
		public bool HasNone;
		public bool HasCreate;

		public GUIContent[] Names;
		public List<string> Paths;

		public int CreateIndex => HasCreate ? Names.Length - 1 : -1;

		public int GetIndex(string path)
		{
			if (Paths == null)
				return -1;

			var index = Paths.IndexOf(path);

			if (HasNone)
			{
				if (index >= 0) index += 2;
				else index = 0;
			}

			return index;
		}

		public string GetPath(int index)
		{
			if (HasNone)
				index -= 2;

			return index >= 0 && index < Paths.Count ? Paths[index] : null;
		}
	}

	[Serializable]
	public class SceneState
	{
		[SerializeField] public SceneData[] Scenes;

		[Serializable]
		public struct SceneData
		{
			public bool IsActive;
			public bool IsLoaded;
			public string Path;
		}
	}

	public class SceneHelper : AssetPostprocessor
	{
		private static SceneList _plainList = new SceneList { HasNone = false, HasCreate = false };
		private static SceneList _noneList = new SceneList { HasNone = true, HasCreate = false };
		private static SceneList _createList = new SceneList { HasNone = false, HasCreate = true };
		private static SceneList _noneAndCreateList = new SceneList { HasNone = true, HasCreate = true };

		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			// adding or removing a scene in build settings doesn't trigger this so the scenes will need to be
			// refreshed manually
			RefreshLists();
		}

		#region Listing

		public static void RefreshLists()
		{
			_plainList.Paths = null;
			_noneList.Paths = null;
			_createList.Paths = null;
			_noneAndCreateList.Paths = null;
		}

		public static SceneList GetSceneList(bool includeNone, bool includeCreate)
		{
			if (includeNone && includeCreate)
				return BuildList(_noneAndCreateList);
			else if (includeNone)
				return BuildList(_noneList);
			else if (includeCreate)
				return BuildList(_createList);
			else
				return BuildList(_plainList);
		}

		private static SceneList BuildList(SceneList list)
		{
			if (list.Paths == null)
			{
				// Removed scenes still show up in build settings as deleted and are thus still listed here.
				// Unfortunate but that is better than the alternative (enumerating the scenes) which would allow
				// selection of scenes that aren't in build settings.

				list.Paths = new List<string>(SceneManager.sceneCountInBuildSettings);
				list.Names = new GUIContent[SceneManager.sceneCountInBuildSettings + (list.HasNone ? 2 : 0) + (list.HasCreate ? 2 : 0)];

				for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
					list.Paths.Add(SceneUtility.GetScenePathByBuildIndex(i));

				var index = 0;
				var prefix = AssetHelper.FindCommonPath(list.Paths);

				if (list.HasNone)
				{
					list.Names[index++] = new GUIContent("None");
					list.Names[index++] = new GUIContent("");
				}

				foreach (var path in list.Paths)
				{
					var scene = path.Substring(prefix.Length);
					list.Names[index++] = new GUIContent(scene);
				}

				if (list.HasCreate)
				{
					list.Names[index++] = new GUIContent("");
					list.Names[index++] = new GUIContent("Create Scene");
				}
			}

			return list;
		}

		#endregion

		#region Creation

		public static Scene CreateScene(string name, Action create)
		{
			var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
			SceneManager.SetActiveScene(scene);

			create();

			EditorSceneManager.SaveScene(scene, "Assets/" + name + ".unity");
			AddSceneToBuild(scene);

			return scene;
		}

		public static void AddSceneToBuild(Scene scene)
		{
			var original = EditorBuildSettings.scenes;
			var newSettings = new EditorBuildSettingsScene[original.Length + 1];
			var sceneToAdd = new EditorBuildSettingsScene(scene.path, true);

			Array.Copy(original, newSettings, original.Length);

			newSettings[newSettings.Length - 1] = sceneToAdd;

			EditorBuildSettings.scenes = newSettings;
		}

		#endregion

		#region Play State

		// SceneSetup is not serializable so this little dance is necessary to persist the editor state through play
		// mode changes

		public static SceneState CaptureState()
		{
			var state = new SceneState();
			var setup = EditorSceneManager.GetSceneManagerSetup();

			state.Scenes = new SceneState.SceneData[setup.Length];

			for (var i = 0; i < setup.Length; i++)
				state.Scenes[i] = new SceneState.SceneData { IsActive = setup[i].isActive, IsLoaded = setup[i].isLoaded, Path = setup[i].path };

			return state;
		}

		public static void RestoreState(SceneState state)
		{
			var scenes = new SceneSetup[state.Scenes.Length];

			for (var i = 0; i < state.Scenes.Length; i++)
				scenes[i] = new SceneSetup { isActive = state.Scenes[i].IsActive, isLoaded = state.Scenes[i].IsLoaded, path = state.Scenes[i].Path };

			EditorSceneManager.RestoreSceneManagerSetup(scenes);
		}

		#endregion
	}
}
