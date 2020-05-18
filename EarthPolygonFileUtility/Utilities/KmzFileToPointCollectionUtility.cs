using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Xml;
using EarthPolygonFileUtility.Entities;

namespace EarthPolygonFileUtility
{
    public class KmzFileToPointCollectionUtility
    {
        public string Url { get; }

        public KmzFileToPointCollectionUtility(string url)
        {
            Url = url;
        }

        public List<Polygon> GetPolygons(int plantId, string downloadedFile, string fileId)
        {
            Directory.CreateDirectory(Program.TemporaryDataDirectory);

            string downloadedZipFile = downloadedFile.Replace(".kmz", ".zip");
            File.Move(downloadedFile, downloadedZipFile);
            string unzippedDirectory = Path.Combine(Program.TemporaryDataDirectory, fileId);
            ZipFile.ExtractToDirectory(downloadedZipFile, unzippedDirectory);

            DirectoryInfo dir = new DirectoryInfo(unzippedDirectory);
            FileInfo[] f = dir.GetFiles();
            FileInfo file = f[0];
            string xmlFilename = file.FullName.Replace(".kml", ".xml");
            File.Move(file.FullName, xmlFilename);

            List<Polygon> polygons = parsePolygonsFromXml(xmlFilename);
            polygons.ForEach(pgon => pgon.PlantID = plantId);
            return polygons;
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

            return polygons;
        }
    }
}