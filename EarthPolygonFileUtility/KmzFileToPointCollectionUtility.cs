using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace EarthPolygonFileUtility
{
    public class KmzFileToPointCollectionUtility
    {
        public static readonly string TemporaryDataDirectory = "temporary-data";
        public string Url { get; }

        public KmzFileToPointCollectionUtility(string url)
        {
            Url = url;
        }

        public void Run()
        {
            string urlHashHex = Math.Abs(Url.GetHashCode()).ToString("X").ToLower();
            Directory.CreateDirectory(TemporaryDataDirectory);

            WebClient wc = new WebClient();
            string tempDownloadBase = urlHashHex + ".kmz";
            string downloadedFile = Path.Combine(TemporaryDataDirectory, tempDownloadBase);
            wc.DownloadFile(Url, downloadedFile);

            string downloadedZipFile = downloadedFile.Replace(".kmz", ".zip");
            File.Move(downloadedFile, downloadedZipFile);
            string unzippedDirectory = Path.Combine(TemporaryDataDirectory, urlHashHex);
            ZipFile.ExtractToDirectory(downloadedZipFile, unzippedDirectory);

            DirectoryInfo dir = new DirectoryInfo(unzippedDirectory);
            FileInfo[] f = dir.GetFiles();
            FileInfo file = f[0];
            string xmlFilename = file.FullName.Replace(".kml", ".xml");
            File.Move(file.FullName, xmlFilename);

            List<Polygon> polygons = parsePolygonsFromXml(xmlFilename);
        }


        private List<Polygon> parsePolygonsFromXml(string filepath)
        {
            List<Polygon> polygons = new List<Polygon>();

            XmlDocument xml = new XmlDocument();
            xml.Load(filepath);
            XmlNamespaceManager manager = new XmlNamespaceManager(xml.NameTable);
            manager.AddNamespace("ns", "http://www.opengis.net/kml/2.2");

            foreach (XmlNode node in xml.SelectNodes("//ns:coordinates", manager))
            {
                Polygon polygon = new Polygon();

                Console.WriteLine("{0}: {1}", node.Name, node.InnerText);
                string text = node.InnerText;
                text = text.Replace("\t", "");
                text = text.Replace("\n", "");

                string[] coordinateStrs = text.Split(" ");

                if (coordinateStrs.Length > 1)
                {
                    foreach (string cs in coordinateStrs)
                        if (!string.IsNullOrEmpty(cs))
                        {
                            string[] parts = cs.Split(",");
                            polygon.Coordinates.Add(new Coordinate()
                            {
                                Latitude = Convert.ToDouble(parts[0]),
                                Longitude = Convert.ToDouble(parts[1])
                            });
                        }

                    polygons.Add(polygon);
                }

            }

            int q = 1;
            return polygons;
        }
    }
}