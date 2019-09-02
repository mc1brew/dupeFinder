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
        static char outputChar_match = 'x';
        static char outpucChar_noMatch = '.';

        static void Main(string[] args)
        {    
            ParameterOptions.Parse(args);
            FileDictionary fileDictionary = new FileDictionary();
            
            //Perform preliminary file scan on the first kilobyte.
            foreach(var directory in ParameterOptions.Directories){
                fileDictionary = PreliminaryFileScan(directory, ParameterOptions.Filters);
            }

            //Process records that represent potential matches
            fileDictionary = ScanAllFiles(fileDictionary);

            //Write the output files to csv
            Console.WriteLine();
            Console.WriteLine($"\n{fileDictionary.Count} Matches Found\n");
            File.Delete(ParameterOptions.Output);
            File.AppendAllLines(ParameterOptions.Output, fileDictionary.ToStringArray());
        }

        public static FileDictionary PreliminaryFileScan(string path, List<string> filters)
        {
            //Get contents of directory;
            int fileCounter = 0;
            string[] filePaths = GetFilteredFiles(path, filters);
            int fileCount = filePaths.Length;
            FileDictionary returnFileDictionary = new FileDictionary();

            Console.WriteLine($"\nBegin Preliminary File Scan: {filePaths.Length} Files");
            foreach(var filePath in filePaths)
            {
                char outputChar = outpucChar_noMatch;
                var firstKilobyteHash = ByteTool.GetKilobyteMd5Hash(filePath);
                
                try {
                    if(returnFileDictionary.Add(firstKilobyteHash, filePath)) outputChar = outputChar_match;
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

            return returnFileDictionary;
        }
        
        /// Process all preliminary scans and do a full byte comparison to see if they are actual matches.
        /// 
        /// fileDictionary: A list of all possible matches.
        public static FileDictionary ScanAllFiles(FileDictionary fileDictionary)
        {
            int fileCount = fileDictionary.Count;
            var filesForFullScan = fileDictionary.Where(x => x.Value.Count > 1);
            FileDictionary returnFileDictionary = new FileDictionary();

            Console.WriteLine($"\n\nBegin Full File Scan: {filesForFullScan.Count()} Files");

            int iterationCounter = 0;
            foreach(var fileDictionaryItem in filesForFullScan)
            {
                char outputChar = outpucChar_noMatch;
                string[] filePathArray = fileDictionaryItem.Value.ToArray();

                //Scrolling through the array of potential file matches to determine if there is a full match.
                //referencePathPosition is the the reference file for the comparison.
                //comparePathPosition is the file being compared.
                for(int referencePathPosition = 0; referencePathPosition < filePathArray.Length; referencePathPosition++)
                {
                    for(int comparePathPosition = referencePathPosition+1; comparePathPosition < filePathArray.Length; comparePathPosition++)
                    {
                        //If the files are a byte match then we get the full hash for the dictionary for accurate tracking.
                        //This is going to be a problem for really big files (over a couple of gigs).
                        //I'll have to figure that out later.
                        if(ByteTool.CompareByteArray(filePathArray[referencePathPosition], filePathArray[comparePathPosition]))
                        {
                            using (MD5 crypt = MD5.Create())
                            {
                                var byteArrayOfFile = File.ReadAllBytes(filePathArray[referencePathPosition]);
                                var fileHash = crypt.ComputeHash(byteArrayOfFile);
                                var fileHashString = BitConverter.ToString(fileHash).Replace("-", "").ToLower().ToLowerInvariant();

                                try {
                                    returnFileDictionary.Add(fileHashString, filePathArray[referencePathPosition]);
                                    //This statement is an if because it's possible the file might have been added once during the comparions.
                                    //Right now the comparison looks like this:
                                    //| F1 | F2 | F3 | F4 |
                                    // If F1 matches F2 and F3 on the first sweep then on the second sweep F2 will match F3.
                                    //This means that it would appear to be another match, but really we already knew this.
                                    //There is probably a more convenient way of dropping those out.  Probably I can just pop it out of the array.
                                    if(returnFileDictionary.Add(fileHashString, filePathArray[comparePathPosition])) outputChar = outputChar_match;
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

        ///Get the paths of the files to be scanned based on the path and designated filters.
        ///
        /// path: directory to look for files.
        /// filters: file types to look for.
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
