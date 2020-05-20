using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EarthPolygonFileUtility
{
    class Program
    {
        public static string TablesDirectory { get; set; }
        public static string KmzFileDirectory { get; set; }
        public static string OutputDirectory { get; set; }
        public static string DriveFileIDCsvPath { get; set; }
        public static string TemporaryFilesDirectory { get; set; } = "temp-files";

        static void Main(string[] args)
        {
            TablesDirectory = args[0];
            KmzFileDirectory = args[1];
            OutputDirectory = args[2];
            DriveFileIDCsvPath = args[3];

            new Controller().Start();
        }
    }
}