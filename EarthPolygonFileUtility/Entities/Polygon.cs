using System.Collections.Generic;

namespace EarthPolygonFileUtility.Entities
{
    public class Polygon
    {
        public int PlantID { get; set; }
        public List<Coordinate> Coordinates { get; set; } = new List<Coordinate>();
    }
}