using Assets._2RGuide.Runtime.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets._2RGuide.Runtime.Helpers
{
    public static class RGuideVector2Extensions
    {
        public static IEnumerable<LineSegment2D> ToLines(this IEnumerable<RGuideVector2> points) 
        {
            for (var idx = 0; idx < (points.Count() - 1); idx++)
            {
                var p1 = points.ElementAt(idx);
                var p2 = points.ElementAt(idx + 1);
                var line = new LineSegment2D(p1, p2);

                yield return line;
            }
        }
    }
}
