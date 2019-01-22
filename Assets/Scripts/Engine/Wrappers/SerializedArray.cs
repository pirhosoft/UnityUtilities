using System;
using System.Collections;
using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	// This class is nothing more than a direct wrapper of the built in Array class from the .net framework. Its purpose
	// is to provide a base class for array types to derive from that can then be targeted by an ArrayDisplay.

	public class SerializedArray<T> : ICloneable, IList, IStructuralComparable, IStructuralEquatable, ICollection, IEnumerable
	{
		public T[] Array => _array;

		// This is protected so it can be found by the editor.
		[SerializeField] protected T[] _array;

		public SerializedArray(int count)
		{
			_array = new T[count];
		}

		public T this[int index]
		{
			get { return _array[index]; }
			set { _array[index] = value; }
		}

		public int Length => _array.Length;

		#region ICollection Implementation

		int ICollection.Count
		{
			get { return ((ICollection)_array).Count; }
		}

		public bool IsSynchronized
		{
			get { return _array.IsSynchronized; }
		}

		public object SyncRoot
		{
			get { return _array.SyncRoot; }
		}

		public void CopyTo(Array array, int index)
		{
			_array.CopyTo(array, index);
		}

		#endregion

		#region IClonable Implementation

		public object Clone()
		{
			return _array.Clone();
		}

		#endregion

		#region IComparable Implementation

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			return ((IStructuralComparable)_array).CompareTo(other, comparer);
		}

		#endregion

		#region IStructuralEquatable Implementation

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			return ((IStructuralEquatable)_array).Equals(other, comparer);
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			return ((IStructuralEquatable)_array).GetHashCode(comparer);
		}

		#endregion

		#region IEnumerable Implementation

		public IEnumerator GetEnumerator()
		{
			return _array.GetEnumerator();
		}

		#endregion

		#region IList Implementation

		object IList.this[int index]
		{
			get { return _array[index]; }
			set { ((IList)_array)[index] = value; }
		}

		public bool IsFixedSize
		{
			get { return _array.IsFixedSize; }
		}

		public bool IsReadOnly
		{
			get { return _array.IsReadOnly; }
		}

		int IList.Add(object value)
		{
			return ((IList)_array).Add(value);
		}

		void IList.Clear()
		{
			((IList)_array).Clear();
		}

		bool IList.Contains(object value)
		{
			return ((IList)_array).Contains(value);
		}

		int IList.IndexOf(object value)
		{
			return ((IList)_array).IndexOf(value);
		}

		void IList.Insert(int index, object value)
		{
			((IList)_array).Insert(index, value);
		}

		void IList.Remove(object value)
		{
			((IList)_array).Remove(value);
		}

		void IList.RemoveAt(int index)
		{
			((IList)_array).RemoveAt(index);
		}

		#endregion
	}
}
