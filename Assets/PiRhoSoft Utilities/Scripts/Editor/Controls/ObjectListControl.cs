using System;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PiRhoSoft.UtilityEditor
{
	public class ObjectListControl : ListControl
	{
		private Action<IList> _customAdd;
		private Action<IList, int> _customEdit;
		private Action<IList, int> _customRemove;
		private Action<Rect, IList, int> _customDraw;

		public ObjectListControl Setup(IList list)
		{
			var reorderableList = new ReorderableList(list, null, false, true, false, false);
			Setup(reorderableList);

			return this;
		}

		public ObjectListControl MakeDrawable(Action<Rect, IList, int> callback)
		{
			_customDraw = callback;
			return this;
		}

		public ObjectListControl MakeAddable(IconButton icon, Action<IList> callback = null)
		{
			MakeHeaderButton(icon, Add, Color.white);
			_customAdd = callback;
			return this;
		}

		public ObjectListControl MakeRemovable(IconButton icon, Action<IList, int> callback = null)
		{
			MakeItemButton(icon, Remove, Color.white);
			_customRemove = callback;
			return this;
		}

		public ObjectListControl MakeEditable(IconButton icon, Action<IList, int> callback = null)
		{
			MakeItemButton(icon, Edit, Color.white);
			_customEdit = callback;
			return this;
		}

		public void DoDefaultAdd()
		{
			List.list.Add(null);
		}

		public void DoDefaultEdit(int index)
		{
			var obj = List.list[index] as Object;
			if (obj != null)
				Selection.activeObject = obj;
		}

		public void DoDefaultRemove(int index)
		{
			List.list.RemoveAt(index);
		}

		private void Add(Rect rect)
		{
			if (_customAdd != null)
				_customAdd(List.list);
			else 
				DoDefaultAdd();
		}

		private void Edit(Rect rect, int index)
		{
			if (_customEdit != null)
				_customEdit(List.list, index);
			else
				DoDefaultEdit(index);
		}

		private void Remove(Rect rect, int index)
		{
			if (_customRemove != null)
				_customRemove(List.list, index);
			else
				DoDefaultRemove(index);
		}

		protected override void Draw(Rect rect, int index)
		{
			_customDraw?.Invoke(rect, List.list, index);
		}
	}
}
