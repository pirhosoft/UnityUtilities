using PiRhoSoft.UtilityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.DocGenEditor
{
	public class DocumentationGeneratorWindow : EditorWindow
	{
		private static string _settingsFile;

		private static readonly TextButton _newButton = new TextButton("New", "Create a new settings file");
		private static readonly TextButton _openButton = new TextButton("Open", "Open an existing settings file");
		private static readonly TextButton _generateButton = new TextButton("Generate", "Generate the categories, log descriptions, and table of contents and validate help url attributes");
		private static readonly TextButton _saveButton = new TextButton("Save", "Save the changes made to the settings");
		private const string _templatesFolder = "Assets/PiRhoSoft DocGen/Templates";

		private Dictionary<string, string> _templates;
		private GenericMenu _templateMenu;
		private GUIContent[] _templateNames;
		private DocumentationGenerator _generator;
		private Editor _editor;
		private GenerationState _state = GenerationState.Waiting;
		private string _message = "";
		private float _progress = 0.0f;
		private Thread _thread = null;
		private Vector2 _scrollPosition;
		private string _applicationPath;

		[MenuItem("Window/PiRho Soft/Documentation Generator")]
		public static void Open()
		{
			GetWindow<DocumentationGeneratorWindow>("Documentation Generator").Show();
		}

		void OnEnable()
		{
			DocumentationGenerator.Initialize();
			_applicationPath = Application.dataPath;

			if (!string.IsNullOrEmpty(_settingsFile))
				LoadGenerator(_settingsFile);

			_state = GenerationState.Waiting;
			LoadTemplates();
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
				EditorGUILayout.Space();

				using (new GUILayout.HorizontalScope())
				{
					GUILayout.FlexibleSpace();

					var popupRect = GUILayoutUtility.GetRect(0.0f, 20.0f);

					if (GUILayout.Button(_newButton.Content, GUILayout.MinWidth(20), GUILayout.MaxWidth(80.0f)))
						_templateMenu.DropDown(popupRect);

					if (GUILayout.Button(_openButton.Content, GUILayout.MinWidth(20), GUILayout.MaxWidth(80.0f)))
						OpenSettings();
				}

				EditorGUILayout.Space();
				EditorGUILayout.Space();

				if (_generator != null)
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						EditorGUILayout.SelectableLabel(_settingsFile);

						if (GUILayout.Button(_generateButton.Content, GUILayout.MaxWidth(80.0f)))
							StartGeneration();

						if (GUILayout.Button(_saveButton.Content, GUILayout.MaxWidth(80.0f)))
							SaveGenerator(_generator, _settingsFile);
					}

					EditorGUILayout.Space();

					using (var scrolling = new EditorGUILayout.ScrollViewScope(_scrollPosition))
					{
						var editor = Editor.CreateEditor(_generator);
						editor.DrawDefaultInspector();
						_scrollPosition = scrolling.scrollPosition;
					}
				}
			}

			if (_state == GenerationState.Done || _state == GenerationState.Error)
			{
				FinishGeneration();
				EditorUtility.ClearProgressBar();
			}
			else if (_state != GenerationState.Waiting)
			{
				EditorUtility.DisplayProgressBar("Generating Documentation", _message, _progress);
			}
		}

		#region Generation

		private enum GenerationState
		{
			Waiting,
			Starting,
			Categories,
			TableOfContents,
			Log,
			Help,
			Done,
			Error
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
			try
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
			catch (Exception exception)
			{
				Debug.LogException(exception);
				SetProgress(GenerationState.Error, 1.0f, "Generation error");
			}
		}

		#endregion

		#region File IO

		private void LoadTemplates()
		{
			_templates = new Dictionary<string, string>();
			_templateMenu = new GenericMenu();

			var folder = Path.Combine(DocumentationGenerator.RootPath, _templatesFolder);
			var files = Directory.EnumerateFiles(folder, "*.json");

			foreach (var file in files)
			{
				try
				{
					var content = File.ReadAllText(file);
					var name = Path.GetFileNameWithoutExtension(file);

					_templates.Add(name, content);
					_templateMenu.AddItem(new GUIContent(name), false, CreateNewSettings, name);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}

			_templateNames = _templates.Select(template => new GUIContent(template.Key)).Prepend(new GUIContent("New")).ToArray();
		}

		private void CreateNewSettings(object templateName)
		{
			var path = EditorUtility.SaveFilePanel("Create Settings File", DocumentationGenerator.RootPath, "generator", "json");

			if (!string.IsNullOrEmpty(path))
				CreateGenerator(path, templateName.ToString());
		}

		private void OpenSettings()
		{
			var path = EditorUtility.OpenFilePanel("Open Settings File", DocumentationGenerator.RootPath, "json");

			if (!string.IsNullOrEmpty(path))
				LoadGenerator(path);
		}

		private void CreateGenerator(string path, string templateName)
		{
			var generator = CreateInstance<DocumentationGenerator>();

			if (_templates.TryGetValue(templateName, out string template))
				JsonUtility.FromJsonOverwrite(template, generator);

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

			_settingsFile = path;
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
