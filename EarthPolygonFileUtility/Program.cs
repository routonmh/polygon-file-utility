using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EarthPolygonFileUtility
{
    class Program
    {
        public static string ConnectionString { get; set; }
        public static string TablesDirectory { get; set; }
        public static string TemporaryDataDirectory { get; set; }
        public static string OutputDirectory { get; set; }

        static void Main(string[] args)
        {
            TablesDirectory = args[0];
            TemporaryDataDirectory = args[1];
            OutputDirectory = args[2];

            new Controller().Start();
        }
    }
}