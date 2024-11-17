using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets._2RGuide.Runtime.Math
{
    public class PolygonComposite
    {
        private List<Polygon> _polygons;
        public PolygonComposite(IEnumerable<Polygon> polygons)
        {
            _polygons = polygons.ToList();
        }

        public bool IsPointInPolygon(RGuideVector2 testPoint)
        {
            return _polygons.Any(p => p.IsPointInPolygon(testPoint));
        }
    }
}
