using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Node
    {
        public Vector2 Position { get; set; }

        public List<Node> Connections { get; }

        public Node()
        {
            Connections = new List<Node>();
        }
    }

    public class AStar
    {
        public Node[] Resolve(Node start, Node goal)
        {
            var queue = new PriorityQueue<Node, float>();
            queue.Enqueue(start, 0);

            var cameFrom = new Dictionary<Node, Node>();

            var gScore = new Dictionary<Node, float>
            {
                { start, 0.0f }
            };//map with default value of Infinity

            var fScore = new Dictionary<Node, float>
            {
                { start, Heuristic(start, goal) }
            };

            while(queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == goal)
                {
                    return ReconstructPath(cameFrom, current, goal);
                }

                foreach (var neighbor in current.Connections)
                {
                    var tentativeGScore = gScore[current] + Vector2.Distance(current.Position, neighbor.Position);
                    if (tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        var currentFScore = tentativeGScore + Heuristic(neighbor, goal);
                        fScore[neighbor] = currentFScore;
                        if (!queue.Contains(neighbor))
                        {
                            queue.Enqueue(neighbor, currentFScore);
                        }
                    }
                }
            }

            return null;
        }

        private Node[] ReconstructPath(Dictionary<Node, Node> cameFrom, Node current, Node goal)
        {
            var path = new List<Node>() { current };
            while (current != goal)
            {
                current = cameFrom[current];
                path.Add(current);
            }
            return path.ToArray();
        }


        private float Heuristic(Node node, Node goal)
        {
            return Vector2.Distance(node.Position, goal.Position);
        }
    }
}