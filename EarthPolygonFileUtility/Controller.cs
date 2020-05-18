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

            // List<Polygon> polygons = fetchAndReadKmzFiles(fileRecordUtility, 750);



        }

        private List<Polygon> fetchAndReadKmzFiles(TableFileRecordUtility fileRecordUtility, int requestDelayMs)
        {
            List<Polygon> allPolygons = new List<Polygon>();

            for (int idx = 0; idx < fileRecordUtility.RegionShapeFileRecords.Count; idx++)
            {
                RegionShapeFile rsf = fileRecordUtility.RegionShapeFileRecords[idx];
                KmzFileToPointCollectionUtility kmzUtility = new KmzFileToPointCollectionUtility(rsf.LinkToFile);
                kmzUtility.GetPolygons(rsf.PlantID).ForEach(x => allPolygons.Add(x));
                // Wait to download the next file
                Thread.Sleep(requestDelayMs);
            }

            return allPolygons;
        }

    }
}