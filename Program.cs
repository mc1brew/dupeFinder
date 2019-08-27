using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using System.Security.Cryptography;

namespace dupeFinder
{
    class Program
    {
        static void Main(string[] args)
        {    
            ParameterOptions.Parse(args);

            FileDictionary fileDictionary = new FileDictionary();

            foreach(var directory in ParameterOptions.Directories){
                fileDictionary = ScanAllFiles(fileDictionary, directory, ParameterOptions.Filters);
            }

            File.AppendAllLines(ParameterOptions.Output, fileDictionary.ToStringArray());
        }

        public static FileDictionary ScanAllFiles(FileDictionary fileDictionary, string path, List<string> filters)
        {
            //Get contents of directory;
            var files = GetFilteredFiles(path, filters);

            int fileCounter = 0;
            int fileCount = files.Length;
            foreach(var file in files)
            {
                var filePath = file;
                var byteArrayOfFile = File.ReadAllBytes(filePath);

                using (MD5 crypt = MD5.Create())
                {
                    var fileHash = crypt.ComputeHash(byteArrayOfFile);
                    var fileHashString = BitConverter.ToString(fileHash).Replace("-", "").ToLower().ToLowerInvariant();

                    try {
                        fileDictionary.Add(fileHashString, filePath);
                    }
                    catch(Exception ex)
                    {
                        throw ex;
                    }
                }

                fileCounter++;
                if(fileCounter%100 == 0)
                {
                    Console.WriteLine($"{fileCounter}/{fileCount} files processed.");
                }
            }

            return fileDictionary;
        }

        public static string[] GetFilteredFiles(string path, List<string> filters)
        {
            List<string> filesArray = new List<string>();

            foreach(string filter in filters)
            {
                filesArray.AddRange(Directory.GetFiles(path, filter, SearchOption.AllDirectories));
            }

            return filesArray.ToArray();
        }
    }
}
