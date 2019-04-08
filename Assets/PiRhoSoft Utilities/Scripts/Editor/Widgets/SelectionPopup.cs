using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PiRhoSoft.UtilityEditor
{
	// Adapted from Unity's AddComponentWindow as linked to in the following thread:
	// https://forum.unity.com/threads/custom-add-component-like-button.439730/

	public class SelectionTree
	{
		public List<GUIContent> Labels = new List<GUIContent>();
		public List<GUIContent[]> Items = new List<GUIContent[]>();

		public void Add(string label, GUIContent[] items)
		{
			Labels.Add(new GUIContent(label));
			Items.Add(items);
		}
	}

	public struct SelectionState
	{
		public int Tab;
		public int Index;
	}

	public class SelectionPopup : EditorWindow
	{
		private class TreeItem
		{
			public int ItemIndex;
			public string Path;
			public string SearchName;
			public string Name;
			public GUIContent Content;

			public TreeItem Parent;
			public List<TreeItem> Children;

			public int SelectedIndex = -1;
			public Vector2 ScrollPosition = Vector2.zero;

			public bool IsBranch => Children != null;
			public TreeItem SelectedItem => IsBranch && SelectedIndex >= 0 && SelectedIndex < Children.Count ? Children[SelectedIndex] : null;

			public static TreeItem Leaf(int index, string path, GUIContent content, TreeItem parent)
			{
				var item = new TreeItem
				{
					ItemIndex = index,
					Path = path,
					SearchName = content.text.Replace(" ", string.Empty).ToLower(),
					Name = content.text,
					Content = content,
					Parent = parent,
				};

				if (item.Parent != null)
					item.Parent.Children.Add(item);

				return item;
			}

			public static TreeItem Branch(string path, GUIContent content, TreeItem parent)
			{
				var item = Leaf(-1, path, content, parent);
				item.Children = new List<TreeItem>();
				return item;
			}
		}

		private static readonly Style Header = new Style(() => new GUIStyle("In BigTitle") { font = EditorStyles.boldLabel.font });
		private static readonly Style ItemButton = new Style(() => new GUIStyle("PR Label") { alignment = TextAnchor.MiddleLeft, fixedHeight = _itemHeight });
		private static readonly Style Background = new Style(() => new GUIStyle("grey_border"));
		private static readonly Style RightArrow = new Style(() => new GUIStyle("AC RightArrow"));
		private static readonly Style LeftArrow = new Style(() => new GUIStyle("AC LeftArrow"));
		private static readonly Style SearchText = new Style(() => new GUIStyle("SearchTextField"));
		private static readonly Style SearchCancelEmpty = new Style(() => new GUIStyle("SearchCancelButtonEmpty"));
		private static readonly Style SearchCancel = new Style(() => new GUIStyle("SearchCancelButton"));
		private static readonly Label FolderIcon = new Label(Icon.BuiltIn("FolderEmpty Icon"));
		private static readonly Label DefaultTypeIcon = new Label(Icon.BuiltIn("cs Script Icon"));

		private const string _invalidTreeWarning = "(USCIT) Unable to show search tree: invalid tabs and content";

		private const int _headerHeight = 25;
		private const int _itemHeight = 20;
		private const int _visibleItemCount = 14;
		private const int _windowHeight = 2 * _headerHeight + _itemHeight * _visibleItemCount;

		private static SelectionPopup _instance;
		private static int _selectionId = -1;
		private static bool _selectionMade = false;
		private static SelectionState _selectionState;

		private List<TreeItem> _roots = new List<TreeItem>();
		private List<List<TreeItem>> _searchList = new List<List<TreeItem>>();

		private int _currentTab;
		private TreeItem _currentBranch;
		private TreeItem _targetBranch;
		private TreeItem _searchRoot;

		private TreeItem _activeBranch => _hasSearch ? _searchRoot : _currentBranch;
		private List<TreeItem> _currentLeaves => _searchList[_currentTab];

		private float _animation = 1.0f;
		private int _animationTarget = 1;
		private long _lastTime;
		private bool _scrollToSelected;
		private string _delayedSearch;
		private string _search = string.Empty;

		private bool _isAnimating => _animation != _animationTarget;
		private bool _hasSearch => !string.IsNullOrEmpty(_search);

		#region Static Interface

		public static bool HasSelection(int id) => _selectionId == id && _selectionMade;
		public static SelectionState TakeSelection() { _selectionId = -1; _selectionMade = false; return _selectionState; }

		public static SelectionState Draw(Rect position, GUIContent label, SelectionState state, SelectionTree tree)
		{
			var id = GUIUtility.GetControlID(FocusType.Passive);

			if (GUI.Button(position, label))
				Show(id, position, state, tree);

			if (HasSelection(id))
				return TakeSelection();

			return state;
		}

		public static void Show(int id, Rect position, SelectionState state, SelectionTree tree)
		{
			if (_instance != null)
				_instance.Close();

			_selectionId = id;
			_selectionMade = false;
			_selectionState = state;

			_instance = CreateInstance<SelectionPopup>();
			_instance.Setup(position, state, tree);

			_instance.ShowAsDropDown(new Rect(GUIUtility.GUIToScreenPoint(position.position), position.size), new Vector2(position.width, _windowHeight));
			_instance.Focus();
		}

		void OnDestroy()
		{
			_instance = null;
		}

		#endregion

		private void Setup(Rect position, SelectionState state, SelectionTree tree)
		{
			CreateTrees(tree);
			SetState(state);
			RebuildSearch();

			wantsMouseMove = true;
		}

		private void CreateTrees(SelectionTree tree)
		{
			for (var t = 0; t < tree.Labels.Count; t++)
			{
				var items = tree.Items[t];
				var label = tree.Labels[t];

				var root = TreeItem.Branch(string.Empty, label, null);
				var rootPath = label.text + "/";
				var leaves = new List<TreeItem>();

				for (var index = 0; index < items.Length; index++)
				{
					var node = items[index];
					var fullPath = rootPath + node.text;
					var submenus = fullPath.Split('/');

					var path = rootPath;
					var child = root;

					for (var i = 1; i < submenus.Length - 1; i++)
					{
						var menu = submenus[i];
						path += menu + "/";

						var previousChild = child;
						child = GetChild(child, path);

						if (child == null)
							child = TreeItem.Branch(path, new GUIContent(menu, FolderIcon.Content.image), previousChild);
					}

					leaves.Add(TreeItem.Leaf(index, path, new GUIContent(submenus.Last(), node.image ?? DefaultTypeIcon.Content.image), child));
				}

				_roots.Add(root);
				_searchList.Add(leaves);
			}
		}

		private TreeItem GetChild(TreeItem parent, string path)
		{
			if (parent.Path == path)
				return parent;

			if (parent.IsBranch)
			{
				foreach (var child in parent.Children)
				{
					var found = GetChild(child, path);
					if (found != null)
						return found;
				}
			}

			return null;
		}

		private void SetState(SelectionState state)
		{
			_currentTab = state.Tab;
			_currentBranch = _currentLeaves[state.Index >= 0 ? state.Index : 0].Parent;
		}

		private void RebuildSearch()
		{
			_searchRoot = TreeItem.Branch("Search", new GUIContent("Search"), null);

			_searchRoot.Children.Clear();
			_searchRoot.SelectedIndex = -1;

			if (!_hasSearch)
			{
				_animationTarget = 1;
				_lastTime = DateTime.Now.Ticks;
			}
			else
			{
				var subwords = _search.ToLower().Split(' ').Where(subword => !string.IsNullOrEmpty(subword)).ToArray();
				var starts = new List<TreeItem>();
				var contains = new List<TreeItem>();

				foreach (var item in _currentLeaves)
				{
					if (subwords.Length > 0 && item.SearchName.StartsWith(subwords.First()))
					{
						starts.Add(item);
					}
					else
					{
						foreach (var subword in subwords)
						{
							if (item.SearchName.Contains(subword))
								contains.Add(item);
						}
					}
				}

				_searchRoot.Children.AddRange(starts);
				_searchRoot.Children.AddRange(contains);
			}
		}


		void OnGUI()
		{
			HandleKeyboard();

			var backgroundRect = new Rect(Vector2.zero, position.size);

			GUI.Label(backgroundRect, GUIContent.none, Background.Content);
			GUI.SetNextControlName("Search");
			EditorGUI.FocusTextInControl("Search");
			RectHelper.TakeVerticalSpace(ref backgroundRect);

			var searchRect = RectHelper.Inset(RectHelper.TakeHeight(ref backgroundRect, _headerHeight), 8, 8, RectHelper.VerticalSpace, RectHelper.VerticalSpace);
			var cancelRect = RectHelper.TakeTrailingWidth(ref searchRect, RectHelper.IconWidth);
			var search = GUI.TextField(searchRect, _delayedSearch ?? _search, SearchText.Content);
			var buttonStyle = string.IsNullOrEmpty(_search) ? SearchCancelEmpty.Content : SearchCancel.Content;

			if (GUI.Button(cancelRect, GUIContent.none, buttonStyle))
			{
				GUI.FocusControl(null);
				search = string.Empty;
			}

			if (search != _search || _delayedSearch != null)
			{
				if (!_isAnimating)
				{
					_search = _delayedSearch ?? search;
					_delayedSearch = null;

					RebuildSearch();
				}
				else
				{
					_delayedSearch = search;
				}
			}

			DrawHeader(backgroundRect, _activeBranch, _animation);

			if (_animation < 1.0f && _targetBranch != null)
				DrawHeader(backgroundRect, _targetBranch, _animation + 1.0f);

			if (_isAnimating && Event.current.type == EventType.Repaint)
			{
				var ticks = DateTime.Now.Ticks;
				var speed = (ticks - _lastTime) / 0.25E+07f;

				_lastTime = ticks;
				_animation = Mathf.MoveTowards(_animation, _animationTarget, speed);

				if (_animationTarget == 0 && _animation == 0.0f)
				{
					_animation = 1.0f;
					_animationTarget = 1;
					_currentBranch = _targetBranch;
					_targetBranch = null;
				}

				Repaint();
			}
		}

		private void DrawHeader(Rect rect, TreeItem parent, float animation)
		{
			if (parent != null)
			{
				animation = Mathf.Floor(animation) + Mathf.SmoothStep(0.0f, 1.0f, Mathf.Repeat(animation, 1.0f));

				rect.x = rect.width * (1 - animation);

				var headerRect = RectHelper.Inset(RectHelper.TakeHeight(ref rect, _headerHeight), 1, 1, 0, 0);

				GUI.Label(headerRect, parent.Content, Header.Content);

				if (parent.Parent != null)
				{
					var arrowRect = RectHelper.AdjustHeight(RectHelper.TakeLeadingIcon(ref headerRect), RectHelper.IconWidth, RectVerticalAlignment.Middle);

					if (GUI.Button(arrowRect, GUIContent.none, LeftArrow.Content))
						GoToParent();
				}
				else if (_roots.Count > 1)
				{
					var leftRect = RectHelper.AdjustHeight(RectHelper.TakeLeadingIcon(ref headerRect), RectHelper.IconWidth, RectVerticalAlignment.Middle);
					var rightRect = RectHelper.AdjustHeight(RectHelper.TakeTrailingIcon(ref headerRect), RectHelper.IconWidth, RectVerticalAlignment.Middle);

					if (GUI.Button(leftRect, GUIContent.none, LeftArrow.Content))
						ChangeTab(-1);

					if (GUI.Button(rightRect, GUIContent.none, RightArrow.Content))
						ChangeTab(1);
				}

				DrawChildren(rect, parent);
			}
		}

		private void DrawChildren(Rect rect, TreeItem parent)
		{
			if (parent.Children.Count > 0)
			{
				var width = parent.Children.Count > _visibleItemCount ? rect.width - RectHelper.IconWidth : rect.width;
				var area = new Rect(Vector2.zero, new Vector2(width, _itemHeight * parent.Children.Count));
				var selectedRect = area;

				using (var scroll = new GUI.ScrollViewScope(rect, parent.ScrollPosition, area))
				{
					parent.ScrollPosition = scroll.scrollPosition;

					for (var i = 0; i < parent.Children.Count; i++)
					{
						var item = parent.Children[i];
						var itemRect = RectHelper.TakeHeight(ref area, _itemHeight);

						if ((Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDown) && parent.SelectedIndex != i && itemRect.Contains(Event.current.mousePosition))
						{
							parent.SelectedIndex = i;
							Repaint();
						}

						var selected = i == parent.SelectedIndex;

						if (selected)
							selectedRect = itemRect;

						if (Event.current.type == EventType.Repaint)
						{
							ItemButton.Content.Draw(itemRect, item.Content, false, false, selected, selected);

							if (item.IsBranch)
							{
								var arrowRect = RectHelper.TakeTrailingIcon(ref itemRect);

								RightArrow.Content.Draw(arrowRect, false, false, false, false);
							}
						}

						if (Event.current.type == EventType.MouseDown && itemRect.Contains(Event.current.mousePosition))
						{
							Event.current.Use();
							parent.SelectedIndex = i;
							SelectChild();
						}
					}
				}

				if (_scrollToSelected && Event.current.type == EventType.Repaint)
				{
					_scrollToSelected = false;

					if (selectedRect.yMax - rect.height > parent.ScrollPosition.y)
					{
						parent.ScrollPosition.y = selectedRect.yMax - rect.height;
						Repaint();
					}

					if (selectedRect.y < parent.ScrollPosition.y)
					{
						parent.ScrollPosition.y = selectedRect.y;
						Repaint();
					}
				}
			}
		}

		private void HandleKeyboard()
		{
			var current = Event.current;
			if (current.type == EventType.KeyDown)
			{
				if (current.keyCode == KeyCode.DownArrow)
				{
					_activeBranch.SelectedIndex = Mathf.Min(_activeBranch.SelectedIndex + 1, _activeBranch.Children.Count - 1);
					_scrollToSelected = true;
					current.Use();
				}

				if (current.keyCode == KeyCode.UpArrow)
				{
					_activeBranch.SelectedIndex = Mathf.Max(_activeBranch.SelectedIndex - 1, 0);
					_scrollToSelected = true;
					current.Use();
				}

				if (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter)
				{
					SelectChild();
					current.Use();
				}

				if (!_hasSearch) // Keep this in the check because it will otherwise override the search text
				{
					if (current.keyCode == KeyCode.LeftArrow || current.keyCode == KeyCode.Backspace)
					{
						GoToParent();
						current.Use();
					}
					if (current.keyCode == KeyCode.RightArrow)
					{
						SelectChild();
						current.Use();
					}
					if (current.keyCode == KeyCode.Escape)
					{
						Close();
						current.Use();
					}
				}
			}
		}

		private void ChangeTab(int increment)
		{
			_currentTab += increment;

			if (_currentTab < 0)
				_currentTab = _roots.Count - 1;
			else if (_currentTab >= _roots.Count)
				_currentTab = 0;

			if (increment < 0)
				AnimateLeft(_roots[_currentTab]);
			else if (increment > 0)
				AnimateRight(_roots[_currentTab]);

			_targetBranch.SelectedIndex = -1;
			_activeBranch.SelectedIndex = -1;
		}

		private void AnimateLeft(TreeItem target)
		{
			_lastTime = DateTime.Now.Ticks;
			_animationTarget = 0;
			_targetBranch = target;
		}

		private void AnimateRight(TreeItem target)
		{
			if (_animationTarget == 0)
			{
				_animationTarget = 1;
				_targetBranch = target;
			}
			else if (_animation == 1.0f)
			{
				_animation = 0.0f;
				_targetBranch = _activeBranch;
				_currentBranch = target;
			}

			_lastTime = DateTime.Now.Ticks;
		}

		private void GoToParent()
		{
			if (_activeBranch.Parent != null)
				AnimateLeft(_activeBranch.Parent);
		}

		private void SelectChild()
		{
			var selection = _activeBranch.SelectedItem;

			if (selection != null)
			{
				if (selection.IsBranch)
				{
					AnimateRight(selection);
				}
				else
				{
					_selectionMade = true;
					_selectionState.Tab = _currentTab;
					_selectionState.Index = selection.ItemIndex;
					Close();
				}
			}
		}
	}
}
