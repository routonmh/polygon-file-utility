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
        public List<Polygon> GetPolygons(int plantId, string kmzFile, string fileId)
        {
            List<Polygon> polygons = new List<Polygon>();

            Directory.CreateDirectory(Program.TemporaryFilesDirectory);

            try
            {
                string zipFilename = kmzFile.Replace(".kmz", ".zip");
                if (File.Exists(zipFilename))
                    File.Delete(zipFilename);
                File.Copy(kmzFile, zipFilename);

                string unzippedDirectory = Path.Combine(Program.TemporaryFilesDirectory, fileId);
                if (Directory.Exists(unzippedDirectory))
                    Directory.Delete(unzippedDirectory, true);
                ZipFile.ExtractToDirectory(zipFilename, unzippedDirectory);

                DirectoryInfo dir = new DirectoryInfo(unzippedDirectory);
                FileInfo[] f = dir.GetFiles();
                FileInfo file = f[0];

                string xmlFilename = file.FullName.Replace(".kml", ".xml");
                File.Copy(file.FullName, xmlFilename);

                polygons = parsePolygonsFromXml(xmlFilename);
                polygons.ForEach(pgon => pgon.PlantID = plantId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get polygons for: {kmzFile}\n{ex.Message}");
            }

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