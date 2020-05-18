using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
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

            List<Polygon> polygons = fetchAndReadKmzFiles(fileRecordUtility);
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

            Console.WriteLine($"{polygons.Count:N0} polygons contained {coordinates.Count:N0} coordinates");

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

            Directory.CreateDirectory(Program.OutputDirectory);

            File.WriteAllText(Path.Combine(Program.OutputDirectory, "plant.csv"), plantRowsStr);
            File.WriteAllText(Path.Combine(Program.OutputDirectory, "attribute.csv"), attributeRowsStr);
            File.WriteAllText(Path.Combine(Program.OutputDirectory, "region-shape-file.csv"), regionShapeFileRowsStr);
            File.WriteAllText(Path.Combine(Program.OutputDirectory, "polygon.csv"), polygonRowsStr);
            File.WriteAllText(Path.Combine(Program.OutputDirectory, "polygon-coordinate.csv"), coordinatesRowsStr);

            Console.WriteLine("Wrote csv files.");
        }

        private List<Polygon> fetchAndReadKmzFiles(TableFileRecordUtility fileRecordUtility)
        {
            List<Polygon> allPolygons = new List<Polygon>();

            for (int idx = 0; idx < fileRecordUtility.RegionShapeFileRecords.Count; idx++)
            {
                RegionShapeFile rsf = fileRecordUtility.RegionShapeFileRecords[idx];

                KmzFileToPointCollectionUtility kmzUtility = new KmzFileToPointCollectionUtility(rsf.LinkToFile);

                int backoffMs = 15000;
                bool successfullyDownloadedFile = false;
                while (!successfullyDownloadedFile)
                {
                    try
                    {
                        Console.WriteLine($"Attempting to download ({idx+1}/" +
                                          $"{fileRecordUtility.RegionShapeFileRecords.Count}): {rsf.LinkToFile}");
                        string fileId = Guid.NewGuid().ToString().ToLower();

                        WebClient wc = new WebClient();
                        wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; " +
                                                     "Windows NT 5.2; .NET CLR 1.0.3705;)");

                        string tempDownloadBase = fileId + ".kmz";
                        string downloadedFile = Path.Combine(Program.TemporaryDataDirectory, tempDownloadBase);
                        wc.DownloadFile(rsf.LinkToFile, downloadedFile);

                        successfullyDownloadedFile = true;
                        kmzUtility.GetPolygons(rsf.PlantID, downloadedFile, fileId).ForEach(x => allPolygons.Add(x));
                        Console.WriteLine("Successfully acquired polygon file.");
                    }
                    catch (WebException ex)
                    {
                        Console.WriteLine($"Failed to acquire file from drive throttling error: {ex.Message}");
                        backoffMs *= 2;
                    }
                    catch (Exception ex)
                    {
                        Console.Write($"Failed to acquire file from drive: {ex.Message}");
                    }

                    int secondsRemaining = (fileRecordUtility.RegionShapeFileRecords.Count -
                                            (idx + 1)) * (backoffMs / 1000);

                    Console.WriteLine($"Waiting for {backoffMs}ms before attempting next request.");
                    Console.WriteLine($"Approximately {secondsRemaining} seconds remaining.");
                    Thread.Sleep(backoffMs);
                }
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