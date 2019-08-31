using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

using System.Linq;

namespace dupeFinder
{
    class Program
    {
        static void Main(string[] args)
        {    
            ParameterOptions.Parse(args);

            FileDictionary fileDictionary = new FileDictionary();

            //Perform preliminary file scan on the first kilobyte.
            foreach(var directory in ParameterOptions.Directories){
                fileDictionary = PreliminaryFileScan(fileDictionary, directory, ParameterOptions.Filters);
            }

            //Process records that represent potential matches
            foreach(var directory in ParameterOptions.Directories){
                fileDictionary = ScanAllFiles(fileDictionary, directory, ParameterOptions.Filters);
            }

            //Write the output files to csv
            Console.WriteLine();
            Console.WriteLine($"\n{fileDictionary.Count} Matches Found\n");
            File.Delete(ParameterOptions.Output);
            File.AppendAllLines(ParameterOptions.Output, fileDictionary.ToStringArray());
        }

        public static FileDictionary PreliminaryFileScan(FileDictionary fileDictionary, string path, List<string> filters)
        {
            //Get contents of directory;
            var filePaths = GetFilteredFiles(path, filters);

            int fileCounter = 0;
            int fileCount = filePaths.Length;

            Console.WriteLine($"\nBegin Preliminary File Scan: {filePaths.Length} Files");
            foreach(var filePath in filePaths)
            {
                char outputChar = '.';

                var firstHundredByteHash = ByteTool.GetHundredByteMd5Hash(filePath);
                try {
                    if(fileDictionary.Add(firstHundredByteHash, filePath)) outputChar = 'x';
                }
                catch(Exception ex)
                {
                    throw ex;
                }

                Console.Write(outputChar);

                fileCounter++;
                if(fileCounter%100 == 0)
                {
                    Console.WriteLine();
                }
            }

            return fileDictionary;
        }
        public static FileDictionary ScanAllFiles(FileDictionary fileDictionary, string path, List<string> filters)
        {
            int fileCount = fileDictionary.Count;
            var filesForFullScan = fileDictionary.Where(x => x.Value.Count > 1);
            FileDictionary returnFileDictionary = new FileDictionary();

            Console.WriteLine($"\n\nBegin Full File Scan: {filesForFullScan.Count()} Files");

            int iterationCounter = 0;
            foreach(var fileDictionaryItem in filesForFullScan)
            {
                char outputChar = '.';
                string[] filePathArray = fileDictionaryItem.Value.ToArray();

                for(int referencePathPosition = 0; referencePathPosition < filePathArray.Length; referencePathPosition++)
                {
                    for(int comparePathPosition = referencePathPosition+1; comparePathPosition < filePathArray.Length; comparePathPosition++)
                    {
                        if(ByteTool.CompareByteArray(filePathArray[referencePathPosition], filePathArray[comparePathPosition]))
                        {
                            using (MD5 crypt = MD5.Create())
                            {
                                var byteArrayOfFile = File.ReadAllBytes(filePathArray[referencePathPosition]);
                                var fileHash = crypt.ComputeHash(byteArrayOfFile);
                                var fileHashString = BitConverter.ToString(fileHash).Replace("-", "").ToLower().ToLowerInvariant();

                                try {
                                    returnFileDictionary.Add(fileHashString, filePathArray[referencePathPosition]);
                                    if(returnFileDictionary.Add(fileHashString, filePathArray[comparePathPosition])) outputChar = 'x';
                                }
                                catch(Exception ex)
                                {
                                    throw ex;
                                }
                            }
                        }
                        Console.Write(outputChar);
                        iterationCounter++;
                        if(iterationCounter%100 == 0)
                        {
                            Console.WriteLine();
                        }
                    }
                }
            }

            return returnFileDictionary;
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
