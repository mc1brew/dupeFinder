using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;



using System.Linq;

namespace DupeFinder
{
    class Program
    {
        static int filesProcessed = 0;
        static void Main(string[] args)
        {    
            ParameterOptions.Parse(args);
            
            DupeFinder dupeFinder = new DupeFinder();
            dupeFinder.DuplicateFound += OutputDuplicateFound;
            dupeFinder.DuplicateNotFound += OutputDuplicateNotFound;
            dupeFinder.FileProcessed += OnFileProcessed;
            dupeFinder.ScanBegins += OnpreliminaryScan;
            FileDictionary fileDictionary = dupeFinder.FindMatches(ParameterOptions.Directories, ParameterOptions.Filters);

            //Write the output files to csv
            Console.WriteLine();
            Console.WriteLine($"\n{fileDictionary.Count} Matches Found\n");
            File.Delete(ParameterOptions.Output);
            File.AppendAllLines(ParameterOptions.Output, fileDictionary.ToStringArray());

            SelectAndDeleteDuplicates(fileDictionary);
        }

        public static void OutputDuplicateFound(object sender, EventArgs e)
        {
            Console.Write("x");
        }

        public static void OutputDuplicateNotFound(object sender, EventArgs e)
        {
            Console.Write(".");
        }

        public static void OnFileProcessed(object sender, EventArgs e)
        {
            filesProcessed++;
            if(filesProcessed%100 == 0)
            {
                Console.WriteLine();
                filesProcessed = 0;
            }
        }
        
        public static void OnpreliminaryScan(object sender, OnScanBeginEventArgs e)
        {
            if(e.PreliminaryScan) Console.WriteLine($"\nBegin Preliminary File Scan: {e.FilesToBeScanned} Files");
            else Console.WriteLine($"\nBegin Full File Scan: {e.FilesToBeScanned} Files");
        }
        public static void SelectAndDeleteDuplicates(FileDictionary fileDictionary)
        {
            foreach (var file in fileDictionary)
            {
                Console.WriteLine($"Duplicate found. Select which file to keep:");

                int i = 1, maxSelection = 0, selection = -1;

                Dictionary<int, string> filePathDictionary = new Dictionary<int, string>();

                foreach (var filePath in file.Value)
                {
                    filePathDictionary.Add(i, filePath);
                    Console.WriteLine($"{i}) {filePath}");
                    i++;
                }
                Console.WriteLine($"\nOr press 0 to skip.\n");

                maxSelection = i - 1;

                while (selection < 0 || selection > maxSelection)
                {
                    var userInput = Console.ReadLine();

                    if(!int.TryParse(userInput, out selection))
                    {
                        selection = -1;
                        Console.WriteLine($"Selection '{userInput}' is not valid.  Please select a value between 0 and {maxSelection}");
                    }
                    else if (selection < 0 || selection > maxSelection)
                    {
                        selection = -1;
                        Console.WriteLine($"Selection '{userInput}' was out of bounds.  Please select a value between 0 and {maxSelection}");
                    }
                }

                if(selection == 0)
                {
                    Console.WriteLine("Skipping selection.\n");
                    continue;
                }
                else {
                    Console.WriteLine($"{filePathDictionary[selection]} is selected.  All others will be deleted.");

                    file.Value.Remove(filePathDictionary[selection]);

                    foreach (var filePath in file.Value)
                    {
                        Console.WriteLine($"Deleting file located at: {filePath}\n");
                        File.Delete(filePath);
                    }
                }
            }
        }
    }
}
