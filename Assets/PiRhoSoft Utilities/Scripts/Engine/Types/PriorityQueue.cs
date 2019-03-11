using System;

namespace PiRhoSoft.UtilityEngine
{
	// MIT License - adopted from https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp

	public class PriorityQueueNode
	{
		public float Priority { get; protected internal set; }
		public int QueueIndex { get; internal set; }
	}

	public sealed class PriorityQueue<T> where T : PriorityQueueNode
	{
		public int Count { get; private set; }
		public int MaxSize => _nodes.Length - 1;
		public T First => _nodes[1];

		private T[] _nodes;

		public PriorityQueue(int maxNodes)
		{
			Count = 0;
			_nodes = new T[maxNodes + 1];
		}

		public void Clear()
		{
			Array.Clear(_nodes, 1, Count);
			Count = 0;
		}

		public bool Contains(T node)
		{
			return (_nodes[node.QueueIndex] == node);
		}

		public void Enqueue(T node, float priority)
		{
			node.Priority = priority;
			Count++;
			_nodes[Count] = node;
			node.QueueIndex = Count;
			CascadeUp(node);
		}

		public T Dequeue()
		{
			T returnMe = _nodes[1];
			//If the node is already the last node, we can remove it immediately
			if (Count == 1)
			{
				_nodes[1] = null;
				Count = 0;
				return returnMe;
			}

			//Swap the node with the last node
			T formerLastNode = _nodes[Count];
			_nodes[1] = formerLastNode;
			formerLastNode.QueueIndex = 1;
			_nodes[Count] = null;
			Count--;

			//Now bubble formerLastNode (which is no longer the last node) down
			CascadeDown(formerLastNode);
			return returnMe;
		}

		public void Resize(int maxNodes)
		{
			T[] newArray = new T[maxNodes + 1];
			int highestIndexToCopy = Math.Min(maxNodes, Count);
			Array.Copy(_nodes, newArray, highestIndexToCopy + 1);
			_nodes = newArray;
		}

		public void UpdatePriority(T node, float priority)
		{
			node.Priority = priority;
			OnNodeUpdated(node);
		}

		public void Remove(T node)
		{
			//If the node is already the last node, we can remove it immediately
			if (node.QueueIndex == Count)
			{
				_nodes[Count] = null;
				Count--;
				return;
			}

			//Swap the node with the last node
			T formerLastNode = _nodes[Count];
			_nodes[node.QueueIndex] = formerLastNode;
			formerLastNode.QueueIndex = node.QueueIndex;
			_nodes[Count] = null;
			Count--;

			//Now bubble formerLastNode (which is no longer the last node) up or down as appropriate
			OnNodeUpdated(formerLastNode);
		}

		private void CascadeUp(T node)
		{
			//aka Heapify-up
			int parent;
			if (node.QueueIndex > 1)
			{
				parent = node.QueueIndex >> 1;
				T parentNode = _nodes[parent];
				if (HasHigherOrEqualPriority(parentNode, node))
					return;

				//Node has lower priority value, so move parent down the heap to make room
				_nodes[node.QueueIndex] = parentNode;
				parentNode.QueueIndex = node.QueueIndex;

				node.QueueIndex = parent;
			}
			else
			{
				return;
			}
			while (parent > 1)
			{
				parent >>= 1;
				T parentNode = _nodes[parent];
				if (HasHigherOrEqualPriority(parentNode, node))
					break;

				//Node has lower priority value, so move parent down the heap to make room
				_nodes[node.QueueIndex] = parentNode;
				parentNode.QueueIndex = node.QueueIndex;

				node.QueueIndex = parent;
			}
			_nodes[node.QueueIndex] = node;
		}

