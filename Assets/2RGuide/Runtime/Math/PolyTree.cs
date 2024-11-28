using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets._2RGuide.Runtime.Math
{
    public class PolyTree
    {
        private class PolyTreeNode
        {
            private List<PolyTreeNode> _children;

            public bool IsHole { get; set; }
            public List<PolyTreeNode> Children => _children;
            public Polygon Polygon { get; }

            public PolyTreeNode(bool isHole, Polygon polygon)
            {
                IsHole = isHole;
                Polygon = polygon;
                _children = new List<PolyTreeNode>();
            }
        }

        private PolyTreeNode _root;

        public PolyTree(IEnumerable<Polygon> polygons)
        {
            _root = new PolyTreeNode(true, Polygon.Infinite);
            PopulateNodes(polygons);
            SetIsHoleState();
        }

        public bool IsPointInside(RGuideVector2 point)
        {
            var node = FindNodeContaining(point, _root);
            return node == null ? false : !node.IsHole;
        }

        private PolyTreeNode FindNodeContaining(RGuideVector2 point, PolyTreeNode node)
        {
            if (node.Polygon.IsPointInPolygon(point))
            {
                foreach(var child in node.Children)
                {
                    var childContaining = FindNodeContaining(point, child);
                    if(childContaining != null)
                    {
                        return childContaining;
                    }
                }
                return node;
            }
            return null;
        }

        private void PopulateNodes(IEnumerable<Polygon> polygons)
        {
            foreach (var polygon in polygons)
            {
                var parent = FindParentNode(polygon);

                if (parent == null)
                {
                    Debug.LogError("Error finding place for polygon");
                }
                else
                {
                    AddToParent(parent, polygon);
                }
            }
        }

        private void SetIsHoleState()
        {
            SetIsHoleState(_root, true);
        }

        private void SetIsHoleState(PolyTreeNode node, bool isHole)
        {
            node.IsHole = isHole;
            foreach (var child in node.Children)
            {
                SetIsHoleState(child, !isHole);
            }
        }

        private PolyTreeNode FindParentNode(Polygon polygon)
        {
            return FindParentNode(_root, polygon);
        }

        private PolyTreeNode FindParentNode(PolyTreeNode node, Polygon polygon)
        {
            if (node.Polygon.Contains(polygon))
            {
                foreach(var child in node.Children)
                {
                    var parent = FindParentNode(child, polygon);
                    if(parent != null)
                    {
                        return parent;
                    }
                }

                return node;
            }
            return null;
        }

        private void AddToParent(PolyTreeNode parent, Polygon polygon)
        {
            var childrenUnderNewPolygon = parent.Children.Where(c => polygon.Contains(c.Polygon));
            var newNode = new PolyTreeNode(false, polygon);
            newNode.Children.AddRange(childrenUnderNewPolygon);
            foreach(var oldChild in childrenUnderNewPolygon)
            {
                parent.Children.Remove(oldChild);
            }
            parent.Children.Add(newNode);
        }
    }
}
