using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts._2RGuide
{
    // https://github.com/BadEcho/core/blob/master/src/Game/ISpatialEntity.cs
    /*public sealed class Quadtree<TData>
    {
        private readonly List<ISpatialEntity> _elements = new List<ISpatialEntity>();

        private readonly int _bucketCapacity;
        private readonly int _maxDepth;

        private Quadtree<TData> _topLeft, _topRight, _bottomLeft, _bottomRight;
        private Func<TData, RectangleF> _getBounds;

        public Quadtree(RectangleF bounds, Func<TData, RectangleF> getBounds)
            : this(bounds, 32, 5, getBounds)
        { }

        public Quadtree(RectangleF bounds, int bucketCapacity, int maxDepth, Func<TData, RectangleF> getBounds)
        {
            _bucketCapacity = bucketCapacity;
            _maxDepth = maxDepth;
            Bounds = bounds;
            _getBounds = getBounds;
        }

        public RectangleF Bounds { get; }

        public int Level { get; set; }

        public bool IsLeaf => _topLeft == null || _topRight == null || _bottomLeft == null || _bottomRight == null;

        public void Insert(ISpatialEntity element)
        {
            if (!Bounds.Contains(element.Bounds))
            {
                throw new ArgumentException("Element outside of bounds", nameof(element));
            }

            // A node exceeding its allotted number of items will get split (if it hasn't been already) into four equal quadrants.
            if (_elements.Count >= _bucketCapacity)
            {
                Split();
            }

            var containingChild = GetContainingChild(element.Bounds);

            if (containingChild != null)
            {
                containingChild.Insert(element);
            }
            else
            {   // If no child was returned, then this is either a leaf node, or the element's boundaries overlap multiple children.
                // Either way, the element gets assigned to this node.
                _elements.Add(element);
            }
        }

        public bool Remove(ISpatialEntity element)
        {
            var containingChild = GetContainingChild(element.Bounds);

            // If no child was returned, then this is the leaf node (or potentially non-leaf node, if the element's boundaries overlap
            // multiple children) containing the element.
            bool removed = containingChild?.Remove(element) ?? _elements.Remove(element);

            // If the total descendant element count is less than the bucket capacity, we ensure the node is in a non-split state.
            if (removed && CountElements() <= _bucketCapacity)
            {
                Merge();
            }

            return removed;
        }

        public IEnumerable<ISpatialEntity> FindCollisions(ISpatialEntity element)
        {
            var nodes = new Queue<Quadtree<TData>>();
            var collisions = new List<ISpatialEntity>();

            nodes.Enqueue(this);

            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();

                if (!element.Bounds.Intersects(node.Bounds))
                    continue;

                collisions.AddRange(node._elements.Where(e => e.Bounds.Intersects(element.Bounds)));

                if (!node.IsLeaf)
                {
                    if (element.Bounds.Intersects(node._topLeft.Bounds))
                    {
                        nodes.Enqueue(node._topLeft);
                    }

                    if (element.Bounds.Intersects(node._topRight.Bounds))
                    {
                        nodes.Enqueue(node._topRight);
                    }

                    if (element.Bounds.Intersects(node._bottomLeft.Bounds))
                    {
                        nodes.Enqueue(node._bottomLeft);
                    }

                    if (element.Bounds.Intersects(node._bottomRight.Bounds))
                    {
                        nodes.Enqueue(node._bottomRight);
                    }
                }
            }

            return collisions;
        }

        public int CountElements()
        {
            int count = _elements.Count;

            if (!IsLeaf)
            {
                count += _topLeft.CountElements();
                count += _topRight.CountElements();
                count += _bottomLeft.CountElements();
                count += _bottomRight.CountElements();
            }

            return count;
        }

        public IEnumerable<ISpatialEntity> GetElements()
        {
            var children = new List<ISpatialEntity>();
            var nodes = new Queue<Quadtree<TData>>();

            nodes.Enqueue(this);

            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();

                if (!node.IsLeaf)
                {
                    nodes.Enqueue(node._topLeft);
                    nodes.Enqueue(node._topRight);
                    nodes.Enqueue(node._bottomLeft);
                    nodes.Enqueue(node._bottomRight);
                }

                children.AddRange(node._elements);
            }

            return children;
        }

        private void Split()
        {   // If we're not a leaf node, then we're already split.
            if (!IsLeaf)
            {
                return;
            }

            // Splitting is only allowed if it doesn't cause us to exceed our maximum depth.
            if (Level + 1 > _maxDepth)
            {
                return;
            }
            
            _topLeft = CreateChild(Bounds.Location);
            _topRight = CreateChild(new PointF(Bounds.Center.X, Bounds.Location.Y));
            _bottomLeft = CreateChild(new PointF(Bounds.Location.X, Bounds.Center.Y));
            _bottomRight = CreateChild(new PointF(Bounds.Center.X, Bounds.Center.Y));

            var elements = _elements.ToList();

            foreach (var element in elements)
            {
                var containingChild = GetContainingChild(element.Bounds);
                // An element is only moved if it completely fits into a child quadrant.
                if (containingChild != null)
                {
                    _elements.Remove(element);

                    containingChild.Insert(element);
                }
            }
        }

        private Quadtree<TData> CreateChild(PointF location)
            => new Quadtree<TData>(new RectangleF(location, new SizeF(Bounds.Size.Width / 2.0f, Bounds.Size.Height / 2.0f)), _bucketCapacity, _maxDepth, _getBounds)
            {
                Level = Level + 1
            };

        private void Merge()
        {   // If we're a leaf node, then there is nothing to merge.
            if (IsLeaf)
            {
                return;
            }

            _elements.AddRange(_topLeft._elements);
            _elements.AddRange(_topRight._elements);
            _elements.AddRange(_bottomLeft._elements);
            _elements.AddRange(_bottomRight._elements);

            _topLeft = _topRight = _bottomLeft = _bottomRight = null;
        }

        private Quadtree<TData> GetContainingChild(IShape bounds)
        {
            if (IsLeaf)
            {
                return null;
            }
            
            if (_topLeft.Bounds.Contains(bounds))
            {
                return _topLeft;
            }

            if (_topRight.Bounds.Contains(bounds))
            {
                return _topRight;
            }

            if (_bottomLeft.Bounds.Contains(bounds))
            {
                return _bottomLeft;
            }

            return _bottomRight.Bounds.Contains(bounds) ? _bottomRight : null;
        }
    }*/
}