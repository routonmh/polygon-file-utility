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

        static void Main(string[] args)
        {
            ConnectionString = args[0];
            TablesDirectory = args[1];

            new Controller().Start();
        }
    }
}