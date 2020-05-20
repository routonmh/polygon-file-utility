using System;
using System.Collections.Generic;
using System.IO;
using EarthPolygonFileUtility.Entities;
using Microsoft.VisualBasic.FileIO;

namespace EarthPolygonFileUtility
{
    public class FileIDNameUtility
    {
        public List<KmzFileInfo> FileInfos { get; set; }

        private static readonly string[] Delimiters = new string[] {","};

        public List<KmzFileInfo> ReadInfoFile(string filepath)
        {
            List<KmzFileInfo> files = new List<KmzFileInfo>();

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
                            string name = fields[1];
                            string origin = fields[2];

                            // Google drive automatically did this, also have to account for changing "'" chars in files.
                            name = FilenameUtility.MakeCompliantFilename(name);

                            KmzFileInfo a = new KmzFileInfo()
                            {
                                FileID = fields[0],
                                Name = name,
                                Origin = origin,
                                FullPath = Path.Combine(Program.KmzFileDirectory, origin, name)
                            };

                            files.Add(a);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to read a row in file info.");
                        }
                    }
                }

            FileInfos = files;
            return files;
        }
    }
}