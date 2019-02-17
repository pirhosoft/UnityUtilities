using System;
using System.Reflection;
using PiRhoSoft.UtilityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PiRhoSoft.UtilityEditor
{
	public class SceneReferenceMaintainer : UnityEditor.AssetModificationProcessor
	{
		private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
		{
			SceneReference.SceneMoved(path, "");
			return AssetDeleteResult.DidNotDelete;
		}

		private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
		{
			SceneReference.SceneMoved(sourcePath, destinationPath);
			return AssetMoveResult.DidNotMove;
		}
	}

	[CustomPropertyDrawer(typeof(SceneReference))]
	public class SceneReferenceDrawer : PropertyDrawer
	{
		public static readonly IconButton _loadSceneButton = new IconButton(IconButton.Load, "Load the selected scene");
		public static readonly IconButton _unloadSceneButton = new IconButton(IconButton.Unload, "Unload the selected scene");
		public static readonly IconButton _refreshScenesButton = new IconButton(IconButton.Refresh, "Refresh the list of scenes");

		private static SceneReference _temporary = new SceneReference();

		public static void Draw(SceneReference scene, GUIContent label, string newSceneName, Action creator)
		{
			var rect = EditorGUILayout.GetControlRect();
			Draw(rect, scene, label, newSceneName, creator);
		}

		public static void Draw(Rect position, SceneReference scene, GUIContent label, string newSceneName, Action newSceneSetup)
		{
			position = EditorGUI.PrefixLabel(position, label);

			var loadRect = RectHelper.TakeTrailingIcon(ref position);
			var refreshRect = RectHelper.TakeTrailingIcon(ref position);

			if (GUI.Button(refreshRect, _refreshScenesButton.Content, GUIStyle.none))
				SceneHelper.RefreshLists();

			var list = SceneHelper.GetSceneList(true, newSceneSetup != null);
			var index = list.GetIndex(scene.Path);
			var selected = EditorGUI.Popup(position, index, list.Names);

			if (selected != index && selected == list.CreateIndex)
			{
				var newScene = SceneHelper.CreateScene(newSceneName, newSceneSetup);
				scene.Path = newScene.path;
			}
			else
			{
				scene.Path = list.GetPath(selected);
			}

			if (!string.IsNullOrEmpty(scene.Path))
			{
				var s = SceneManager.GetSceneByPath(scene.Path);

				using (ColorScope.ContentColor(Color.black))
				{
					if (s.IsValid() && s.isLoaded)
					{
						if (GUI.Button(loadRect, _unloadSceneButton.Content, GUIStyle.none))
						{
							if (EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new Scene[] { s }))
								EditorSceneManager.CloseScene(s, true);
						}
					}
					else
					{
						if (GUI.Button(loadRect, _loadSceneButton.Content, GUIStyle.none))
						{
							s = EditorSceneManager.OpenScene(scene.Path, OpenSceneMode.Additive);
							SceneManager.SetActiveScene(s);
						}
					}
				}
			}
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var attribute = TypeHelper.GetAttribute<SceneReferenceAttribute>(fieldInfo);
			var pathProperty = property.FindPropertyRelative(nameof(SceneReference.Path));

			label.tooltip = Label.GetTooltip(fieldInfo);
			_temporary.Path = pathProperty.stringValue;

			if (attribute != null)
			{
				var method = fieldInfo.DeclaringType.GetMethod(attribute.Creator, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				void creator() => method?.Invoke(method.IsStatic ? null : property.serializedObject.targetObject, null);

				Draw(position, _temporary, label, attribute.Name, creator);
			}
			else
			{
				Draw(position, _temporary, label, null, null);
			}

			pathProperty.stringValue = _temporary.Path;
		}
	}
}
