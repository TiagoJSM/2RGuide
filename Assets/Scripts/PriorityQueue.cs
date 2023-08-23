using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    class PriorityQueue<TData, TPriority> where TPriority : IComparable
    {
        private struct DataNode
        {
            public TData data;
            public TPriority priority;
        }

        private List<DataNode> _list;
        public int Count { get { return _list.Count; } }
        public readonly bool IsDescending;

        public PriorityQueue()
        {
            _list = new List<DataNode>();
        }

        public PriorityQueue(bool isdesc)
            : this()
        {
            IsDescending = isdesc;
        }

        public PriorityQueue(int capacity)
            : this(capacity, false)
        { }

        public PriorityQueue(IEnumerable<TData> collection)
            : this(collection, false)
        { }

        public PriorityQueue(int capacity, bool isdesc)
        {
            _list = new List<DataNode>(capacity);
            IsDescending = isdesc;
        }

        public PriorityQueue(IEnumerable<TData> collection, bool isdesc)
            : this()
        {
            IsDescending = isdesc;
            foreach (var item in collection)
                Enqueue(item, default);
        }


        public void Enqueue(TData x, TPriority priority)
        {
            var dataNode = new DataNode
            {
                data = x,
                priority = priority
            };
            _list.Add(dataNode);
            int i = Count - 1;

            while (i > 0)
            {
                int p = (i - 1) / 2;
                if ((IsDescending ? -1 : 1) * _list[p].priority.CompareTo(dataNode.priority) <= 0) break;

                _list[i] = _list[p];
                i = p;
            }

            if (Count > 0) _list[i] = dataNode;
        }

        public TData Dequeue()
        {
            TData target = Peek();
            var root = _list[Count - 1];
            _list.RemoveAt(Count - 1);

            int i = 0;
            while (i * 2 + 1 < Count)
            {
                int a = i * 2 + 1;
                int b = i * 2 + 2;
                int c = b < Count && (IsDescending ? -1 : 1) * _list[b].priority.CompareTo(_list[a].priority) < 0 ? b : a;

                if ((IsDescending ? -1 : 1) * _list[c].priority.CompareTo(root.priority) >= 0) break;
                _list[i] = _list[c];
                i = c;
            }

            if (Count > 0) _list[i] = root;
            return target;
        }

        public TData Peek()
        {
            if (Count == 0) throw new InvalidOperationException("Queue is empty.");
            return _list[0].data;
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(TData node)
        {
            return _list.Any(n => n.data.Equals(node));
        }
    }
}