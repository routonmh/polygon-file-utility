using System;
using System.Collections.Generic;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace EarthPolygonFileUtility
{
    public class Controller
    {
        public Controller()
        {
        }

        public void Start()
        {
            List<string> urls = GetUrls();
            KmzFileToPointCollectionUtility kmzUtility = new KmzFileToPointCollectionUtility(urls[0]);
            kmzUtility.Run();

        }

        public List<string> GetUrls()
        {
            List<string> urls = new List<string>();

            MySqlConnection conn = new MySqlConnection(Program.ConnectionString);
            conn.Open();
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT LinkToFile FROM region_shape_file";

            DbDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string link = reader["LinkToFile"] as string ?? string.Empty;
                if (!string.IsNullOrEmpty(link))
                {
                    link = link.Replace("open?", "uc?export=download&");
                    urls.Add(link.Replace("#", ""));
                }
            }

            return urls;
        }
    }
}