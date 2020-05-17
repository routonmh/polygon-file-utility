using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EarthPolygonFileUtility
{
    class Program
    {
        public static string ConnectionString { get; set; }

        static void Main(string[] args)
        {
            ConnectionString = args[0];

            new Controller().Start();
        }
    }
}