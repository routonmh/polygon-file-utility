using System.IO;

namespace EarthPolygonFileUtility
{
    public class FilenameUtility
    {
        public static string MakeCompliantFilename(string filename)
        {
            string compliantName = filename.Replace("'", "_");
            return compliantName;
        }
    }
}