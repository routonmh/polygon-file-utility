using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using EarthPolygonFileUtility.Entities;

namespace EarthPolygonFileUtility
{
    public class Controller
    {
        public void Start()
        {
            TableFileRecordUtility fileRecordUtility = new TableFileRecordUtility();
            fileRecordUtility.Run();

            Console.WriteLine($"Got {fileRecordUtility.PlantRecords.Count} plant records.");
            Console.WriteLine($"Got {fileRecordUtility.AttributeRecords.Count} attribute records.");
            Console.WriteLine($"Got {fileRecordUtility.RegionShapeFileRecords.Count} regions shape file records.");

            fileRecordUtility.AttributeRecords =
                fileRecordUtility.AttributeRecords.Select((it, idx) =>
                {
                    it.AttributeID = idx;
                    return it;
                }).ToList();

            fileRecordUtility.RegionShapeFileRecords =
                fileRecordUtility.RegionShapeFileRecords.Select((it, idx) =>
                {
                    it.RegionShapeFileID = idx;
                    return it;
                }).ToList();

            List<Polygon> polygons = fetchAndReadKmzFiles(fileRecordUtility, 250);
            List<Coordinate> coordinates = new List<Coordinate>();

            int coordinateIdx = 0;
            polygons = polygons.Select((it, idx) =>
            {
                it.PolygonID = idx;
                it.Coordinates.ForEach(x =>
                {
                    x.PolygonID = idx;
                    x.CoordinateID = coordinateIdx++;
                    coordinates.Add(x);
                });
                return it;
            }).ToList();

            string plantRowsStr = createCsvRowStr(new List<KeyValuePair<string, bool>>()
            {
                new KeyValuePair<string, bool>("PlantID", false),
                new KeyValuePair<string, bool>("CommonName", true),
                new KeyValuePair<string, bool>("ScientificName", true),
                new KeyValuePair<string, bool>("PlantDescription", true),
                new KeyValuePair<string, bool>("IsEdible", false)
            }, fileRecordUtility.PlantRecords.Select(it => new List<string>
            {
                it.PlantID.ToString(), it.CommonName, it.ScientificName, it.PlantDescription, it.IsEdible ? "1" : "0"
            }).ToList());

            string attributeRowsStr = createCsvRowStr(new List<KeyValuePair<string, bool>>()
            {
                new KeyValuePair<string, bool>("AttributeID", false),
                new KeyValuePair<string, bool>("PlantID", false),
                new KeyValuePair<string, bool>("AttributeDescription", true)
            }, fileRecordUtility.AttributeRecords.Select(it => new List<string>
            {
                it.AttributeID.ToString(), it.PlantID.ToString(), it.AttributeDescription
            }).ToList());

            string regionShapeFileRowsStr = createCsvRowStr(new List<KeyValuePair<string, bool>>()
            {
                new KeyValuePair<string, bool>("RegionShapeFileID", false),
                new KeyValuePair<string, bool>("PlantID", false),
                new KeyValuePair<string, bool>("LinkToFile", true)
            }, fileRecordUtility.RegionShapeFileRecords.Select(it => new List<string>
            {
                it.RegionShapeFileID.ToString(), it.PlantID.ToString(), it.LinkToFile
            }).ToList());

            string polygonRowsStr = createCsvRowStr(new List<KeyValuePair<string, bool>>()
            {
                new KeyValuePair<string, bool>("PolygonID", false),
                new KeyValuePair<string, bool>("PlantID", false)
            }, polygons.Select(it => new List<string>
            {
                it.PolygonID.ToString(), it.PlantID.ToString()
            }).ToList());

            string coordinatesRowsStr = createCsvRowStr(new List<KeyValuePair<string, bool>>()
            {
                new KeyValuePair<string, bool>("CoordinateID", false),
                new KeyValuePair<string, bool>("PolygonID", false),
                new KeyValuePair<string, bool>("Latitude", false),
                new KeyValuePair<string, bool>("Longitude", false)
            }, coordinates.Select(it => new List<string>
            {
                it.CoordinateID.ToString(), it.PolygonID.ToString(), it.Latitude.ToString(), it.Longitude.ToString()
            }).ToList());

            File.WriteAllText("plant.csv", plantRowsStr);
            File.WriteAllText("attribute.csv", attributeRowsStr);
            File.WriteAllText("region-shape-file.csv", regionShapeFileRowsStr);
            File.WriteAllText("polygon.csv", polygonRowsStr);
            File.WriteAllText("polygon-coordinate.csv", coordinatesRowsStr);

            Console.WriteLine("Wrote csv files.");
        }

        private List<Polygon> fetchAndReadKmzFiles(TableFileRecordUtility fileRecordUtility, int requestDelayMs)
        {
            List<Polygon> allPolygons = new List<Polygon>();

            for (int idx = 0; idx < fileRecordUtility.RegionShapeFileRecords.Count; idx++)
            {
                RegionShapeFile rsf = fileRecordUtility.RegionShapeFileRecords[idx];
                Console.WriteLine($"Downloading: {rsf.LinkToFile}");
                KmzFileToPointCollectionUtility kmzUtility = new KmzFileToPointCollectionUtility(rsf.LinkToFile);
                kmzUtility.GetPolygons(rsf.PlantID).ForEach(x => allPolygons.Add(x));
                // Wait to download the next file
                Thread.Sleep(requestDelayMs);
            }

            return allPolygons;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="rows">Key FieldName, Value: HasQuotes in CSV</param>
        /// <param name="includeHeaders"></param>
        /// <returns></returns>
        private string createCsvRowStr(List<KeyValuePair<string, bool>> fields, List<List<string>> rows,
            bool includeHeaders = false)
        {
            string strOfRows = "";

            if (includeHeaders)
                fields.ForEach(it => strOfRows += (string.Join(
                    ",", fields.Select(x => x.Key).ToArray()) + "\r\n"));

            for (int fvlIdx = 0; fvlIdx < rows.Count; fvlIdx++)
            {
                List<string> fieldValueList = rows[fvlIdx];

                for (int idx = 0; idx < fieldValueList.Count; idx++)
                {
                    bool inQuotes = fields[idx].Value;
                    if (inQuotes && fieldValueList[idx].Length > 0)
                        fieldValueList[idx] = $"'{fieldValueList[idx]}'";

                    fieldValueList[idx] = fieldValueList[idx].Replace("\"", "\\\"");
                }

                strOfRows += string.Join(",", fieldValueList) + "\r\n";
            }

            return strOfRows;
        }
    }
}