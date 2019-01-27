using System.Collections.Generic;
using UnityEngine;

namespace PiRhoSoft.UtilityEngine
{
	public class DropTable<T>
	{
		[SerializeField] private int _totalWeight = 0;
		[SerializeField] private List<int> _weights = new List<int>();
		[SerializeField] private List<T> _values = new List<T>();

		public int Count => _values.Count;
		public int TotalWeight => _totalWeight;
		public List<int> Weights => _weights;
		public List<T> Values => _values;

		public void Add(int weight, T value)
		{
			_weights.Add(weight);
			_values.Add(value);
			_totalWeight += weight;
		}

		public bool Remove(int index)
		{
			if (index < 0 || index >= _values.Count)
				return false;

			_totalWeight -= _weights[index];
			_weights.RemoveAt(index);
			_values.RemoveAt(index);

			return true;
		}

		public void Clear()
		{
			_totalWeight = 0;
			_weights.Clear();
			_values.Clear();
		}

		public int GetWeight(int index)
		{
			if (index < 0 || index >= _weights.Count)
				return 0;

			return _weights[index];
		}

		public float GetPercentageWeight(int index)
		{
			if (index < 0 || index >= _weights.Count || _totalWeight == 0.0f)
				return 0.0f;

			return _weights[index] * 100.0f / _totalWeight;
		}

		public T GetValue(int index)
		{
			if (index < 0 || index >= _values.Count)
				return default;

			return _values[index];
		}

		public bool ChangeWeight(int index, int weight)
		{
			if (index < 0 || index >= _values.Count)
				return false;

			var old = _weights[index];
			_totalWeight += weight - old;
			_weights[index] = weight;

			return true;
		}

		public bool ChangeValue(int index, T value)
		{
			if (index < 0 || index >= _values.Count)
				return false;

			_values[index] = value;
			return true;
		}

		public T PickValue()
		{
			var value = Random.Range(0, _totalWeight);
			for (var i = 0; i < _weights.Count; i++)
			{
				var weight = _weights[i];
				if (value < weight)
					return _values[i];

				value -= weight;
			}

			return default;
		}
	}
}
