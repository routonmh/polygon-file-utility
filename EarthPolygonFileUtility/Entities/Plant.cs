namespace EarthPolygonFileUtility.Entities
{
    public class Plant
    {
        public int PlantID { get; set; }
        public string CommonName { get; set; }
        public string ScientificName { get; set; }
        public string PlantDescription { get; set; }
        public bool IsEdible { get; set; }
    }
}