using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EarthPolygonFileUtility.Entities;
using Microsoft.VisualBasic.FileIO;
using Attribute = EarthPolygonFileUtility.Entities.Attribute;

namespace EarthPolygonFileUtility
{
    public class TableFileRecordUtility
    {
        public List<Plant> PlantRecords { get; set; } = new List<Plant>();
        public List<Attribute> AttributeRecords { get; set; } = new List<Attribute>();
        public List<RegionShapeFile> RegionShapeFileRecords { get; set; } = new List<RegionShapeFile>();

        private static readonly int KeyCombinationBaseOffset = 1000;
        private static readonly string[] Delimiters = new string[] {","};

        public void Run()
        {
            string[] directories = Directory.GetDirectories(Program.TablesDirectory);
            directories.ToList().ForEach(it =>
                Console.WriteLine($"Directory found: {it}"));

            int idx = 0;
            directories.ToList().ForEach(it =>
            {
                List<string> files = Directory.GetFiles(it).ToList();

                if (files.Count >= 3)
                {
                    string plantFilename = files.Find(x => x.Contains("Plant"));
                    string attributeFilename = files.Find(x => x.Contains("Attribute"));
                    string regionShapeFilename = files.Find(x => x.Contains("RegionShapeFile"));

                    ReadPlantsCsv(plantFilename, idx)
                        .ForEach(x => PlantRecords.Add(x));
                    ReadAttributesCsv(attributeFilename, idx)
                        .ForEach(x => AttributeRecords.Add(x));
                    ReadRegionShapeFileCsv(regionShapeFilename, idx)
                        .ForEach(x => RegionShapeFileRecords.Add(x));

                    idx++;
                }
            });
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public List<Plant> ReadPlantsCsv(string filepath, int plantIdOffsetIdx)
        {
            List<Plant> plants = new List<Plant>();

            if (!string.IsNullOrEmpty(filepath))
                using (TextFieldParser parser = new TextFieldParser(filepath))
                {
                    parser.SetDelimiters(Delimiters);
                    parser.HasFieldsEnclosedInQuotes = true;

                    parser.ReadLine();

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();

                        try
                        {
                            int offsetPlantId = Convert.ToInt32(fields[0]) +
                                                (KeyCombinationBaseOffset * plantIdOffsetIdx);
                            Plant p = new Plant()
                            {
                                PlantID = offsetPlantId,
                                CommonName = fields[1],
                                ScientificName = fields[2],
                                PlantDescription = fields[3],
                                IsEdible = fields[4].Equals("TRUE")
                            };

                            plants.Add(p);
                        }
                        catch (FormatException ex)
                        {
                            Console.WriteLine("Can't add this plant.");
                        }
                    }
                }

            return plants;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="plantIdOffsetIdx"></param>
        /// <returns></returns>
        public List<Attribute> ReadAttributesCsv(string filepath, int plantIdOffsetIdx)
        {
            List<Attribute> attributes = new List<Attribute>();

            if (!string.IsNullOrEmpty(filepath))
                using (TextFieldParser parser = new TextFieldParser(filepath))
                {
                    parser.SetDelimiters(Delimiters);
                    parser.HasFieldsEnclosedInQuotes = true;

                    parser.ReadLine();

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();

                        try
                        {
                            int offsetPlantId = Convert.ToInt32(fields[1]) +
                                                (KeyCombinationBaseOffset * plantIdOffsetIdx);

                            Attribute a = new Attribute()
                            {
                                PlantID = offsetPlantId,
                                AttributeDescription = fields[2]
                            };

                            attributes.Add(a);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to get a Plant ID on AttributeID: {fields[0]}");
                        }
                    }
                }

            return attributes;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="plantIdOffsetIdx"></param>
        /// <returns></returns>
        public List<RegionShapeFile> ReadRegionShapeFileCsv(string filepath, int plantIdOffsetIdx)
        {
            List<RegionShapeFile> regionShapeFiles = new List<RegionShapeFile>();

            if (!string.IsNullOrEmpty(filepath))
                using (TextFieldParser parser = new TextFieldParser(filepath))
                {
                    parser.SetDelimiters(Delimiters);
                    parser.HasFieldsEnclosedInQuotes = true;

                    parser.ReadLine();

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();

                        try
                        {
                            int offsetPlantId = Convert.ToInt32(fields[1]) +
                                                (KeyCombinationBaseOffset * plantIdOffsetIdx);

                            string url = fields[2];
                            url = url.Replace("open?", "uc?export=download&");
                            url = url.Replace("#", "");

                            RegionShapeFile rsf = new RegionShapeFile()
                            {
                                PlantID = offsetPlantId,
                                LinkToFile = url
                            };

                            regionShapeFiles.Add(rsf);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to get a PlantID ID on RegionShapeFileID: {fields[0]}");
                        }
                    }
                }

            return regionShapeFiles;
        }
    }
}