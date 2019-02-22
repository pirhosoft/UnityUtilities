using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PiRhoSoft.UtilityEditor
{
	[InitializeOnLoad]
	public static class SelectionHistory
	{
		public const string Window = "History";
		public const string ShowMenu = "Window/PiRhoSoft Utility/Selection History";
		public const string MoveBackMenu = "Edit/Navigation/Move Back &LEFT";
		public const string MoveForwardMenu = "Edit/Navigation/Move Forward &RIGHT";

		public const string EmptyLabel = "(nothing)";
		public const string MultipleLabel = "(multiple)";
		public const string DeletedLabel = "(deleted)";

		public static readonly TextButton BackButton = new TextButton("< Back", "Edit the previous object in the history", "");
		public static readonly TextButton ForwardButton = new TextButton("Forward >", "Edit the next object in the history", "");

		public static int Current { get; private set; }
		public static List<Object[]> History { get; } = new List<Object[]>();

		private const int _capacity = 100;
		private static bool _skipNextSelection = false;

		static SelectionHistory()
		{
			Selection.selectionChanged += SelectionChanged;
			EditorApplication.playModeStateChanged += OnPlayModeChanged;
		}

		[MenuItem(MoveBackMenu, validate = true)]
		public static bool CanMoveBack()
		{
			return Current > 0;
		}

		[MenuItem(MoveForwardMenu, validate = true)]
		public static bool CanMoveForward()
		{
			return Current < History.Count - 1;
		}

		[MenuItem(MoveBackMenu, priority = 1)]
		public static void MoveBack()
		{
			if (CanMoveBack())
				Select(--Current);
		}

		[MenuItem(MoveForwardMenu, priority = 2)]
		public static void MoveForward()
		{
			if (CanMoveForward())
				Select(++Current);
		}

		public static void GoTo(int index)
		{
			if (index != Current && index >= 0 && index < History.Count)
			{
				Current = index;
				Select(index);
			}
		}

		public static void Clear()
		{
			Current = 0;
			History.Clear();
		}

		private static void SelectionChanged()
		{
			if (!_skipNextSelection)
			{
				var trailing = History.Count - Current - 1;

				if (trailing > 0)
					History.RemoveRange(Current + 1, trailing);

				if (Current == _capacity)
					History.RemoveAt(0);

				History.Add(Selection.objects); // it doesn't seem like Unity ever reuses this array but if it does it would need to be copied here
				Current = History.Count - 1;
			}
			else
			{
				_skipNextSelection = false;
			}
		}

		private static void Select(int index)
		{
			_skipNextSelection = true;
			Selection.objects = History[index];
		}

		private static void OnPlayModeChanged(PlayModeStateChange state)
		{
			switch (state)
			{
				case PlayModeStateChange.ExitingEditMode: Clear(); break;
				case PlayModeStateChange.EnteredEditMode: Clear(); break;
			}
		}

	}
}
