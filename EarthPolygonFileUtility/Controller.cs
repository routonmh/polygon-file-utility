
using System;
using System.Collections.Generic;
using System.Data.Common;

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



            // List<string> urls = GetUrls();

            // KmzFileToPointCollectionUtility kmzUtility = new KmzFileToPointCollectionUtility(urls[0]);
            // kmzUtility.GetPolygons();

        }
    }
}