using System;
using NDesk.Options;
using System.IO;
using System.Collections.Generic;

namespace DupeFinder
{
    public static class ParameterOptions
    {
        static OptionSet _os;
        static List<string> _directories;
        static List<string> _filters;

        static string _output;

        public static List<string> Directories {get {
            return _directories;
        }}

        public static List<string> Filters { get {
            return _filters;
        }}

        public static string Output { get {
            return _output;
        }}

        
        public static void Parse(string[] parameters)
        {
            _os = new OptionSet() {
                { "d|directory=", "The name of directory to process.", d => ParameterOptions.proccessDirectories(d) },
                { "f|filters=", "Filters to limit file processing.", f => ParameterOptions.processFilters(f) },
                { "o|output=", "Output of processing.", f => ParameterOptions.processOutput(f) },
            };

            _os.Parse(parameters);
        }

        private static void proccessDirectories(string directoryList)
        {
            if(string.IsNullOrEmpty(directoryList))
            {
                Console.WriteLine("Directory parameter is not defined.");
            }

            List<string> directoryArray = new List<string>(directoryList.Split(","));

            foreach(string directory in directoryArray)
            {
                if(!Directory.Exists(directory))
                {
                    Console.WriteLine($"Directory: [{directory}] does not exist.");

                    throw new Exception($"Directory: [{directory}] does not exist.");
                }
            }

            _directories = directoryArray;
        }

        private static void processFilters(string filters)
        {
            List<string> filtersList = new List<string>();
            
            if(!string.IsNullOrEmpty(filters))
            {
                foreach(string filter in filters.Split(","))
                {
                    filtersList.Add(filter);
                }
            }

            _filters = filtersList;
        }

        private static void processOutput(string output)
        {
            if(string.IsNullOrEmpty(output))
            {
                Console.WriteLine("Output is undefined.");
                throw new Exception("Output file is undefined.");
            }

            _output = output;
        }
    }
}