using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	public class SelectionHistoryWindow : EditorWindow
	{
		private Vector2 _scrollPosition;

		[MenuItem(SelectionHistory.ShowMenu)]
		public static void Open()
		{
			GetWindow<SelectionHistoryWindow>(SelectionHistory.Window).Show();
		}

		private void OnEnable()
		{
			Selection.selectionChanged += SelectionChanged;
		}

		private void OnDisable()
		{
			Selection.selectionChanged -= SelectionChanged;
		}

		private void SelectionChanged()
		{
			_scrollPosition.y = SelectionHistory.Current * EditorGUIUtility.singleLineHeight;
			Repaint();
		}

		void OnGUI()
		{
			var brightness = 0.8549f; // same as the top of the Inspector window
			var rect = new Rect(0, 0, position.width, EditorGUIUtility.singleLineHeight + 10);
			EditorGUI.DrawRect(rect, new Color(brightness, brightness, brightness));

			GUILayout.Space(5);

			using (new EditorGUILayout.HorizontalScope())
			{
				using (new EditorGUI.DisabledScope(!SelectionHistory.CanMoveBack()))
				{
					if (GUILayout.Button(SelectionHistory.BackButton.Content))
						SelectionHistory.MoveBack();
				}

				GUILayout.FlexibleSpace();

				using (new EditorGUI.DisabledScope(!SelectionHistory.CanMoveForward()))
				{
					if (GUILayout.Button(SelectionHistory.ForwardButton.Content))
						SelectionHistory.MoveForward();
				}
			}

			GUILayout.Space(1);

			using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height - 26)))
			{
				for (var i = 0; i < SelectionHistory.History.Count; i++)
				{
					var selection = SelectionHistory.History[i];
					var text = GetName(selection);

					if (GUILayout.Button(new GUIContent(text), i == SelectionHistory.Current ? EditorStyles.boldLabel : EditorStyles.label))
						SelectionHistory.GoTo(i);
				}

				_scrollPosition = scroll.scrollPosition;
			}
		}

		private string GetName(Object[] objects)
		{
			if (objects == null || objects.Length == 0)
				return SelectionHistory.EmptyLabel;

			if (objects.Length > 1)
				return SelectionHistory.MultipleLabel;
			
			if (objects[0] == null)
				return SelectionHistory.DeletedLabel;

			if (string.IsNullOrEmpty(objects[0].name))
				return string.Format("({0})", objects[0].GetType().Name);

			return objects[0].name;
		}
	}
}
