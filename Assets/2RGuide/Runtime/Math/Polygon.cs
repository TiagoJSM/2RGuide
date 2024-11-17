using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets._2RGuide.Runtime.Math
{
    public class Polygon
    {
        private List<RGuideVector2> _polygonVertices;
        public Polygon(IEnumerable<RGuideVector2> polygonVertices) 
        {
            _polygonVertices = polygonVertices.ToList();
        }

        /// <summary>
        /// Determines if the given point is inside the polygon, taken from https://stackoverflow.com/questions/4243042/c-sharp-point-in-polygon
        /// </summary>
        /// <param name="polygon">the vertices of polygon</param>
        /// <param name="testPoint">the given point</param>
        /// <returns>true if the point is inside the polygon; otherwise, false</returns>
        public bool IsPointInPolygon(RGuideVector2 testPoint)
        {
            var result = false;
            var j = _polygonVertices.Count - 1;
            for (int i = 0; i < _polygonVertices.Count; i++)
            {
                if (_polygonVertices[i].y < testPoint.y && _polygonVertices[j].y >= testPoint.y ||
                    _polygonVertices[j].y < testPoint.y && _polygonVertices[i].y >= testPoint.y)
                {
                    if (_polygonVertices[i].x + (testPoint.y - _polygonVertices[i].y) /
                       (_polygonVertices[j].y - _polygonVertices[i].y) *
                       (_polygonVertices[j].x - _polygonVertices[i].x) < testPoint.x)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }
    }
}