		private void CascadeDown(T node)
		{
			//aka Heapify-down
			int finalQueueIndex = node.QueueIndex;
			int childLeftIndex = 2 * finalQueueIndex;

			// If leaf node, we're done
			if (childLeftIndex > Count)
			{
				return;
			}

			// Check if the left-child is higher-priority than the current node
			int childRightIndex = childLeftIndex + 1;
			T childLeft = _nodes[childLeftIndex];
			if (HasHigherPriority(childLeft, node))
			{
				// Check if there is a right child. If not, swap and finish.
				if (childRightIndex > Count)
				{
					node.QueueIndex = childLeftIndex;
					childLeft.QueueIndex = finalQueueIndex;
					_nodes[finalQueueIndex] = childLeft;
					_nodes[childLeftIndex] = node;
					return;
				}
				// Check if the left-child is higher-priority than the right-child
				T childRight = _nodes[childRightIndex];
				if (HasHigherPriority(childLeft, childRight))
				{
					// left is highest, move it up and continue
					childLeft.QueueIndex = finalQueueIndex;
					_nodes[finalQueueIndex] = childLeft;
					finalQueueIndex = childLeftIndex;
				}
				else
				{
					// right is even higher, move it up and continue
					childRight.QueueIndex = finalQueueIndex;
					_nodes[finalQueueIndex] = childRight;
					finalQueueIndex = childRightIndex;
				}
			}
			// Not swapping with left-child, does right-child exist?
			else if (childRightIndex > Count)
			{
				return;
			}
			else
			{
				// Check if the right-child is higher-priority than the current node
				T childRight = _nodes[childRightIndex];
				if (HasHigherPriority(childRight, node))
				{
					childRight.QueueIndex = finalQueueIndex;
					_nodes[finalQueueIndex] = childRight;
					finalQueueIndex = childRightIndex;
				}
				// Neither child is higher-priority than current, so finish and stop.
				else
				{
					return;
				}
			}

			while (true)
			{
				childLeftIndex = 2 * finalQueueIndex;

				// If leaf node, we're done
				if (childLeftIndex > Count)
				{
					node.QueueIndex = finalQueueIndex;
					_nodes[finalQueueIndex] = node;
					break;
				}

				// Check if the left-child is higher-priority than the current node
				childRightIndex = childLeftIndex + 1;
				childLeft = _nodes[childLeftIndex];
				if (HasHigherPriority(childLeft, node))
				{
					// Check if there is a right child. If not, swap and finish.
					if (childRightIndex > Count)
					{
						node.QueueIndex = childLeftIndex;
						childLeft.QueueIndex = finalQueueIndex;
						_nodes[finalQueueIndex] = childLeft;
						_nodes[childLeftIndex] = node;
						break;
					}
					// Check if the left-child is higher-priority than the right-child
					T childRight = _nodes[childRightIndex];
					if (HasHigherPriority(childLeft, childRight))
					{
						// left is highest, move it up and continue
						childLeft.QueueIndex = finalQueueIndex;
						_nodes[finalQueueIndex] = childLeft;
						finalQueueIndex = childLeftIndex;
					}
					else
					{
						// right is even higher, move it up and continue
						childRight.QueueIndex = finalQueueIndex;
						_nodes[finalQueueIndex] = childRight;
						finalQueueIndex = childRightIndex;
					}
				}
				// Not swapping with left-child, does right-child exist?
				else if (childRightIndex > Count)
				{
					node.QueueIndex = finalQueueIndex;
					_nodes[finalQueueIndex] = node;
					break;
				}
				else
				{
					// Check if the right-child is higher-priority than the current node
					T childRight = _nodes[childRightIndex];
					if (HasHigherPriority(childRight, node))
					{
						childRight.QueueIndex = finalQueueIndex;
						_nodes[finalQueueIndex] = childRight;
						finalQueueIndex = childRightIndex;
					}
					// Neither child is higher-priority than current, so finish and stop.
					else
					{
						node.QueueIndex = finalQueueIndex;
						_nodes[finalQueueIndex] = node;
						break;
					}
				}
			}
		}

		private bool HasHigherPriority(T higher, T lower)
		{
			return (higher.Priority < lower.Priority);
		}

		private bool HasHigherOrEqualPriority(T higher, T lower)
		{
			return (higher.Priority <= lower.Priority);
		}


		private void OnNodeUpdated(T node)
		{
			//Bubble the updated node up or down as appropriate
			int parentIndex = node.QueueIndex >> 1;

			if (parentIndex > 0 && HasHigherPriority(node, _nodes[parentIndex]))
			{
				CascadeUp(node);
			}
			else
			{
				//Note that CascadeDown will be called if parentNode == node (that is, node is the root)
				CascadeDown(node);
			}
		}
	}
}