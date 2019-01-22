using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	// This class is nothing more than a direct wrapper of the built in List class from the .net framework. Its purpose
	// is to provide a base class for list types to derive from that can then be targeted by a ListDisplay.

	[Serializable]
	public class SerializedList<T> : ICollection<T>, IEnumerable<T>, IEnumerable, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection, IList
	{
		public List<T> List => _list;

		// This is protected so it can be found by the editor.
		[SerializeField] protected List<T> _list = new List<T>();

		#region ICollection<T> Implementation

		public int Count
		{
			get { return _list.Count; }
		}

		bool ICollection<T>.IsReadOnly
		{
			get { return false; }
		}

		public void Add(T item)
		{
			_list.Add(item);
		}

		public bool Remove(T item)
		{
			return _list.Remove(item);
		}

		public void Clear()
		{
			_list.Clear();
		}

		public bool Contains(T item)
		{
			return _list.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_list.CopyTo(array, arrayIndex);
		}

		#endregion

		#region ICollection Implementation

		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		object ICollection.SyncRoot
		{
			get { return this; }
		}

		void ICollection.CopyTo(Array array, int index)
		{
			((ICollection)_list).CopyTo(array, index);
		}

		#endregion

		#region IEnumerable<T> Implementation

		public IEnumerator<T> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		#endregion

		#region IEnumerable Implementation

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		#endregion

		#region IList<T> Implementation

		public T this[int index]
		{
			get { return _list[index]; }
			set { _list[index] = value; }
		}

		public int IndexOf(T item)
		{
			return _list.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			_list.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			_list.RemoveAt(index);
		}

		#endregion

		#region IList Implementation

		object IList.this[int index]
		{
			get { return _list[index]; }
			set { ((IList)_list)[index] = value; }
		}

		bool IList.IsFixedSize
		{
			get { return false; }
		}

		bool IList.IsReadOnly
		{
			get { return false; }
		}

		int IList.Add(object value)
		{
			return ((IList)_list).Add(value);
		}

		void IList.Insert(int index, object value)
		{
			((IList)_list).Insert(index, value);
		}

		void IList.Remove(object value)
		{
			((IList)_list).Remove(value);
		}

		bool IList.Contains(object value)
		{
			return ((IList)_list).Contains(value);
		}

		int IList.IndexOf(object value)
		{
			return ((IList)_list).IndexOf(value);
		}

		#endregion
	}
}
