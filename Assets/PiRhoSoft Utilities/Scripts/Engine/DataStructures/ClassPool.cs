using System.Collections.Generic;

namespace PiRhoSoft.UtilityEngine
{
	public interface IPoolable
	{
		void Reset();
	}

	public interface IPoolInfo
	{
		int Size { get; }
		int Growth { get; }
	}

	public interface IClassPool<T> where T : IPoolable
	{
		void Grow();
		T Reserve();
		void Release(T value);
	}

	public class ClassPool<T, I> : IClassPool<T> where T : IPoolable, new() where I : IPoolInfo, new()
	{
		private static IPoolInfo _info = new I();
		private Stack<T> _freeList;

		public ClassPool()
		{
			_freeList = new Stack<T>(_info.Size);

			for (var i = 0; i < _info.Size; i++)
				Release(new T());
		}

		public void Grow()
		{
			for (var i = 0; i < _info.Growth; i++)
				Release(new T());
		}

		public T Reserve()
		{
			if (_freeList.Count == 0)
				Grow();

			return _freeList.Pop();
		}

		public void Release(T value)
		{
			value.Reset();
			_freeList.Push(value);
		}
	}

	public class PoolInfo_2_1 : IPoolInfo
	{
		public int Size => 2;
		public int Growth => 1;
	}

	public class PoolInfo_10_5 : IPoolInfo
	{
		public int Size => 10;
		public int Growth => 5;
	}

	public class PoolInfo_25_5 : IPoolInfo
	{
		public int Size => 25;
		public int Growth => 5;
	}

	public class PoolInfo_50_10 : IPoolInfo
	{
		public int Size => 50;
		public int Growth => 10;
	}

	public class PoolInfo_100_10 : IPoolInfo
	{
		public int Size => 100;
		public int Growth => 10;
	}
}
