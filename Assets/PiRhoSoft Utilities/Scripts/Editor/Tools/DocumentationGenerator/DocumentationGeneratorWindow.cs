using System;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class DocumentationGeneratorWindow : EditorWindow
	{
		private static StringPreference _settingsFile = new StringPreference("PiRhoSoft.DocumentationGenerator.SettingsFile", "");

		private static readonly TextButton _newButton = new TextButton("New", "Create a new settings file");
		private static readonly TextButton _openButton = new TextButton("Open", "Open an existing settings file");
		private static readonly TextButton _generateButton = new TextButton("Generate", "Generate the categories, log descriptions, and table of contents and validate help url attributes");

		private DocumentationGenerator _generator;
		private Editor _editor;
		private GenerationState _state = GenerationState.Waiting;
		private string _message = "";
		private float _progress = 0.0f;
		private Thread _thread = null;
		private Vector2 _scrollPosition;
		private string _applicationPath;

		[MenuItem("Window/PiRhoSoft Utility/Documentation Generator")]
		public static void Open()
		{
			GetWindow<DocumentationGeneratorWindow>("Documentation Generator").Show();
		}

		void OnEnable()
		{
			DocumentationGenerator.Initialize();
			LoadGenerator(_settingsFile.Value);
			_applicationPath = Application.dataPath;
		}

		void OnDisable()
		{
			UnloadGenerator();
		}

		void OnInspectorUpdate()
		{
			if (_state != GenerationState.Waiting)
				Repaint();
		}

		void OnGUI()
		{
			using (new EditorGUI.DisabledScope(_state != GenerationState.Waiting))
			{
				DrawFilePicker();

				if (_generator != null)
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						if (GUILayout.Button(_generateButton.Content, GUILayout.ExpandWidth(true)))
							StartGeneration();
					}

					using (var scrolling = new EditorGUILayout.ScrollViewScope(_scrollPosition))
					{
						var editor = Editor.CreateEditor(_generator);
						var changed = editor.DrawDefaultInspector();

						if (changed)
							SaveGenerator(_generator, _settingsFile.Value);

						_scrollPosition = scrolling.scrollPosition;
					}
				}
			}

			DrawProgress();
		}

		#region Drawing

		private void DrawFilePicker()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.PrefixLabel("Settings File");

				EditorGUILayout.SelectableLabel(_settingsFile.Value, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.MinWidth(5));

				if (GUILayout.Button(_newButton.Content, GUILayout.MinWidth(20), GUILayout.MaxWidth(40.0f)))
				{
					var path = EditorUtility.SaveFilePanel("Create Settings File", DocumentationGenerator.RootPath, "settings", "json");

					if (!string.IsNullOrEmpty(path))
						CreateGenerator(path);
				}

				if (GUILayout.Button(_openButton.Content, GUILayout.MinWidth(20), GUILayout.MaxWidth(40.0f)))
				{
					var path = EditorUtility.OpenFilePanel("Open Settings File", DocumentationGenerator.RootPath, "json");

					if (!string.IsNullOrEmpty(path))
						LoadGenerator(path);
				}
			}
		}

		private void DrawProgress()
		{
			if (_state == GenerationState.Done)
			{
				FinishGeneration();
				EditorUtility.ClearProgressBar();
			}
			else if (_state != GenerationState.Waiting)
			{
				EditorUtility.DisplayProgressBar("Generating Documentation", _message, _progress);
			}
		}

		#endregion

		#region Generation

		private enum GenerationState
		{
			Waiting,
			Starting,
			Categories,
			TableOfContents,
			Log,
			Help,
			Done
		}

		private void SetProgress(GenerationState state, float progress, string message)
		{
			_state = state;
			_progress = progress;
			_message = message;
		}

		private void StartGeneration()
		{
			if (_thread == null)
			{
				SetProgress(GenerationState.Starting, 0.0f, "Setting up generator");
				_thread = new Thread(GenerateAllThread);
				_thread.Start();
			}
		}

		private void FinishGeneration()
		{
			_thread.Join();
			_state = GenerationState.Waiting;
			_thread = null;
		}

		private void GenerateAllThread()
		{
			var steps = _generator.Categories.Count + 3.0f;
			var progress = 1.0f;

			foreach (var category in _generator.Categories)
			{
				SetProgress(GenerationState.Categories, progress / steps, string.Format("Generating {0} category", category.Name));
				category.Generate(_generator.Categories, _generator.OutputDirectory);
				progress += 1.0f;
			}

			SetProgress(GenerationState.TableOfContents, progress / steps, "Generating table of contents");
			_generator.TableOfContents.Generate(_applicationPath, _generator.Categories, _generator.OutputDirectory);
			progress += 1.0f;

			SetProgress(GenerationState.Log, progress / steps, "Generating log descriptions");
			_generator.LogDescriptions.Generate(_generator.OutputDirectory);
			progress += 1.0f;

			SetProgress(GenerationState.Help, progress / steps, "Validating help urls");
			_generator.HelpUrls.Validate();
			progress += 1.0f;

			SetProgress(GenerationState.Done, 1.0f, "Generation complete");
		}

		#endregion

		#region File IO

		private void CreateGenerator(string path)
		{
			var generator = CreateInstance<DocumentationGenerator>();
			generator.SetDefaults();

			if (SaveGenerator(generator, path))
				SetGenerator(generator, path);
			else
				DestroyImmediate(generator);
		}

		private void LoadGenerator(string path)
		{
			try
			{
				var content = File.ReadAllText(path);
				var generator = CreateInstance<DocumentationGenerator>();
				JsonUtility.FromJsonOverwrite(content, generator);
				SetGenerator(generator, path);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				SetGenerator(null, "");
			}
		}

		private void UnloadGenerator()
		{
			if (_generator != null)
			{
				DestroyImmediate(_generator);
				_generator = null;
				_editor = null;
			}
		}

		private void SetGenerator(DocumentationGenerator generator, string path)
		{
			UnloadGenerator();

			_settingsFile.Value = path;
			_generator = generator;
			_editor = generator != null ? Editor.CreateEditor(_generator) : null;
		}

		private bool SaveGenerator(DocumentationGenerator generator, string path)
		{
			try
			{
				var content = JsonUtility.ToJson(generator, true);
				var outputFile = new FileInfo(path);

				Directory.CreateDirectory(outputFile.Directory.FullName);
				File.WriteAllText(outputFile.FullName, content);
				return true;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return false;
			}
		}

		#endregion
	}
}
